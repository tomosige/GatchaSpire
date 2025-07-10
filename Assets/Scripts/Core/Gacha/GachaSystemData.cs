using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャシステムの設定データ
    /// </summary>
    [CreateAssetMenu(fileName = "GachaSystemData", menuName = "GatchaSpire/Gacha System Data")]
    public class GachaSystemData : ScriptableObject, IValidatable
    {
        [Header("基本設定")]
        [SerializeField] private string gachaSystemId = "basic_gacha";
        [SerializeField] private string displayName = "基本ガチャ";
        [SerializeField] private int baseCost = 100;
        [SerializeField] private bool isActive = true;
        
        [Header("排出確率設定")]
        [SerializeField] private GachaDropRate[] dropRates = new GachaDropRate[0];
        [SerializeField] private float guaranteedRareRate = 5.0f;
        [SerializeField] private int guaranteedRareCount = 20;
        
        [Header("アップグレード設定")]
        [SerializeField] private int maxUpgradeLevel = 10;
        [SerializeField] private GachaUpgrade[] upgrades = new GachaUpgrade[0];
        
        [Header("天井システム")]
        [SerializeField] private bool hasCeiling = true;
        [SerializeField] private int ceilingCount = 100;
        [SerializeField] private CharacterRarity ceilingRarity = CharacterRarity.Epic;
        
        [Header("ピックアップ設定")]
        [SerializeField] private bool hasPickup = false;
        [SerializeField] private List<int> pickupCharacterIds = new List<int>();
        [SerializeField] private float pickupRate = 2.0f;

        // プロパティ
        public string GachaSystemId => gachaSystemId;
        public string DisplayName => displayName;
        public int BaseCost => baseCost;
        public bool IsActive => isActive;
        public bool HasCeiling => hasCeiling;
        public int CeilingCount => ceilingCount;
        public CharacterRarity CeilingRarity => ceilingRarity;
        public bool HasPickup => hasPickup;
        public float PickupRate => pickupRate;
        public List<int> PickupCharacterIds => pickupCharacterIds;
        public int MaxUpgradeLevel => maxUpgradeLevel;

        /// <summary>
        /// 基本レアリティ排出率を取得
        /// </summary>
        public float GetBaseRarityRate(CharacterRarity rarity)
        {
            var dropRate = dropRates.FirstOrDefault(r => r.rarity == rarity);
            return dropRate.baseRate;
        }

        /// <summary>
        /// レベル別アップグレードコストを取得
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= maxUpgradeLevel)
                return 0;

            var upgrade = upgrades.FirstOrDefault(u => u.level == currentLevel + 1);
            return upgrade.cost;
        }

        /// <summary>
        /// レベル別レアリティボーナスを取得
        /// </summary>
        public float GetLevelRarityBonus(int level, CharacterRarity rarity)
        {
            var dropRate = dropRates.FirstOrDefault(r => r.rarity == rarity);
            return dropRate.upgradeBonus * level;
        }

        /// <summary>
        /// 基本ガチャコストを取得
        /// </summary>
        public int GetBaseCost() => baseCost;

        /// <summary>
        /// レベル別コスト削減率を取得
        /// </summary>
        public float GetCostReduction(int level)
        {
            float totalReduction = 0f;
            
            for (int i = 1; i <= level; i++)
            {
                var upgrade = upgrades.FirstOrDefault(u => u.level == i);
                if (upgrade.level > 0)
                {
                    var costReductionEffect = upgrade.effects.FirstOrDefault(e => e.type == GachaUpgradeType.CostReduction);
                    if (costReductionEffect.type == GachaUpgradeType.CostReduction)
                    {
                        totalReduction += costReductionEffect.value;
                    }
                }
            }
            
            return Mathf.Min(totalReduction, 0.5f); // 最大50%削減
        }

        /// <summary>
        /// レベル別同時排出数を取得
        /// </summary>
        public int GetSimultaneousPullCount(int level)
        {
            int pullCount = 1;
            
            for (int i = 1; i <= level; i++)
            {
                var upgrade = upgrades.FirstOrDefault(u => u.level == i);
                if (upgrade.level > 0)
                {
                    var simultaneousEffect = upgrade.effects.FirstOrDefault(e => e.type == GachaUpgradeType.SimultaneousPull);
                    if (simultaneousEffect.type == GachaUpgradeType.SimultaneousPull)
                    {
                        pullCount += (int)simultaneousEffect.value;
                    }
                }
            }
            
            return pullCount;
        }

        /// <summary>
        /// 保証レア以上の確率を取得
        /// </summary>
        public float GetGuaranteedRareRate() => guaranteedRareRate;

        /// <summary>
        /// 保証レア以上までの回数を取得
        /// </summary>
        public int GetGuaranteedRareCount() => guaranteedRareCount;

        /// <summary>
        /// 抽選重みを取得
        /// </summary>
        public int GetWeight(CharacterRarity rarity)
        {
            var dropRate = dropRates.FirstOrDefault(r => r.rarity == rarity);
            return dropRate.weight;
        }

        /// <summary>
        /// アップグレード情報を取得
        /// </summary>
        public GachaUpgrade GetUpgrade(int level)
        {
            return upgrades.FirstOrDefault(u => u.level == level);
        }

        /// <summary>
        /// 全アップグレード情報を取得
        /// </summary>
        public GachaUpgrade[] GetAllUpgrades() => upgrades;

        /// <summary>
        /// 全ドロップレート情報を取得
        /// </summary>
        public GachaDropRate[] GetAllDropRates() => dropRates;

        /// <summary>
        /// データの妥当性検証
        /// </summary>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            // 基本設定の検証
            if (string.IsNullOrEmpty(gachaSystemId))
                result.AddError("ガチャシステムIDが未設定です");
            
            if (string.IsNullOrEmpty(displayName))
                result.AddError("表示名が未設定です");
            
            if (baseCost <= 0)
                result.AddError("基本コストは正の値である必要があります");
            
            // 排出確率の検証
            if (dropRates == null || dropRates.Length == 0)
                result.AddError("排出確率が設定されていません");
            else
            {
                float totalRate = dropRates.Sum(r => r.baseRate);
                if (Math.Abs(totalRate - 100f) > 0.01f)
                    result.AddError($"排出確率の合計が100%ではありません（現在: {totalRate:F2}%）");
                
                foreach (var rate in dropRates)
                {
                    if (rate.baseRate < 0)
                        result.AddError($"レアリティ {rate.rarity} の排出確率が負の値です");
                    if (rate.weight <= 0)
                        result.AddError($"レアリティ {rate.rarity} の重みが0以下です");
                }
            }
            
            // アップグレード設定の検証
            if (maxUpgradeLevel < 0)
                result.AddError("最大アップグレードレベルは0以上である必要があります");
            
            if (upgrades != null)
            {
                var duplicateLevels = upgrades.GroupBy(u => u.level)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);
                
                foreach (var level in duplicateLevels)
                {
                    result.AddError($"レベル {level} のアップグレードが重複しています");
                }
            }
            
            // 天井システムの検証
            if (hasCeiling)
            {
                if (ceilingCount <= 0)
                    result.AddError("天井回数は正の値である必要があります");
                
                if (ceilingCount > 1000)
                    result.AddWarning("天井回数が1000を超えています。適切な値か確認してください");
            }
            
            // ピックアップの検証
            if (hasPickup)
            {
                if (pickupCharacterIds == null || pickupCharacterIds.Count == 0)
                    result.AddError("ピックアップが有効ですが、対象キャラクターが設定されていません");
                
                if (pickupRate <= 0)
                    result.AddError("ピックアップ率は正の値である必要があります");
            }
            
            return result;
        }

        /// <summary>
        /// Unity OnValidate
        /// </summary>
        private void OnValidate()
        {
            var validation = Validate();
            if (!validation.IsValid)
            {
                Debug.LogWarning($"[GachaSystemData] バリデーションエラー: {validation.GetSummary()}", this);
            }
        }
    }

    /// <summary>
    /// ガチャ排出率設定
    /// </summary>
    [Serializable]
    public struct GachaDropRate
    {
        public CharacterRarity rarity;      // レアリティ
        public float baseRate;              // 基本確率 (%)
        public float upgradeBonus;          // アップグレード時ボーナス (%)
        public int weight;                  // 抽選重み
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaDropRate(CharacterRarity rarity, float baseRate, float upgradeBonus, int weight)
        {
            this.rarity = rarity;
            this.baseRate = baseRate;
            this.upgradeBonus = upgradeBonus;
            this.weight = weight;
        }
    }

    /// <summary>
    /// ガチャアップグレード設定
    /// </summary>
    [Serializable]
    public struct GachaUpgrade
    {
        public int level;                   // アップグレードレベル
        public int cost;                    // アップグレードコスト
        public string effectDescription;    // 効果説明
        public GachaUpgradeEffect[] effects; // 効果配列
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaUpgrade(int level, int cost, string effectDescription, GachaUpgradeEffect[] effects)
        {
            this.level = level;
            this.cost = cost;
            this.effectDescription = effectDescription;
            this.effects = effects;
        }
    }

    /// <summary>
    /// ガチャアップグレード効果
    /// </summary>
    [Serializable]
    public struct GachaUpgradeEffect
    {
        public GachaUpgradeType type;       // 効果タイプ
        public CharacterRarity targetRarity; // 対象レアリティ
        public float value;                 // 効果値
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaUpgradeEffect(GachaUpgradeType type, CharacterRarity targetRarity, float value)
        {
            this.type = type;
            this.targetRarity = targetRarity;
            this.value = value;
        }
    }
}