using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Gold;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// CharacterInventoryManager のテストクラス
    /// </summary>
    [DefaultExecutionOrder(100)] // 統合テストの後に実行
    public class CharacterInventoryManagerTest : TestExclusiveBase, IUnityResettable
    {
        [Header("テストデータ設定")]
        [SerializeField] private bool createTestData = true;
        [SerializeField] private bool cleanupAfterTests = true;
        [SerializeField] private int testCharacterCount = 10;
        [SerializeField] private int testGoldAmount = 10000;

        private List<string> testResults = new List<string>();
        private CharacterInventoryManager inventoryManager;
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private List<Character> testCharacters = new List<Character>();

        /// <summary>
        /// 全てのテストを実行
        /// </summary>
        public override IEnumerator RunAllTests()
        {
            ReportInfo("CharacterInventoryManager テストを開始します");
            testResults.Clear();

            // 前提条件の確認
            yield return StartCoroutine(SetupTestEnvironment());

            // 基本機能テスト
            yield return StartCoroutine(TestBasicInventoryOperations());

            // 売却機能テスト
            yield return StartCoroutine(TestSellOperations());

            // 合成機能テスト
            yield return StartCoroutine(TestFuseOperations());

            // 経験値化機能テスト
            yield return StartCoroutine(TestConvertToExpOperations());

            // ロック・お気に入り機能テスト
            yield return StartCoroutine(TestLockAndFavoriteOperations());

            // エラーハンドリングテスト
            yield return StartCoroutine(TestErrorHandling());

            // パフォーマンステスト
            yield return StartCoroutine(TestPerformance());

            // テスト結果の表示
            DisplayTestResults();

            // クリーンアップ
            if (cleanupAfterTests)
            {
                yield return StartCoroutine(CleanupTestData());
            }

            ReportInfo("CharacterInventoryManager テストが完了しました");
        }

        /// <summary>
        /// テスト環境のセットアップ
        /// </summary>
        private IEnumerator SetupTestEnvironment()
        {
            ReportInfo("=== テスト環境セットアップ ===");

            // 依存システムの取得
            inventoryManager = CharacterInventoryManager.Instance;
            goldManager = GoldManager.Instance;
            characterDatabase = CharacterDatabase.Instance;

            if (inventoryManager == null)
            {
                AddTestResult("FAIL", "CharacterInventoryManager が見つかりません");
                yield break;
            }

            if (goldManager == null)
            {
                AddTestResult("FAIL", "GoldManager が見つかりません");
                yield break;
            }

            if (characterDatabase == null)
            {
                AddTestResult("FAIL", "CharacterDatabase が見つかりません");
                yield break;
            }

            // テストデータの作成
            if (createTestData)
            {
                yield return StartCoroutine(CreateTestData());
            }

            AddTestResult("PASS", "テスト環境セットアップ完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テストデータの作成
        /// </summary>
        private IEnumerator CreateTestData()
        {
            testCharacters.Clear();

            // インベントリを完全クリア（前回のテスト結果をクリア）
            ReportInfo($"リセット前のインベントリ数: {inventoryManager.OwnedCharacterCount}");
            inventoryManager.ResetSystem();
            ReportInfo($"リセット後のインベントリ数: {inventoryManager.OwnedCharacterCount}");

            // 初期ゴールドを設定
            goldManager.SetGold(testGoldAmount);

            // テスト用キャラクターを作成
            var allCharacterData = characterDatabase.AllCharacters;
            if (allCharacterData.Any())
            {
                for (int i = 0; i < testCharacterCount && i < allCharacterData.Count; i++)
                {
                    var characterData = allCharacterData[i];
                    var character = new Character(characterData, 1);
                    testCharacters.Add(character);
                    inventoryManager.AddCharacter(character);
                }

                ReportInfo($"{testCharacters.Count}体のテストキャラクターを作成しました");
            }
            else
            {
                ReportWarning("テスト用キャラクターデータが見つかりません");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 基本インベントリ操作のテスト
        /// </summary>
        private IEnumerator TestBasicInventoryOperations()
        {
            ReportInfo("=== 基本インベントリ操作テスト ===");

            // キャラクター数チェック
            var initialCount = inventoryManager.OwnedCharacterCount;
            if (initialCount == testCharacters.Count)
            {
                AddTestResult("PASS", $"キャラクター数確認: {initialCount}体");
            }
            else
            {
                AddTestResult("FAIL", $"キャラクター数不一致: 期待値{testCharacters.Count}、実際{initialCount}");
            }

            // キャラクター取得テスト
            if (testCharacters.Any())
            {
                var testChar = testCharacters[0];
                var retrieved = inventoryManager.GetCharacter(testChar.InstanceId);
                if (retrieved != null && retrieved.InstanceId == testChar.InstanceId)
                {
                    AddTestResult("PASS", "キャラクター取得テスト");
                }
                else
                {
                    AddTestResult("FAIL", "キャラクター取得テスト");
                }

                // 存在チェックテスト
                if (inventoryManager.HasCharacter(testChar.InstanceId))
                {
                    AddTestResult("PASS", "キャラクター存在チェックテスト");
                }
                else
                {
                    AddTestResult("FAIL", "キャラクター存在チェックテスト");
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 売却操作のテスト
        /// </summary>
        private IEnumerator TestSellOperations()
        {
            ReportInfo("=== 売却操作テスト ===");

            if (!testCharacters.Any())
            {
                AddTestResult("SKIP", "売却テスト用キャラクターが不足");
                yield break;
            }

            var initialGold = goldManager.CurrentGold;
            var sellTargets = testCharacters.Take(2).Select(c => c.InstanceId).ToList();
            var expectedPrice = testCharacters.Take(2).Sum(c => c.CharacterData.SellPrice);

            // 売却実行
            if (inventoryManager.SellCharacters(sellTargets, out int earnedGold))
            {
                if (earnedGold == expectedPrice)
                {
                    AddTestResult("PASS", $"売却テスト: {earnedGold}ゴールド獲得");
                }
                else
                {
                    AddTestResult("FAIL", $"売却価格不一致: 期待値{expectedPrice}、実際{earnedGold}");
                }

                // ゴールド増加確認
                var currentGold = goldManager.CurrentGold;
                if (currentGold == initialGold + earnedGold)
                {
                    AddTestResult("PASS", "ゴールド増加確認");
                }
                else
                {
                    AddTestResult("FAIL", "ゴールド増加確認失敗");
                }

                // キャラクター削除確認
                var deletedCount = 0;
                foreach (var targetId in sellTargets)
                {
                    if (!inventoryManager.HasCharacter(targetId))
                    {
                        deletedCount++;
                    }
                }

                if (deletedCount == sellTargets.Count)
                {
                    AddTestResult("PASS", "売却キャラクター削除確認");
                }
                else
                {
                    AddTestResult("FAIL", "売却キャラクター削除確認失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "売却操作失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 合成操作のテスト
        /// </summary>
        private IEnumerator TestFuseOperations()
        {
            ReportInfo("=== 合成操作テスト ===");

            var remainingCharacters = testCharacters.Where(c => inventoryManager.HasCharacter(c.InstanceId)).ToList();
            if (remainingCharacters.Count < 3)
            {
                AddTestResult("SKIP", "合成テスト用キャラクターが不足");
                yield break;
            }

            var baseCharacter = remainingCharacters[0];
            var materialCharacters = remainingCharacters.Skip(1).Take(2).ToList();
            var materialIds = materialCharacters.Select(c => c.InstanceId).ToList();

            var initialLevel = baseCharacter.CurrentLevel;
            var initialExp = baseCharacter.CurrentExp;

            // 合成実行
            if (inventoryManager.FuseCharacters(baseCharacter.InstanceId, materialIds, out Character resultCharacter))
            {
                if (resultCharacter != null && resultCharacter.InstanceId == baseCharacter.InstanceId)
                {
                    AddTestResult("PASS", "合成操作実行");

                    // 経験値増加確認
                    if (resultCharacter.CurrentExp > initialExp)
                    {
                        AddTestResult("PASS", $"経験値増加確認: {resultCharacter.CurrentExp - initialExp}増加");
                    }
                    else
                    {
                        AddTestResult("FAIL", "経験値増加確認失敗");
                    }

                    // 素材キャラクター削除確認
                    var deletedCount = 0;
                    foreach (var materialId in materialIds)
                    {
                        if (!inventoryManager.HasCharacter(materialId))
                        {
                            deletedCount++;
                        }
                    }

                    if (deletedCount == materialIds.Count)
                    {
                        AddTestResult("PASS", "素材キャラクター削除確認");
                    }
                    else
                    {
                        AddTestResult("FAIL", "素材キャラクター削除確認失敗");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "合成結果キャラクター確認失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "合成操作失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 経験値化操作のテスト
        /// </summary>
        private IEnumerator TestConvertToExpOperations()
        {
            ReportInfo("=== 経験値化操作テスト ===");

            var remainingCharacters = testCharacters.Where(c => inventoryManager.HasCharacter(c.InstanceId)).ToList();
            if (remainingCharacters.Count < 3)
            {
                AddTestResult("SKIP", "経験値化テスト用キャラクターが不足");
                yield break;
            }

            var receivingCharacter = remainingCharacters[0];
            var targetCharacters = remainingCharacters.Skip(1).Take(2).ToList();
            var targetIds = targetCharacters.Select(c => c.InstanceId).ToList();

            var initialExp = receivingCharacter.CurrentExp;

            // 経験値化実行
            if (inventoryManager.ConvertCharactersToExp(targetIds, receivingCharacter.InstanceId, out int expGained))
            {
                if (expGained > 0)
                {
                    AddTestResult("PASS", $"経験値化操作実行: {expGained}経験値獲得");

                    // 経験値増加確認
                    if (receivingCharacter.CurrentExp > initialExp)
                    {
                        AddTestResult("PASS", "経験値増加確認");
                    }
                    else
                    {
                        AddTestResult("FAIL", "経験値増加確認失敗");
                    }

                    // 対象キャラクター削除確認
                    var deletedCount = 0;
                    foreach (var targetId in targetIds)
                    {
                        if (!inventoryManager.HasCharacter(targetId))
                        {
                            deletedCount++;
                        }
                    }

                    if (deletedCount == targetIds.Count)
                    {
                        AddTestResult("PASS", "経験値化対象キャラクター削除確認");
                    }
                    else
                    {
                        AddTestResult("FAIL", "経験値化対象キャラクター削除確認失敗");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "経験値獲得量が0");
                }
            }
            else
            {
                AddTestResult("FAIL", "経験値化操作失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ロック・お気に入り操作のテスト
        /// </summary>
        private IEnumerator TestLockAndFavoriteOperations()
        {
            ReportInfo("=== ロック・お気に入り操作テスト ===");

            var remainingCharacters = testCharacters.Where(c => inventoryManager.HasCharacter(c.InstanceId)).ToList();
            if (!remainingCharacters.Any())
            {
                AddTestResult("SKIP", "ロック・お気に入りテスト用キャラクターが不足");
                yield break;
            }

            var testCharacter = remainingCharacters[0];
            var testIds = new List<string> { testCharacter.InstanceId };

            // ロック操作テスト
            if (inventoryManager.SetCharacterLockState(testIds, true))
            {
                if (inventoryManager.IsCharacterLocked(testCharacter.InstanceId))
                {
                    AddTestResult("PASS", "キャラクターロックテスト");
                }
                else
                {
                    AddTestResult("FAIL", "キャラクターロックテスト");
                }

                // アンロックテスト
                if (inventoryManager.SetCharacterLockState(testIds, false))
                {
                    if (!inventoryManager.IsCharacterLocked(testCharacter.InstanceId))
                    {
                        AddTestResult("PASS", "キャラクターアンロックテスト");
                    }
                    else
                    {
                        AddTestResult("FAIL", "キャラクターアンロックテスト");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "キャラクターアンロック操作失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "キャラクターロック操作失敗");
            }

            // お気に入り操作テスト
            if (inventoryManager.SetCharacterFavoriteState(testIds, true))
            {
                if (inventoryManager.IsCharacterFavorite(testCharacter.InstanceId))
                {
                    AddTestResult("PASS", "キャラクターお気に入りテスト");
                }
                else
                {
                    AddTestResult("FAIL", "キャラクターお気に入りテスト");
                }

                // お気に入り解除テスト
                if (inventoryManager.SetCharacterFavoriteState(testIds, false))
                {
                    if (!inventoryManager.IsCharacterFavorite(testCharacter.InstanceId))
                    {
                        AddTestResult("PASS", "キャラクターお気に入り解除テスト");
                    }
                    else
                    {
                        AddTestResult("FAIL", "キャラクターお気に入り解除テスト");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "キャラクターお気に入り解除操作失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "キャラクターお気に入り操作失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// エラーハンドリングのテスト
        /// </summary>
        private IEnumerator TestErrorHandling()
        {
            ReportInfo("=== エラーハンドリングテスト ===");

            // 無効なキャラクターIDでの操作テスト
            var invalidIds = new List<string> { "invalid_id" };

            // 無効な売却テスト
            if (!inventoryManager.SellCharacters(invalidIds, out int earnedGold))
            {
                AddTestResult("PASS", "無効ID売却エラーハンドリング");
            }
            else
            {
                AddTestResult("FAIL", "無効ID売却エラーハンドリング");
            }

            // 無効な合成テスト
            if (!inventoryManager.FuseCharacters("invalid_base", invalidIds, out Character result))
            {
                AddTestResult("PASS", "無効ID合成エラーハンドリング");
            }
            else
            {
                AddTestResult("FAIL", "無効ID合成エラーハンドリング");
            }

            // 無効な経験値化テスト
            if (!inventoryManager.ConvertCharactersToExp(invalidIds, "invalid_receiver", out int expGained))
            {
                AddTestResult("PASS", "無効ID経験値化エラーハンドリング");
            }
            else
            {
                AddTestResult("FAIL", "無効ID経験値化エラーハンドリング");
            }

            // null/空リストでの操作テスト
            if (!inventoryManager.SellCharacters(null, out earnedGold) && 
                !inventoryManager.SellCharacters(new List<string>(), out earnedGold))
            {
                AddTestResult("PASS", "null/空リストエラーハンドリング");
            }
            else
            {
                AddTestResult("FAIL", "null/空リストエラーハンドリング");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// パフォーマンステスト
        /// </summary>
        private IEnumerator TestPerformance()
        {
            ReportInfo("=== パフォーマンステスト ===");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 大量操作のシミュレーション
            var testOperations = 100;
            var successCount = 0;

            for (int i = 0; i < testOperations; i++)
            {
                // 有効なCharacterDataを使用してキャラクターを作成
                if (testCharacters.Any())
                {
                    var baseData = testCharacters[i % testCharacters.Count].CharacterData;
                    var testChar = new Character(baseData, 1);
                    if (inventoryManager.AddCharacter(testChar))
                    {
                        successCount++;
                    }
                }
                else
                {
                    // testCharactersが空の場合はcharacterDatabaseから取得
                    var allCharacterData = characterDatabase.AllCharacters;
                    if (allCharacterData.Any())
                    {
                        var characterData = allCharacterData[i % allCharacterData.Count];
                        var testChar = new Character(characterData, 1);
                        if (inventoryManager.AddCharacter(testChar))
                        {
                            successCount++;
                        }
                    }
                }
            }

            stopwatch.Stop();
            var averageTime = stopwatch.ElapsedMilliseconds / (float)testOperations;

            if (averageTime < 1.0f) // 1ms以下
            {
                AddTestResult("PASS", $"パフォーマンステスト: 平均{averageTime:F3}ms/操作");
            }
            else
            {
                AddTestResult("WARN", $"パフォーマンス警告: 平均{averageTime:F3}ms/操作");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テストデータのクリーンアップ
        /// </summary>
        private IEnumerator CleanupTestData()
        {
            ReportInfo("=== テストデータクリーンアップ ===");

            if (inventoryManager != null)
            {
                inventoryManager.ResetSystem();
            }

            testCharacters.Clear();
            AddTestResult("INFO", "テストデータクリーンアップ完了");

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テスト結果を追加
        /// </summary>
        private void AddTestResult(string status, string message)
        {
            var result = $"[{status}] {message}";
            testResults.Add(result);

            if (showDetailedLogs)
            {
                switch (status)
                {
                    case "PASS":
                        ReportInfo(result);
                        break;
                    case "FAIL":
                        // 失敗テストは特別なフォーマットでUnityConsoleに直接出力
                        Debug.LogError($"🔥 TEST FAILURE 🔥 [{SystemName}] {message}");
                        ReportError(result);
                        break;
                    case "WARN":
                        ReportWarning(result);
                        break;
                    default:
                        ReportInfo(result);
                        break;
                }
            }
        }

        /// <summary>
        /// テスト結果の表示
        /// </summary>
        private void DisplayTestResults()
        {
            var passCount = testResults.Count(r => r.Contains("[PASS]"));
            var failCount = testResults.Count(r => r.Contains("[FAIL]"));
            var warnCount = testResults.Count(r => r.Contains("[WARN]"));
            var skipCount = testResults.Count(r => r.Contains("[SKIP]"));

            ReportInfo("=== テスト結果サマリー ===");
            ReportInfo($"成功: {passCount}, 失敗: {failCount}, 警告: {warnCount}, スキップ: {skipCount}");

            if (failCount > 0)
            {
                // 失敗したテストの詳細をUnity Consoleに出力
                Debug.LogError($"📊 TEST SUMMARY: {failCount} FAILURES DETECTED in {SystemName}");
                var failures = testResults.Where(r => r.Contains("[FAIL]")).ToList();
                foreach (var failure in failures)
                {
                    Debug.LogError($"❌ FAILED: {failure}");
                }
                ReportError("一部のテストが失敗しました");
            }
            else if (warnCount > 0)
            {
                ReportWarning("一部のテストで警告が発生しました");
            }
            else
            {
                Debug.Log($"✅ ALL TESTS PASSED in {SystemName}");
                ReportInfo("全てのテストが正常に完了しました");
            }
        }

    }
}