using UnityEngine;
using System;

namespace GatchaSpire.Core.Gold
{
    /// <summary>
    /// ゴールド計算ロジックを提供するクラス
    /// </summary>
    public class GoldCalculator
    {
        private DevelopmentSettings _developmentSettings;
        private bool _isInitialized;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="developmentSettings">開発設定</param>
        public void Initialize(DevelopmentSettings developmentSettings)
        {
            _developmentSettings = developmentSettings;
            _isInitialized = true;
        }

        /// <summary>
        /// 戦闘報酬計算
        /// </summary>
        /// <param name="enemyLevel">敵のレベル</param>
        /// <param name="playerLevel">プレイヤーのレベル</param>
        /// <param name="combatPerformance">戦闘パフォーマンス (0.0 - 1.0)</param>
        /// <returns>戦闘報酬ゴールド</returns>
        public int CalculateCombatReward(int enemyLevel, int playerLevel, float combatPerformance)
        {
            if (!_isInitialized)
            {
                Debug.LogError("GoldCalculatorが初期化されていません");
                return 0;
            }

            // 基本報酬計算
            float baseReward = enemyLevel * 10f;
            
            // レベル差補正
            float levelDifference = enemyLevel - playerLevel;
            float levelMultiplier = 1.0f + (levelDifference * 0.1f);
            levelMultiplier = Mathf.Clamp(levelMultiplier, 0.5f, 2.0f);
            
            // パフォーマンス補正
            float performanceMultiplier = Mathf.Clamp(combatPerformance, 0.1f, 1.0f);
            
            // 開発設定倍率の適用
            float devMultiplier = GetGoldMultiplier();
            
            // 最終計算
            float finalReward = baseReward * levelMultiplier * performanceMultiplier * devMultiplier;
            
            return Mathf.RoundToInt(finalReward);
        }

        /// <summary>
        /// ガチャコスト計算
        /// </summary>
        /// <param name="gachaType">ガチャタイプ</param>
        /// <param name="pullCount">引く回数</param>
        /// <returns>ガチャコスト</returns>
        public int CalculateGachaCost(GachaType gachaType, int pullCount)
        {
            if (!_isInitialized)
            {
                Debug.LogError("GoldCalculatorが初期化されていません");
                return 0;
            }

            int baseCost = GetBaseGachaCost(gachaType);
            
            // 複数引き割引
            float discountMultiplier = CalculateMultiPullDiscount(pullCount);
            
            // 開発設定倍率の適用（コストなので逆倍率）
            float devMultiplier = 1.0f / GetGoldMultiplier();
            
            float totalCost = baseCost * pullCount * discountMultiplier * devMultiplier;
            
            return Mathf.RoundToInt(totalCost);
        }

        /// <summary>
        /// アップグレードコスト計算
        /// </summary>
        /// <param name="currentLevel">現在のレベル</param>
        /// <param name="targetLevel">目標レベル</param>
        /// <param name="upgradeType">アップグレードタイプ</param>
        /// <returns>アップグレードコスト</returns>
        public int CalculateUpgradeCost(int currentLevel, int targetLevel, UpgradeType upgradeType)
        {
            if (!_isInitialized)
            {
                Debug.LogError("GoldCalculatorが初期化されていません");
                return 0;
            }

            if (targetLevel <= currentLevel)
            {
                return 0;
            }

            float totalCost = 0;
            
            for (int level = currentLevel; level < targetLevel; level++)
            {
                float levelCost = CalculateSingleLevelUpgradeCost(level, upgradeType);
                totalCost += levelCost;
            }

            // 開発設定倍率の適用（コストなので逆倍率）
            float devMultiplier = 1.0f / GetGoldMultiplier();
            totalCost *= devMultiplier;
            
            return Mathf.RoundToInt(totalCost);
        }

        /// <summary>
        /// 売却価格計算
        /// </summary>
        /// <param name="itemType">アイテムタイプ</param>
        /// <param name="itemLevel">アイテムレベル</param>
        /// <param name="itemRarity">アイテムレアリティ</param>
        /// <returns>売却価格</returns>
        public int CalculateSellPrice(ItemType itemType, int itemLevel, ItemRarity itemRarity)
        {
            if (!_isInitialized)
            {
                Debug.LogError("GoldCalculatorが初期化されていません");
                return 0;
            }

            float basePrice = GetBaseItemValue(itemType, itemRarity);
            
            // レベル補正
            float levelMultiplier = 1.0f + (itemLevel - 1) * 0.1f;
            
            // 売却価格は基本価格の30%
            float sellPriceMultiplier = 0.3f;
            
            // 開発設定倍率の適用
            float devMultiplier = GetGoldMultiplier();
            
            float finalPrice = basePrice * levelMultiplier * sellPriceMultiplier * devMultiplier;
            
            return Mathf.RoundToInt(finalPrice);
        }

        /// <summary>
        /// 開発設定からゴールド倍率を取得
        /// </summary>
        /// <returns>ゴールド倍率</returns>
        private float GetGoldMultiplier()
        {
            if (_developmentSettings != null)
            {
                return _developmentSettings.GlobalGoldMultiplier;
            }
            return 1.0f;
        }

        /// <summary>
        /// ガチャタイプごとの基本コストを取得
        /// </summary>
        /// <param name="gachaType">ガチャタイプ</param>
        /// <returns>基本コスト</returns>
        private int GetBaseGachaCost(GachaType gachaType)
        {
            return gachaType switch
            {
                GachaType.Normal => 100,
                GachaType.Premium => 300,
                GachaType.Special => 500,
                _ => 100
            };
        }

        /// <summary>
        /// 複数引き割引を計算
        /// </summary>
        /// <param name="pullCount">引く回数</param>
        /// <returns>割引倍率</returns>
        private float CalculateMultiPullDiscount(int pullCount)
        {
            return pullCount switch
            {
                1 => 1.0f,
                10 => 0.9f,  // 10%割引
                100 => 0.8f, // 20%割引
                _ => 1.0f
            };
        }

        /// <summary>
        /// 単一レベルアップグレードコストを計算
        /// </summary>
        /// <param name="level">現在のレベル</param>
        /// <param name="upgradeType">アップグレードタイプ</param>
        /// <returns>単一レベルコスト</returns>
        private float CalculateSingleLevelUpgradeCost(int level, UpgradeType upgradeType)
        {
            float baseCost = GetBaseUpgradeCost(upgradeType);
            
            // レベルごとの指数的コスト増加
            float levelMultiplier = Mathf.Pow(1.5f, level - 1);
            
            return baseCost * levelMultiplier;
        }

        /// <summary>
        /// アップグレードタイプごとの基本コストを取得
        /// </summary>
        /// <param name="upgradeType">アップグレードタイプ</param>
        /// <returns>基本コスト</returns>
        private float GetBaseUpgradeCost(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.Character => 50f,
                UpgradeType.Equipment => 30f,
                UpgradeType.Skill => 100f,
                _ => 50f
            };
        }

        /// <summary>
        /// アイテムタイプとレアリティごとの基本価値を取得
        /// </summary>
        /// <param name="itemType">アイテムタイプ</param>
        /// <param name="itemRarity">アイテムレアリティ</param>
        /// <returns>基本価値</returns>
        private float GetBaseItemValue(ItemType itemType, ItemRarity itemRarity)
        {
            float typeMultiplier = itemType switch
            {
                ItemType.Character => 1.0f,
                ItemType.Equipment => 0.8f,
                ItemType.Consumable => 0.3f,
                _ => 1.0f
            };

            float rarityMultiplier = itemRarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 2.0f,
                ItemRarity.Rare => 5.0f,
                ItemRarity.Epic => 10.0f,
                ItemRarity.Legendary => 20.0f,
                _ => 1.0f
            };

            return 100f * typeMultiplier * rarityMultiplier;
        }
    }

    /// <summary>
    /// ガチャタイプ列挙型
    /// </summary>
    public enum GachaType
    {
        Normal,
        Premium,
        Special
    }

    /// <summary>
    /// アップグレードタイプ列挙型
    /// </summary>
    public enum UpgradeType
    {
        Character,
        Equipment,
        Skill
    }

    /// <summary>
    /// アイテムタイプ列挙型
    /// </summary>
    public enum ItemType
    {
        Character,
        Equipment,
        Consumable
    }

    /// <summary>
    /// アイテムレアリティ列挙型
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}