using UnityEngine;
using System;
using System.Collections.Generic;
using GatchaSpire.Core.Error;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターのランタイムインスタンス
    /// CharacterDataからの初期化、レベルアップ、ステータス計算を管理
    /// </summary>
    [Serializable]
    public class Character
    {
        [Header("基本情報")]
        [SerializeField] private string instanceId;
        [SerializeField] private CharacterData characterData;
        [SerializeField] private int currentLevel;
        [SerializeField] private int currentExp;
        [SerializeField] private DateTime acquiredDate;

        [Header("現在のステータス")]
        [SerializeField] private CharacterStats currentStats;
        [SerializeField] private int currentHP;
        [SerializeField] private int currentMP;

        [Header("強化状態")]
        [SerializeField] private Dictionary<StatType, int> permanentBoosts = new Dictionary<StatType, int>();
        [SerializeField] private Dictionary<StatType, int> temporaryBoosts = new Dictionary<StatType, int>();
        [SerializeField] private List<string> unlockedAbilities = new List<string>();

        [Header("スキルシステム")]
        [SerializeField] private CharacterSkillProgression skillProgression;
        [SerializeField] private CharacterSkillCooldowns skillCooldowns;

        // プロパティ
        public string InstanceId => instanceId;
        public CharacterData CharacterData => characterData;
        public int CurrentLevel => currentLevel;
        public int CurrentExp => currentExp;
        public DateTime AcquiredDate => acquiredDate;
        public CharacterStats CurrentStats => currentStats;
        public int CurrentHP => currentHP;
        public int CurrentMP => currentMP;
        public int MaxHP => currentStats.GetFinalStat(StatType.HP);
        public int MaxMP => currentStats.GetFinalStat(StatType.MP);
        public Dictionary<StatType, int> PermanentBoosts => new Dictionary<StatType, int>(permanentBoosts);
        public List<string> UnlockedAbilities => new List<string>(unlockedAbilities);

        // スキルシステムプロパティ
        public CharacterSkillProgression SkillProgression => skillProgression;
        public CharacterSkillCooldowns SkillCooldowns => skillCooldowns;

        // 計算プロパティ
        public bool IsMaxLevel => currentLevel >= characterData.MaxLevel;
        public int ExpToNextLevel => IsMaxLevel ? 0 : characterData.CalculateExpForLevel(currentLevel + 1) - currentExp;
        public float LevelProgress => IsMaxLevel ? 1.0f : (float)currentExp / characterData.CalculateExpForLevel(currentLevel + 1);
        public int BattlePower => currentStats.CalculateBattlePower();
        public bool IsAlive => currentHP > 0;
        public float HPPercentage => MaxHP > 0 ? (float)currentHP / MaxHP : 0f;
        public float MPPercentage => MaxMP > 0 ? (float)currentMP / MaxMP : 0f;

        /// <summary>
        /// デフォルトコンストラクタ（シリアライゼーション用）
        /// </summary>
        public Character()
        {
            instanceId = Guid.NewGuid().ToString();
            acquiredDate = DateTime.Now;
            permanentBoosts = new Dictionary<StatType, int>();
            temporaryBoosts = new Dictionary<StatType, int>();
            unlockedAbilities = new List<string>();
            
            // スキルシステム初期化
            skillProgression = new CharacterSkillProgression(1);
            skillCooldowns = new CharacterSkillCooldowns();
        }

        /// <summary>
        /// CharacterDataから初期化するコンストラクタ
        /// </summary>
        /// <param name="data">キャラクターデータ</param>
        /// <param name="level">初期レベル</param>
        public Character(CharacterData data, int level = -1) : this()
        {
            if (data == null)
            {
                Debug.LogError("[Character] CharacterDataがnullです");
                return;
            }

            InitializeFromData(data, level);
        }

        /// <summary>
        /// CharacterDataからキャラクターを初期化
        /// </summary>
        /// <param name="data">キャラクターデータ</param>
        /// <param name="level">初期レベル（-1の場合はベースレベル）</param>
        public void InitializeFromData(CharacterData data, int level = -1)
        {
            try
            {
                characterData = data;
                currentLevel = level > 0 ? level : data.BaseLevel;
                currentLevel = Mathf.Clamp(currentLevel, data.BaseLevel, data.MaxLevel);
                currentExp = data.CalculateExpForLevel(currentLevel);

                // 基本ステータスの設定
                currentStats = data.BaseStats;
                
                // レベルに応じた成長を適用
                if (currentLevel > data.BaseLevel)
                {
                    ApplyLevelGrowth();
                }

                // 永続強化を適用
                ApplyPermanentBoosts();

                // HP/MPを最大値に設定
                RestoreToFullHealth();

                // スキルシステム初期化（レベルに応じて）
                skillProgression = new CharacterSkillProgression(currentLevel);
                skillCooldowns = new CharacterSkillCooldowns();

                Debug.Log($"[Character] {data.CharacterName} をレベル{currentLevel}で初期化しました");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Character] 初期化エラー: {e.Message}");
            }
        }

        /// <summary>
        /// 経験値を追加してレベルアップを処理
        /// </summary>
        /// <param name="exp">追加経験値</param>
        /// <returns>レベルアップした回数</returns>
        public int AddExperience(int exp)
        {
            if (exp <= 0 || IsMaxLevel)
                return 0;

            int levelUps = 0;
            int oldLevel = currentLevel;
            currentExp += exp;

            // レベルアップチェック
            while (!IsMaxLevel)
            {
                int expForNextLevel = characterData.CalculateExpForLevel(currentLevel + 1);
                if (currentExp < expForNextLevel)
                    break;

                currentLevel++;
                levelUps++;

                Debug.Log($"[Character] {characterData.CharacterName} がレベル{currentLevel}になりました！");
            }

            // レベルアップした場合はステータスを再計算
            if (levelUps > 0)
            {
                ApplyLevelGrowth();
                ApplyPermanentBoosts();
                
                // HP/MPを最大値まで回復
                RestoreToFullHealth();
                
                // スキル習得処理（レベルアップ時自動実行）
                if (skillProgression != null)
                {
                    var skillUnlockResults = skillProgression.LevelUp(currentLevel, characterData.CharacterName);
                    if (skillUnlockResults.Count > 0)
                    {
                        Debug.Log($"[Character] {characterData.CharacterName}がレベル{oldLevel}→{currentLevel}で{skillUnlockResults.Count}個のスキルを習得しました");
                        foreach (var result in skillUnlockResults)
                        {
                            Debug.Log($"[Character] {result.ToString()}");
                        }
                    }
                }
            }

            return levelUps;
        }

        /// <summary>
        /// 永続的なステータス強化を追加
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <param name="boost">強化値</param>
        public void AddPermanentBoost(StatType statType, int boost)
        {
            if (boost == 0)
                return;

            if (permanentBoosts.ContainsKey(statType))
                permanentBoosts[statType] += boost;
            else
                permanentBoosts[statType] = boost;

            RefreshStats();
            Debug.Log($"[Character] {characterData.CharacterName}の{statType}を{boost}永続強化しました");
        }

        /// <summary>
        /// 一時的なステータス強化を追加
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <param name="boost">強化値</param>
        public void AddTemporaryBoost(StatType statType, int boost)
        {
            if (boost == 0)
                return;

            if (temporaryBoosts.ContainsKey(statType))
                temporaryBoosts[statType] += boost;
            else
                temporaryBoosts[statType] = boost;

            RefreshStats();
        }

        /// <summary>
        /// 一時的な強化をクリア
        /// </summary>
        public void ClearTemporaryBoosts()
        {
            temporaryBoosts.Clear();
            RefreshStats();
        }

        /// <summary>
        /// 特殊能力をアンロック
        /// </summary>
        /// <param name="abilityName">能力名</param>
        public void UnlockAbility(string abilityName)
        {
            if (string.IsNullOrEmpty(abilityName) || unlockedAbilities.Contains(abilityName))
                return;

            unlockedAbilities.Add(abilityName);
            Debug.Log($"[Character] {characterData.CharacterName}が{abilityName}をアンロックしました");
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        /// <returns>実際に受けたダメージ</returns>
        public int TakeDamage(int damage)
        {
            if (damage <= 0)
                return 0;

            int actualDamage = Mathf.Min(damage, currentHP);
            currentHP = Mathf.Max(0, currentHP - damage);

            if (!IsAlive)
            {
                Debug.Log($"[Character] {characterData.CharacterName}が倒れました");
            }

            return actualDamage;
        }

        /// <summary>
        /// HPを回復
        /// </summary>
        /// <param name="healing">回復量</param>
        /// <returns>実際に回復した量</returns>
        public int Heal(int healing)
        {
            if (healing <= 0)
                return 0;

            int actualHealing = Mathf.Min(healing, MaxHP - currentHP);
            currentHP = Mathf.Min(MaxHP, currentHP + healing);

            return actualHealing;
        }

        /// <summary>
        /// MPを消費
        /// </summary>
        /// <param name="cost">消費MP</param>
        /// <returns>消費できた場合true</returns>
        public bool ConsumeMP(int cost)
        {
            if (cost <= 0 || currentMP < cost)
                return false;

            currentMP -= cost;
            return true;
        }

        /// <summary>
        /// MPを回復
        /// </summary>
        /// <param name="recovery">回復量</param>
        /// <returns>実際に回復した量</returns>
        public int RecoverMP(int recovery)
        {
            if (recovery <= 0)
                return 0;

            int actualRecovery = Mathf.Min(recovery, MaxMP - currentMP);
            currentMP = Mathf.Min(MaxMP, currentMP + recovery);

            return actualRecovery;
        }

        /// <summary>
        /// HP/MPを最大値まで回復
        /// </summary>
        public void RestoreToFullHealth()
        {
            currentHP = MaxHP;
            currentMP = MaxMP;
        }

        /// <summary>
        /// レベル成長を適用
        /// </summary>
        private void ApplyLevelGrowth()
        {
            currentStats = characterData.BaseStats;
            var growthRates = characterData.GetGrowthRates();
            currentStats.ApplyLevelGrowth(currentLevel, growthRates);
        }

        /// <summary>
        /// 永続強化を適用
        /// </summary>
        private void ApplyPermanentBoosts()
        {
            foreach (var boost in permanentBoosts)
            {
                currentStats.AddModifier(boost.Key, boost.Value);
            }
        }

        /// <summary>
        /// ステータスを再計算
        /// </summary>
        private void RefreshStats()
        {
            // 一時的な強化をクリアして再計算
            currentStats.ClearModifiers();
            
            // レベル成長を適用
            ApplyLevelGrowth();
            
            // 永続強化を適用
            ApplyPermanentBoosts();
            
            // 一時的な強化を適用
            foreach (var boost in temporaryBoosts)
            {
                currentStats.AddModifier(boost.Key, boost.Value);
            }

            // HP/MPが最大値を超えないように調整
            currentHP = Mathf.Min(currentHP, MaxHP);
            currentMP = Mathf.Min(currentMP, MaxMP);
        }

        /// <summary>
        /// キャラクター情報の詳細文字列を取得
        /// </summary>
        /// <returns>詳細情報</returns>
        public string GetDetailedInfo()
        {
            var info = $"=== {characterData.CharacterName} ===\n";
            info += $"ID: {instanceId}\n";
            info += $"レベル: {currentLevel}/{characterData.MaxLevel}\n";
            info += $"経験値: {currentExp} (次まで: {ExpToNextLevel})\n";
            info += $"HP: {currentHP}/{MaxHP} ({HPPercentage:P1})\n";
            info += $"MP: {currentMP}/{MaxMP} ({MPPercentage:P1})\n";
            info += $"戦闘力: {BattlePower}\n";
            info += $"取得日時: {acquiredDate:yyyy/MM/dd HH:mm}\n";
            info += $"\n{currentStats.GetDetailedInfo()}";
            
            if (permanentBoosts.Count > 0)
            {
                info += "\n=== 永続強化 ===\n";
                foreach (var boost in permanentBoosts)
                {
                    info += $"{boost.Key}: +{boost.Value}\n";
                }
            }

            if (unlockedAbilities.Count > 0)
            {
                info += "\n=== アンロック済み能力 ===\n";
                foreach (var ability in unlockedAbilities)
                {
                    info += $"- {ability}\n";
                }
            }

            return info;
        }

        /// <summary>
        /// 指定スキルが使用可能かどうかを判定
        /// スキル習得状況とクールダウン状況を総合的に判定
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="currentTime">現在時刻（デフォルトはTime.time）</param>
        /// <returns>使用可能かどうか</returns>
        public bool CanUseSkill(int skillSlot, float currentTime = -1f)
        {
            if (currentTime < 0f)
            {
                currentTime = Time.time;
            }

            // スキル習得確認
            if (!skillProgression.IsSkillUnlocked(skillSlot))
            {
                return false;
            }

            // クールダウン確認
            return skillCooldowns.IsSkillReady(skillSlot, currentTime);
        }

        /// <summary>
        /// スキルを使用してクールダウンを開始
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="currentTime">現在時刻（デフォルトはTime.time）</param>
        /// <returns>使用に成功したかどうか</returns>
        public bool UseSkill(int skillSlot, float currentTime = -1f)
        {
            if (currentTime < 0f)
            {
                currentTime = Time.time;
            }

            // 使用可能性確認
            if (!CanUseSkill(skillSlot, currentTime))
            {
                return false;
            }

            // スキル情報取得
            var skill = skillProgression.GetSkill(skillSlot);
            if (skill == null)
            {
                Debug.LogError($"[Character] スキルスロット{skillSlot}のスキル情報が見つかりません");
                return false;
            }

            // クールダウン開始
            skillCooldowns.UseSkill(skillSlot, currentTime, skill.CooldownTime);
            
            Debug.Log($"[Character] {characterData.CharacterName}がスキル「{skill.SkillName}」を使用しました");
            return true;
        }

        /// <summary>
        /// スキルシステムの状況を取得
        /// </summary>
        /// <param name="currentTime">現在時刻（デフォルトはTime.time）</param>
        /// <returns>スキル状況の文字列</returns>
        public string GetSkillStatus(float currentTime = -1f)
        {
            if (currentTime < 0f)
            {
                currentTime = Time.time;
            }

            var status = $"=== {characterData.CharacterName} スキル状況 ===\n";
            status += $"レベル: {currentLevel}, 習得スキル数: {skillProgression.UnlockedSkillCount}/{skillProgression.MaxSkillSlots}\n";
            status += $"クールダウン: {skillCooldowns.GetCooldownStatus(currentTime)}\n";
            
            return status;
        }

        /// <summary>
        /// 簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"{characterData.CharacterName} Lv.{currentLevel} (BP:{BattlePower})";
        }
    }
}