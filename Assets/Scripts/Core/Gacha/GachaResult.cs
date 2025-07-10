using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャ実行結果
    /// </summary>
    [Serializable]
    public class GachaResult
    {
        [SerializeField] private string gachaSystemId;
        [SerializeField] private List<GachaPullResult> pullResults = new List<GachaPullResult>();
        [SerializeField] private int goldSpent;
        [SerializeField] private int totalPulls;
        [SerializeField] private DateTime pullTime;
        [SerializeField] private GachaStatus status;
        [SerializeField] private int gachaLevel;
        [SerializeField] private bool wasGuaranteed;
        [SerializeField] private bool wasCeiling;

        // プロパティ
        public string GachaSystemId => gachaSystemId;
        public List<GachaPullResult> PullResults => pullResults;
        public int GoldSpent => goldSpent;
        public int TotalPulls => totalPulls;
        public DateTime PullTime => pullTime;
        public GachaStatus Status => status;
        public int GachaLevel => gachaLevel;
        public bool WasGuaranteed => wasGuaranteed;
        public bool WasCeiling => wasCeiling;
        public bool IsSuccess => status == GachaStatus.Success;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaResult(string gachaSystemId, int goldSpent, int gachaLevel, GachaStatus status = GachaStatus.Success)
        {
            this.gachaSystemId = gachaSystemId;
            this.goldSpent = goldSpent;
            this.gachaLevel = gachaLevel;
            this.status = status;
            this.pullTime = DateTime.Now;
            this.pullResults = new List<GachaPullResult>();
            this.totalPulls = 0;
            this.wasGuaranteed = false;
            this.wasCeiling = false;
        }

        /// <summary>
        /// プル結果を追加
        /// </summary>
        public void AddPullResult(CharacterData character, CharacterRarity rarity, GachaResultType resultType)
        {
            var pullResult = new GachaPullResult(character, rarity, resultType);
            pullResults.Add(pullResult);
            totalPulls++;

            // 特殊な結果フラグを設定
            if (resultType == GachaResultType.Guaranteed)
                wasGuaranteed = true;
            if (resultType == GachaResultType.Ceiling)
                wasCeiling = true;
        }

        /// <summary>
        /// 指定レアリティのキャラクター数を取得
        /// </summary>
        public int GetCharacterCount(CharacterRarity rarity)
        {
            return pullResults.Count(r => r.Rarity == rarity);
        }

        /// <summary>
        /// 最高レアリティを取得
        /// </summary>
        public CharacterRarity GetHighestRarity()
        {
            if (pullResults.Count == 0)
                return CharacterRarity.Common;
            
            return pullResults.Max(r => r.Rarity);
        }

        /// <summary>
        /// 取得したキャラクターリストを取得
        /// </summary>
        public List<CharacterData> GetCharacters()
        {
            return pullResults.Select(r => r.Character).ToList();
        }

        /// <summary>
        /// レアリティ別の統計情報を取得
        /// </summary>
        public Dictionary<CharacterRarity, int> GetRarityStatistics()
        {
            var statistics = new Dictionary<CharacterRarity, int>();
            
            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                statistics[rarity] = GetCharacterCount(rarity);
            }
            
            return statistics;
        }

        /// <summary>
        /// 結果サマリーを取得
        /// </summary>
        public string GetSummary()
        {
            if (!IsSuccess)
                return $"ガチャ失敗: {status}";
            
            var summary = $"ガチャ実行結果 (Level {gachaLevel})\n";
            summary += $"消費ゴールド: {goldSpent}\n";
            summary += $"排出数: {totalPulls}\n";
            
            var rarityStats = GetRarityStatistics();
            foreach (var stat in rarityStats.Where(s => s.Value > 0))
            {
                summary += $"{stat.Key}: {stat.Value}体\n";
            }
            
            if (wasGuaranteed)
                summary += "保証発動あり\n";
            if (wasCeiling)
                summary += "天井発動あり\n";
            
            return summary;
        }
    }

    /// <summary>
    /// 単一プル結果
    /// </summary>
    [Serializable]
    public class GachaPullResult
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private CharacterRarity rarity;
        [SerializeField] private GachaResultType resultType;
        [SerializeField] private bool isNewCharacter;

        // プロパティ
        public CharacterData Character => characterData;
        public CharacterRarity Rarity => rarity;
        public GachaResultType ResultType => resultType;
        public bool IsNewCharacter => isNewCharacter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaPullResult(CharacterData character, CharacterRarity rarity, GachaResultType resultType, bool isNewCharacter = false)
        {
            this.characterData = character;
            this.rarity = rarity;
            this.resultType = resultType;
            this.isNewCharacter = isNewCharacter;
        }

        /// <summary>
        /// 結果の説明を取得
        /// </summary>
        public string GetDescription()
        {
            var description = $"{characterData.CharacterName} ({rarity})";
            
            switch (resultType)
            {
                case GachaResultType.Guaranteed:
                    description += " [保証]";
                    break;
                case GachaResultType.Ceiling:
                    description += " [天井]";
                    break;
                case GachaResultType.Pickup:
                    description += " [ピックアップ]";
                    break;
                case GachaResultType.Bonus:
                    description += " [ボーナス]";
                    break;
            }
            
            if (isNewCharacter)
                description += " [NEW]";
            
            return description;
        }
    }

    /// <summary>
    /// ガチャアップグレードプレビュー
    /// </summary>
    [Serializable]
    public class GachaUpgradePreview
    {
        [SerializeField] private int currentLevel;
        [SerializeField] private int nextLevel;
        [SerializeField] private int upgradeCost;
        [SerializeField] private string effectDescription;
        [SerializeField] private Dictionary<CharacterRarity, float> currentRates = new Dictionary<CharacterRarity, float>();
        [SerializeField] private Dictionary<CharacterRarity, float> nextRates = new Dictionary<CharacterRarity, float>();
        [SerializeField] private int currentCost;
        [SerializeField] private int nextCost;
        [SerializeField] private bool canUpgrade;

        // プロパティ
        public int CurrentLevel => currentLevel;
        public int NextLevel => nextLevel;
        public int UpgradeCost => upgradeCost;
        public string EffectDescription => effectDescription;
        public Dictionary<CharacterRarity, float> CurrentRates => currentRates;
        public Dictionary<CharacterRarity, float> NextRates => nextRates;
        public int CurrentCost => currentCost;
        public int NextCost => nextCost;
        public bool CanUpgrade => canUpgrade;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaUpgradePreview(int currentLevel, int nextLevel, int upgradeCost, string effectDescription, bool canUpgrade)
        {
            this.currentLevel = currentLevel;
            this.nextLevel = nextLevel;
            this.upgradeCost = upgradeCost;
            this.effectDescription = effectDescription;
            this.canUpgrade = canUpgrade;
            this.currentRates = new Dictionary<CharacterRarity, float>();
            this.nextRates = new Dictionary<CharacterRarity, float>();
        }

        /// <summary>
        /// 現在のレート設定
        /// </summary>
        public void SetCurrentRates(Dictionary<CharacterRarity, float> rates)
        {
            currentRates = new Dictionary<CharacterRarity, float>(rates);
        }

        /// <summary>
        /// 次のレート設定
        /// </summary>
        public void SetNextRates(Dictionary<CharacterRarity, float> rates)
        {
            nextRates = new Dictionary<CharacterRarity, float>(rates);
        }

        /// <summary>
        /// コスト設定
        /// </summary>
        public void SetCosts(int currentCost, int nextCost)
        {
            this.currentCost = currentCost;
            this.nextCost = nextCost;
        }

        /// <summary>
        /// レート変化の取得
        /// </summary>
        public Dictionary<CharacterRarity, float> GetRateChanges()
        {
            var changes = new Dictionary<CharacterRarity, float>();
            
            foreach (var rarity in currentRates.Keys)
            {
                var currentRate = currentRates.GetValueOrDefault(rarity, 0f);
                var nextRate = nextRates.GetValueOrDefault(rarity, 0f);
                changes[rarity] = nextRate - currentRate;
            }
            
            return changes;
        }

        /// <summary>
        /// プレビューサマリー取得
        /// </summary>
        public string GetSummary()
        {
            var summary = $"レベル {currentLevel} → {nextLevel}\n";
            summary += $"コスト: {upgradeCost}ゴールド\n";
            summary += $"効果: {effectDescription}\n";
            summary += $"ガチャコスト: {currentCost} → {nextCost}\n";
            
            var rateChanges = GetRateChanges();
            foreach (var change in rateChanges.Where(c => Math.Abs(c.Value) > 0.01f))
            {
                var sign = change.Value > 0 ? "+" : "";
                summary += $"{change.Key}: {sign}{change.Value:F2}%\n";
            }
            
            return summary;
        }
    }
}