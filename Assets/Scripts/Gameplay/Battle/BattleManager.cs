using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Gold;
using GatchaSpire.Gameplay.Board;

namespace GatchaSpire.Gameplay.Battle
{
    /// <summary>
    /// 基本的な戦闘システム管理クラス
    /// リアルタイム戦闘フロー制御、TFT方式移動システム、段階的ダメージ増加対応
    /// </summary>
    [DefaultExecutionOrder(-35)] // GachaSystemManager後、CharacterInventoryManager前
    public class BattleManager : GameSystemBase
    {
        [Header("戦闘設定")]
        [SerializeField] private float fixedTimeStep = 0.1f;
        [SerializeField] private float battleTimeLimit = 60f;
        [SerializeField] private float damageEscalationStartTime = 30f;
        [SerializeField] private float damageEscalationInterval = 10f;
        [SerializeField] private float damageEscalationPercent = 5f;

        [Header("移動・攻撃設定")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private bool enableDebugVisualization = true;

        // システム依存関係
        private BoardStateManager boardManager;
        private CharacterDatabase characterDatabase;
        private GoldManager goldManager;
        private CharacterInventoryManager inventoryManager;

        // 戦闘状態
        private BattleState currentBattleState = BattleState.Idle;
        private float battleTimer = 0f;
        private float accumulator = 0f;
        private float lastDamageEscalationTime = 0f;
        private int damageEscalationLevel = 0;

        // 戦闘データ
        private BattleSetup currentBattleSetup;
        private List<CombatCharacter> playerCombatCharacters = new List<CombatCharacter>();
        private List<CombatCharacter> enemyCombatCharacters = new List<CombatCharacter>();
        private BattleResult pendingBattleResult;

        // イベント
        public event Action<BattleState, BattleState> OnBattleStateChanged;
        public event Action<CombatCharacter, CombatCharacter, int> OnCharacterTakeDamage;
        public event Action<CombatCharacter, Vector2Int, Vector2Int> OnCharacterMove;
        public event Action<CombatCharacter> OnCharacterDefeated;
        public event Action<BattleResult> OnBattleEnded;

        // スキル・シナジーシステム（将来実装）
        private ISkillSystem skillSystem;
        private ISynergySystem synergySystem;

        protected override string SystemName => "BattleManager";

        /// <summary>
        /// Unity Awake - GameSystemBaseの自動登録処理を呼び出し
        /// </summary>
        private void Awake()
        {
            OnAwake();
        }

        // プロパティ
        public BattleState CurrentBattleState => currentBattleState;
        public float BattleTimer => battleTimer;
        public float TimeRemaining => Mathf.Max(0f, battleTimeLimit - battleTimer);
        public bool IsBattleActive => currentBattleState == BattleState.InProgress;
        public List<CombatCharacter> PlayerCombatCharacters => new List<CombatCharacter>(playerCombatCharacters);
        public List<CombatCharacter> EnemyCombatCharacters => new List<CombatCharacter>(enemyCombatCharacters);

        protected override void OnSystemInitialize()
        {
            // 依存システムの取得
            boardManager = FindObjectOfType<BoardStateManager>();
            characterDatabase = CharacterDatabase.Instance;
            goldManager = GoldManager.Instance;
            inventoryManager = FindObjectOfType<CharacterInventoryManager>();

            // 将来実装されるシステムの参照を取得
            skillSystem = GetComponent<ISkillSystem>();
            synergySystem = GetComponent<ISynergySystem>();

            // バリデーション
            if (boardManager == null)
            {
                ReportCritical("BoardStateManagerが見つかりません", null);
                return;
            }

            if (characterDatabase == null)
            {
                ReportCritical("CharacterDatabaseが見つかりません", null);
                return;
            }

            // イベント登録
            if (boardManager != null)
            {
                boardManager.OnCharacterPlaced += OnBoardCharacterPlaced;
                boardManager.OnCharacterRemoved += OnBoardCharacterRemoved;
            }

            ReportInfo("戦闘システムを初期化しました");
        }

        protected override void OnSystemUpdate()
        {
            if (!IsBattleActive)
            {
                return;
            }

            // 固定タイムステップによる戦闘処理
            accumulator += Time.deltaTime;

            while (accumulator >= fixedTimeStep)
            {
                ProcessBattleTick();
                accumulator -= fixedTimeStep;
            }

            // 戦闘時間管理
            battleTimer += Time.deltaTime;
            CheckBattleTimeLimit();
            ProcessDamageEscalation();
        }

        /// <summary>
        /// 戦闘を開始
        /// </summary>
        /// <param name="setup">戦闘セットアップ</param>
        /// <returns>戦闘開始に成功したかどうか</returns>
        public bool StartBattle(BattleSetup setup)
        {
            try
            {
                if (currentBattleState != BattleState.Idle)
                {
                    ReportWarning("既に戦闘中または戦闘準備中です");
                    return false;
                }

                if (setup == null)
                {
                    ReportError("戦闘セットアップがnullです");
                    return false;
                }

                if (enableDebugLogs)
                {
                    ReportInfo($"戦闘開始処理: {setup.BattleName}, 敵数={setup.EnemyCharacters.Count}, IsValid={setup.IsValid()}");
                }

                SetBattleState(BattleState.Preparing);
                currentBattleSetup = setup;

                // 敵キャラクターをボードに配置
                if (!PlaceEnemyCharacters(setup))
                {
                    ReportError("敵キャラクターの配置に失敗しました");
                    SetBattleState(BattleState.Idle);
                    return false;
                }

                // 戦闘キャラクター初期化
                if (!InitializeCombatCharacters())
                {
                    ReportError("戦闘キャラクターの初期化に失敗しました");
                    SetBattleState(BattleState.Idle);
                    return false;
                }

                // 戦闘開始
                battleTimer = 0f;
                accumulator = 0f;
                lastDamageEscalationTime = 0f;
                damageEscalationLevel = 0;

                SetBattleState(BattleState.InProgress);
                ReportInfo("戦闘を開始しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"戦闘開始エラー: {e.Message}", e);
                SetBattleState(BattleState.Idle);
                return false;
            }
        }

        /// <summary>
        /// 戦闘を強制終了
        /// </summary>
        public void ForceEndBattle()
        {
            if (currentBattleState == BattleState.Idle)
            {
                return;
            }

            try
            {
                SetBattleState(BattleState.Ending);
                
                // 引き分け結果を作成
                pendingBattleResult = new BattleResult
                {
                    BattleOutcome = BattleOutcome.Draw,
                    BattleDuration = battleTimer,
                    GoldReward = 0,
                    ExperienceReward = 0,
                    WasForceEnded = true
                };

                FinalizeBattle();
                ReportInfo("戦闘を強制終了しました");
            }
            catch (Exception e)
            {
                ReportError($"戦闘強制終了エラー: {e.Message}", e);
                SetBattleState(BattleState.Idle);
            }
        }

        /// <summary>
        /// 戦闘状態を変更
        /// </summary>
        /// <param name="newState">新しい戦闘状態</param>
        private void SetBattleState(BattleState newState)
        {
            if (currentBattleState == newState)
            {
                return;
            }

            var oldState = currentBattleState;
            currentBattleState = newState;

            OnBattleStateChanged?.Invoke(oldState, newState);

            if (enableDebugLogs)
            {
                ReportInfo($"戦闘状態変更: {oldState} → {newState}");
            }
        }

        /// <summary>
        /// 敵キャラクターをボードに配置
        /// </summary>
        /// <param name="setup">戦闘セットアップ</param>
        /// <returns>配置に成功したかどうか</returns>
        private bool PlaceEnemyCharacters(BattleSetup setup)
        {
            try
            {
                var enemyPlacements = setup.GetEnemyPlacements();
                
                if (enableDebugLogs)
                {
                    ReportInfo($"敵キャラクター配置開始: {enemyPlacements.Count}体");
                }

                foreach (var (character, position) in enemyPlacements)
                {
                    if (character == null)
                    {
                        ReportWarning($"nullの敵キャラクターをスキップします（位置: {position}）");
                        continue;
                    }

                    bool placed = boardManager.PlaceCharacter(character, position, false);
                    if (!placed)
                    {
                        ReportWarning($"敵キャラクター {character.CharacterData.CharacterName} の配置に失敗しました（位置: {position}）");
                        continue;
                    }

                    if (enableDebugLogs)
                    {
                        ReportInfo($"敵キャラクター配置完了: {character.CharacterData.CharacterName} at {position}");
                    }
                }

                if (enableDebugLogs)
                {
                    ReportInfo($"敵キャラクター配置完了: ボード上の敵数 = {boardManager.EnemyCharacters.Count}");
                }

                return boardManager.EnemyCharacters.Count > 0;
            }
            catch (Exception e)
            {
                ReportError($"敵キャラクター配置エラー: {e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// 戦闘キャラクターを初期化
        /// </summary>
        /// <returns>初期化に成功したかどうか</returns>
        private bool InitializeCombatCharacters()
        {
            playerCombatCharacters.Clear();
            enemyCombatCharacters.Clear();

            // プレイヤーキャラクターの戦闘用初期化
            foreach (var character in boardManager.PlayerCharacters)
            {
                var combatCharacter = new CombatCharacter(character, true);
                combatCharacter.BoardPosition = boardManager.GetCharacterPosition(character);
                playerCombatCharacters.Add(combatCharacter);
            }

            // 敵キャラクターの戦闘用初期化
            foreach (var character in boardManager.EnemyCharacters)
            {
                var combatCharacter = new CombatCharacter(character, false);
                combatCharacter.BoardPosition = boardManager.GetCharacterPosition(character);
                enemyCombatCharacters.Add(combatCharacter);
            }

            if (playerCombatCharacters.Count == 0)
            {
                ReportWarning("プレイヤーキャラクターが存在しません");
                return false;
            }

            if (enemyCombatCharacters.Count == 0)
            {
                ReportWarning("敵キャラクターが存在しません");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 戦闘ティック処理（0.1秒間隔）
        /// </summary>
        private void ProcessBattleTick()
        {
            try
            {
                // スキルクールダウン更新
                UpdateSkillCooldowns();

                // キャラクター行動処理
                ProcessCharacterActions();

                // 勝敗判定
                CheckBattleVictoryConditions();
            }
            catch (Exception e)
            {
                ReportError($"戦闘ティック処理エラー: {e.Message}", e);
            }
        }

        /// <summary>
        /// スキルクールダウン更新
        /// </summary>
        private void UpdateSkillCooldowns()
        {
            // スキルシステムが利用可能な場合のみ実行
            skillSystem?.UpdateSkillCooldowns(fixedTimeStep);

            // 基本実装：各キャラクターの基本攻撃クールダウン更新
            foreach (var character in playerCombatCharacters.Concat(enemyCombatCharacters))
            {
                if (character.LastAttackTime > 0f)
                {
                    character.LastAttackTime = Mathf.Max(0f, character.LastAttackTime - fixedTimeStep);
                }
            }
        }

        /// <summary>
        /// キャラクター行動処理
        /// </summary>
        private void ProcessCharacterActions()
        {
            // プレイヤーキャラクターの行動
            foreach (var character in playerCombatCharacters)
            {
                if (character.IsAlive)
                {
                    ProcessCharacterAction(character, enemyCombatCharacters);
                }
            }

            // 敵キャラクターの行動
            foreach (var character in enemyCombatCharacters)
            {
                if (character.IsAlive)
                {
                    ProcessCharacterAction(character, playerCombatCharacters);
                }
            }
        }

        /// <summary>
        /// 個別キャラクターの行動処理
        /// </summary>
        /// <param name="actor">行動するキャラクター</param>
        /// <param name="enemies">敵チーム</param>
        private void ProcessCharacterAction(CombatCharacter actor, List<CombatCharacter> enemies)
        {
            // 最寄りの敵を検索
            var nearestEnemy = FindNearestEnemy(actor, enemies);
            if (nearestEnemy == null)
            {
                return;
            }

            float distance = CalculateDistance(actor.BoardPosition, nearestEnemy.BoardPosition);
            int attackRange = actor.BaseCharacter.CurrentStats.GetFinalStat(StatType.AttackRange);

            // 射程内の場合は攻撃
            if (distance <= attackRange && actor.LastAttackTime <= 0f)
            {
                PerformAttack(actor, nearestEnemy);
            }
            // 射程外の場合は移動
            else if (distance > attackRange)
            {
                MoveTowardsTarget(actor, nearestEnemy);
            }
        }

        /// <summary>
        /// 攻撃処理
        /// </summary>
        /// <param name="attacker">攻撃者</param>
        /// <param name="target">対象</param>
        private void PerformAttack(CombatCharacter attacker, CombatCharacter target)
        {
            try
            {
                int attackPower = attacker.BaseCharacter.CurrentStats.GetFinalStat(StatType.Attack);
                int defense = target.BaseCharacter.CurrentStats.GetFinalStat(StatType.Defense);
                
                // ダメージ計算（エスカレーション込み）
                int baseDamage = Mathf.Max(1, attackPower - defense);
                float escalationMultiplier = 1f + (damageEscalationLevel * damageEscalationPercent / 100f);
                int finalDamage = Mathf.RoundToInt(baseDamage * escalationMultiplier);

                // ダメージ適用
                int actualDamage = target.BaseCharacter.TakeDamage(finalDamage);
                
                // 攻撃クールダウン設定
                float attackSpeed = attacker.BaseCharacter.CurrentStats.GetFinalStat(StatType.AttackSpeed);
                attacker.LastAttackTime = 1f / Mathf.Max(0.1f, attackSpeed * 0.1f); // 攻撃速度をクールダウンに変換

                // イベント発火
                OnCharacterTakeDamage?.Invoke(attacker, target, actualDamage);

                // 撃破判定
                if (!target.IsAlive)
                {
                    OnCharacterDefeated?.Invoke(target);
                    if (enableDebugLogs)
                    {
                        ReportInfo($"{target.BaseCharacter.CharacterData.CharacterName}が撃破されました");
                    }
                }

                if (enableDebugLogs)
                {
                    ReportInfo($"{attacker.BaseCharacter.CharacterData.CharacterName}が{target.BaseCharacter.CharacterData.CharacterName}に{actualDamage}ダメージ");
                }
            }
            catch (Exception e)
            {
                ReportError($"攻撃処理エラー: {e.Message}", e);
            }
        }

        /// <summary>
        /// 対象に向かって移動
        /// </summary>
        /// <param name="mover">移動するキャラクター</param>
        /// <param name="target">目標</param>
        private void MoveTowardsTarget(CombatCharacter mover, CombatCharacter target)
        {
            try
            {
                var currentPos = mover.BoardPosition;
                var targetPos = target.BoardPosition;

                // 最適な移動方向を計算（マンハッタン距離ベース）
                Vector2Int direction = Vector2Int.zero;
                
                if (Mathf.Abs(targetPos.x - currentPos.x) > Mathf.Abs(targetPos.y - currentPos.y))
                {
                    direction.x = targetPos.x > currentPos.x ? 1 : -1;
                }
                else
                {
                    direction.y = targetPos.y > currentPos.y ? 1 : -1;
                }

                var newPosition = currentPos + direction;

                // 移動先の有効性をチェック
                if (boardManager.IsValidPosition(newPosition) && 
                    boardManager.GetCharacterAt(newPosition) == null)
                {
                    // ボード上でキャラクターを移動
                    if (boardManager.MoveCharacter(mover.BaseCharacter, newPosition))
                    {
                        mover.BoardPosition = newPosition;
                        OnCharacterMove?.Invoke(mover, currentPos, newPosition);

                        if (enableDebugLogs)
                        {
                            ReportInfo($"{mover.BaseCharacter.CharacterData.CharacterName}が{currentPos}から{newPosition}に移動");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ReportError($"移動処理エラー: {e.Message}", e);
            }
        }

        /// <summary>
        /// 最寄りの敵を検索
        /// </summary>
        /// <param name="character">基準キャラクター</param>
        /// <param name="enemies">敵リスト</param>
        /// <returns>最寄りの生存している敵</returns>
        private CombatCharacter FindNearestEnemy(CombatCharacter character, List<CombatCharacter> enemies)
        {
            CombatCharacter nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                float distance = CalculateDistance(character.BoardPosition, enemy.BoardPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// マンハッタン距離を計算
        /// </summary>
        /// <param name="pos1">位置1</param>
        /// <param name="pos2">位置2</param>
        /// <returns>距離</returns>
        private float CalculateDistance(Vector2Int pos1, Vector2Int pos2)
        {
            return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
        }

        /// <summary>
        /// 勝敗判定
        /// </summary>
        private void CheckBattleVictoryConditions()
        {
            bool playersAlive = playerCombatCharacters.Any(c => c.IsAlive);
            bool enemiesAlive = enemyCombatCharacters.Any(c => c.IsAlive);

            if (!playersAlive && !enemiesAlive)
            {
                // 引き分け
                EndBattle(BattleOutcome.Draw);
            }
            else if (!enemiesAlive)
            {
                // プレイヤー勝利
                EndBattle(BattleOutcome.Victory);
            }
            else if (!playersAlive)
            {
                // プレイヤー敗北
                EndBattle(BattleOutcome.Defeat);
            }
        }

        /// <summary>
        /// 戦闘時間制限チェック
        /// </summary>
        private void CheckBattleTimeLimit()
        {
            if (battleTimer >= battleTimeLimit)
            {
                ReportInfo("戦闘時間制限に達しました");
                EndBattle(BattleOutcome.Draw);
            }
        }

        /// <summary>
        /// ダメージエスカレーション処理
        /// </summary>
        private void ProcessDamageEscalation()
        {
            if (battleTimer >= damageEscalationStartTime && 
                battleTimer >= lastDamageEscalationTime + damageEscalationInterval)
            {
                damageEscalationLevel++;
                lastDamageEscalationTime = battleTimer;

                if (enableDebugLogs)
                {
                    ReportInfo($"ダメージエスカレーション発動 レベル{damageEscalationLevel} (+{damageEscalationPercent * damageEscalationLevel}%)");
                }
            }
        }

        /// <summary>
        /// 戦闘終了処理
        /// </summary>
        /// <param name="outcome">戦闘結果</param>
        private void EndBattle(BattleOutcome outcome)
        {
            try
            {
                SetBattleState(BattleState.Ending);

                // 戦闘結果を作成
                pendingBattleResult = new BattleResult
                {
                    BattleOutcome = outcome,
                    BattleDuration = battleTimer,
                    GoldReward = CalculateGoldReward(outcome),
                    ExperienceReward = CalculateExperienceReward(outcome),
                    WasForceEnded = false
                };

                FinalizeBattle();
            }
            catch (Exception e)
            {
                ReportError($"戦闘終了処理エラー: {e.Message}", e);
                SetBattleState(BattleState.Idle);
            }
        }

        /// <summary>
        /// 戦闘の最終処理
        /// </summary>
        private void FinalizeBattle()
        {
            try
            {
                // 報酬付与
                if (pendingBattleResult.GoldReward > 0 && goldManager != null)
                {
                    goldManager.AddGold(pendingBattleResult.GoldReward, "戦闘報酬");
                }

                // 経験値付与（生存プレイヤーキャラクターに分配）
                if (pendingBattleResult.ExperienceReward > 0)
                {
                    var alivePlayerCharacters = playerCombatCharacters.Where(c => c.IsAlive).ToList();
                    if (alivePlayerCharacters.Count > 0)
                    {
                        int expPerCharacter = pendingBattleResult.ExperienceReward / alivePlayerCharacters.Count;
                        foreach (var character in alivePlayerCharacters)
                        {
                            character.BaseCharacter.AddExperience(expPerCharacter);
                        }
                    }
                }

                // イベント発火
                OnBattleEnded?.Invoke(pendingBattleResult);

                // 状態リセット
                SetBattleState(BattleState.Idle);
                currentBattleSetup = null;
                playerCombatCharacters.Clear();
                enemyCombatCharacters.Clear();

                ReportInfo($"戦闘終了: {pendingBattleResult.BattleOutcome} (時間: {pendingBattleResult.BattleDuration:F1}秒)");
            }
            catch (Exception e)
            {
                ReportError($"戦闘最終処理エラー: {e.Message}", e);
                SetBattleState(BattleState.Idle);
            }
        }

        /// <summary>
        /// ゴールド報酬を計算
        /// </summary>
        /// <param name="outcome">戦闘結果</param>
        /// <returns>ゴールド報酬</returns>
        private int CalculateGoldReward(BattleOutcome outcome)
        {
            return outcome switch
            {
                BattleOutcome.Victory => 100,
                BattleOutcome.Draw => 25,
                BattleOutcome.Defeat => 10,
                _ => 0
            };
        }

        /// <summary>
        /// 経験値報酬を計算
        /// </summary>
        /// <param name="outcome">戦闘結果</param>
        /// <returns>経験値報酬</returns>
        private int CalculateExperienceReward(BattleOutcome outcome)
        {
            return outcome switch
            {
                BattleOutcome.Victory => 50,
                BattleOutcome.Draw => 15,
                BattleOutcome.Defeat => 5,
                _ => 0
            };
        }

        /// <summary>
        /// ボード上にキャラクターが配置された時の処理
        /// </summary>
        /// <param name="character">配置されたキャラクター</param>
        /// <param name="position">配置位置</param>
        private void OnBoardCharacterPlaced(Character character, Vector2Int position)
        {
            // 戦闘中の場合は動的に戦闘キャラクターを追加
            if (IsBattleActive)
            {
                bool isPlayerCharacter = boardManager.PlayerCharacters.Contains(character);
                var combatCharacter = new CombatCharacter(character, isPlayerCharacter);
                combatCharacter.BoardPosition = position;

                if (isPlayerCharacter)
                {
                    playerCombatCharacters.Add(combatCharacter);
                }
                else
                {
                    enemyCombatCharacters.Add(combatCharacter);
                }
            }
        }

        /// <summary>
        /// ボード上からキャラクターが除去された時の処理
        /// </summary>
        /// <param name="character">除去されたキャラクター</param>
        /// <param name="position">除去位置</param>
        private void OnBoardCharacterRemoved(Character character, Vector2Int position)
        {
            // 戦闘中の場合は動的に戦闘キャラクターを除去
            if (IsBattleActive)
            {
                playerCombatCharacters.RemoveAll(c => c.BaseCharacter == character);
                enemyCombatCharacters.RemoveAll(c => c.BaseCharacter == character);
            }
        }

        /// <summary>
        /// デバッグ用戦闘情報表示
        /// </summary>
        public void DebugPrintBattleInfo()
        {
            if (!enableDebugLogs)
            {
                return;
            }

            string info = "=== 戦闘情報 ===\n";
            info += $"状態: {currentBattleState}\n";
            info += $"経過時間: {battleTimer:F1}秒 / {battleTimeLimit}秒\n";
            info += $"ダメージエスカレーション: レベル{damageEscalationLevel}\n";
            info += $"プレイヤー生存: {playerCombatCharacters.Count(c => c.IsAlive)}/{playerCombatCharacters.Count}\n";
            info += $"敵生存: {enemyCombatCharacters.Count(c => c.IsAlive)}/{enemyCombatCharacters.Count}";

            Debug.Log(info);
        }

        protected override void OnSystemShutdown()
        {
            // イベント登録解除
            if (boardManager != null)
            {
                boardManager.OnCharacterPlaced -= OnBoardCharacterPlaced;
                boardManager.OnCharacterRemoved -= OnBoardCharacterRemoved;
            }
        }

        protected override void OnSystemReset()
        {
            ForceEndBattle();
        }
    }
}