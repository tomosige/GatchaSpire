using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Battle
{
    /// <summary>
    /// 戦闘状態の列挙型
    /// </summary>
    public enum BattleState
    {
        Idle,        // 待機中
        Preparing,   // 戦闘準備中
        InProgress,  // 戦闘進行中
        Ending       // 戦闘終了処理中
    }

    /// <summary>
    /// 戦闘結果の列挙型
    /// </summary>
    public enum BattleOutcome
    {
        Victory,     // 勝利
        Defeat,      // 敗北
        Draw         // 引き分け
    }

    /// <summary>
    /// 戦闘セットアップクラス
    /// </summary>
    [System.Serializable]
    public class BattleSetup
    {
        [Header("戦闘設定")]
        public string BattleName = "";
        public float TimeLimit = 60f;
        public bool EnableDamageEscalation = true;
        public float DamageEscalationStartTime = 30f;

        [Header("敵構成")]
        public List<Character> EnemyCharacters = new List<Character>();
        public List<Vector2Int> EnemyPositions = new List<Vector2Int>();

        [Header("報酬設定")]
        public int BaseGoldReward = 100;
        public int BaseExperienceReward = 50;
        public float RewardMultiplier = 1f;

        /// <summary>
        /// セットアップの妥当性を確認
        /// </summary>
        /// <returns>妥当性</returns>
        public bool IsValid()
        {
            if (EnemyCharacters.Count == 0)
            {
                return false;
            }

            if (EnemyPositions.Count != EnemyCharacters.Count)
            {
                return false;
            }

            if (TimeLimit <= 0f)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 敵キャラクターと位置のペアを取得
        /// </summary>
        /// <returns>キャラクターと位置のペア</returns>
        public List<(Character character, Vector2Int position)> GetEnemyPlacements()
        {
            var placements = new List<(Character, Vector2Int)>();
            
            for (int i = 0; i < EnemyCharacters.Count && i < EnemyPositions.Count; i++)
            {
                placements.Add((EnemyCharacters[i], EnemyPositions[i]));
            }

            return placements;
        }
    }

    /// <summary>
    /// 戦闘結果クラス
    /// </summary>
    [System.Serializable]
    public class BattleResult
    {
        [Header("戦闘結果")]
        public BattleOutcome BattleOutcome = BattleOutcome.Draw;
        public float BattleDuration = 0f;
        public bool WasForceEnded = false;

        [Header("報酬")]
        public int GoldReward = 0;
        public int ExperienceReward = 0;

        [Header("統計")]
        public int TotalDamageDealt = 0;
        public int TotalDamageReceived = 0;
        public int EnemiesDefeated = 0;
        public int PlayersLost = 0;

        /// <summary>
        /// 戦闘結果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報</returns>
        public string GetDetailedInfo()
        {
            var info = $"=== 戦闘結果 ===\n";
            info += $"結果: {GetOutcomeText()}\n";
            info += $"戦闘時間: {BattleDuration:F1}秒\n";
            info += $"ゴールド報酬: {GoldReward}\n";
            info += $"経験値報酬: {ExperienceReward}\n";
            info += $"与ダメージ: {TotalDamageDealt}\n";
            info += $"被ダメージ: {TotalDamageReceived}\n";
            info += $"撃破数: {EnemiesDefeated}\n";
            info += $"損失数: {PlayersLost}";

            if (WasForceEnded)
            {
                info += "\n※ 強制終了されました";
            }

            return info;
        }

        /// <summary>
        /// 戦闘結果のテキスト表現を取得
        /// </summary>
        /// <returns>結果テキスト</returns>
        private string GetOutcomeText()
        {
            return BattleOutcome switch
            {
                BattleOutcome.Victory => "勝利",
                BattleOutcome.Defeat => "敗北",
                BattleOutcome.Draw => "引き分け",
                _ => "不明"
            };
        }
    }

    /// <summary>
    /// 戦闘統計クラス
    /// </summary>
    [System.Serializable]
    public class BattleStatistics
    {
        [Header("戦闘回数")]
        public int TotalBattles = 0;
        public int Victories = 0;
        public int Defeats = 0;
        public int Draws = 0;

        [Header("時間統計")]
        public float TotalBattleTime = 0f;
        public float AverageBattleTime = 0f;
        public float ShortestBattle = float.MaxValue;
        public float LongestBattle = 0f;

        [Header("ダメージ統計")]
        public int TotalDamageDealt = 0;
        public int TotalDamageReceived = 0;
        public int TotalEnemiesDefeated = 0;
        public int TotalPlayersLost = 0;

        [Header("報酬統計")]
        public int TotalGoldEarned = 0;
        public int TotalExperienceEarned = 0;

        /// <summary>
        /// 戦闘結果を統計に追加
        /// </summary>
        /// <param name="result">戦闘結果</param>
        public void AddBattleResult(BattleResult result)
        {
            TotalBattles++;

            switch (result.BattleOutcome)
            {
                case BattleOutcome.Victory:
                    Victories++;
                    break;
                case BattleOutcome.Defeat:
                    Defeats++;
                    break;
                case BattleOutcome.Draw:
                    Draws++;
                    break;
            }

            // 時間統計更新
            TotalBattleTime += result.BattleDuration;
            AverageBattleTime = TotalBattleTime / TotalBattles;
            ShortestBattle = Mathf.Min(ShortestBattle, result.BattleDuration);
            LongestBattle = Mathf.Max(LongestBattle, result.BattleDuration);

            // ダメージ統計更新
            TotalDamageDealt += result.TotalDamageDealt;
            TotalDamageReceived += result.TotalDamageReceived;
            TotalEnemiesDefeated += result.EnemiesDefeated;
            TotalPlayersLost += result.PlayersLost;

            // 報酬統計更新
            TotalGoldEarned += result.GoldReward;
            TotalExperienceEarned += result.ExperienceReward;
        }

        /// <summary>
        /// 勝率を取得
        /// </summary>
        /// <returns>勝率（0-1）</returns>
        public float GetWinRate()
        {
            return TotalBattles > 0 ? (float)Victories / TotalBattles : 0f;
        }

        /// <summary>
        /// 統計情報の詳細テキストを取得
        /// </summary>
        /// <returns>統計情報</returns>
        public string GetDetailedInfo()
        {
            var info = $"=== 戦闘統計 ===\n";
            info += $"総戦闘数: {TotalBattles}\n";
            info += $"勝利: {Victories} ({GetWinRate():P1})\n";
            info += $"敗北: {Defeats}\n";
            info += $"引き分け: {Draws}\n";
            info += $"平均戦闘時間: {AverageBattleTime:F1}秒\n";
            info += $"最短戦闘: {(ShortestBattle < float.MaxValue ? ShortestBattle.ToString("F1") : "N/A")}秒\n";
            info += $"最長戦闘: {LongestBattle:F1}秒\n";
            info += $"総与ダメージ: {TotalDamageDealt}\n";
            info += $"総被ダメージ: {TotalDamageReceived}\n";
            info += $"総獲得ゴールド: {TotalGoldEarned}\n";
            info += $"総獲得経験値: {TotalExperienceEarned}";

            return info;
        }
    }

    /// <summary>
    /// 戦闘用キャラクタークラス
    /// </summary>
    [System.Serializable]
    public class CombatCharacter
    {
        [Header("基本情報")]
        public Character BaseCharacter;
        public bool IsPlayerCharacter;

        [Header("戦闘状態")]
        public Vector2Int BoardPosition = Vector2Int.zero;
        public float LastAttackTime = 0f;
        public Dictionary<int, float> SkillCooldowns = new Dictionary<int, float>();

        // プロパティ
        public bool IsAlive => BaseCharacter.IsAlive;
        public string Name => BaseCharacter.CharacterData.CharacterName;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseCharacter">ベースキャラクター</param>
        /// <param name="isPlayerCharacter">プレイヤーキャラクターかどうか</param>
        public CombatCharacter(Character baseCharacter, bool isPlayerCharacter)
        {
            BaseCharacter = baseCharacter;
            IsPlayerCharacter = isPlayerCharacter;
            SkillCooldowns = new Dictionary<int, float>();
        }

        /// <summary>
        /// スキルクールダウンを取得
        /// </summary>
        /// <param name="skillId">スキルID</param>
        /// <returns>残りクールダウン時間</returns>
        public float GetSkillCooldown(int skillId)
        {
            return SkillCooldowns.TryGetValue(skillId, out float cooldown) ? cooldown : 0f;
        }

        /// <summary>
        /// スキルクールダウンを設定
        /// </summary>
        /// <param name="skillId">スキルID</param>
        /// <param name="cooldownTime">クールダウン時間</param>
        public void SetSkillCooldown(int skillId, float cooldownTime)
        {
            SkillCooldowns[skillId] = cooldownTime;
        }

        /// <summary>
        /// スキルが使用可能かチェック
        /// </summary>
        /// <param name="skillId">スキルID</param>
        /// <returns>使用可能かどうか</returns>
        public bool CanUseSkill(int skillId)
        {
            return GetSkillCooldown(skillId) <= 0f;
        }

        /// <summary>
        /// 戦闘用詳細情報を取得
        /// </summary>
        /// <returns>詳細情報</returns>
        public string GetCombatInfo()
        {
            var info = $"=== {Name} ===\n";
            info += $"チーム: {(IsPlayerCharacter ? "プレイヤー" : "敵")}\n";
            info += $"位置: {BoardPosition}\n";
            info += $"HP: {BaseCharacter.CurrentHP}/{BaseCharacter.MaxHP}\n";
            info += $"MP: {BaseCharacter.CurrentMP}/{BaseCharacter.MaxMP}\n";
            info += $"攻撃クールダウン: {LastAttackTime:F1}秒\n";
            info += $"生存状態: {(IsAlive ? "生存" : "撃破")}";

            if (SkillCooldowns.Count > 0)
            {
                info += "\n=== スキルクールダウン ===\n";
                foreach (var cooldown in SkillCooldowns)
                {
                    info += $"スキル{cooldown.Key}: {cooldown.Value:F1}秒\n";
                }
            }

            return info;
        }

        /// <summary>
        /// 簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"{Name}({(IsPlayerCharacter ? "P" : "E")}) HP:{BaseCharacter.CurrentHP}/{BaseCharacter.MaxHP} @{BoardPosition}";
        }
    }
}