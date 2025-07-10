using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GatchaSpire.Core.Gold
{
    /// <summary>
    /// ゴールド取引履歴管理クラス
    /// </summary>
    public class GoldTransactionHistory
    {
        private List<GoldTransaction> _transactions;
        private readonly int _maxHistorySize;
        private readonly string _persistentFilePath;

        // 分析用キャッシュ
        private GoldAnalytics _cachedAnalytics;
        private DateTime _lastAnalyticsUpdate;

        /// <summary>
        /// 取引数
        /// </summary>
        public int TransactionCount => _transactions.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxHistorySize">最大履歴サイズ</param>
        public GoldTransactionHistory(int maxHistorySize = 1000)
        {
            _maxHistorySize = maxHistorySize;
            _transactions = new List<GoldTransaction>();
            _persistentFilePath = Path.Combine(Application.persistentDataPath, "gold_transactions.json");
            
            LoadTransactionsFromFile();
        }

        /// <summary>
        /// 取引を記録
        /// </summary>
        /// <param name="amount">取引量（負の値は消費）</param>
        /// <param name="reason">理由</param>
        /// <param name="balanceAfter">取引後残高</param>
        public void RecordTransaction(int amount, string reason, int balanceAfter)
        {
            var transaction = new GoldTransaction
            {
                id = Guid.NewGuid().ToString(),
                amount = amount,
                reason = reason,
                balanceAfter = balanceAfter,
                timestamp = DateTime.Now
            };

            _transactions.Add(transaction);

            // 最大サイズを超えた場合、古い取引を削除
            if (_transactions.Count > _maxHistorySize)
            {
                _transactions.RemoveAt(0);
            }

            // 分析キャッシュを無効化
            _cachedAnalytics = null;

            // 定期的にファイルに保存
            if (_transactions.Count % 10 == 0)
            {
                SaveTransactionsToFile();
            }
        }

        /// <summary>
        /// 最新の取引を取得
        /// </summary>
        /// <param name="count">取得数</param>
        /// <returns>最新の取引リスト</returns>
        public List<GoldTransaction> GetRecentTransactions(int count = 10)
        {
            return _transactions.TakeLast(count).ToList();
        }

        /// <summary>
        /// 期間内の取引を取得
        /// </summary>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>期間内の取引リスト</returns>
        public List<GoldTransaction> GetTransactionsInRange(DateTime startTime, DateTime endTime)
        {
            return _transactions.Where(t => t.timestamp >= startTime && t.timestamp <= endTime).ToList();
        }

        /// <summary>
        /// 理由別取引を取得
        /// </summary>
        /// <param name="reason">理由</param>
        /// <returns>該当する取引リスト</returns>
        public List<GoldTransaction> GetTransactionsByReason(string reason)
        {
            return _transactions.Where(t => t.reason.Contains(reason)).ToList();
        }

        /// <summary>
        /// 取引履歴をクリア
        /// </summary>
        public void Clear()
        {
            _transactions.Clear();
            _cachedAnalytics = null;
            SaveTransactionsToFile();
        }

        /// <summary>
        /// 取引履歴を復元
        /// </summary>
        /// <param name="transactions">復元する取引リスト</param>
        public void RestoreTransactions(List<GoldTransaction> transactions)
        {
            _transactions = transactions ?? new List<GoldTransaction>();
            _cachedAnalytics = null;
        }

        /// <summary>
        /// リアルタイム分析情報を取得
        /// </summary>
        /// <returns>分析情報</returns>
        public GoldAnalytics GetAnalytics()
        {
            // キャッシュが有効かチェック
            if (_cachedAnalytics != null && 
                DateTime.Now - _lastAnalyticsUpdate < TimeSpan.FromMinutes(5))
            {
                return _cachedAnalytics;
            }

            // 分析を実行
            _cachedAnalytics = CalculateAnalytics();
            _lastAnalyticsUpdate = DateTime.Now;

            return _cachedAnalytics;
        }

        /// <summary>
        /// 分析情報を計算
        /// </summary>
        /// <returns>分析情報</returns>
        private GoldAnalytics CalculateAnalytics()
        {
            if (_transactions.Count == 0)
            {
                return new GoldAnalytics();
            }

            var analytics = new GoldAnalytics
            {
                totalTransactions = _transactions.Count,
                totalEarned = _transactions.Where(t => t.amount > 0).Sum(t => t.amount),
                totalSpent = _transactions.Where(t => t.amount < 0).Sum(t => Math.Abs(t.amount)),
                averageTransactionAmount = _transactions.Average(t => Math.Abs(t.amount)),
                largestEarning = _transactions.Where(t => t.amount > 0).DefaultIfEmpty().Max(t => t?.amount ?? 0),
                largestSpending = _transactions.Where(t => t.amount < 0).DefaultIfEmpty().Min(t => t?.amount ?? 0),
                mostCommonReason = GetMostCommonReason(),
                transactionsByHour = CalculateTransactionsByHour(),
                earningsToSpendingRatio = CalculateEarningsToSpendingRatio()
            };

            return analytics;
        }

        /// <summary>
        /// 最も多い取引理由を取得
        /// </summary>
        /// <returns>最も多い取引理由</returns>
        private string GetMostCommonReason()
        {
            return _transactions
                .GroupBy(t => t.reason)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "";
        }

        /// <summary>
        /// 時間帯別取引数を計算
        /// </summary>
        /// <returns>時間帯別取引数</returns>
        private Dictionary<int, int> CalculateTransactionsByHour()
        {
            var hourlyTransactions = new Dictionary<int, int>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyTransactions[hour] = 0;
            }

            foreach (var transaction in _transactions)
            {
                hourlyTransactions[transaction.timestamp.Hour]++;
            }

            return hourlyTransactions;
        }

        /// <summary>
        /// 収入と支出の比率を計算
        /// </summary>
        /// <returns>収入と支出の比率</returns>
        private float CalculateEarningsToSpendingRatio()
        {
            var totalEarned = _transactions.Where(t => t.amount > 0).Sum(t => t.amount);
            var totalSpent = _transactions.Where(t => t.amount < 0).Sum(t => Math.Abs(t.amount));

            if (totalSpent == 0) return float.PositiveInfinity;
            return (float)totalEarned / totalSpent;
        }

        /// <summary>
        /// 取引履歴をファイルに保存
        /// </summary>
        private void SaveTransactionsToFile()
        {
            try
            {
                var saveData = new TransactionSaveData
                {
                    transactions = _transactions.TakeLast(500).ToList() // 最新500件のみ保存
                };

                string jsonData = JsonUtility.ToJson(saveData);
                File.WriteAllText(_persistentFilePath, jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"取引履歴の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイルから取引履歴を読み込み
        /// </summary>
        private void LoadTransactionsFromFile()
        {
            try
            {
                if (File.Exists(_persistentFilePath))
                {
                    string jsonData = File.ReadAllText(_persistentFilePath);
                    var saveData = JsonUtility.FromJson<TransactionSaveData>(jsonData);
                    
                    _transactions = saveData.transactions ?? new List<GoldTransaction>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"取引履歴の読み込みに失敗しました: {ex.Message}");
                _transactions = new List<GoldTransaction>();
            }
        }

        /// <summary>
        /// 統計情報を文字列で取得
        /// </summary>
        /// <returns>統計情報文字列</returns>
        public string GetStatisticsString()
        {
            var analytics = GetAnalytics();
            
            return $"取引履歴統計:\n" +
                   $"- 総取引数: {analytics.totalTransactions:N0}\n" +
                   $"- 総収入: {analytics.totalEarned:N0}\n" +
                   $"- 総支出: {analytics.totalSpent:N0}\n" +
                   $"- 平均取引額: {analytics.averageTransactionAmount:F1}\n" +
                   $"- 最大収入: {analytics.largestEarning:N0}\n" +
                   $"- 最大支出: {Math.Abs(analytics.largestSpending):N0}\n" +
                   $"- 最多取引理由: {analytics.mostCommonReason}\n" +
                   $"- 収支比率: {analytics.earningsToSpendingRatio:F2}";
        }
    }

    /// <summary>
    /// ゴールド取引データ
    /// </summary>
    [Serializable]
    public class GoldTransaction
    {
        public string id;
        public int amount;
        public string reason;
        public int balanceAfter;
        public DateTime timestamp;
    }

    /// <summary>
    /// ゴールド分析データ
    /// </summary>
    [Serializable]
    public class GoldAnalytics
    {
        public int totalTransactions;
        public int totalEarned;
        public int totalSpent;
        public double averageTransactionAmount;
        public int largestEarning;
        public int largestSpending;
        public string mostCommonReason;
        public Dictionary<int, int> transactionsByHour;
        public float earningsToSpendingRatio;
    }

    /// <summary>
    /// 取引履歴保存データ
    /// </summary>
    [Serializable]
    public class TransactionSaveData
    {
        public List<GoldTransaction> transactions;
    }
}