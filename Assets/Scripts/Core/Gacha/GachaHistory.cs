using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャ履歴管理
    /// </summary>
    [Serializable]
    public class GachaHistory
    {
        [SerializeField] private List<GachaResult> history = new List<GachaResult>();
        [SerializeField] private int maxHistoryCount = 100;
        [SerializeField] private int totalPulls = 0;
        [SerializeField] private long totalGoldSpent = 0L;
        [SerializeField] private Dictionary<CharacterRarity, int> totalCharacterCounts = new Dictionary<CharacterRarity, int>();
        [SerializeField] private int consecutiveCount = 0;
        [SerializeField] private int consecutiveWithoutRare = 0;

        // プロパティ
        public List<GachaResult> History => history;
        public int MaxHistoryCount => maxHistoryCount;
        public int TotalPulls => totalPulls;
        public long TotalGoldSpent => totalGoldSpent;
        public Dictionary<CharacterRarity, int> TotalCharacterCounts => totalCharacterCounts;
        public int ConsecutiveCount => consecutiveCount;
        public int ConsecutiveWithoutRare => consecutiveWithoutRare;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GachaHistory(int maxHistoryCount = 100)
        {
            this.maxHistoryCount = maxHistoryCount;
            this.history = new List<GachaResult>();
            this.totalCharacterCounts = new Dictionary<CharacterRarity, int>();
            
            // レアリティ別カウントを初期化
            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                totalCharacterCounts[rarity] = 0;
            }
        }

        /// <summary>
        /// ガチャ結果を追加
        /// </summary>
        public void AddResult(GachaResult result)
        {
            if (result == null || !result.IsSuccess)
                return;

            // 履歴に追加
            history.Add(result);
            
            // 最大履歴数を超えた場合は古いものを削除
            if (history.Count > maxHistoryCount)
            {
                history.RemoveAt(0);
            }

            // 統計情報を更新
            UpdateStatistics(result);
        }

        /// <summary>
        /// 統計情報を更新
        /// </summary>
        private void UpdateStatistics(GachaResult result)
        {
            // 基本統計の更新
            totalPulls += result.TotalPulls;
            totalGoldSpent += result.GoldSpent;
            consecutiveCount++;

            // レアリティ別統計の更新
            bool hasRareOrAbove = false;
            foreach (var pullResult in result.PullResults)
            {
                if (totalCharacterCounts.ContainsKey(pullResult.Rarity))
                {
                    totalCharacterCounts[pullResult.Rarity]++;
                }
                
                // レア以上が出たかチェック
                if (pullResult.Rarity >= CharacterRarity.Rare)
                {
                    hasRareOrAbove = true;
                }
            }

            // 連続でレア以上が出ていない回数を更新
            if (hasRareOrAbove)
            {
                consecutiveWithoutRare = 0;
            }
            else
            {
                consecutiveWithoutRare++;
            }
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void ClearHistory()
        {
            history.Clear();
            ResetStatistics();
        }

        /// <summary>
        /// 統計情報をリセット
        /// </summary>
        public void ResetStatistics()
        {
            totalPulls = 0;
            totalGoldSpent = 0L;
            consecutiveCount = 0;
            consecutiveWithoutRare = 0;
            
            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                totalCharacterCounts[rarity] = 0;
            }
        }

        /// <summary>
        /// 指定期間の履歴を取得
        /// </summary>
        public List<GachaResult> GetHistoryInPeriod(DateTime startTime, DateTime endTime)
        {
            return history.Where(r => r.PullTime >= startTime && r.PullTime <= endTime).ToList();
        }

        /// <summary>
        /// 最近の履歴を取得
        /// </summary>
        public List<GachaResult> GetRecentHistory(int count)
        {
            count = Math.Min(count, history.Count);
            return history.Skip(history.Count - count).Take(count).ToList();
        }

        /// <summary>
        /// 指定レアリティの排出率を取得
        /// </summary>
        public float GetActualRate(CharacterRarity rarity)
        {
            if (totalPulls == 0)
                return 0f;
            
            int count = totalCharacterCounts.GetValueOrDefault(rarity, 0);
            return (float)count / totalPulls * 100f;
        }

        /// <summary>
        /// 全レアリティの排出率を取得
        /// </summary>
        public Dictionary<CharacterRarity, float> GetAllActualRates()
        {
            var rates = new Dictionary<CharacterRarity, float>();
            
            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                rates[rarity] = GetActualRate(rarity);
            }
            
            return rates;
        }

        /// <summary>
        /// 1回あたりの平均コストを取得
        /// </summary>
        public float GetAverageCostPerPull()
        {
            if (totalPulls == 0)
                return 0f;
            
            return (float)totalGoldSpent / totalPulls;
        }

        /// <summary>
        /// 指定レアリティを取得するまでの平均コストを取得
        /// </summary>
        public float GetAverageCostForRarity(CharacterRarity rarity)
        {
            int count = totalCharacterCounts.GetValueOrDefault(rarity, 0);
            if (count == 0)
                return 0f;
            
            return (float)totalGoldSpent / count;
        }

        /// <summary>
        /// 履歴統計のサマリーを取得
        /// </summary>
        public string GetStatisticsSummary()
        {
            var summary = "=== ガチャ統計 ===\n";
            summary += $"総回数: {totalPulls}回\n";
            summary += $"総消費ゴールド: {totalGoldSpent:N0}\n";
            summary += $"平均コスト: {GetAverageCostPerPull():F1}ゴールド/回\n";
            summary += $"連続実行回数: {consecutiveCount}回\n";
            summary += $"レア以上なし連続: {consecutiveWithoutRare}回\n\n";
            
            summary += "=== レアリティ別統計 ===\n";
            foreach (CharacterRarity rarity in Enum.GetValues(typeof(CharacterRarity)))
            {
                int count = totalCharacterCounts.GetValueOrDefault(rarity, 0);
                float rate = GetActualRate(rarity);
                summary += $"{rarity}: {count}体 ({rate:F2}%)\n";
            }
            
            return summary;
        }

        /// <summary>
        /// 最後のガチャ結果を取得
        /// </summary>
        public GachaResult GetLastResult()
        {
            return history.LastOrDefault();
        }

        /// <summary>
        /// 天井システム用の連続回数を取得
        /// </summary>
        public int GetConsecutiveWithoutRarity(CharacterRarity minRarity)
        {
            int consecutiveCount = 0;
            
            // 履歴を逆順で確認
            for (int i = history.Count - 1; i >= 0; i--)
            {
                var result = history[i];
                bool hasMinRarity = result.PullResults.Any(r => r.Rarity >= minRarity);
                
                if (hasMinRarity)
                {
                    break;
                }
                
                consecutiveCount += result.TotalPulls;
            }
            
            return consecutiveCount;
        }

        /// <summary>
        /// 履歴からピックアップ排出率を計算
        /// </summary>
        public float GetPickupRate(List<int> pickupCharacterIds)
        {
            if (pickupCharacterIds == null || pickupCharacterIds.Count == 0)
                return 0f;
            
            int totalPulls = 0;
            int pickupPulls = 0;
            
            foreach (var result in history)
            {
                foreach (var pullResult in result.PullResults)
                {
                    totalPulls++;
                    
                    if (pullResult.Character != null && 
                        pickupCharacterIds.Contains(pullResult.Character.CharacterId))
                    {
                        pickupPulls++;
                    }
                }
            }
            
            return totalPulls > 0 ? (float)pickupPulls / totalPulls * 100f : 0f;
        }
    }
}