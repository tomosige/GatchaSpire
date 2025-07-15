using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core.Gold
{
    /// <summary>
    /// ゴールドシステムのテストクラス
    /// </summary>
    public class GoldSystemTest : TestExclusiveBase
    {
        public override float MaxExecutionTimeSeconds => 120f; // 2分

        private List<string> testResults = new List<string>();
        private GoldManager goldManager;

        /// <summary>
        /// 全てのテストを実行
        /// </summary>
        /// <returns></returns>
        public override IEnumerator RunAllTests()
        {
            ReportInfo("ゴールドシステムテストを開始します");
            testResults.Clear();

            // GoldManagerの取得
            goldManager = GoldManager.Instance;
            if (goldManager == null)
            {
                ReportError("GoldManagerが見つかりません");
                yield break;
            }

            // 基本操作テスト
            yield return StartCoroutine(TestBasicOperations());
            yield return new WaitForSeconds(0.1f);

            // 計算機テスト
            yield return StartCoroutine(TestCalculator());
            yield return new WaitForSeconds(0.1f);

            // 取引履歴テスト
            yield return StartCoroutine(TestTransactionHistory());
            yield return new WaitForSeconds(0.1f);

            // エラー条件テスト
            yield return StartCoroutine(TestErrorConditions());
            yield return new WaitForSeconds(0.1f);

            // 永続化テスト
            yield return StartCoroutine(TestPersistence());
            yield return new WaitForSeconds(0.1f);

            // 統合テスト
            yield return StartCoroutine(TestIntegration());

            // 結果表示
            ShowTestResults();
        }

        /// <summary>
        /// 基本操作テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestBasicOperations()
        {
            ReportInfo("基本操作テストを開始");

            // 初期状態の確認
            int initialGold = goldManager.CurrentGold;
            testResults.Add($"✓ 初期ゴールド確認: {initialGold}");

            // ゴールド追加テスト
            int addedAmount = goldManager.AddGold(100, "テスト追加");
            if (addedAmount == 100 && goldManager.CurrentGold == initialGold + 100)
            {
                testResults.Add("✓ ゴールド追加: 正常動作");
            }
            else
            {
                testResults.Add($"✗ ゴールド追加: 失敗 (期待値: {initialGold + 100}, 実際: {goldManager.CurrentGold})");
            }

            yield return new WaitForSeconds(0.1f);

            // ゴールド消費テスト
            bool spendResult = goldManager.SpendGold(50, "テスト消費");
            if (spendResult && goldManager.CurrentGold == initialGold + 50)
            {
                testResults.Add("✓ ゴールド消費: 正常動作");
            }
            else
            {
                testResults.Add($"✗ ゴールド消費: 失敗 (期待値: {initialGold + 50}, 実際: {goldManager.CurrentGold})");
            }

            // 支払い可能性チェック
            bool canAfford = goldManager.CanAfford(goldManager.CurrentGold);
            bool cannotAfford = !goldManager.CanAfford(goldManager.CurrentGold + 1);
            
            if (canAfford && cannotAfford)
            {
                testResults.Add("✓ 支払い可能性チェック: 正常動作");
            }
            else
            {
                testResults.Add("✗ 支払い可能性チェック: 失敗");
            }

            yield return null;
        }

        /// <summary>
        /// 計算機テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestCalculator()
        {
            ReportInfo("計算機テストを開始");

            var calculator = goldManager.Calculator;
            if (calculator == null)
            {
                testResults.Add("✗ 計算機: インスタンスが見つかりません");
                yield break;
            }

            // 戦闘報酬計算テスト
            int combatReward = calculator.CalculateCombatReward(10, 5, 1.0f);
            if (combatReward > 0)
            {
                testResults.Add($"✓ 戦闘報酬計算: {combatReward}ゴールド");
            }
            else
            {
                testResults.Add("✗ 戦闘報酬計算: 失敗");
            }

            yield return new WaitForSeconds(0.05f);

            // ガチャコスト計算テスト
            int gachaCost = calculator.CalculateGachaCost(GachaType.Normal, 1);
            if (gachaCost > 0)
            {
                testResults.Add($"✓ ガチャコスト計算: {gachaCost}ゴールド");
            }
            else
            {
                testResults.Add("✗ ガチャコスト計算: 失敗");
            }

            yield return new WaitForSeconds(0.05f);

            // アップグレードコスト計算テスト
            int upgradeCost = calculator.CalculateUpgradeCost(1, 5, UpgradeType.Character);
            if (upgradeCost > 0)
            {
                testResults.Add($"✓ アップグレードコスト計算: {upgradeCost}ゴールド");
            }
            else
            {
                testResults.Add("✗ アップグレードコスト計算: 失敗");
            }

            yield return new WaitForSeconds(0.05f);

            // 売却価格計算テスト
            int sellPrice = calculator.CalculateSellPrice(ItemType.Character, 1, ItemRarity.Common);
            if (sellPrice > 0)
            {
                testResults.Add($"✓ 売却価格計算: {sellPrice}ゴールド");
            }
            else
            {
                testResults.Add("✗ 売却価格計算: 失敗");
            }

            yield return null;
        }

        /// <summary>
        /// 取引履歴テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestTransactionHistory()
        {
            ReportInfo("取引履歴テストを開始");

            var history = goldManager.TransactionHistory;
            if (history == null)
            {
                testResults.Add("✗ 取引履歴: インスタンスが見つかりません");
                yield break;
            }

            // 取引履歴の確認
            int initialCount = history.TransactionCount;
            
            // 新しい取引を追加
            goldManager.AddGold(10, "履歴テスト");
            
            if (history.TransactionCount == initialCount + 1)
            {
                testResults.Add("✓ 取引履歴記録: 正常動作");
            }
            else
            {
                testResults.Add($"✗ 取引履歴記録: 失敗 (期待値: {initialCount + 1}, 実際: {history.TransactionCount})");
            }

            yield return new WaitForSeconds(0.1f);

            // 最新取引の取得
            var recentTransactions = history.GetRecentTransactions(1);
            if (recentTransactions.Count > 0 && recentTransactions[0].reason == "履歴テスト")
            {
                testResults.Add("✓ 最新取引取得: 正常動作");
            }
            else
            {
                testResults.Add("✗ 最新取引取得: 失敗");
            }

            // 統計情報の取得
            var analytics = history.GetAnalytics();
            if (analytics.totalTransactions > 0)
            {
                testResults.Add($"✓ 統計情報: 取引数{analytics.totalTransactions}件");
            }
            else
            {
                testResults.Add("✗ 統計情報: 取得失敗");
            }

            yield return null;
        }

        /// <summary>
        /// エラー条件テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestErrorConditions()
        {
            ReportInfo("エラー条件テストを開始");

            // 負の値での追加テスト
            int invalidAdd = goldManager.AddGold(-10, "無効な追加");
            if (invalidAdd == 0)
            {
                testResults.Add("✓ 負の値追加エラー: 正常処理");
            }
            else
            {
                testResults.Add("✗ 負の値追加エラー: 異常処理");
            }

            yield return new WaitForSeconds(0.05f);

            // 不足金額での消費テスト
            int currentGold = goldManager.CurrentGold;
            bool invalidSpend = goldManager.SpendGold(currentGold + 1000, "不足消費");
            if (!invalidSpend)
            {
                testResults.Add("✓ 不足金額消費エラー: 正常処理");
            }
            else
            {
                testResults.Add("✗ 不足金額消費エラー: 異常処理");
            }

            yield return new WaitForSeconds(0.05f);

            // 負の値での消費テスト
            bool negativeSpend = goldManager.SpendGold(-10, "負の値消費");
            if (!negativeSpend)
            {
                testResults.Add("✓ 負の値消費エラー: 正常処理");
            }
            else
            {
                testResults.Add("✗ 負の値消費エラー: 異常処理");
            }

            yield return null;
        }

        /// <summary>
        /// 永続化テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestPersistence()
        {
            ReportInfo("永続化テストを開始");

            // 現在の状態を保存
            int currentGold = goldManager.CurrentGold;
            goldManager.SaveData();

            // 少し待つ
            yield return new WaitForSeconds(0.1f);

            // 手動で値を変更
            goldManager.AddGold(999, "永続化テスト");
            int modifiedGold = goldManager.CurrentGold;

            // データを再読み込み
            goldManager.LoadData();
            
            if (goldManager.CurrentGold == currentGold)
            {
                testResults.Add("✓ 永続化: 正常動作");
            }
            else
            {
                testResults.Add($"✗ 永続化: 失敗 (期待値: {currentGold}, 実際: {goldManager.CurrentGold})");
            }

            yield return null;
        }

        /// <summary>
        /// 統合テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestIntegration()
        {
            ReportInfo("統合テストを開始");

            // 開発設定の確認
            var devSettings = Resources.Load<DevelopmentSettings>("DevelopmentSettings");
            if (devSettings != null)
            {
                testResults.Add($"✓ 開発設定連携: 倍率{devSettings.GlobalGoldMultiplier}x");
            }
            else
            {
                testResults.Add("⚠ 開発設定連携: 設定ファイルが見つかりません");
            }

            yield return new WaitForSeconds(0.1f);

            // システムリセットテスト
            int beforeReset = goldManager.CurrentGold;
            goldManager.ResetSystem();
            
            if (goldManager.CurrentGold != beforeReset)
            {
                testResults.Add("✓ システムリセット: 正常動作");
            }
            else
            {
                testResults.Add("✗ システムリセット: 失敗");
            }

            yield return null;
        }

        /// <summary>
        /// テスト結果を表示
        /// </summary>
        private void ShowTestResults()
        {
            ReportInfo("=== ゴールドシステムテスト結果 ===");
            
            int successCount = 0;
            int warningCount = 0;
            
            foreach (var result in testResults)
            {
                if (showDetailedLogs)
                {
                    ReportInfo(result);
                }
                
                if (result.StartsWith("✓")) successCount++;
                else if (result.StartsWith("⚠")) warningCount++;
            }

            string summary = $"テスト完了: {successCount}成功, {testResults.Count - successCount - warningCount}失敗, {warningCount}警告";
            
            if (successCount == testResults.Count - warningCount)
            {
                ReportInfo($"🎉 {summary} - ゴールドシステムが正常に動作しています");
            }
            else
            {
                ReportWarning($"⚠️ {summary} - 一部に問題があります");
            }

            // デバッグ情報の表示
            if (showDetailedLogs && goldManager != null)
            {
                ReportInfo("=== デバッグ情報 ===");
                ReportInfo(goldManager.GetDebugInfo());
                ReportInfo(goldManager.TransactionHistory.GetStatisticsString());
            }
        }


        /// <summary>
        /// パフォーマンステストを実行
        /// </summary>
        [ContextMenu("Run Performance Test")]
        public void RunPerformanceTest()
        {
            if (!Application.isPlaying)
            {
                ReportWarning("パフォーマンステストは実行時のみ動作します");
                return;
            }

            StartCoroutine(RunPerformanceTestCoroutine());
        }

        /// <summary>
        /// パフォーマンステストのコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerator RunPerformanceTestCoroutine()
        {
            ReportInfo("パフォーマンステストを開始");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 大量の取引を実行
            for (int i = 0; i < 1000; i++)
            {
                goldManager.AddGold(1, $"パフォーマンステスト{i}");
                
                if (i % 100 == 0)
                {
                    yield return null; // フレーム分散
                }
            }

            stopwatch.Stop();
            ReportInfo($"パフォーマンステスト完了: 1000回取引を{stopwatch.ElapsedMilliseconds}msで実行");

            // 分析データの取得時間測定
            stopwatch.Restart();
            var analytics = goldManager.TransactionHistory.GetAnalytics();
            stopwatch.Stop();
            
            ReportInfo($"分析データ取得: {stopwatch.ElapsedMilliseconds}ms (取引数: {analytics.totalTransactions})");
        }
    }
}