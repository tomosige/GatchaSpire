using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Gold;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Gacha;
using System.Linq;

namespace GatchaSpire.Core.Tests
{
    /// <summary>
    /// コアシステム統合テストクラス
    /// ゴールド、キャラクター、ガチャシステムの連携動作を検証
    /// </summary>
    public class CoreSystemIntegrationTest : GameSystemBase
    {
        [Header("統合テスト設定")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool showDetailedLogs = true;
        [SerializeField] private int testGoldAmount = 50000;
        [SerializeField] private int bulkTestIterations = 1000;

        protected override string SystemName => "CoreSystemIntegrationTest";

        private List<string> testResults = new List<string>();
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private GachaSystemManager gachaManager;

        // テスト結果統計
        private IntegrationTestStatistics testStats = new IntegrationTestStatistics();

        private void Awake()
        {
            OnAwake();
        }

        protected override void OnSystemInitialize()
        {
            testResults = new List<string>();
            priority = SystemPriority.Lowest;
        }

        protected override void OnSystemStart()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllIntegrationTests());
            }
        }

        /// <summary>
        /// 全統合テストを実行
        /// </summary>
        public IEnumerator RunAllIntegrationTests()
        {
            ReportInfo("=== コアシステム統合テスト開始 ===");
            testResults.Clear();
            testStats.Reset();

            yield return new WaitForSeconds(2f); // システム初期化完了を待つ

            // システム参照の取得
            InitializeSystemReferences();

            if (!ValidateSystemAvailability())
            {
                ReportError("必要なシステムが見つかりません。テストを中止します。");
                yield break;
            }

            // システム間連携テスト
            yield return StartCoroutine(TestSystemIntegration());
            yield return new WaitForSeconds(0.5f);

            // パフォーマンステスト
            yield return StartCoroutine(TestPerformance());
            yield return new WaitForSeconds(0.5f);

            // エラーシナリオテスト
            yield return StartCoroutine(TestErrorScenarios());
            yield return new WaitForSeconds(0.5f);

            // 結果表示
            ShowTestResults();
            ReportInfo("=== コアシステム統合テスト完了 ===");
        }

        /// <summary>
        /// システム参照の初期化
        /// </summary>
        private void InitializeSystemReferences()
        {
            goldManager = GoldManager.Instance;
            characterDatabase = CharacterDatabase.Instance;
            gachaManager = GachaSystemManager.Instance;

            AddTestResult($"✓ システム参照初期化完了");
            AddTestResult($"  - GoldManager: {(goldManager != null ? "OK" : "NG")}");
            AddTestResult($"  - CharacterDatabase: {(characterDatabase != null ? "OK" : "NG")}");
            AddTestResult($"  - GachaSystemManager: {(gachaManager != null ? "OK" : "NG")}");
        }

        /// <summary>
        /// システムの利用可能性を検証
        /// </summary>
        private bool ValidateSystemAvailability()
        {
            if (goldManager == null || !goldManager.IsInitialized())
            {
                ReportError("GoldManagerが利用できません");
                return false;
            }

            if (characterDatabase == null || !characterDatabase.IsInitialized())
            {
                ReportError("CharacterDatabaseが利用できません");
                return false;
            }

            if (gachaManager == null || !gachaManager.IsInitialized())
            {
                ReportError("GachaSystemManagerが利用できません");
                return false;
            }

            AddTestResult("✓ 全システムが利用可能");
            return true;
        }

        /// <summary>
        /// システム間連携テスト
        /// </summary>
        private IEnumerator TestSystemIntegration()
        {
            ReportInfo("=== システム間連携テスト ===");

            // テスト用ゴールドを準備
            goldManager.AddGold(testGoldAmount, "統合テスト用");
            int initialGold = goldManager.CurrentGold;
            AddTestResult($"✓ テスト用ゴールド準備: {initialGold}");

            yield return new WaitForSeconds(0.1f);

            // ゴールド → ガチャ → キャラクター獲得の流れをテスト
            yield return StartCoroutine(TestGoldToGachaFlow());
            yield return new WaitForSeconds(0.2f);

            // ガチャアップグレード機能のテスト
            yield return StartCoroutine(TestGachaUpgradeFlow());
            yield return new WaitForSeconds(0.2f);

            // システム状態の一貫性テスト
            yield return StartCoroutine(TestSystemConsistency());

            ReportInfo("システム間連携テスト完了");
        }

        /// <summary>
        /// ゴールド→ガチャ→キャラクター獲得フローのテスト
        /// </summary>
        private IEnumerator TestGoldToGachaFlow()
        {
            AddTestResult("--- ゴールド→ガチャ→キャラクター獲得フロー ---");

            // 初期状態の記録
            int goldBefore = goldManager.CurrentGold;
            int gachaCost = gachaManager.GetGachaCost();

            if (goldBefore < gachaCost)
            {
                ReportError($"テストに十分なゴールドがありません (必要: {gachaCost}, 現在: {goldBefore})");
                yield break;
            }

            // ガチャ実行
            var characters = gachaManager.PullGacha(1);
            int goldAfter = goldManager.CurrentGold;

            // 結果検証
            if (characters.Count > 0)
            {
                AddTestResult($"✓ ガチャ実行成功: {characters.Count}体獲得");
                testStats.successfulGachaPulls++;

                foreach (var character in characters)
                {
                    AddTestResult($"  - {character.CharacterName} (レアリティ: {character.Rarity})");
                }
            }
            else
            {
                AddTestResult("✗ ガチャ実行失敗: キャラクターが獲得できませんでした");
                testStats.failedGachaPulls++;
            }

            // ゴールド消費の確認
            int expectedGoldAfter = goldBefore - gachaCost;
            if (goldAfter == expectedGoldAfter)
            {
                AddTestResult($"✓ ゴールド消費正常: {gachaCost} 消費");
            }
            else
            {
                AddTestResult($"✗ ゴールド消費異常: 期待値{expectedGoldAfter}, 実際{goldAfter}");
                testStats.goldConsistencyErrors++;
            }

            yield return null;
        }

        /// <summary>
        /// ガチャアップグレードフローのテスト
        /// </summary>
        private IEnumerator TestGachaUpgradeFlow()
        {
            AddTestResult("--- ガチャアップグレードフロー ---");

            int initialLevel = gachaManager.GetCurrentLevel();
            int upgradeCost = gachaManager.GetUpgradeCost();
            bool canUpgrade = gachaManager.CanUpgradeGacha();

            AddTestResult($"現在レベル: {initialLevel}, アップグレードコスト: {upgradeCost}");

            if (canUpgrade && goldManager.CurrentGold >= upgradeCost)
            {
                int goldBefore = goldManager.CurrentGold;
                bool upgraded = gachaManager.UpgradeGacha();
                int goldAfter = goldManager.CurrentGold;
                int newLevel = gachaManager.GetCurrentLevel();

                if (upgraded && newLevel == initialLevel + 1)
                {
                    AddTestResult($"✓ アップグレード成功: レベル{initialLevel} → {newLevel}");
                    testStats.successfulUpgrades++;

                    // ゴールド消費確認
                    if (goldAfter == goldBefore - upgradeCost)
                    {
                        AddTestResult($"✓ アップグレードコスト正常: {upgradeCost} 消費");
                    }
                    else
                    {
                        AddTestResult($"✗ アップグレードコスト異常: 期待値{goldBefore - upgradeCost}, 実際{goldAfter}");
                        testStats.goldConsistencyErrors++;
                    }
                }
                else
                {
                    AddTestResult("✗ アップグレード失敗");
                    testStats.failedUpgrades++;
                }
            }
            else
            {
                AddTestResult("⚠ アップグレード条件未満（ゴールド不足またはレベル上限）");
            }

            yield return null;
        }

        /// <summary>
        /// システム状態の一貫性テスト
        /// </summary>
        private IEnumerator TestSystemConsistency()
        {
            AddTestResult("--- システム状態一貫性 ---");

            // 各システムの健康状態チェック
            bool goldHealthy = goldManager.IsSystemHealthy();
            bool gachaHealthy = gachaManager.IsSystemHealthy();
            bool dbHealthy = characterDatabase.IsInitialized();

            AddTestResult($"✓ システム健康状態");
            AddTestResult($"  - GoldManager: {(goldHealthy ? "健康" : "異常")}");
            AddTestResult($"  - GachaSystemManager: {(gachaHealthy ? "健康" : "異常")}");
            AddTestResult($"  - CharacterDatabase: {(dbHealthy ? "健康" : "異常")}");

            if (goldHealthy && gachaHealthy && dbHealthy)
            {
                testStats.systemConsistencyChecks++;
            }

            // 取引履歴の整合性チェック
            var goldHistory = goldManager.TransactionHistory;
            if (goldHistory != null && goldHistory.TransactionCount > 0)
            {
                var analytics = goldHistory.GetAnalytics();
                AddTestResult($"✓ 取引履歴整合性: {analytics.totalTransactions}件の取引記録");
            }

            yield return null;
        }

        /// <summary>
        /// パフォーマンステスト
        /// </summary>
        private IEnumerator TestPerformance()
        {
            ReportInfo("=== パフォーマンステスト ===");

            yield return StartCoroutine(TestBulkOperations());
            yield return StartCoroutine(TestMemoryUsage());
            yield return StartCoroutine(TestFramerateImpact());

            ReportInfo("パフォーマンステスト完了");
        }

        /// <summary>
        /// 大量操作時の安定性テスト
        /// </summary>
        private IEnumerator TestBulkOperations()
        {
            AddTestResult("--- 大量操作安定性テスト ---");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int successCount = 0;
            int errorCount = 0;

            // 十分なゴールドを用意
            goldManager.AddGold(bulkTestIterations * 1000, "パフォーマンステスト用");

            for (int i = 0; i < bulkTestIterations; i++)
            {

                if (gachaManager.CanPullGacha())
                {
                    var result = gachaManager.PullGacha(1);
                    if (result.Count > 0)
                    {
                        successCount++;
                    }
                }

                // 100回ごとにフレーム分散
                if (i % 100 == 0)
                {
                    yield return null;
                }

            }

            stopwatch.Stop();
            float averageTime = stopwatch.ElapsedMilliseconds / (float)bulkTestIterations;

            AddTestResult($"✓ 大量操作テスト結果:");
            AddTestResult($"  - 成功: {successCount}/{bulkTestIterations}");
            AddTestResult($"  - エラー: {errorCount}件");
            AddTestResult($"  - 平均処理時間: {averageTime:F3}ms/回");
            AddTestResult($"  - 総処理時間: {stopwatch.ElapsedMilliseconds}ms");

            testStats.bulkOperationTime = stopwatch.ElapsedMilliseconds;
            testStats.bulkOperationErrors = errorCount;

            if (averageTime > 5.0f)
            {
                ReportWarning($"大量操作の処理時間が長いです: {averageTime:F3}ms/回");
            }
        }

        /// <summary>
        /// メモリ使用量測定テスト
        /// </summary>
        private IEnumerator TestMemoryUsage()
        {
            AddTestResult("--- メモリ使用量測定 ---");

            // GCを実行してベースライン取得
            System.GC.Collect();
            yield return new WaitForSeconds(0.1f);

            long memoryBefore = System.GC.GetTotalMemory(false);

            // 一定数のガチャを実行
            for (int i = 0; i < 100; i++)
            {
                if (gachaManager.CanPullGacha())
                {
                    gachaManager.PullGacha(1);
                }

                if (i % 10 == 0)
                {
                    yield return null;
                }
            }

            long memoryAfter = System.GC.GetTotalMemory(false);
            long memoryDelta = memoryAfter - memoryBefore;

            AddTestResult($"✓ メモリ使用量測定:");
            AddTestResult($"  - 実行前: {memoryBefore / 1024 / 1024:F2} MB");
            AddTestResult($"  - 実行後: {memoryAfter / 1024 / 1024:F2} MB");
            AddTestResult($"  - 増加量: {memoryDelta / 1024 / 1024:F2} MB");

            testStats.memoryDelta = memoryDelta;

            if (memoryDelta > 10 * 1024 * 1024) // 10MB以上の増加
            {
                ReportWarning($"メモリ使用量の増加が大きいです: {memoryDelta / 1024 / 1024:F2} MB");
            }
        }

        /// <summary>
        /// フレームレート影響測定テスト
        /// </summary>
        private IEnumerator TestFramerateImpact()
        {
            AddTestResult("--- フレームレート影響測定 ---");

            float baselineFramerate = 1.0f / Time.unscaledDeltaTime;
            List<float> frameratesSamples = new List<float>();

            // 負荷テスト実行中のフレームレート測定
            for (int i = 0; i < 60; i++) // 1秒間測定（60フレーム）
            {
                if (gachaManager.CanPullGacha())
                {
                    gachaManager.PullGacha(1);
                }

                frameratesSamples.Add(1.0f / Time.unscaledDeltaTime);
                yield return null;
            }

            float averageFramerate = frameratesSamples.Count > 0 ?
                frameratesSamples.Average() : 0f;
            float minFramerate = frameratesSamples.Count > 0 ?
                frameratesSamples.Min() : 0f;

            AddTestResult($"✓ フレームレート測定:");
            AddTestResult($"  - ベースライン: {baselineFramerate:F1} FPS");
            AddTestResult($"  - 負荷時平均: {averageFramerate:F1} FPS");
            AddTestResult($"  - 負荷時最低: {minFramerate:F1} FPS");

            testStats.framerateImpact = baselineFramerate - averageFramerate;

            if (averageFramerate < baselineFramerate * 0.9f) // 10%以上の低下
            {
                ReportWarning($"フレームレートの低下が大きいです: {baselineFramerate - averageFramerate:F1} FPS低下");
            }
        }

        /// <summary>
        /// エラーシナリオテスト
        /// </summary>
        private IEnumerator TestErrorScenarios()
        {
            ReportInfo("=== エラーシナリオテスト ===");

            yield return StartCoroutine(TestSystemErrorHandling());
            yield return StartCoroutine(TestRecoveryProcesses());
            yield return StartCoroutine(TestDataConsistency());

            ReportInfo("エラーシナリオテスト完了");
        }

        /// <summary>
        /// システムエラーハンドリングテスト
        /// </summary>
        private IEnumerator TestSystemErrorHandling()
        {
            AddTestResult("--- システムエラーハンドリング ---");

            // ゴールド不足でのガチャ実行テスト
            int originalGold = goldManager.CurrentGold;
            goldManager.SpendGold(originalGold, "エラーテスト用");

            var characters = gachaManager.PullGacha(1);
            if (characters.Count == 0)
            {
                AddTestResult("✓ ゴールド不足時の適切なエラーハンドリング");
                testStats.errorHandlingChecks++;
            }
            else
            {
                AddTestResult("✗ ゴールド不足時のエラーハンドリングが不適切");
            }

            // ゴールド復旧
            goldManager.AddGold(originalGold, "エラーテスト復旧");

            // 無効なパラメータでのガチャ実行テスト
            var invalidResult = gachaManager.PullGacha(-1);
            if (invalidResult.Count == 0)
            {
                AddTestResult("✓ 無効パラメータの適切なエラーハンドリング");
                testStats.errorHandlingChecks++;
            }

            yield return null;
        }

        /// <summary>
        /// 復旧処理テスト
        /// </summary>
        private IEnumerator TestRecoveryProcesses()
        {
            AddTestResult("--- 復旧処理テスト ---");

            // システムリセットテスト
            int levelBefore = gachaManager.GetCurrentLevel();
            gachaManager.ResetGachaSystem();
            int levelAfter = gachaManager.GetCurrentLevel();

            if (levelAfter == 1)
            {
                AddTestResult($"✓ ガチャシステムリセット正常: レベル{levelBefore} → {levelAfter}");
                testStats.recoveryProcessChecks++;
            }
            else
            {
                AddTestResult($"✗ ガチャシステムリセット異常: 期待値1, 実際{levelAfter}");
            }

            // ゴールドシステムリセットテスト
            int goldBefore = goldManager.CurrentGold;
            goldManager.ResetSystem();
            int goldAfter = goldManager.CurrentGold;

            if (goldAfter != goldBefore)
            {
                AddTestResult($"✓ ゴールドシステムリセット正常: {goldBefore} → {goldAfter}");
                testStats.recoveryProcessChecks++;
            }

            yield return null;
        }

        /// <summary>
        /// データ整合性テスト
        /// </summary>
        private IEnumerator TestDataConsistency()
        {
            AddTestResult("--- データ整合性テスト ---");

            // データベースの整合性チェック
            characterDatabase.ValidateAllData();
            AddTestResult("✓ キャラクターデータベース整合性チェック実行");

            // 取引履歴の整合性チェック
            var analytics = goldManager.TransactionHistory.GetAnalytics();
            if (analytics.totalTransactions > 0)
            {
                AddTestResult($"✓ 取引履歴整合性: {analytics.totalTransactions}件記録");
                testStats.dataConsistencyChecks++;
            }

            // ガチャ履歴の整合性チェック
            var gachaHistory = gachaManager.History;
            if (gachaHistory != null)
            {
                AddTestResult($"✓ ガチャ履歴整合性: {gachaHistory.TotalPulls}回記録");
                testStats.dataConsistencyChecks++;
            }

            yield return null;
        }

        /// <summary>
        /// テスト結果を表示
        /// </summary>
        private void ShowTestResults()
        {
            ReportInfo("=== 統合テスト結果サマリー ===");

            int successCount = 0;
            int warningCount = 0;
            int errorCount = 0;

            foreach (var result in testResults)
            {
                if (showDetailedLogs)
                {
                    ReportInfo(result);
                }

                if (result.StartsWith("✓")) successCount++;
                else if (result.StartsWith("⚠")) warningCount++;
                else if (result.StartsWith("✗")) errorCount++;
            }

            // 統計情報の表示
            ReportInfo("=== テスト統計 ===");
            ReportInfo($"成功: {successCount}, 警告: {warningCount}, エラー: {errorCount}");
            ReportInfo($"ガチャ実行成功: {testStats.successfulGachaPulls}回");
            ReportInfo($"アップグレード成功: {testStats.successfulUpgrades}回");
            ReportInfo($"大量操作処理時間: {testStats.bulkOperationTime}ms");
            ReportInfo($"メモリ増加量: {testStats.memoryDelta / 1024 / 1024:F2}MB");

            // 総合判定
            if (errorCount == 0)
            {
                ReportInfo("🎉 統合テスト成功 - 全システムが正常に連携しています");
            }
            else if (errorCount < 3)
            {
                ReportWarning("⚠️ 統合テスト部分成功 - 一部に問題がありますが動作可能です");
            }
            else
            {
                ReportError("❌ 統合テスト失敗 - 重大な問題があります");
            }
        }

        /// <summary>
        /// テスト結果をリストに追加
        /// </summary>
        private void AddTestResult(string result)
        {
            testResults.Add(result);
        }

        /// <summary>
        /// 手動でテストを実行
        /// </summary>
        [ContextMenu("Run Integration Tests")]
        public void RunIntegrationTestsManually()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunAllIntegrationTests());
            }
            else
            {
                ReportWarning("統合テストは実行時のみ動作します");
            }
        }

        /// <summary>
        /// テスト結果の取得
        /// </summary>
        public List<string> GetTestResults()
        {
            return new List<string>(testResults);
        }

        /// <summary>
        /// テスト統計の取得
        /// </summary>
        public IntegrationTestStatistics GetTestStatistics()
        {
            return testStats;
        }
    }

    /// <summary>
    /// 統合テスト統計情報
    /// </summary>
    [System.Serializable]
    public class IntegrationTestStatistics
    {
        public int successfulGachaPulls = 0;
        public int failedGachaPulls = 0;
        public int successfulUpgrades = 0;
        public int failedUpgrades = 0;
        public int goldConsistencyErrors = 0;
        public int systemConsistencyChecks = 0;
        public long bulkOperationTime = 0;
        public int bulkOperationErrors = 0;
        public long memoryDelta = 0;
        public float framerateImpact = 0f;
        public int errorHandlingChecks = 0;
        public int recoveryProcessChecks = 0;
        public int dataConsistencyChecks = 0;

        public void Reset()
        {
            successfulGachaPulls = 0;
            failedGachaPulls = 0;
            successfulUpgrades = 0;
            failedUpgrades = 0;
            goldConsistencyErrors = 0;
            systemConsistencyChecks = 0;
            bulkOperationTime = 0;
            bulkOperationErrors = 0;
            memoryDelta = 0;
            framerateImpact = 0f;
            errorHandlingChecks = 0;
            recoveryProcessChecks = 0;
            dataConsistencyChecks = 0;
        }
    }
}