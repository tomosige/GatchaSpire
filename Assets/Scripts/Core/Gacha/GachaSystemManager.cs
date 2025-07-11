using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Gold;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャシステム統合管理クラス
    /// </summary>
    [DefaultExecutionOrder(-40)] // GoldManagerの後に実行
    public class GachaSystemManager : GameSystemBase, IPersistentSystem
    {
        [Header("ガチャ設定")]
        [SerializeField] private GachaSystemData gachaSystemData;
        
        [Header("Unity統合")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool validateOnStart = true;

        // システム関連
        private static GachaSystemManager _instance;
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;

        // ガチャ状態
        private int currentGachaLevel = 1;
        private GachaHistory gachaHistory;
        private GachaResult lastPullResult;
        private System.Random randomGenerator;

        // プロパティ
        public static GachaSystemManager Instance => _instance;
        public int CurrentGachaLevel => currentGachaLevel;
        public GachaHistory History => gachaHistory;
        public GachaResult LastPullResult => lastPullResult;

        protected override string SystemName => "GachaSystemManager";

        #region Unity ライフサイクル

        private void Awake()
        {
            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        #endregion

        #region GameSystemBase 実装

        protected override void OnSystemAwake()
        {
            // シングルトン設定
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                ReportWarning("GachaSystemManagerの複数インスタンスを検出しました。重複インスタンスを破棄します。");
                Destroy(gameObject);
                return;
            }

            InitializeReferences();
        }

        protected override void OnSystemInitialize()
        {
            InitializeGachaSystem();
            InitializeRandom();
            ApplyDevelopmentSettings();
            ReportInfo("ガチャシステムが初期化されました");
        }

        protected override void OnSystemStart()
        {
            if (validateOnStart)
            {
                ValidateSystem();
            }
        }

        protected override void OnSystemUpdate()
        {
            // ガチャシステムは更新を必要としない
        }

        protected override void OnSystemShutdown()
        {
            SaveGachaData();
        }

        protected override void OnSystemDestroy()
        {
            SaveGachaData();
        }

        protected override void OnSystemReset()
        {
            ResetGachaSystem();
        }

        protected override void OnUnityStateReset()
        {
            ResetGachaSystem();
        }

        #endregion

        #region IPersistentSystem実装

        public void SaveData()
        {
            SaveGachaData();
        }

        public void LoadData()
        {
            LoadGachaData();
        }

        public bool HasSaveData()
        {
            // TODO: セーブデータの存在確認
            return true; // 仮実装
        }

        public void DeleteSaveData()
        {
            // TODO: セーブデータの削除
            ReportInfo("ガチャセーブデータを削除しました");
        }

        #endregion

        #region 基本ガチャ機能

        /// <summary>
        /// ガチャ実行
        /// </summary>
        public List<CharacterData> PullGacha(int pullCount = 1)
        {
            try
            {
                if (!ValidateGachaOperation())
                {
                    return new List<CharacterData>();
                }

                if (!CanPullGacha())
                {
                    ReportWarning("ガチャを実行できません");
                    return new List<CharacterData>();
                }

                int totalCost = GetGachaCostWithDevelopmentSettings() * pullCount;
                
                // ゴールドの消費
                if (!goldManager.SpendGold(totalCost))
                {
                    ReportWarning("ゴールドが不足しています");
                    lastPullResult = new GachaResult(gachaSystemData.GachaSystemId, 0, currentGachaLevel, GachaStatus.InsufficientGold);
                    return new List<CharacterData>();
                }

                // ガチャ実行
                var result = ExecuteGachaPull(pullCount, totalCost);
                lastPullResult = result;

                // 履歴に追加
                gachaHistory.AddResult(result);

                ReportInfo($"ガチャを実行しました: {pullCount}回, コスト: {totalCost}, 結果: {result.PullResults.Count}体");
                return result.GetCharacters();
            }
            catch (Exception e)
            {
                ReportError("ガチャ実行中にエラーが発生しました", e);
                return new List<CharacterData>();
            }
        }

        /// <summary>
        /// ガチャ実行可能判定
        /// </summary>
        public bool CanPullGacha()
        {
            if (!IsInitialized())
                return false;

            if (gachaSystemData == null || !gachaSystemData.IsActive)
                return false;

            if (goldManager == null)
                return false;

            int cost = GetGachaCostWithDevelopmentSettings();
            return goldManager.CurrentGold >= cost;
        }

        /// <summary>
        /// 現在のガチャコスト取得
        /// </summary>
        public int GetGachaCost()
        {
            if (gachaSystemData == null)
                return 0;

            int baseCost = gachaSystemData.GetBaseCost();
            float reduction = gachaSystemData.GetCostReduction(currentGachaLevel);
            return Mathf.RoundToInt(baseCost * (1f - reduction));
        }

        /// <summary>
        /// 最後のガチャ結果取得
        /// </summary>
        public GachaResult GetLastPullResult()
        {
            return lastPullResult;
        }

        #endregion

        #region ガチャアップグレード機能

        /// <summary>
        /// ガチャレベルアップ実行
        /// </summary>
        public bool UpgradeGacha()
        {
            try
            {
                if (!CanUpgradeGacha())
                {
                    ReportWarning("ガチャをアップグレードできません");
                    return false;
                }

                int cost = GetUpgradeCost();
                
                if (!goldManager.SpendGold(cost))
                {
                    ReportWarning("アップグレードのゴールドが不足しています");
                    return false;
                }

                currentGachaLevel++;
                ReportInfo($"ガチャをレベル{currentGachaLevel}にアップグレードしました");
                return true;
            }
            catch (Exception e)
            {
                ReportError("ガチャアップグレード中にエラーが発生しました", e);
                return false;
            }
        }

        /// <summary>
        /// アップグレード可能判定
        /// </summary>
        public bool CanUpgradeGacha()
        {
            if (!IsInitialized() || gachaSystemData == null)
                return false;

            if (currentGachaLevel >= gachaSystemData.MaxUpgradeLevel)
                return false;

            int cost = GetUpgradeCost();
            return goldManager.CurrentGold >= cost;
        }

        /// <summary>
        /// アップグレードコスト取得
        /// </summary>
        public int GetUpgradeCost()
        {
            if (gachaSystemData == null)
                return 0;

            return gachaSystemData.GetUpgradeCost(currentGachaLevel);
        }

        /// <summary>
        /// 現在のガチャレベル取得
        /// </summary>
        public int GetCurrentLevel()
        {
            return currentGachaLevel;
        }

        /// <summary>
        /// アップグレードプレビュー取得
        /// </summary>
        public GachaUpgradePreview GetUpgradePreview()
        {
            if (gachaSystemData == null)
                return null;

            int nextLevel = currentGachaLevel + 1;
            int cost = GetUpgradeCost();
            bool canUpgrade = CanUpgradeGacha();
            
            var upgrade = gachaSystemData.GetUpgrade(nextLevel);
            string description = upgrade.effectDescription ?? "レベルアップ効果";

            var preview = new GachaUpgradePreview(currentGachaLevel, nextLevel, cost, description, canUpgrade);
            
            // 現在と次のレートを設定
            var currentRates = GetCurrentRarityRates();
            var nextRates = GetRarityRatesForLevel(nextLevel);
            preview.SetCurrentRates(currentRates);
            preview.SetNextRates(nextRates);
            
            // コスト設定
            int currentCost = GetGachaCost();
            int nextCost = GetGachaCostForLevel(nextLevel);
            preview.SetCosts(currentCost, nextCost);

            return preview;
        }

        #endregion

        #region 統合管理機能

        /// <summary>
        /// ガチャシステム全体のリセット
        /// </summary>
        public void ResetGachaSystem()
        {
            currentGachaLevel = 1;
            gachaHistory?.ClearHistory();
            lastPullResult = null;
            ReportInfo("ガチャシステムをリセットしました");
        }

        /// <summary>
        /// システム情報取得
        /// </summary>
        public GachaSystemInfo GetSystemInfo()
        {
            if (gachaSystemData == null)
                return null;

            return new GachaSystemInfo(
                currentGachaLevel,
                GetCurrentRarityRates(),
                GetGachaCost(),
                gachaSystemData.GetSimultaneousPullCount(currentGachaLevel),
                gachaHistory?.GetStatisticsSummary() ?? "統計情報なし"
            );
        }

        /// <summary>
        /// システム初期化
        /// </summary>
        public void InitializeGachaSystem(GachaSystemData data)
        {
            if (data == null)
            {
                ReportError("ガチャシステムデータがnullです");
                return;
            }

            gachaSystemData = data;
            Initialize();
        }

        #endregion

        #region 内部実装

        /// <summary>
        /// 参照初期化
        /// </summary>
        private void InitializeReferences()
        {
            goldManager = FindObjectOfType<GoldManager>();
            characterDatabase = FindObjectOfType<CharacterDatabase>();
        }

        /// <summary>
        /// ガチャシステム初期化
        /// </summary>
        private void InitializeGachaSystem()
        {
            if (gachaSystemData == null)
            {
                ReportError("GachaSystemDataが設定されていません");
                return;
            }

            currentGachaLevel = 1;
            gachaHistory = new GachaHistory();
            lastPullResult = null;
        }

        /// <summary>
        /// ランダム生成器初期化
        /// </summary>
        private void InitializeRandom()
        {
            randomGenerator = new System.Random(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// ガチャ実行
        /// </summary>
        private GachaResult ExecuteGachaPull(int pullCount, int totalCost)
        {
            var result = new GachaResult(gachaSystemData.GachaSystemId, totalCost, currentGachaLevel);
            
            int simultaneousPulls = gachaSystemData.GetSimultaneousPullCount(currentGachaLevel);
            int totalPulls = pullCount * simultaneousPulls;

            for (int i = 0; i < totalPulls; i++)
            {
                var rarity = CalculateRarityRate();
                var character = SelectRandomCharacter(rarity);
                var resultType = DetermineResultType(rarity, i);
                
                result.AddPullResult(character, rarity, resultType);
            }

            return result;
        }

        /// <summary>
        /// レアリティ排出率計算
        /// </summary>
        private CharacterRarity CalculateRarityRate()
        {
            // 天井システムチェック
            if (gachaSystemData.HasCeiling)
            {
                int consecutiveCount = gachaHistory.GetConsecutiveWithoutRarity(gachaSystemData.CeilingRarity);
                if (consecutiveCount >= gachaSystemData.CeilingCount)
                {
                    return gachaSystemData.CeilingRarity;
                }
            }

            // 保証システムチェック
            int consecutiveWithoutRare = gachaHistory.ConsecutiveWithoutRare;
            if (consecutiveWithoutRare >= gachaSystemData.GetGuaranteedRareCount())
            {
                return CharacterRarity.Rare;
            }

            // 通常抽選
            var rates = GetCurrentRarityRates();
            float roll = (float)randomGenerator.NextDouble() * 100f;
            float cumulative = 0f;

            foreach (var rate in rates.OrderByDescending(r => r.Key))
            {
                cumulative += rate.Value;
                if (roll <= cumulative)
                {
                    return rate.Key;
                }
            }

            return CharacterRarity.Common;
        }

        /// <summary>
        /// ランダムキャラクター選択
        /// </summary>
        private CharacterData SelectRandomCharacter(CharacterRarity rarity)
        {
            if (characterDatabase == null)
            {
                ReportError("CharacterDatabaseが見つかりません");
                return null;
            }

            var characters = characterDatabase.GetCharactersByRarity(rarity);
            if (characters.Count == 0)
            {
                ReportWarning($"レアリティ{rarity}のキャラクターが見つかりません");
                return null;
            }

            // ピックアップ処理
            if (gachaSystemData.HasPickup && gachaSystemData.PickupCharacterIds.Count > 0)
            {
                var pickupCharacters = characters.Where(c => gachaSystemData.PickupCharacterIds.Contains(c.CharacterId)).ToList();
                if (pickupCharacters.Count > 0)
                {
                    float pickupRoll = (float)randomGenerator.NextDouble() * 100f;
                    if (pickupRoll <= gachaSystemData.PickupRate)
                    {
                        int index = randomGenerator.Next(pickupCharacters.Count);
                        return pickupCharacters[index];
                    }
                }
            }

            // 通常抽選
            int randomIndex = randomGenerator.Next(characters.Count);
            return characters[randomIndex];
        }

        /// <summary>
        /// 結果タイプ判定
        /// </summary>
        private GachaResultType DetermineResultType(CharacterRarity rarity, int pullIndex)
        {
            // 天井判定
            if (gachaSystemData.HasCeiling)
            {
                int consecutiveCount = gachaHistory.GetConsecutiveWithoutRarity(gachaSystemData.CeilingRarity);
                if (consecutiveCount >= gachaSystemData.CeilingCount && rarity >= gachaSystemData.CeilingRarity)
                {
                    return GachaResultType.Ceiling;
                }
            }

            // 保証判定
            int consecutiveWithoutRare = gachaHistory.ConsecutiveWithoutRare;
            if (consecutiveWithoutRare >= gachaSystemData.GetGuaranteedRareCount() && rarity >= CharacterRarity.Rare)
            {
                return GachaResultType.Guaranteed;
            }

            return GachaResultType.Normal;
        }

        /// <summary>
        /// 現在のレアリティ排出率取得
        /// </summary>
        private Dictionary<CharacterRarity, float> GetCurrentRarityRates()
        {
            return GetRarityRatesForLevel(currentGachaLevel);
        }

        /// <summary>
        /// 指定レベルのレアリティ排出率取得
        /// </summary>
        private Dictionary<CharacterRarity, float> GetRarityRatesForLevel(int level)
        {
            var rates = new Dictionary<CharacterRarity, float>();
            
            if (gachaSystemData == null)
                return rates;

            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                float baseRate = gachaSystemData.GetBaseRarityRate(rarity);
                float bonus = gachaSystemData.GetLevelRarityBonus(level, rarity);
                rates[rarity] = baseRate + bonus;
            }

            return rates;
        }

        /// <summary>
        /// 指定レベルのガチャコスト取得
        /// </summary>
        private int GetGachaCostForLevel(int level)
        {
            if (gachaSystemData == null)
                return 0;

            int baseCost = gachaSystemData.GetBaseCost();
            float reduction = gachaSystemData.GetCostReduction(level);
            return Mathf.RoundToInt(baseCost * (1f - reduction));
        }

        /// <summary>
        /// ガチャ操作検証
        /// </summary>
        private bool ValidateGachaOperation()
        {
            if (!IsInitialized())
            {
                ReportError("ガチャシステムが初期化されていません");
                return false;
            }

            if (gachaSystemData == null)
            {
                ReportError("ガチャシステムデータが設定されていません");
                return false;
            }

            if (goldManager == null)
            {
                ReportError("ゴールドマネージャーが見つかりません");
                return false;
            }

            if (characterDatabase == null)
            {
                ReportError("キャラクターデータベースが見つかりません");
                return false;
            }

            return true;
        }

        /// <summary>
        /// システム検証
        /// </summary>
        private void ValidateSystem()
        {
            if (gachaSystemData != null)
            {
                var validation = gachaSystemData.Validate();
                if (!validation.IsValid)
                {
                    foreach (var error in validation.Errors)
                    {
                        ReportError($"データ検証エラー: {error}");
                    }
                }
            }
        }

        /// <summary>
        /// ガチャデータ保存
        /// </summary>
        private void SaveGachaData()
        {
            // TODO: セーブシステムとの連携
            if (enableDebugLogs)
                ReportInfo("ガチャデータを保存しました");
        }

        /// <summary>
        /// ガチャデータ読み込み
        /// </summary>
        private void LoadGachaData()
        {
            // TODO: セーブシステムとの連携
            if (enableDebugLogs)
                ReportInfo("ガチャデータを読み込みました");
        }

        /// <summary>
        /// 開発設定の適用
        /// </summary>
        private void ApplyDevelopmentSettings()
        {
            var devSettings = Resources.Load<DevelopmentSettings>("Settings/DevelopmentSettings");
            if (devSettings == null) return;

            enableDebugLogs = devSettings.EnableAllDebugLogs;
            
            // チート機能
            if (devSettings.NoGachaCost)
            {
                ReportInfo("開発設定: ガチャコスト無料化が有効");
            }
        }

        /// <summary>
        /// 開発設定適用したガチャコストを取得
        /// </summary>
        private int GetGachaCostWithDevelopmentSettings()
        {
            var devSettings = Resources.Load<DevelopmentSettings>("Settings/DevelopmentSettings");
            if (devSettings != null && devSettings.NoGachaCost)
            {
                return 0; // 無料
            }
            
            return GetGachaCost();
        }

        #endregion

        #region 開発用メソッド

        /// <summary>
        /// システムの健康状態を取得
        /// </summary>
        public bool IsSystemHealthy()
        {
            return IsInitialized() && 
                   goldManager != null && 
                   characterDatabase != null && 
                   gachaSystemData != null;
        }

        /// <summary>
        /// システムの詳細情報を取得
        /// </summary>
        public string GetSystemDetails()
        {
            return $"GachaSystemManager - レベル{currentGachaLevel}, 初期化済み: {IsInitialized()}, " +
                   $"総ガチャ回数: {gachaHistory?.TotalPulls ?? 0}";
        }

        /// <summary>
        /// テスト用ガチャ実行
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestGachaPull()
        {
            if (!IsInitialized())
            {
                ReportInfo("テスト実行: システムが初期化されていません");
                return;
            }

            var result = PullGacha(10);
            ReportInfo($"テストガチャ結果: {result.Count}体のキャラクターを取得");
        }

        #endregion
    }

    /// <summary>
    /// ガチャシステム情報
    /// </summary>
    [Serializable]
    public class GachaSystemInfo
    {
        [SerializeField] private int currentLevel;
        [SerializeField] private Dictionary<CharacterRarity, float> currentRarityRates;
        [SerializeField] private int currentCost;
        [SerializeField] private int currentSimultaneousPulls;
        [SerializeField] private string systemStatusSummary;

        public int CurrentLevel => currentLevel;
        public Dictionary<CharacterRarity, float> CurrentRarityRates => currentRarityRates;
        public int CurrentCost => currentCost;
        public int CurrentSimultaneousPulls => currentSimultaneousPulls;
        public string SystemStatusSummary => systemStatusSummary;

        public GachaSystemInfo(int currentLevel, Dictionary<CharacterRarity, float> currentRarityRates, 
            int currentCost, int currentSimultaneousPulls, string systemStatusSummary)
        {
            this.currentLevel = currentLevel;
            this.currentRarityRates = currentRarityRates;
            this.currentCost = currentCost;
            this.currentSimultaneousPulls = currentSimultaneousPulls;
            this.systemStatusSummary = systemStatusSummary;
        }

        /// <summary>
        /// 現在のレベルを取得
        /// </summary>
        public int GetCurrentLevel() => currentLevel;

        /// <summary>
        /// 現在のレアリティ排出率を取得
        /// </summary>
        public float GetCurrentRarityRate(CharacterRarity rarity)
        {
            return currentRarityRates.GetValueOrDefault(rarity, 0f);
        }

        /// <summary>
        /// 現在のガチャコストを取得
        /// </summary>
        public int GetCurrentCost() => currentCost;

        /// <summary>
        /// 現在の同時排出数を取得
        /// </summary>
        public int GetCurrentSimultaneousPulls() => currentSimultaneousPulls;

        /// <summary>
        /// システム状態サマリーを取得
        /// </summary>
        public string GetSystemStatusSummary() => systemStatusSummary;
    }
}