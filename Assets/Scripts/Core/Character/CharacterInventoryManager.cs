using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Error;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Gold;
using GatchaSpire.Core.Character.Operations;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターインベントリの統合管理クラス
    /// 売却、合成、経験値化などの操作を統一的に管理
    /// </summary>
    [DefaultExecutionOrder(-10)] // 他のマネージャーが登録された後に実行
    public class CharacterInventoryManager : GameSystemBase, IUnityGameSystem
    {
        [Header("システム設定")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private int maxConcurrentOperations = 5;

        [Header("インベントリ設定")]
        [SerializeField] private int maxCharacterSlots = 1000;
        [SerializeField] private bool allowDuplicateCharacters = true;
        [SerializeField] private bool autoSortInventory = true;

        [Header("操作設定")]
        [SerializeField] private float operationTimeoutSeconds = 30f;
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private bool validateOperationsBeforeExecution = true;

        // システム参照
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private DevelopmentSettings developmentSettings;

        // インベントリデータ
        private Dictionary<string, Character> ownedCharacters = new Dictionary<string, Character>();
        private HashSet<string> lockedCharacters = new HashSet<string>();
        private HashSet<string> favoriteCharacters = new HashSet<string>();
        
        // 操作管理
        private Queue<CharacterOperation> pendingOperations = new Queue<CharacterOperation>();
        private Dictionary<string, CharacterOperation> activeOperations = new Dictionary<string, CharacterOperation>();
        private object operationLock = new object();

        // インスタンス管理
        private static CharacterInventoryManager instance;
        public static CharacterInventoryManager Instance => instance;

        // IUnityGameSystem 実装
        public bool RequiresUpdate => pendingOperations.Count > 0 || activeOperations.Count > 0;
        public int ExecutionOrder => 100;
        public bool PersistAcrossScenes => persistAcrossScenes;
        public bool RequiresMainThread => true;

        // プロパティ
        public int OwnedCharacterCount => ownedCharacters.Count;
        public int AvailableSlots => maxCharacterSlots - ownedCharacters.Count;
        public int LockedCharacterCount => lockedCharacters.Count;
        public int FavoriteCharacterCount => favoriteCharacters.Count;
        public bool IsInventoryFull => ownedCharacters.Count >= maxCharacterSlots;
        public List<Character> AllCharacters => ownedCharacters.Values.ToList();

        private void Awake()
        {
            base.OnAwake();
        }

        #region Unity Lifecycle

        public void OnAwake()
        {
            if (instance == null)
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeCollections();
        }

        public void OnStart()
        {
            InitializeDependencies();
            LoadInventoryData();
        }

        public void OnDestroy()
        {
            if (instance == this)
            {
                SaveInventoryData();
                instance = null;
            }
        }

        #endregion

        #region GameSystemBase 実装

        protected override void OnSystemInitialize()
        {
            try
            {
                InitializeDependencies();
                InitializeCollections();
                LoadInventoryData();
                
                LogDebug("CharacterInventoryManager が正常に初期化されました");
            }
            catch (Exception e)
            {
                ReportError($"初期化中にエラーが発生しました: {e.Message}");
            }
        }

        public void ResetSystem()
        {
            try
            {
                lock (operationLock)
                {
                    // 進行中の操作をクリア
                    pendingOperations.Clear();
                    activeOperations.Clear();
                    
                    // インベントリデータをクリア
                    ownedCharacters.Clear();
                    lockedCharacters.Clear();
                    favoriteCharacters.Clear();
                }
                
                LogDebug("CharacterInventoryManager がリセットされました");
            }
            catch (Exception e)
            {
                ReportError($"リセット中にエラーが発生しました: {e.Message}");
            }
        }

        public void Update()
        {
            if (!IsInitialized())
            {
                return;
            }

            ProcessPendingOperations();
            CleanupExpiredOperations();
        }

        public void Shutdown()
        {
            try
            {
                SaveInventoryData();
                LogDebug("CharacterInventoryManager がシャットダウンしました");
            }
            catch (Exception e)
            {
                ReportError($"シャットダウン中にエラーが発生しました: {e.Message}");
            }
        }

        protected override string SystemName => "CharacterInventoryManager";

        public override bool IsInitialized() => 
            goldManager != null && 
            characterDatabase != null;

        public SystemPriority Priority => SystemPriority.Medium;

        public SystemPriority GetInitializationPriority() => SystemPriority.Medium;

        public List<string> GetDependencies() => new List<string>
        {
            "GoldManager", 
            "CharacterDatabase"
        };

        #endregion

        #region キャラクター基本操作

        /// <summary>
        /// キャラクターを追加
        /// </summary>
        /// <param name="character">追加するキャラクター</param>
        /// <returns>成功した場合true</returns>
        public bool AddCharacter(Character character)
        {
            if (character == null)
            {
                ReportWarning("追加しようとしたキャラクターがnullです");
                return false;
            }

            if (character.CharacterData == null)
            {
                ReportWarning("キャラクターのCharacterDataがnullです");
                return false;
            }

            if (IsInventoryFull)
            {
                ReportWarning("インベントリが満杯のため、キャラクターを追加できません");
                return false;
            }

            lock (operationLock)
            {
                if (ownedCharacters.ContainsKey(character.InstanceId))
                {
                    ReportWarning($"キャラクター {character.InstanceId} は既に存在します");
                    return false;
                }

                ownedCharacters[character.InstanceId] = character;
            }

            LogDebug($"キャラクター {character.CharacterData.CharacterName} (ID: {character.InstanceId}) を追加しました");
            return true;
        }

        /// <summary>
        /// キャラクターを取得
        /// </summary>
        /// <param name="instanceId">インスタンスID</param>
        /// <returns>キャラクター（存在しない場合null）</returns>
        public Character GetCharacter(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return null;
            }

            lock (operationLock)
            {
                return ownedCharacters.TryGetValue(instanceId, out var character) ? character : null;
            }
        }

        /// <summary>
        /// キャラクターが存在するかチェック
        /// </summary>
        /// <param name="instanceId">インスタンスID</param>
        /// <returns>存在する場合true</returns>
        public bool HasCharacter(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }

            lock (operationLock)
            {
                return ownedCharacters.ContainsKey(instanceId);
            }
        }

        /// <summary>
        /// キャラクターがロックされているかチェック
        /// </summary>
        /// <param name="instanceId">インスタンスID</param>
        /// <returns>ロックされている場合true</returns>
        public bool IsCharacterLocked(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }

            lock (operationLock)
            {
                return lockedCharacters.Contains(instanceId);
            }
        }

        /// <summary>
        /// キャラクターがお気に入りかチェック
        /// </summary>
        /// <param name="instanceId">インスタンスID</param>
        /// <returns>お気に入りの場合true</returns>
        public bool IsCharacterFavorite(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }

            lock (operationLock)
            {
                return favoriteCharacters.Contains(instanceId);
            }
        }

        #endregion

        #region 売却操作

        /// <summary>
        /// キャラクターを売却
        /// </summary>
        /// <param name="characterIds">売却するキャラクターIDリスト</param>
        /// <param name="totalGoldEarned">獲得ゴールド総額</param>
        /// <param name="allowLocked">ロック済みキャラクターの売却を許可</param>
        /// <param name="allowFavorites">お気に入りキャラクターの売却を許可</param>
        /// <returns>成功した場合true</returns>
        public bool SellCharacters(List<string> characterIds, out int totalGoldEarned, bool allowLocked = false, bool allowFavorites = false)
        {
            totalGoldEarned = 0;

            if (characterIds == null || !characterIds.Any())
            {
                ReportWarning("売却対象のキャラクターが指定されていません");
                return false;
            }

            try
            {
                var validCharacters = new List<Character>();
                var totalPrice = 0;

                // 売却前検証
                foreach (var characterId in characterIds)
                {
                    if (!ValidateCharacterForSale(characterId, allowLocked, allowFavorites, out var character))
                    {
                        continue;
                    }

                    validCharacters.Add(character);
                    totalPrice += character.CharacterData.SellPrice;
                }

                if (!validCharacters.Any())
                {
                    ReportWarning("売却可能なキャラクターがありません");
                    return false;
                }

                // ゴールド追加
                if (goldManager.AddGold(totalPrice) == 0)
                {
                    ReportError("ゴールドの追加に失敗しました");
                    return false;
                }

                // キャラクター削除
                lock (operationLock)
                {
                    foreach (var character in validCharacters)
                    {
                        ownedCharacters.Remove(character.InstanceId);
                        lockedCharacters.Remove(character.InstanceId);
                        favoriteCharacters.Remove(character.InstanceId);
                    }
                }

                totalGoldEarned = totalPrice;
                LogDebug($"{validCharacters.Count}体のキャラクターを{totalPrice}ゴールドで売却しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター売却中にエラーが発生しました: {e.Message}");
                return false;
            }
        }

        #endregion

        #region 合成操作

        /// <summary>
        /// キャラクターを合成
        /// </summary>
        /// <param name="baseCharacterId">ベースキャラクターID</param>
        /// <param name="materialCharacterIds">素材キャラクターIDリスト</param>
        /// <param name="resultCharacter">合成後のキャラクター</param>
        /// <param name="preserveLevel">ベースレベルを保持するか</param>
        /// <returns>成功した場合true</returns>
        public bool FuseCharacters(string baseCharacterId, List<string> materialCharacterIds, out Character resultCharacter, bool preserveLevel = true)
        {
            resultCharacter = null;

            if (string.IsNullOrEmpty(baseCharacterId) || materialCharacterIds == null || !materialCharacterIds.Any())
            {
                ReportWarning("合成に必要なキャラクターが指定されていません");
                return false;
            }

            try
            {
                // ベースキャラクター取得
                var baseCharacter = GetCharacter(baseCharacterId);
                if (baseCharacter == null)
                {
                    ReportWarning($"ベースキャラクター {baseCharacterId} が見つかりません");
                    return false;
                }

                // 素材キャラクター検証
                var materialCharacters = new List<Character>();
                foreach (var materialId in materialCharacterIds)
                {
                    if (!ValidateCharacterForOperation(materialId, out var material))
                    {
                        continue;
                    }

                    if (materialId == baseCharacterId)
                    {
                        ReportWarning("ベースキャラクターを素材として使用することはできません");
                        continue;
                    }

                    materialCharacters.Add(material);
                }

                if (!materialCharacters.Any())
                {
                    ReportWarning("合成可能な素材キャラクターがありません");
                    return false;
                }

                // 経験値計算
                var totalExp = 0;
                foreach (var material in materialCharacters)
                {
                    totalExp += material.CharacterData.SellPrice * 2; // 売却価格の2倍を経験値として換算
                }

                // ベースキャラクターに経験値追加
                var levelUps = baseCharacter.AddExperience(totalExp);

                // 素材キャラクター削除
                lock (operationLock)
                {
                    foreach (var material in materialCharacters)
                    {
                        ownedCharacters.Remove(material.InstanceId);
                        lockedCharacters.Remove(material.InstanceId);
                        favoriteCharacters.Remove(material.InstanceId);
                    }
                }

                resultCharacter = baseCharacter;
                LogDebug($"合成完了: {materialCharacters.Count}体の素材で{totalExp}経験値獲得、{levelUps}レベルアップ");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター合成中にエラーが発生しました: {e.Message}");
                return false;
            }
        }

        #endregion

        #region 経験値化操作

        /// <summary>
        /// キャラクターを経験値に変換
        /// </summary>
        /// <param name="targetCharacterIds">変換対象キャラクターIDリスト</param>
        /// <param name="receivingCharacterId">経験値受取キャラクターID</param>
        /// <param name="totalExpGained">獲得経験値総額</param>
        /// <param name="conversionRate">変換率</param>
        /// <returns>成功した場合true</returns>
        public bool ConvertCharactersToExp(List<string> targetCharacterIds, string receivingCharacterId, out int totalExpGained, float conversionRate = 1.0f)
        {
            totalExpGained = 0;

            if (targetCharacterIds == null || !targetCharacterIds.Any() || string.IsNullOrEmpty(receivingCharacterId))
            {
                ReportWarning("経験値化に必要なキャラクターが指定されていません");
                return false;
            }

            try
            {
                // 受取キャラクター取得
                var receivingCharacter = GetCharacter(receivingCharacterId);
                if (receivingCharacter == null)
                {
                    ReportWarning($"経験値受取キャラクター {receivingCharacterId} が見つかりません");
                    return false;
                }

                // 変換対象キャラクター検証
                var targetCharacters = new List<Character>();
                foreach (var targetId in targetCharacterIds)
                {
                    if (targetId == receivingCharacterId)
                    {
                        ReportWarning("経験値受取キャラクターを変換対象にすることはできません");
                        continue;
                    }

                    if (!ValidateCharacterForOperation(targetId, out var target))
                    {
                        continue;
                    }

                    targetCharacters.Add(target);
                }

                if (!targetCharacters.Any())
                {
                    ReportWarning("経験値化可能なキャラクターがありません");
                    return false;
                }

                // 経験値計算
                var totalExp = 0;
                foreach (var target in targetCharacters)
                {
                    var baseExp = target.CharacterData.SellPrice * 2; // 基本経験値
                    var levelBonus = target.CurrentLevel * 10; // レベルボーナス
                    var rarityBonus = (int)target.CharacterData.Rarity * 50; // レアリティボーナス
                    
                    totalExp += Mathf.RoundToInt((baseExp + levelBonus + rarityBonus) * conversionRate);
                }

                // 経験値追加
                var levelUps = receivingCharacter.AddExperience(totalExp);

                // 変換対象キャラクター削除
                lock (operationLock)
                {
                    foreach (var target in targetCharacters)
                    {
                        ownedCharacters.Remove(target.InstanceId);
                        lockedCharacters.Remove(target.InstanceId);
                        favoriteCharacters.Remove(target.InstanceId);
                    }
                }

                totalExpGained = totalExp;
                LogDebug($"経験値化完了: {targetCharacters.Count}体を変換して{totalExp}経験値獲得、{levelUps}レベルアップ");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター経験値化中にエラーが発生しました: {e.Message}");
                return false;
            }
        }

        #endregion

        #region ロック・お気に入り操作

        /// <summary>
        /// キャラクターのロック状態を変更
        /// </summary>
        /// <param name="characterIds">対象キャラクターIDリスト</param>
        /// <param name="lockState">ロック状態（true:ロック、false:アンロック）</param>
        /// <returns>成功した場合true</returns>
        public bool SetCharacterLockState(List<string> characterIds, bool lockState)
        {
            if (characterIds == null || !characterIds.Any())
            {
                ReportWarning("ロック状態変更対象のキャラクターが指定されていません");
                return false;
            }

            try
            {
                var validCharacters = new List<string>();

                foreach (var characterId in characterIds)
                {
                    if (!HasCharacter(characterId))
                    {
                        ReportWarning($"キャラクター {characterId} が見つかりません");
                        continue;
                    }

                    validCharacters.Add(characterId);
                }

                if (!validCharacters.Any())
                {
                    ReportWarning("ロック状態変更可能なキャラクターがありません");
                    return false;
                }

                lock (operationLock)
                {
                    foreach (var characterId in validCharacters)
                    {
                        if (lockState)
                        {
                            lockedCharacters.Add(characterId);
                        }
                        else
                        {
                            lockedCharacters.Remove(characterId);
                        }
                    }
                }

                var action = lockState ? "ロック" : "アンロック";
                LogDebug($"{validCharacters.Count}体のキャラクターを{action}しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"ロック状態変更中にエラーが発生しました: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// キャラクターのお気に入り状態を変更
        /// </summary>
        /// <param name="characterIds">対象キャラクターIDリスト</param>
        /// <param name="favoriteState">お気に入り状態（true:お気に入り、false:解除）</param>
        /// <returns>成功した場合true</returns>
        public bool SetCharacterFavoriteState(List<string> characterIds, bool favoriteState)
        {
            if (characterIds == null || !characterIds.Any())
            {
                ReportWarning("お気に入り状態変更対象のキャラクターが指定されていません");
                return false;
            }

            try
            {
                var validCharacters = new List<string>();

                foreach (var characterId in characterIds)
                {
                    if (!HasCharacter(characterId))
                    {
                        ReportWarning($"キャラクター {characterId} が見つかりません");
                        continue;
                    }

                    validCharacters.Add(characterId);
                }

                if (!validCharacters.Any())
                {
                    ReportWarning("お気に入り状態変更可能なキャラクターがありません");
                    return false;
                }

                lock (operationLock)
                {
                    foreach (var characterId in validCharacters)
                    {
                        if (favoriteState)
                        {
                            favoriteCharacters.Add(characterId);
                        }
                        else
                        {
                            favoriteCharacters.Remove(characterId);
                        }
                    }
                }

                var action = favoriteState ? "お気に入りに追加" : "お気に入りから削除";
                LogDebug($"{validCharacters.Count}体のキャラクターを{action}しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"お気に入り状態変更中にエラーが発生しました: {e.Message}");
                return false;
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 依存関係を初期化
        /// </summary>
        private void InitializeDependencies()
        {
            var coordinator = UnityGameSystemCoordinator.Instance;
            
            goldManager = coordinator?.GetSystem<GoldManager>("GoldManager");
            characterDatabase = coordinator?.GetSystem<CharacterDatabase>("CharacterDatabase");
            developmentSettings = Resources.Load<DevelopmentSettings>("Settings/DevelopmentSettings");

            if (goldManager == null)
            {
                ReportError("GoldManager が見つかりません");
            }

            if (characterDatabase == null)
            {
                ReportError("CharacterDatabase が見つかりません");
            }
        }

        /// <summary>
        /// コレクションを初期化
        /// </summary>
        private void InitializeCollections()
        {
            ownedCharacters = new Dictionary<string, Character>();
            lockedCharacters = new HashSet<string>();
            favoriteCharacters = new HashSet<string>();
            pendingOperations = new Queue<CharacterOperation>();
            activeOperations = new Dictionary<string, CharacterOperation>();
        }

        /// <summary>
        /// 操作用キャラクター検証（汎用）
        /// </summary>
        private bool ValidateCharacterForOperation(string characterId, out Character character)
        {
            character = null;

            if (string.IsNullOrEmpty(characterId))
            {
                return false;
            }

            if (!HasCharacter(characterId))
            {
                ReportWarning($"キャラクター {characterId} が見つかりません");
                return false;
            }

            character = GetCharacter(characterId);
            return character != null;
        }

        /// <summary>
        /// 売却用キャラクター検証
        /// </summary>
        private bool ValidateCharacterForSale(string characterId, bool allowLocked, bool allowFavorites, out Character character)
        {
            character = null;

            if (string.IsNullOrEmpty(characterId))
            {
                return false;
            }

            if (!HasCharacter(characterId))
            {
                ReportWarning($"キャラクター {characterId} が見つかりません");
                return false;
            }

            if (!allowLocked && IsCharacterLocked(characterId))
            {
                ReportWarning($"キャラクター {characterId} はロックされています");
                return false;
            }

            if (!allowFavorites && IsCharacterFavorite(characterId))
            {
                ReportWarning($"キャラクター {characterId} はお気に入りです");
                return false;
            }

            character = GetCharacter(characterId);
            return character != null;
        }

        /// <summary>
        /// インベントリデータを読み込み
        /// </summary>
        private void LoadInventoryData()
        {
            // TODO: セーブデータからの読み込み実装
            LogDebug("インベントリデータを読み込みました");
        }

        /// <summary>
        /// インベントリデータを保存
        /// </summary>
        private void SaveInventoryData()
        {
            // TODO: セーブデータへの保存実装
            LogDebug("インベントリデータを保存しました");
        }

        /// <summary>
        /// 保留中の操作を処理
        /// </summary>
        private void ProcessPendingOperations()
        {
            // TODO: 操作キューの処理実装
        }

        /// <summary>
        /// 期限切れ操作をクリーンアップ
        /// </summary>
        private void CleanupExpiredOperations()
        {
            // TODO: タイムアウト操作の処理実装
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            if (!enableDebugLogs)
            {
                return;
            }

            if (developmentSettings != null && developmentSettings.EnableAllDebugLogs)
            {
                ReportInfo(message);
            }
        }

        #endregion
    }
}