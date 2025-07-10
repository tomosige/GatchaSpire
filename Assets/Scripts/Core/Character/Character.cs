using UnityEngine;
using System;
using System.Collections.Generic;
using GatchaSpire.Core.Error;

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
        /// 簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"{characterData.CharacterName} Lv.{currentLevel} (BP:{BattlePower})";
        }
    }
}