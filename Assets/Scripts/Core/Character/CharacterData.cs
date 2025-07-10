using UnityEngine;
using System.Collections.Generic;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターデータのScriptableObject
    /// 基本パラメータ、バリデーション、エディタプレビュー機能を提供
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "GatchaSpire/Character/Character Data")]
    public class CharacterData : ScriptableObject, IValidatable
    {
        [Header("基本情報")]
        [SerializeField] private string characterName = "";
        [SerializeField] private string description = "";
        [SerializeField] private int characterId = 0;
        
        [Header("分類")]
        [SerializeField] private CharacterRarity rarity = CharacterRarity.Common;
        [SerializeField] private CharacterRace race = CharacterRace.Human;
        [SerializeField] private CharacterClass characterClass = CharacterClass.Warrior;
        [SerializeField] private CharacterElement element = CharacterElement.None;
        [SerializeField] private CharacterRole role = CharacterRole.DPS;

        [Header("レベルシステム")]
        [SerializeField] private int baseLevel = 1;
        [SerializeField] private int maxLevel = 100;
        [SerializeField] private int expToNextLevel = 100;
        [SerializeField] private float expGrowthRate = 1.2f;

        [Header("基本ステータス")]
        [SerializeField] private CharacterStats baseStats = new CharacterStats(100, 50, 10, 8, 12, 6, 5, 8);

        [Header("成長率（レベルアップ時の増加率）")]
        [SerializeField, Range(0f, 1f)] private float hpGrowthRate = 0.1f;
        [SerializeField, Range(0f, 1f)] private float mpGrowthRate = 0.08f;
        [SerializeField, Range(0f, 1f)] private float attackGrowthRate = 0.12f;
        [SerializeField, Range(0f, 1f)] private float defenseGrowthRate = 0.1f;
        [SerializeField, Range(0f, 1f)] private float speedGrowthRate = 0.05f;
        [SerializeField, Range(0f, 1f)] private float magicGrowthRate = 0.08f;
        [SerializeField, Range(0f, 1f)] private float resistanceGrowthRate = 0.06f;
        [SerializeField, Range(0f, 1f)] private float luckGrowthRate = 0.03f;

        [Header("アート素材")]
        [SerializeField] private Sprite characterIcon;
        [SerializeField] private Sprite characterPortrait;
        [SerializeField] private GameObject characterModel;

        [Header("コスト設定")]
        [SerializeField] private int gachaCost = 100;
        [SerializeField] private int upgradeCost = 50;
        [SerializeField] private int sellPrice = 30;

        [Header("特殊効果")]
        [SerializeField] private List<string> specialAbilities = new List<string>();
        [SerializeField] private List<string> passiveSkills = new List<string>();

        // プロパティ
        public string CharacterName => characterName;
        public string Description => description;
        public int CharacterId => characterId;
        public CharacterRarity Rarity => rarity;
        public CharacterRace Race => race;
        public CharacterClass CharacterClass => characterClass;
        public CharacterElement Element => element;
        public CharacterRole Role => role;
        public int BaseLevel => baseLevel;
        public int MaxLevel => maxLevel;
        public int ExpToNextLevel => expToNextLevel;
        public float ExpGrowthRate => expGrowthRate;
        public CharacterStats BaseStats => baseStats;
        public Sprite CharacterIcon => characterIcon;
        public Sprite CharacterPortrait => characterPortrait;
        public GameObject CharacterModel => characterModel;
        public int GachaCost => gachaCost;
        public int UpgradeCost => upgradeCost;
        public int SellPrice => sellPrice;
        public List<string> SpecialAbilities => new List<string>(specialAbilities);
        public List<string> PassiveSkills => new List<string>(passiveSkills);

        /// <summary>
        /// 成長率のディクショナリを取得
        /// </summary>
        /// <returns>ステータス成長率</returns>
        public Dictionary<StatType, float> GetGrowthRates()
        {
            return new Dictionary<StatType, float>
            {
                { StatType.HP, hpGrowthRate },
                { StatType.MP, mpGrowthRate },
                { StatType.Attack, attackGrowthRate },
                { StatType.Defense, defenseGrowthRate },
                { StatType.Speed, speedGrowthRate },
                { StatType.Magic, magicGrowthRate },
                { StatType.Resistance, resistanceGrowthRate },
                { StatType.Luck, luckGrowthRate }
            };
        }

        /// <summary>
        /// 指定レベルでの必要経験値を計算
        /// </summary>
        /// <param name="level">目標レベル</param>
        /// <returns>必要経験値</returns>
        public int CalculateExpForLevel(int level)
        {
            if (level <= baseLevel)
                return 0;

            int totalExp = 0;
            for (int i = baseLevel; i < level; i++)
            {
                totalExp += Mathf.RoundToInt(expToNextLevel * Mathf.Pow(expGrowthRate, i - baseLevel));
            }
            return totalExp;
        }

        /// <summary>
        /// 経験値から現在レベルを計算
        /// </summary>
        /// <param name="currentExp">現在の経験値</param>
        /// <returns>現在レベル</returns>
        public int CalculateLevelFromExp(int currentExp)
        {
            int level = baseLevel;
            int requiredExp = 0;

            while (level < maxLevel)
            {
                int expForNextLevel = Mathf.RoundToInt(expToNextLevel * Mathf.Pow(expGrowthRate, level - baseLevel));
                
                if (currentExp < requiredExp + expForNextLevel)
                    break;
                
                requiredExp += expForNextLevel;
                level++;
            }

            return level;
        }

        /// <summary>
        /// レアリティによるステータス倍率を取得
        /// </summary>
        /// <returns>ステータス倍率</returns>
        public float GetRarityStatMultiplier()
        {
            return rarity switch
            {
                CharacterRarity.Common => 1.0f,
                CharacterRarity.Uncommon => 1.1f,
                CharacterRarity.Rare => 1.25f,
                CharacterRarity.Epic => 1.5f,
                CharacterRarity.Legendary => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 戦闘力の基本値を計算
        /// </summary>
        /// <returns>基本戦闘力</returns>
        public int CalculateBaseBattlePower()
        {
            var adjustedStats = baseStats;
            float multiplier = GetRarityStatMultiplier();
            
            // レアリティ倍率を適用した仮想ステータスで戦闘力計算
            var tempStats = new CharacterStats(
                Mathf.RoundToInt(baseStats.BaseHP * multiplier),
                Mathf.RoundToInt(baseStats.BaseMP * multiplier),
                Mathf.RoundToInt(baseStats.BaseAttack * multiplier),
                Mathf.RoundToInt(baseStats.BaseDefense * multiplier),
                Mathf.RoundToInt(baseStats.BaseSpeed * multiplier),
                Mathf.RoundToInt(baseStats.BaseMagic * multiplier),
                Mathf.RoundToInt(baseStats.BaseResistance * multiplier),
                Mathf.RoundToInt(baseStats.BaseLuck * multiplier)
            );

            return tempStats.CalculateBattlePower();
        }

        /// <summary>
        /// データバリデーション
        /// </summary>
        /// <returns>バリデーション結果</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            // 基本情報チェック
            if (string.IsNullOrEmpty(characterName))
                result.AddError("キャラクター名が設定されていません");
            
            if (characterId <= 0)
                result.AddError("キャラクターIDは1以上である必要があります");

            // レベル設定チェック
            if (baseLevel <= 0)
                result.AddError("基本レベルは1以上である必要があります");
            
            if (maxLevel <= baseLevel)
                result.AddError("最大レベルは基本レベルより大きい必要があります");
            
            if (expToNextLevel <= 0)
                result.AddError("次レベルまでの経験値は1以上である必要があります");

            // ステータスチェック
            if (!baseStats.IsValid())
                result.AddError("基本ステータスが無効です");

            // 成長率チェック
            if (hpGrowthRate < 0 || hpGrowthRate > 1)
                result.AddWarning("HP成長率が推奨範囲外です (0-1)");
            
            if (attackGrowthRate < 0 || attackGrowthRate > 1)
                result.AddWarning("攻撃力成長率が推奨範囲外です (0-1)");

            // コスト設定チェック
            if (gachaCost < 0)
                result.AddError("ガチャコストは0以上である必要があります");
            
            if (upgradeCost < 0)
                result.AddError("アップグレードコストは0以上である必要があります");
            
            if (sellPrice < 0)
                result.AddError("売却価格は0以上である必要があります");

            // アート素材チェック
            if (characterIcon == null)
                result.AddWarning("キャラクターアイコンが設定されていません");
            
            if (characterPortrait == null)
                result.AddWarning("キャラクターポートレートが設定されていません");

            // バランスチェック
            int battlePower = CalculateBaseBattlePower();
            if (battlePower > 10000)
                result.AddWarning("戦闘力が高すぎる可能性があります");
            else if (battlePower < 100)
                result.AddWarning("戦闘力が低すぎる可能性があります");

            return result;
        }

        /// <summary>
        /// エディタプレビュー用の情報を取得
        /// </summary>
        /// <returns>プレビュー情報</returns>
        public string GetPreviewInfo()
        {
            var info = $"=== {characterName} ===\n";
            info += $"ID: {characterId}\n";
            info += $"レアリティ: {rarity}\n";
            info += $"種族: {race}\n";
            info += $"クラス: {characterClass}\n";
            info += $"属性: {element}\n";
            info += $"役割: {role}\n";
            info += $"レベル範囲: {baseLevel} - {maxLevel}\n";
            info += $"基本戦闘力: {CalculateBaseBattlePower()}\n";
            info += $"ガチャコスト: {gachaCost}\n";
            info += $"特殊能力数: {specialAbilities.Count}\n";
            info += $"パッシブスキル数: {passiveSkills.Count}\n";
            info += $"\n基本ステータス:\n{baseStats.ToString()}";
            
            return info;
        }

        /// <summary>
        /// Unityエディタでの値変更時の検証
        /// </summary>
        private void OnValidate()
        {
            // 値の範囲制限
            characterId = Mathf.Max(0, characterId);
            baseLevel = Mathf.Max(1, baseLevel);
            maxLevel = Mathf.Max(baseLevel + 1, maxLevel);
            expToNextLevel = Mathf.Max(1, expToNextLevel);
            expGrowthRate = Mathf.Max(1.0f, expGrowthRate);
            
            gachaCost = Mathf.Max(0, gachaCost);
            upgradeCost = Mathf.Max(0, upgradeCost);
            sellPrice = Mathf.Max(0, sellPrice);

            // 成長率の制限
            hpGrowthRate = Mathf.Clamp01(hpGrowthRate);
            mpGrowthRate = Mathf.Clamp01(mpGrowthRate);
            attackGrowthRate = Mathf.Clamp01(attackGrowthRate);
            defenseGrowthRate = Mathf.Clamp01(defenseGrowthRate);
            speedGrowthRate = Mathf.Clamp01(speedGrowthRate);
            magicGrowthRate = Mathf.Clamp01(magicGrowthRate);
            resistanceGrowthRate = Mathf.Clamp01(resistanceGrowthRate);
            luckGrowthRate = Mathf.Clamp01(luckGrowthRate);

            // バリデーション実行
            var validation = Validate();
            if (!validation.IsValid && Application.isPlaying)
            {
                Debug.LogWarning($"[CharacterData] {name}: {validation.GetSummary()}");
            }
        }
    }
}