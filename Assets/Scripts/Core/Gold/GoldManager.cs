using UnityEngine;
using System;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core.Gold
{
    /// <summary>
    /// Unity特化ゴールドシステムのメインマネージャー
    /// IUnityGameSystem, IUnityResettable, IApplicationLifecycleを実装
    /// </summary>
    public class GoldManager : GameSystemBase, IUnityResettable, IPersistentSystem
    {
        [Header("ゴールドシステム設定")]
        [SerializeField] private int initialGold = 100;
        [SerializeField] private int maxGold = 999999;
        [SerializeField] private bool enableAutosave = true;
        [SerializeField] private float autosaveInterval = 30f;

        // シングルトンパターン
        private static GoldManager _instance;
        public static GoldManager Instance => _instance;

        // 現在のゴールド量
        private int _currentGold;
        public int CurrentGold => _currentGold;

        // イベントシステム
        public event Action<int> OnGoldChanged;
        public event Action<int, int> OnGoldSpent;
        public event Action<int, int> OnGoldEarned;
        public event Action<string> OnTransactionFailed;

        // 取引履歴
        private GoldTransactionHistory _transactionHistory;
        public GoldTransactionHistory TransactionHistory => _transactionHistory;

        // ゴールド計算機
        private GoldCalculator _calculator;
        public GoldCalculator Calculator => _calculator;

        // 開発設定
        private DevelopmentSettings _developmentSettings;

        // 永続化
        private const string GOLD_SAVE_KEY = "GatchaSpire_Gold";
        private float _lastAutosaveTime;

        protected override string SystemName => "GoldManager";

        // Awakeで初期化
        private void Awake()
        {
            OnAwake();
        }

        protected override void OnSystemInitialize()
        {
            // シングルトン設定
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                ReportWarning("GoldManagerの複数インスタンスを検出しました。重複インスタンスを破棄します。");
                Destroy(gameObject);
                return;
            }
            
            // 依存システムの初期化
            _transactionHistory = new GoldTransactionHistory();
            _calculator = new GoldCalculator();
            
            // 開発設定の取得
            _developmentSettings = Resources.Load<DevelopmentSettings>("DevelopmentSettings");
            if (_developmentSettings == null)
            {
                ReportWarning("DevelopmentSettingsが見つかりません。デフォルト設定を使用します。");
            }

            // 永続化データの読み込み
            LoadGoldData();
            
            // 計算機の初期化
            _calculator.Initialize(_developmentSettings);
            
            ReportInfo("ゴールドシステムが初期化されました");
        }

        protected override void OnSystemStart()
        {
            _lastAutosaveTime = Time.time;
        }

        protected override void OnSystemUpdate()
        {
            // 自動保存の処理
            if (enableAutosave && Time.time - _lastAutosaveTime >= autosaveInterval)
            {
                SaveGoldData();
                _lastAutosaveTime = Time.time;
            }
        }

        protected override void OnSystemShutdown()
        {
            SaveGoldData();
            
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// ゴールドを追加
        /// </summary>
        /// <param name="amount">追加量</param>
        /// <param name="reason">理由</param>
        /// <returns>実際に追加された量</returns>
        public int AddGold(int amount, string reason = "")
        {
            if (amount <= 0)
            {
                ReportWarning($"無効なゴールド追加量: {amount}");
                OnTransactionFailed?.Invoke($"無効な追加量: {amount}");
                return 0;
            }

            int previousGold = _currentGold;
            int actualAmount = Mathf.Min(amount, maxGold - _currentGold);
            
            if (actualAmount <= 0)
            {
                ReportWarning("ゴールドが上限に達しています");
                OnTransactionFailed?.Invoke("ゴールドが上限に達しています");
                return 0;
            }

            _currentGold += actualAmount;
            
            // 取引履歴に記録
            _transactionHistory.RecordTransaction(actualAmount, reason, _currentGold);
            
            // イベント発火
            OnGoldEarned?.Invoke(actualAmount, _currentGold);
            OnGoldChanged?.Invoke(_currentGold);
            
            ReportInfo($"ゴールド追加: {actualAmount} (理由: {reason})");
            return actualAmount;
        }

        /// <summary>
        /// ゴールドを消費
        /// </summary>
        /// <param name="amount">消費量</param>
        /// <param name="reason">理由</param>
        /// <returns>消費が成功したかどうか</returns>
        public bool SpendGold(int amount, string reason = "")
        {
            if (amount < 0)
            {
                ReportWarning($"無効なゴールド消費量: {amount}");
                OnTransactionFailed?.Invoke($"無効な消費量: {amount}");
                return false;
            }

            if (_currentGold < amount)
            {
                ReportWarning($"ゴールドが不足しています (必要: {amount}, 現在: {_currentGold})");
                OnTransactionFailed?.Invoke($"ゴールドが不足しています (必要: {amount}, 現在: {_currentGold})");
                return false;
            }

            int previousGold = _currentGold;
            _currentGold -= amount;
            
            // 取引履歴に記録（消費は負の値で記録）
            _transactionHistory.RecordTransaction(-amount, reason, _currentGold);
            
            // イベント発火
            OnGoldSpent?.Invoke(amount, _currentGold);
            OnGoldChanged?.Invoke(_currentGold);
            
            ReportInfo($"ゴールド消費: {amount} (理由: {reason})");
            return true;
        }

        /// <summary>
        /// ゴールドを設定（デバッグ用）
        /// </summary>
        /// <param name="amount">設定量</param>
        public void SetGold(int amount)
        {
            if (_developmentSettings == null || !_developmentSettings.EnableDebugCommands)
            {
                ReportWarning("デバッグコマンドが無効です");
                return;
            }

            amount = Mathf.Clamp(amount, 0, maxGold);
            int previousGold = _currentGold;
            _currentGold = amount;
            
            _transactionHistory.RecordTransaction(amount - previousGold, "デバッグ設定", _currentGold);
            
            OnGoldChanged?.Invoke(_currentGold);
            ReportInfo($"ゴールドをデバッグ設定: {amount}");
        }

        /// <summary>
        /// ゴールドの支払い可能性をチェック
        /// </summary>
        /// <param name="amount">チェック量</param>
        /// <returns>支払い可能かどうか</returns>
        public bool CanAfford(int amount)
        {
            return _currentGold >= amount && amount > 0;
        }

        /// <summary>
        /// システムリセット（IUnityResettable実装）
        /// </summary>
        override public void ResetSystem()
        {
            _currentGold = initialGold;
            _transactionHistory.Clear();
            
            OnGoldChanged?.Invoke(_currentGold);
            ReportInfo("ゴールドシステムをリセットしました");
        }

        /// <summary>
        /// データ保存（IPersistentSystem実装）
        /// </summary>
        public void SaveData()
        {
            SaveGoldData();
        }

        /// <summary>
        /// 未実装（IPersistentSystem実装）
        /// </summary>
        public bool HasSaveData()
        {
            return false;
        }

        /// <summary>
        /// データ読み込み（IPersistentSystem実装）
        /// </summary>
        public void LoadData()
        {
            LoadGoldData();
        }

        /// <summary>
        /// 未実装（IPersistentSystem実装）
        /// </summary>
        public void DeleteSaveData()
        {

        }

        /// <summary>
        /// ゴールドデータの保存
        /// </summary>
        private void SaveGoldData()
        {
            try
            {
                var goldData = new GoldSaveData
                {
                    currentGold = _currentGold,
                    transactionHistory = _transactionHistory.GetRecentTransactions(100) // 最新100件のみ保存
                };
                
                string jsonData = JsonUtility.ToJson(goldData);
                PlayerPrefs.SetString(GOLD_SAVE_KEY, jsonData);
                PlayerPrefs.Save();
                
                ReportInfo("ゴールドデータを保存しました");
            }
            catch (Exception ex)
            {
                ReportError($"ゴールドデータの保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ゴールドデータの読み込み
        /// </summary>
        private void LoadGoldData()
        {
            try
            {
                if (PlayerPrefs.HasKey(GOLD_SAVE_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(GOLD_SAVE_KEY);
                    var goldData = JsonUtility.FromJson<GoldSaveData>(jsonData);
                    
                    _currentGold = Mathf.Clamp(goldData.currentGold, 0, maxGold);
                    
                    // 取引履歴の復元
                    if (goldData.transactionHistory != null)
                    {
                        _transactionHistory.RestoreTransactions(goldData.transactionHistory);
                    }
                    
                    ReportInfo($"ゴールドデータを読み込みました (金額: {_currentGold})");
                }
                else
                {
                    _currentGold = initialGold;
                    ReportInfo($"初期ゴールドを設定しました (金額: {_currentGold})");
                }
            }
            catch (Exception ex)
            {
                ReportError($"ゴールドデータの読み込みに失敗しました: {ex.Message}");
                _currentGold = initialGold;
            }
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGoldData();
            }
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveGoldData();
            }
        }

        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        /// <returns>デバッグ情報</returns>
        public string GetDebugInfo()
        {
            return $"ゴールドシステム状態:\n" +
                   $"- 現在のゴールド: {_currentGold:N0}\n" +
                   $"- 最大ゴールド: {maxGold:N0}\n" +
                   $"- 自動保存: {enableAutosave}\n" +
                   $"- 取引履歴数: {_transactionHistory.TransactionCount}\n" +
                   $"- 計算機状態: {(_calculator != null ? "初期化済み" : "未初期化")}";
        }

        /// <summary>
        /// システムの健康状態を取得
        /// </summary>
        public bool IsSystemHealthy()
        {
            return IsInitialized();
        }
    }

    /// <summary>
    /// ゴールドデータの保存用クラス
    /// </summary>
    [Serializable]
    public class GoldSaveData
    {
        public int currentGold;
        public List<GoldTransaction> transactionHistory;
    }
}