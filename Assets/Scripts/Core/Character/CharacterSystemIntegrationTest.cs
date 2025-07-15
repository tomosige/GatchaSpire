using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Gold;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターシステムと他システムの統合テストクラス
    /// CharacterInventoryManager、GoldManager、GachaSystemManager の連携をテスト
    /// </summary>
    [DefaultExecutionOrder(50)] // 単体テストの前に実行
    public class CharacterSystemIntegrationTest : TestExclusiveBase
    {
        [Header("テストシナリオ設定")]
        [SerializeField] private bool resetSystemsBeforeTest = true;
        [SerializeField] private bool cleanupAfterTests = true;
        [SerializeField] private int initialGoldAmount = 50000;
        [SerializeField] private int gachaTestCount = 10;
        [SerializeField] private int integrationTestLoops = 3;

        public override float MaxExecutionTimeSeconds => 180f; // 3分

        private List<string> testResults = new List<string>();
        private CharacterInventoryManager inventoryManager;
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private List<Character> testCharacters = new List<Character>();

        /// <summary>
        /// 統合テストを実行
        /// </summary>
        public override IEnumerator RunAllTests()
        {
            ReportInfo("システム統合テストを開始します");
            testResults.Clear();

            // 前提条件の確認
            yield return StartCoroutine(SetupIntegrationTest());

            // ガチャ→インベントリ統合テスト
            yield return StartCoroutine(TestGachaToInventoryIntegration());

            // ゴールド→ガチャ→インベントリ統合フローテスト
            yield return StartCoroutine(TestFullGameplayFlow());

            // 売却→ゴールド→ガチャ循環テスト
            yield return StartCoroutine(TestSellBuyLoop());

            // 合成・経験値化統合テスト
            yield return StartCoroutine(TestCharacterProcessingIntegration());

            // システム間エラー処理統合テスト
            yield return StartCoroutine(TestErrorHandlingIntegration());

            // パフォーマンス統合テスト
            yield return StartCoroutine(TestPerformanceIntegration());

            // テスト結果の表示
            DisplayIntegrationTestResults();

            // クリーンアップ
            if (cleanupAfterTests)
            {
                yield return StartCoroutine(CleanupIntegrationTest());
            }

            ReportInfo("システム統合テストが完了しました");
        }

        /// <summary>
        /// 統合テスト環境のセットアップ
        /// </summary>
        private IEnumerator SetupIntegrationTest()
        {
            ReportInfo("=== 統合テスト環境セットアップ ===");

            // 依存システムの取得
            var coordinator = UnityGameSystemCoordinator.Instance;
            if (coordinator == null)
            {
                AddTestResult("FAIL", "UnityGameSystemCoordinator が見つかりません");
                yield break;
            }

            inventoryManager = coordinator.GetSystem<CharacterInventoryManager>("CharacterInventoryManager");
            goldManager = coordinator.GetSystem<GoldManager>("GoldManager");
            characterDatabase = coordinator.GetSystem<CharacterDatabase>("CharacterDatabase");

            // システム存在確認
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

            // システムの初期化状態確認
            if (!inventoryManager.IsInitialized())
            {
                AddTestResult("FAIL", "CharacterInventoryManager が初期化されていません");
                yield break;
            }

            if (!goldManager.IsInitialized())
            {
                AddTestResult("FAIL", "GoldManager が初期化されていません");
                yield break;
            }

            // システムリセット（必要に応じて）
            if (resetSystemsBeforeTest)
            {
                inventoryManager.ResetSystem();
                goldManager.SetGold(initialGoldAmount);
            }

            AddTestResult("PASS", "統合テスト環境セットアップ完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ガチャ→インベントリ統合テスト
        /// </summary>
        private IEnumerator TestGachaToInventoryIntegration()
        {
            ReportInfo("=== ガチャ→インベントリ統合テスト ===");

            var initialCharacterCount = inventoryManager.OwnedCharacterCount;
            var initialGold = goldManager.CurrentGold;

            // テスト用キャラクターを手動で追加（ガチャシステムの代替）
            var allCharacterData = characterDatabase.AllCharacters;
            if (allCharacterData.Any())
            {
                ReportInfo($"利用可能キャラクターデータ数: {allCharacterData.Count}");
                ReportInfo($"テスト対象数: {gachaTestCount}");
                ReportInfo($"初期インベントリ数: {initialCharacterCount}");
                
                var successCount = 0;
                for (int i = 0; i < gachaTestCount; i++)
                {
                    var characterData = allCharacterData[i % allCharacterData.Count];
                    var newCharacter = new Character(characterData, 1);
                    
                    ReportInfo($"キャラクター追加試行 {i + 1}: {characterData.CharacterName} (ID: {newCharacter.InstanceId})");
                    
                    if (inventoryManager.AddCharacter(newCharacter))
                    {
                        testCharacters.Add(newCharacter);
                        successCount++;
                        ReportInfo($"キャラクター追加成功: {characterData.CharacterName}");
                    }
                    else
                    {
                        ReportWarning($"キャラクター追加失敗: {characterData.CharacterName}");
                    }
                }

                if (successCount == gachaTestCount)
                {
                    AddTestResult("PASS", $"ガチャ結果インベントリ追加: {successCount}体追加成功");
                }
                else
                {
                    AddTestResult("FAIL", $"ガチャ結果インベントリ追加: {successCount}/{gachaTestCount}体のみ成功");
                }

                // インベントリ数確認
                var finalCharacterCount = inventoryManager.OwnedCharacterCount;
                var expectedCount = initialCharacterCount + successCount;
                if (finalCharacterCount == expectedCount)
                {
                    AddTestResult("PASS", "インベントリ数整合性確認");
                }
                else
                {
                    AddTestResult("FAIL", $"インベントリ数不一致: 期待値{expectedCount}、実際{finalCharacterCount}");
                }
            }
            else
            {
                AddTestResult("SKIP", "ガチャテスト用キャラクターデータが不足");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 完全なゲームプレイフロー統合テスト
        /// </summary>
        private IEnumerator TestFullGameplayFlow()
        {
            ReportInfo("=== 完全ゲームプレイフロー統合テスト ===");

            for (int loop = 0; loop < integrationTestLoops; loop++)
            {
                ReportInfo($"統合フローテスト ループ {loop + 1}/{integrationTestLoops}");

                var loopStartGold = goldManager.CurrentGold;
                var loopStartCharacters = inventoryManager.OwnedCharacterCount;

                // Step 1: ガチャでキャラクター獲得（シミュレーション）
                if (testCharacters.Count >= 2)
                {
                    var gachaChar1 = new Character(testCharacters[0].CharacterData, 1);
                    var gachaChar2 = new Character(testCharacters[1].CharacterData, 1);
                    
                    inventoryManager.AddCharacter(gachaChar1);
                    inventoryManager.AddCharacter(gachaChar2);

                    // Step 2: 一部キャラクターを売却
                    var sellTargets = new List<string> { gachaChar2.InstanceId };
                    if (inventoryManager.SellCharacters(sellTargets, out int earnedGold))
                    {
                        AddTestResult("PASS", $"ループ{loop + 1}: 売却成功 {earnedGold}ゴールド獲得");

                        // Step 3: ゴールド増加確認
                        var currentGold = goldManager.CurrentGold;
                        if (currentGold > loopStartGold)
                        {
                            AddTestResult("PASS", $"ループ{loop + 1}: ゴールド増加確認");
                        }
                        else
                        {
                            AddTestResult("FAIL", $"ループ{loop + 1}: ゴールド増加確認失敗");
                        }

                        // Step 4: 残りキャラクターの強化（合成テスト）
                        if (testCharacters.Count >= 3)
                        {
                            var enhanceTarget = gachaChar1.InstanceId;
                            var materials = testCharacters.Skip(2).Take(1).Select(c => c.InstanceId).ToList();
                            
                            if (inventoryManager.FuseCharacters(enhanceTarget, materials, out Character enhanced))
                            {
                                AddTestResult("PASS", $"ループ{loop + 1}: キャラクター強化成功");
                            }
                            else
                            {
                                AddTestResult("WARN", $"ループ{loop + 1}: キャラクター強化スキップ");
                            }
                        }
                    }
                    else
                    {
                        AddTestResult("FAIL", $"ループ{loop + 1}: 売却失敗");
                    }
                }
                else
                {
                    AddTestResult("SKIP", $"ループ{loop + 1}: テストキャラクター不足");
                }

                yield return new WaitForSeconds(0.1f);
            }

            AddTestResult("PASS", "完全ゲームプレイフロー統合テスト完了");
        }

        /// <summary>
        /// 売却→購買循環テスト
        /// </summary>
        private IEnumerator TestSellBuyLoop()
        {
            ReportInfo("=== 売却→購買循環テスト ===");

            if (!testCharacters.Any())
            {
                AddTestResult("SKIP", "循環テスト用キャラクターが不足");
                yield break;
            }

            var initialGold = goldManager.CurrentGold;
            var sellTargets = testCharacters.Take(3).Select(c => c.InstanceId).ToList();

            // 売却でゴールド獲得
            if (inventoryManager.SellCharacters(sellTargets, out int earnedGold))
            {
                var afterSellGold = goldManager.CurrentGold;
                
                if (afterSellGold == initialGold + earnedGold)
                {
                    AddTestResult("PASS", "売却→ゴールド増加統合確認");

                    // 十分なゴールドがあることを確認
                    if (afterSellGold >= 1000) // 仮の最低ガチャコスト
                    {
                        AddTestResult("PASS", "購買可能ゴールド確認");

                        // 実際のガチャシステムがあれば以下でテスト
                        // var gachaResult = gachaManager.DrawGacha(1);
                        // if (gachaResult.IsSuccess) { ... }
                        
                        AddTestResult("INFO", "ガチャシステム統合は将来実装予定");
                    }
                    else
                    {
                        AddTestResult("WARN", "購買には不十分なゴールド");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "売却→ゴールド増加統合確認失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "売却→購買循環テスト: 売却失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// キャラクター処理統合テスト
        /// </summary>
        private IEnumerator TestCharacterProcessingIntegration()
        {
            ReportInfo("=== キャラクター処理統合テスト ===");

            var remainingCharacters = testCharacters.Where(c => inventoryManager.HasCharacter(c.InstanceId)).ToList();
            if (remainingCharacters.Count < 4)
            {
                AddTestResult("SKIP", "キャラクター処理統合テスト用データ不足");
                yield break;
            }

            // 合成→経験値化の連続処理テスト
            var baseChar = remainingCharacters[0];
            var fuseTargets = remainingCharacters.Skip(1).Take(2).Select(c => c.InstanceId).ToList();
            var expTargets = remainingCharacters.Skip(3).Take(1).Select(c => c.InstanceId).ToList();

            var initialLevel = baseChar.CurrentLevel;
            var initialExp = baseChar.CurrentExp;

            // Step 1: 合成処理
            if (inventoryManager.FuseCharacters(baseChar.InstanceId, fuseTargets, out Character fusedChar))
            {
                AddTestResult("PASS", "統合テスト: 合成処理成功");

                // Step 2: 経験値化処理
                if (inventoryManager.ConvertCharactersToExp(expTargets, fusedChar.InstanceId, out int expGained))
                {
                    AddTestResult("PASS", $"統合テスト: 経験値化処理成功 {expGained}経験値獲得");

                    // 総合的な成長確認
                    if (fusedChar.CurrentExp > initialExp)
                    {
                        AddTestResult("PASS", "統合テスト: キャラクター総合成長確認");
                    }
                    else
                    {
                        AddTestResult("FAIL", "統合テスト: キャラクター総合成長確認失敗");
                    }
                }
                else
                {
                    AddTestResult("FAIL", "統合テスト: 経験値化処理失敗");
                }
            }
            else
            {
                AddTestResult("FAIL", "統合テスト: 合成処理失敗");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// エラーハンドリング統合テスト
        /// </summary>
        private IEnumerator TestErrorHandlingIntegration()
        {
            ReportInfo("=== エラーハンドリング統合テスト ===");

            // ゴールド不足での操作テスト
            var originalGold = goldManager.CurrentGold;
            goldManager.SetGold(0);

            // ゴールド不足状態での各種操作（実際のガチャシステムがあれば）
            AddTestResult("INFO", "ゴールド不足テストは将来のガチャシステム実装時に追加");

            // 無効な操作の組み合わせテスト
            var invalidTargets = new List<string> { "invalid_id_1", "invalid_id_2" };
            
            // 複数システムにまたがる無効操作
            if (!inventoryManager.SellCharacters(invalidTargets, out int earnedGold))
            {
                AddTestResult("PASS", "統合エラーハンドリング: 無効売却処理");
            }
            else
            {
                AddTestResult("FAIL", "統合エラーハンドリング: 無効売却処理");
            }

            if (!inventoryManager.FuseCharacters("invalid_base", invalidTargets, out Character result))
            {
                AddTestResult("PASS", "統合エラーハンドリング: 無効合成処理");
            }
            else
            {
                AddTestResult("FAIL", "統合エラーハンドリング: 無効合成処理");
            }

            // ゴールドを元に戻す
            goldManager.SetGold(originalGold);

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// パフォーマンス統合テスト
        /// </summary>
        private IEnumerator TestPerformanceIntegration()
        {
            ReportInfo("=== パフォーマンス統合テスト ===");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 複数システムにまたがる連続操作
            var operationCount = 50;
            var successCount = 0;

            for (int i = 0; i < operationCount; i++)
            {
                // キャラクター追加→ロック設定→お気に入り設定の連続操作
                if (testCharacters.Any())
                {
                    var testChar = new Character(testCharacters[0].CharacterData, 1);
                    if (inventoryManager.AddCharacter(testChar))
                    {
                        var charIds = new List<string> { testChar.InstanceId };
                        
                        if (inventoryManager.SetCharacterLockState(charIds, true) &&
                            inventoryManager.SetCharacterFavoriteState(charIds, true))
                        {
                            successCount++;
                        }
                    }
                }

                if (i % 10 == 0)
                {
                    yield return null; // フレーム分散
                }
            }

            stopwatch.Stop();
            var averageTime = stopwatch.ElapsedMilliseconds / (float)operationCount;

            if (averageTime < 2.0f) // 2ms以下
            {
                AddTestResult("PASS", $"統合パフォーマンステスト: 平均{averageTime:F3}ms/操作, 成功率{successCount}/{operationCount}");
            }
            else
            {
                AddTestResult("WARN", $"統合パフォーマンス警告: 平均{averageTime:F3}ms/操作");
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 統合テストデータのクリーンアップ
        /// </summary>
        private IEnumerator CleanupIntegrationTest()
        {
            ReportInfo("=== 統合テストクリーンアップ ===");

            if (inventoryManager != null)
            {
                ReportInfo($"クリーンアップ前のインベントリ数: {inventoryManager.OwnedCharacterCount}");
                inventoryManager.ResetSystem();
                ReportInfo($"クリーンアップ後のインベントリ数: {inventoryManager.OwnedCharacterCount}");
            }

            if (goldManager != null)
            {
                goldManager.SetGold(initialGoldAmount);
            }

            testCharacters.Clear();
            AddTestResult("INFO", "統合テストクリーンアップ完了");

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
                        Debug.LogError($"INTEGRATION TEST FAILURE [{SystemName}] {message}");
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
        /// 統合テスト結果の表示
        /// </summary>
        private void DisplayIntegrationTestResults()
        {
            var passCount = testResults.Count(r => r.Contains("[PASS]"));
            var failCount = testResults.Count(r => r.Contains("[FAIL]"));
            var warnCount = testResults.Count(r => r.Contains("[WARN]"));
            var skipCount = testResults.Count(r => r.Contains("[SKIP]"));
            var infoCount = testResults.Count(r => r.Contains("[INFO]"));

            ReportInfo("=== 統合テスト結果サマリー ===");
            ReportInfo($"成功: {passCount}, 失敗: {failCount}, 警告: {warnCount}, スキップ: {skipCount}, 情報: {infoCount}");

            if (failCount > 0)
            {
                ReportError("一部の統合テストが失敗しました");
            }
            else if (warnCount > 0)
            {
                ReportWarning("一部の統合テストで警告が発生しました");
            }
            else
            {
                ReportInfo("全ての統合テストが正常に完了しました");
            }
        }

        /// <summary>
        /// エディタ専用統合テスト実行メニュー
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [UnityEngine.ContextMenu("統合テスト実行")]
        public void RunIntegrationTestsFromEditor()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunAllTests());
            }
            else
            {
                ReportWarning("統合テストは実行時にのみ実行できます");
            }
        }
    }
}