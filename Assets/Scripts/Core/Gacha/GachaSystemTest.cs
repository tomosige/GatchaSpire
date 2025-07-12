using System.Collections.Generic;
using UnityEngine;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Gold;
using GatchaSpire.Core.Systems;
using System.Collections;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャシステムのテストクラス
    /// </summary>
    public class GachaSystemTest : TestExclusiveBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool createTestGachaData = true;
        [SerializeField] private int testGoldAmount = 10000;

        public override float MaxExecutionTimeSeconds => 180f; // 3分

        private List<string> testResults = new List<string>();
        private GachaSystemManager gachaManager;
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private GachaSystemData generatedTestData;

        /// <summary>
        /// 全テストを実行
        /// </summary>
        public override IEnumerator RunAllTests()
        {
            yield return new WaitForSeconds(1f); // システム初期化を待つ
            
            InitializeTestEnvironment();
            
            if (ValidateTestPreconditions())
            {
                TestBasicGachaFunctionality();
                yield return new WaitForSeconds(0.5f);
                
                TestGachaUpgradeFunctionality();
                yield return new WaitForSeconds(0.5f);
                
                TestErrorHandling();
                yield return new WaitForSeconds(0.5f);
                
                TestPerformance();
                yield return new WaitForSeconds(0.5f);
                
                TestIntegration();
                
                ReportInfo("全テストが完了しました");
            }
            else
            {
                ReportError("テストの前提条件が満たされていません");
            }
        }

        /// <summary>
        /// テスト環境の初期化
        /// </summary>
        private void InitializeTestEnvironment()
        {
            gachaManager = FindObjectOfType<GachaSystemManager>();
            goldManager = FindObjectOfType<GoldManager>();
            characterDatabase = FindObjectOfType<CharacterDatabase>();

            // テスト用ガチャデータの生成
            if (createTestGachaData && generatedTestData == null)
            {
                CreateTestGachaData();
            }

            // テスト用のゴールドを付与
            if (goldManager != null)
            {
                goldManager.AddGold(testGoldAmount, "テスト用ゴールド");
                AddTestResult($"テスト用ゴールドを付与: {testGoldAmount}");
            }
        }

        /// <summary>
        /// テスト前提条件の検証
        /// </summary>
        private bool ValidateTestPreconditions()
        {
            if (gachaManager == null)
            {
                ReportError("GachaSystemManagerが見つかりません");
                return false;
            }

            if (goldManager == null)
            {
                ReportError("GoldManagerが見つかりません");
                return false;
            }

            if (characterDatabase == null)
            {
                ReportError("CharacterDatabaseが見つかりません");
                return false;
            }

            if (!gachaManager.IsInitialized())
            {
                ReportError("GachaSystemManagerが初期化されていません");
                return false;
            }

            LogTest("テスト前提条件をクリアしました");
            return true;
        }

        /// <summary>
        /// 基本ガチャ機能のテスト
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestBasicGachaFunctionality()
        {
            ReportInfo("=== 基本ガチャ機能テスト ===");
            
            // 1. ガチャコスト取得テスト
            int cost = gachaManager.GetGachaCost();
            if (cost <= 0)
            {
                ReportError("ガチャコストが正の値である必要があります");
                return;
            }
            LogTest($"ガチャコスト: {cost}");
            
            // 2. ガチャ実行可能判定テスト
            bool canPull = gachaManager.CanPullGacha();
            if (!canPull)
            {
                ReportError("十分なゴールドがあればガチャを実行できる必要があります");
                return;
            }
            LogTest($"ガチャ実行可能: {canPull}");
            
            // 3. 単発ガチャテスト
            long goldBefore = goldManager.CurrentGold;
            var characters = gachaManager.PullGacha(1);
            long goldAfter = goldManager.CurrentGold;
            
            if (characters.Count == 0)
            {
                ReportError("ガチャ結果でキャラクターが取得される必要があります");
                return;
            }
            if (goldAfter != goldBefore - cost)
            {
                ReportError($"ゴールドが正しく消費される必要があります (期待値: {goldBefore - cost}, 実際: {goldAfter})");
                return;
            }
            LogTest($"単発ガチャ結果: {characters.Count}体取得");
            
            // 4. 10連ガチャテスト
            goldBefore = goldManager.CurrentGold;
            characters = gachaManager.PullGacha(10);
            goldAfter = goldManager.CurrentGold;
            
            if (characters.Count < 10)
            {
                ReportError($"10連ガチャで10体以上取得される必要があります (実際: {characters.Count}体)");
                return;
            }
            if (goldAfter != goldBefore - (cost * 10))
            {
                ReportError($"10連分のゴールドが消費される必要があります (期待値: {goldBefore - (cost * 10)}, 実際: {goldAfter})");
                return;
            }
            LogTest($"10連ガチャ結果: {characters.Count}体取得");
            
            // 5. ガチャ結果履歴テスト
            var lastResult = gachaManager.GetLastPullResult();
            if (lastResult == null)
            {
                ReportError("ガチャ結果が記録される必要があります");
                return;
            }
            if (!lastResult.IsSuccess)
            {
                ReportError("ガチャ結果が成功である必要があります");
                return;
            }
            LogTest($"ガチャ結果履歴: {lastResult.GetSummary()}");
            
            ReportInfo("基本ガチャ機能テストが完了しました");
        }

        /// <summary>
        /// ガチャアップグレード機能のテスト
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestGachaUpgradeFunctionality()
        {
            ReportInfo("=== ガチャアップグレード機能テスト ===");
            
            // 1. 初期レベル確認
            int initialLevel = gachaManager.GetCurrentLevel();
            if (initialLevel < 1)
            {
                ReportError($"初期レベルは1以上である必要があります (実際: {initialLevel})");
                return;
            }
            LogTest($"初期レベル: {initialLevel}");
            
            // 2. アップグレードコスト取得
            int upgradeCost = gachaManager.GetUpgradeCost();
            LogTest($"アップグレードコスト: {upgradeCost}");
            
            // 3. アップグレード可能判定
            bool canUpgrade = gachaManager.CanUpgradeGacha();
            LogTest($"アップグレード可能: {canUpgrade}");
            
            // 4. アップグレードプレビュー取得
            var preview = gachaManager.GetUpgradePreview();
            if (preview != null)
            {
                if (preview.CurrentLevel != initialLevel)
                {
                    ReportError($"プレビューの現在レベルが正しい必要があります (期待値: {initialLevel}, 実際: {preview.CurrentLevel})");
                    return;
                }
                if (preview.NextLevel != initialLevel + 1)
                {
                    ReportError($"プレビューの次レベルが正しい必要があります (期待値: {initialLevel + 1}, 実際: {preview.NextLevel})");
                    return;
                }
                LogTest($"アップグレードプレビュー: {preview.GetSummary()}");
            }
            
            // 5. アップグレード実行（可能な場合）
            if (canUpgrade)
            {
                long goldBefore = goldManager.CurrentGold;
                bool upgraded = gachaManager.UpgradeGacha();
                long goldAfter = goldManager.CurrentGold;
                
                if (!upgraded)
                {
                    ReportError("アップグレードが成功する必要があります");
                    return;
                }
                if (goldAfter != goldBefore - upgradeCost)
                {
                    ReportError($"アップグレードコストが正しく消費される必要があります (期待値: {goldBefore - upgradeCost}, 実際: {goldAfter})");
                    return;
                }
                if (gachaManager.GetCurrentLevel() != initialLevel + 1)
                {
                    ReportError($"レベルが正しく上がる必要があります (期待値: {initialLevel + 1}, 実際: {gachaManager.GetCurrentLevel()})");
                    return;
                }
                LogTest($"アップグレード成功: レベル{initialLevel} → {gachaManager.GetCurrentLevel()}");
            }
            
            ReportInfo("ガチャアップグレード機能テストが完了しました");
        }

        /// <summary>
        /// エラーハンドリングのテスト
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestErrorHandling()
        {
            ReportInfo("=== エラーハンドリングテスト ===");
            
            // 1. ゴールド不足時のガチャテスト
            int originalGold = goldManager.CurrentGold;
            goldManager.SpendGold(originalGold); // 全ゴールドを消費
            
            bool canPullWithoutGold = gachaManager.CanPullGacha();
            if (canPullWithoutGold)
            {
                ReportError("ゴールドがない場合はガチャ実行不可である必要があります");
                return;
            }
            
            var charactersWithoutGold = gachaManager.PullGacha(1);
            if (charactersWithoutGold.Count != 0)
            {
                ReportError($"ゴールドがない場合はキャラクターが取得されない必要があります (実際: {charactersWithoutGold.Count}体)");
                return;
            }
            
            var lastResult = gachaManager.GetLastPullResult();
            if (lastResult == null || lastResult.Status != GachaStatus.InsufficientGold)
            {
                ReportError($"ゴールド不足の結果が記録される必要があります (実際ステータス: {lastResult?.Status})");
                return;
            }
            
            LogTest("ゴールド不足時のエラーハンドリングテスト完了");
            
            // 2. ゴールドを復旧
            goldManager.AddGold(originalGold);
            
            // 3. 無効なパラメータテスト
            var invalidCharacters = gachaManager.PullGacha(0);
            if (invalidCharacters.Count != 0)
            {
                ReportError($"無効なパラメータでは結果が返されない必要があります (実際: {invalidCharacters.Count}体)");
                return;
            }
            
            LogTest("無効パラメータテスト完了");
            
            ReportInfo("エラーハンドリングテストが完了しました");
        }

        /// <summary>
        /// パフォーマンステスト
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestPerformance()
        {
            ReportInfo("=== パフォーマンステスト ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 1000回の単発ガチャ実行
            for (int i = 0; i < 1000; i++)
            {
                if (gachaManager.CanPullGacha())
                {
                    gachaManager.PullGacha(1);
                }
            }
            
            stopwatch.Stop();
            float averageTime = stopwatch.ElapsedMilliseconds / 1000.0f;
            
            LogTest($"1000回ガチャ実行時間: {stopwatch.ElapsedMilliseconds}ms");
            LogTest($"平均実行時間: {averageTime:F3}ms/回");
            
            if (averageTime > 10.0f) // 10ms以上は警告
            {
                ReportWarning(
                    $"ガチャ実行が遅い可能性があります: {averageTime:F3}ms/回");
            }
            
            ReportInfo("パフォーマンステストが完了しました");
        }

        /// <summary>
        /// システム統合テスト
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TestIntegration()
        {
            ReportInfo("=== システム統合テスト ===");
            
            // 1. システム情報取得テスト
            var systemInfo = gachaManager.GetSystemInfo();
            if (systemInfo == null)
            {
                ReportError("システム情報が取得される必要があります");
                return;
            }
            if (systemInfo.GetCurrentLevel() <= 0)
            {
                ReportError($"システム情報のレベルが正しい必要があります (実際: {systemInfo.GetCurrentLevel()})");
                return;
            }
            LogTest($"システム情報: レベル{systemInfo.GetCurrentLevel()}, コスト{systemInfo.GetCurrentCost()}");
            
            // 2. 履歴統計テスト
            var history = gachaManager.History;
            if (history == null)
            {
                ReportError("ガチャ履歴が取得される必要があります");
                return;
            }
            if (history.TotalPulls <= 0)
            {
                ReportError($"履歴に実行記録がある必要があります (実際: {history.TotalPulls}回)");
                return;
            }
            LogTest($"履歴統計: {history.GetStatisticsSummary()}");
            
            // 3. リセットテスト
            int levelBeforeReset = gachaManager.GetCurrentLevel();
            gachaManager.ResetGachaSystem();
            int levelAfterReset = gachaManager.GetCurrentLevel();
            
            if (levelAfterReset != 1)
            {
                ReportError($"リセット後はレベル1である必要があります (実際: {levelAfterReset})");
                return;
            }
            LogTest($"リセットテスト: レベル{levelBeforeReset} → {levelAfterReset}");
            
            // 4. 再初期化テスト
            if (generatedTestData != null)
            {
                gachaManager.InitializeGachaSystem(generatedTestData);
                if (!gachaManager.IsInitialized())
                {
                    ReportError("再初期化が成功する必要があります");
                    return;
                }
                AddTestResult("再初期化テスト完了");
            }
            
            ReportInfo("システム統合テストが完了しました");
        }


        /// <summary>
        /// テスト用ガチャデータを生成
        /// </summary>
        private void CreateTestGachaData()
        {
            generatedTestData = ScriptableObject.CreateInstance<GachaSystemData>();
            generatedTestData.name = "TestGachaData";
            
            // 基本設定
            var gachaSystemIdField = typeof(GachaSystemData).GetField("gachaSystemId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gachaSystemIdField?.SetValue(generatedTestData, "test_gacha");
            
            var displayNameField = typeof(GachaSystemData).GetField("displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            displayNameField?.SetValue(generatedTestData, "テスト用ガチャ");
            
            var baseCostField = typeof(GachaSystemData).GetField("baseCost", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            baseCostField?.SetValue(generatedTestData, 100);
            
            var isActiveField = typeof(GachaSystemData).GetField("isActive", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            isActiveField?.SetValue(generatedTestData, true);
            
            AddTestResult("テスト用ガチャデータを生成しました");
        }

        /// <summary>
        /// テスト結果をリストに追加
        /// </summary>
        private void AddTestResult(string result)
        {
            testResults.Add(result);
            
            if (showDetailedLogs)
            {
                ReportInfo(result);
            }
        }
        
        /// <summary>
        /// テストログ出力（後方互換性のため残す）
        /// </summary>
        private void LogTest(string message)
        {
            AddTestResult(message);
        }

        /// <summary>
        /// テスト結果の取得
        /// </summary>
        public List<string> GetTestResults()
        {
            return new List<string>(testResults);
        }

        /// <summary>
        /// テスト結果をクリア
        /// </summary>
        public void ClearTestResults()
        {
            testResults.Clear();
        }
    }
}