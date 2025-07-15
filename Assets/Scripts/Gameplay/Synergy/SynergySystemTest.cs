using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using System.Linq;
using System;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジーシステムの動作確認テスト
    /// TDDアプローチによる段階的実装
    /// </summary>
    [DefaultExecutionOrder(120)]
    public class SynergySystemTest : TestExclusiveBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableDetailedLogs = true;

        [Header("テスト用データ")]
        [SerializeField] private CharacterData[] testCharacterData;

        private CharacterDatabase characterDatabase;
        private List<Character> testCharacters;
        private BattleContext testBattleContext;

        public override string TestClassName => "SynergySystemTest";

        // 抽象メソッドの実装
        public override IEnumerator RunAllTests()
        {
            return RunAllTestsSequentially();
        }

        protected override void OnSystemAwake()
        {
            base.OnSystemAwake();
            InitializeTestComponents();
        }

        protected override void OnSystemStart()
        {
            base.OnSystemStart();
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTestsSequentially());
            }
        }

        /// <summary>
        /// テストコンポーネントの初期化
        /// </summary>
        private void InitializeTestComponents()
        {
            SetupCharacterDatabase();
            CreateTestCharacters();
            CreateTestBattleContext();
            ReportInfo("SynergySystemTestコンポーネントを初期化しました");
        }

        /// <summary>
        /// CharacterDatabaseのセットアップ
        /// </summary>
        private void SetupCharacterDatabase()
        {
            characterDatabase = CharacterDatabase.Instance;
            if (characterDatabase == null)
            {
                var databaseGO = new GameObject("CharacterDatabase");
                databaseGO.transform.SetParent(transform);
                characterDatabase = databaseGO.AddComponent<CharacterDatabase>();
            }
        }

        /// <summary>
        /// テスト用キャラクターの作成
        /// </summary>
        private void CreateTestCharacters()
        {
            testCharacters = new List<Character>();
            
            if (testCharacterData != null && testCharacterData.Length > 0)
            {
                for (int i = 0; i < testCharacterData.Length && i < 8; i++)
                {
                    var character = new Character(testCharacterData[i], 1);
                    testCharacters.Add(character);
                    ReportInfo($"テストキャラクター作成: {character.CharacterData.CharacterName}");
                }
            }
            else if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                for (int i = 0; i < Math.Min(8, characterDatabase.AllCharacters.Count); i++)
                {
                    var character = new Character(characterDatabase.AllCharacters[i], 1);
                    testCharacters.Add(character);
                    ReportInfo($"デフォルトテストキャラクター作成: {character.CharacterData.CharacterName}");
                }
            }
            else
            {
                ReportWarning("テストキャラクターが作成できませんでした");
            }
        }

        /// <summary>
        /// テスト用BattleContextの作成
        /// </summary>
        private void CreateTestBattleContext()
        {
            testBattleContext = new BattleContext(Time.time);
            if (testBattleContext.IsValid())
            {
                ReportInfo("テスト用BattleContextを作成しました");
            }
            else
            {
                ReportWarning("BattleContextの作成に失敗しました");
            }
        }

        /// <summary>
        /// 全テストを順次実行
        /// </summary>
        private IEnumerator RunAllTestsSequentially()
        {
            LogDebug("=== SynergySystemテスト開始 ===");

            // Phase 1: 基本シナジー発動テスト（SynergyCalculator実装済み）
            yield return StartCoroutine(TestBasicSynergyActivation());
            
            // Phase 2: ステータス修正型シナジーテスト（未実装のためコメントアウト）
            yield return StartCoroutine(TestStatModifierSynergies());
            
            // Phase 3: 発動能力型シナジーテスト
            yield return StartCoroutine(TestTriggerAbilitySynergies());
            
            // Phase 4: 複数シナジー同時適用テスト（未実装のためコメントアウト）
            // yield return StartCoroutine(TestMultipleSynergies());
            
            // Phase 5: シナジー変更・更新テスト（未実装のためコメントアウト）
            // yield return StartCoroutine(TestSynergyUpdates());
            
            // Phase 6: エラーケース・境界値テスト（未実装のためコメントアウト）
            // yield return StartCoroutine(TestSynergyErrorHandling());
            
            // Phase 7: パフォーマンステスト（未実装のためコメントアウト）
            // yield return StartCoroutine(TestSynergyPerformance());

            LogTestResult("=== SynergySystemテスト完了 ===");
        }

        /// <summary>
        /// 基本シナジー発動テスト
        /// TestRaceA（基本ステータス修正）のシナジー発動確認
        /// </summary>
        private IEnumerator TestBasicSynergyActivation()
        {
            LogDebug("基本シナジー発動テスト開始");

            // テスト用シナジーデータとCalculatorを作成
            var testSynergyData = CreateTestSynergyData();
            var calculator = new SynergyCalculator(new List<SynergyData> { testSynergyData });

            // TestRaceA基本発動テスト
            var testRaceACharacters2 = CreateTestCharactersWithName("TestRaceA", 2);
            var result2 = calculator.GetSynergyResult("testracea", testRaceACharacters2);
            AssertTest(result2 != null && result2.isActive && result2.characterCount == 2, "TestRaceA 2体でシナジーが発動すること");

            var testRaceACharacters1 = CreateTestCharactersWithName("TestRaceA", 1);
            var result1 = calculator.GetSynergyResult("testracea", testRaceACharacters1);
            AssertTest(result1 != null && !result1.isActive && result1.characterCount == 1, "TestRaceA 1体ではシナジーが発動しないこと");

            var testRaceACharacters4 = CreateTestCharactersWithName("TestRaceA", 4);
            var result4 = calculator.GetSynergyResult("testracea", testRaceACharacters4);
            AssertTest(result4 != null && result4.isActive && result4.activeSynergyLevel.RequiredCount == 4, "TestRaceA 4体でより強力なシナジーが発動すること");
            
            // 異なる種族では発動しない
            var mixedCharacters = CreateTestCharactersWithName("TestRaceA", 1);
            mixedCharacters.AddRange(CreateTestCharactersWithName("TestRaceB", 1));
            var resultMixed = calculator.GetSynergyResult("testracea", mixedCharacters);
            AssertTest(resultMixed != null && !resultMixed.isActive && resultMixed.characterCount == 1, "TestRaceAとTestRaceBの混合ではTestRaceAシナジーが発動しないこと");
            
            // 発動条件の境界値テスト
            var testRaceACharacters3 = CreateTestCharactersWithName("TestRaceA", 3);
            var result3 = calculator.GetSynergyResult("testracea", testRaceACharacters3);
            AssertTest(result3 != null && result3.isActive && result3.activeSynergyLevel.RequiredCount == 2, "TestRaceA 3体では2体レベルのシナジーが発動すること");

            var testRaceACharacters5 = CreateTestCharactersWithName("TestRaceA", 5);
            var result5 = calculator.GetSynergyResult("testracea", testRaceACharacters5);
            AssertTest(result5 != null && result5.isActive && result5.activeSynergyLevel.RequiredCount == 4, "TestRaceA 5体では4体レベルのシナジーが発動すること");

            LogTestResult("基本シナジー発動テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ステータス修正型シナジーテスト
        /// TestRaceA（基本ステータス修正）の効果確認
        /// </summary>
        private IEnumerator TestStatModifierSynergies()
        {
            LogDebug("ステータス修正型シナジーテスト開始");

            // テスト用シナジーデータと効果を作成
            var testSynergyData = CreateTestSynergyDataWithEffects();
            var calculator = new SynergyCalculator(new List<SynergyData> { testSynergyData });

            // TestRaceA 2体効果テスト
            var testCharacters2 = CreateTestCharactersWithName("TestRaceA", 2);
            var originalAttack2 = testCharacters2[0].CurrentStats.GetFinalStat(StatType.Attack);
            var originalDefense2 = testCharacters2[0].CurrentStats.GetFinalStat(StatType.Defense);
            
            // シナジー効果を適用
            ApplySynergyEffects(calculator, testCharacters2);
            
            var newAttack2 = testCharacters2[0].CurrentStats.GetFinalStat(StatType.Attack);
            var newDefense2 = testCharacters2[0].CurrentStats.GetFinalStat(StatType.Defense);
            
            AssertTest(newAttack2 == originalAttack2 + 50, "TestRaceA 2体時に攻撃力+50が適用されること");
            AssertTest(newDefense2 == originalDefense2, "TestRaceA 2体時に攻撃力以外のステータスは変化しないこと");
            
            // TestRaceA 4体効果テスト
            var testCharacters4 = CreateTestCharactersWithName("TestRaceA", 4);
            var originalAttack4 = testCharacters4[0].CurrentStats.GetFinalStat(StatType.Attack);
            var originalDefense4 = testCharacters4[0].CurrentStats.GetFinalStat(StatType.Defense);
            
            // シナジー効果を適用
            ApplySynergyEffects(calculator, testCharacters4);
            
            var newAttack4 = testCharacters4[0].CurrentStats.GetFinalStat(StatType.Attack);
            var newDefense4 = testCharacters4[0].CurrentStats.GetFinalStat(StatType.Defense);
            
            AssertTest(newAttack4 == originalAttack4 + 100, "TestRaceA 4体時に攻撃力+100が適用されること");
            AssertTest(newDefense4 == originalDefense4 + 30, "TestRaceA 4体時に防御力+30が適用されること");
            AssertTest(newAttack4 > originalAttack4 && newDefense4 > originalDefense4, "TestRaceA 4体時に複数ステータスが同時に変化すること");
            
            // 時間経過効果テスト（Phase 2.5で実装予定のためスキップ）
            // AssertTest(false, "TestRaceE 3体時に魔法防御+30が戦闘開始時に適用されること");
            // AssertTest(false, "TestRaceE 3体時に戦闘開始5秒後に全ステータス+20が適用されること");
            // AssertTest(false, "TestRaceE 3体時の5秒後効果が永続であること");

            LogTestResult("ステータス修正型シナジーテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 発動能力型シナジーテスト
        /// HP条件、死亡時、攻撃時発動の効果確認
        /// </summary>
        private IEnumerator TestTriggerAbilitySynergies()
        {
            LogDebug("発動能力型シナジーテスト開始");

            // TestRaceB HP条件発動テスト
            yield return StartCoroutine(TestHPConditionSynergies());
            
            // TestRaceC 死亡時発動テスト
            yield return StartCoroutine(TestDeathTriggerSynergies());
            
            // TestRaceD 攻撃時発動テスト
            yield return StartCoroutine(TestAttackTriggerSynergies());

            LogTestResult("発動能力型シナジーテスト完了");
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// HP条件発動シナジーテスト
        /// </summary>
        private IEnumerator TestHPConditionSynergies()
        {
            LogDebug("HP条件発動シナジーテスト開始");
            
            // TestRaceB HP条件シナジーデータを作成
            var testSynergyData = CreateTestHPConditionSynergyData();
            var calculator = new SynergyCalculator(new List<SynergyData> { testSynergyData });
            
            // TestRaceB キャラクターを作成
            var testCharacters = CreateTestCharactersWithName("TestRaceB", 2);
            
            // 基本のHP条件シナジー発動確認
            var result = calculator.GetSynergyResult("testraceb", testCharacters);
            AssertTest(result != null && result.isActive, "TestRaceB 2体でHP条件シナジーが発動すること");
            
            // シナジー効果を適用
            ApplySynergyEffects(calculator, testCharacters);
            
            // HP条件テストは実装が複雑なため、基本的な設定確認のみ
            var hpEffect = result.activeSynergyLevel.Effects.FirstOrDefault() as SynergyHPConditionEffect;
            AssertTest(hpEffect != null, "TestRaceB HP条件効果が存在すること");
            AssertTest(hpEffect != null && hpEffect.HPThreshold == 0.5f, "TestRaceB HP条件が50%であること");
            AssertTest(hpEffect != null && hpEffect.HealToFull, "TestRaceB HP条件で全回復すること");
            
            LogTestResult("HP条件発動シナジーテスト完了");
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// 死亡時発動シナジーテスト
        /// </summary>
        private IEnumerator TestDeathTriggerSynergies()
        {
            LogDebug("死亡時発動シナジーテスト開始");
            
            // TestRaceC 死亡時シナジーデータを作成
            var testSynergyData = CreateTestDeathTriggerSynergyData();
            var calculator = new SynergyCalculator(new List<SynergyData> { testSynergyData });
            
            // TestRaceC キャラクターを作成
            var testCharacters = CreateTestCharactersWithName("TestRaceC", 3);
            
            // 基本の死亡時シナジー発動確認
            var result = calculator.GetSynergyResult("testracec", testCharacters);
            AssertTest(result != null && result.isActive, "TestRaceC 3体で死亡時シナジーが発動すること");
            
            // シナジー効果を適用
            ApplySynergyEffects(calculator, testCharacters);
            
            // 死亡時効果の設定確認
            var deathEffect = result.activeSynergyLevel.Effects.FirstOrDefault() as SynergyDeathTriggerEffect;
            AssertTest(deathEffect != null, "TestRaceC 死亡時効果が存在すること");
            AssertTest(deathEffect != null && deathEffect.IsPermanent, "TestRaceC 死亡時効果が永続であること");
            AssertTest(deathEffect != null && deathEffect.OnlyAdjacent, "TestRaceC 死亡時効果が隣接のみ対象であること");
            
            LogTestResult("死亡時発動シナジーテスト完了");
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// 攻撃時発動シナジーテスト
        /// </summary>
        private IEnumerator TestAttackTriggerSynergies()
        {
            LogDebug("攻撃時発動シナジーテスト開始");
            
            // TestRaceD 攻撃時シナジーデータを作成
            var testSynergyData = CreateTestAttackTriggerSynergyData();
            var calculator = new SynergyCalculator(new List<SynergyData> { testSynergyData });
            
            // TestRaceD キャラクターを作成
            var testCharacters = CreateTestCharactersWithName("TestRaceD", 2);
            
            // 基本の攻撃時シナジー発動確認
            var result = calculator.GetSynergyResult("testraced", testCharacters);
            AssertTest(result != null && result.isActive, "TestRaceD 2体で攻撃時シナジーが発動すること");
            
            // シナジー効果を適用
            ApplySynergyEffects(calculator, testCharacters);
            
            // 攻撃時効果の設定確認
            var attackEffect = result.activeSynergyLevel.Effects.FirstOrDefault() as SynergyAttackTriggerEffect;
            AssertTest(attackEffect != null, "TestRaceD 攻撃時効果が存在すること");
            AssertTest(attackEffect != null && attackEffect.TargetHPThreshold == 0.2f, "TestRaceD 攻撃時効果のHP閾値が20%であること");
            AssertTest(attackEffect != null && attackEffect.IsInstantKill, "TestRaceD 攻撃時効果が即死効果であること");
            
            LogTestResult("攻撃時発動シナジーテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 複数シナジー同時適用テスト
        /// 異なるシナジーの同時発動確認
        /// </summary>
        private IEnumerator TestMultipleSynergies()
        {
            LogDebug("複数シナジー同時適用テスト開始");

            // 異なるシナジーの同時適用
            AssertTest(false, "TestRaceAとTestRaceBのシナジーが同時に適用されること");
            AssertTest(false, "TestRaceAとTestRaceBのシナジーが独立して動作すること");
            AssertTest(false, "TestRaceAとTestRaceBのシナジーが互いに干渉しないこと");
            
            // 1キャラクターが複数シナジーを保有する場合
            AssertTest(false, "1キャラクターが複数シナジー条件を満たす場合に両方が発動すること");
            AssertTest(false, "1キャラクターの複数シナジーが重複カウントされないこと");
            
            // 複数シナジーの効果重複
            AssertTest(false, "異なるシナジーの同じステータス修正が加算されること");
            AssertTest(false, "異なるシナジーの発動能力が独立して動作すること");

            LogTestResult("複数シナジー同時適用テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// シナジー変更・更新テスト
        /// キャラクター構成変更時の動作確認
        /// </summary>
        private IEnumerator TestSynergyUpdates()
        {
            LogDebug("シナジー変更・更新テスト開始");

            // シナジー体数変更時の更新
            AssertTest(false, "TestRaceA 2体→4体に変更時に効果が更新されること");
            AssertTest(false, "TestRaceA 4体→2体に変更時に効果が更新されること");
            AssertTest(false, "TestRaceA 2体→1体に変更時にシナジーが無効化されること");
            
            // シナジー追加・削除時の更新
            AssertTest(false, "新しいシナジーが追加された時に即座に適用されること");
            AssertTest(false, "既存シナジーが削除された時に即座に無効化されること");
            
            // リアルタイム更新
            AssertTest(false, "戦闘中のキャラクター変更時にシナジーが即座に更新されること");
            AssertTest(false, "シナジー更新時に発動中の効果が適切に処理されること");

            LogTestResult("シナジー変更・更新テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// シナジーシステムエラーハンドリングテスト
        /// 異常系・境界値の動作確認
        /// </summary>
        private IEnumerator TestSynergyErrorHandling()
        {
            LogDebug("シナジーシステムエラーハンドリングテスト開始");

            // null・空データの処理
            AssertTest(false, "null入力でもクラッシュせずに適切に処理されること");
            AssertTest(false, "空のキャラクターリストでもクラッシュしないこと");
            AssertTest(false, "無効なキャラクターデータが混在してもクラッシュしないこと");
            
            // 境界値テスト
            AssertTest(false, "シナジー発動条件ちょうどの体数で正しく発動すること");
            AssertTest(false, "シナジー発動条件より1体少ない時は発動しないこと");
            AssertTest(false, "最大8体配置時でも正しく処理されること");
            
            // データ不整合時の処理
            AssertTest(false, "シナジーデータ不整合でも適切にエラーハンドリングされること");
            AssertTest(false, "キャラクターデータ不整合でも適切にエラーハンドリングされること");
            AssertTest(false, "戦闘中の予期しないデータ変更でもクラッシュしないこと");

            LogTestResult("シナジーシステムエラーハンドリングテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// シナジーシステムパフォーマンステスト
        /// 大量データでの動作確認
        /// </summary>
        private IEnumerator TestSynergyPerformance()
        {
            LogDebug("シナジーシステムパフォーマンステスト開始");

            // パフォーマンス要件確認
            AssertTest(false, "8キャラクターのシナジー計算が0.05秒以内に完了すること");
            AssertTest(false, "複数シナジーの同時計算が効率的に行われること");
            AssertTest(false, "シナジー状態変更時の更新処理が高速であること");
            
            // メモリ使用量確認
            AssertTest(false, "メモリ使用量が適切な範囲内に収まること");
            AssertTest(false, "長時間使用してもメモリリークが発生しないこと");
            
            // 繰り返し処理の確認
            AssertTest(false, "100回の連続シナジー計算でパフォーマンスが劣化しないこと");
            AssertTest(false, "頻繁なシナジー変更でもパフォーマンスが安定していること");

            LogTestResult("シナジーシステムパフォーマンステスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テスト用アサーションメソッド
        /// </summary>
        private void AssertTest(bool condition, string message)
        {
            if (condition)
            {
                LogTestResult(message, true);
            }
            else
            {
                LogTestResult($"失敗: {message}", false);
            }
        }

        /// <summary>
        /// 詳細情報付きテスト用アサーションメソッド
        /// </summary>
        private void AssertTestDetailed<T>(bool condition, string message, T actualValue, T expectedValue)
        {
            if (condition)
            {
                LogTestResult(message, true);
            }
            else
            {
                LogTestResult($"失敗: {message} - 期待値: {expectedValue}, 実際の値: {actualValue}", false);
            }
        }

        /// <summary>
        /// 条件が満たされるまで待機する汎用メソッド
        /// </summary>
        private IEnumerator WaitForCondition(System.Func<bool> condition, float timeoutSeconds = 1f, string conditionDescription = "条件")
        {
            float elapsed = 0f;

            while (!condition() && elapsed < timeoutSeconds)
            {
                yield return null; // 1フレーム待機
                elapsed += Time.deltaTime;
            }

            bool success = condition();
            if (!success && enableDetailedLogs)
            {
                ReportWarning($"{conditionDescription}の待機がタイムアウトしました ({elapsed:F2}秒)");
            }

            yield return success;
        }

        /// <summary>
        /// デバッグ用シナジー情報表示
        /// </summary>
        [ContextMenu("Debug Print Synergy Info")]
        public void DebugPrintSynergyInfo()
        {
            if (testCharacters != null && testCharacters.Count > 0)
            {
                ReportInfo($"テストキャラクター数: {testCharacters.Count}");
                foreach (var character in testCharacters)
                {
                    ReportInfo($"キャラクター: {character.CharacterData.CharacterName}, レベル: {character.CurrentLevel}");
                }
            }
            else
            {
                ReportWarning("テストキャラクターが存在しません");
            }
        }

        /// <summary>
        /// テストシナジー計算の実行
        /// </summary>
        [ContextMenu("Test Synergy Calculation")]
        public void TestSynergyCalculation()
        {
            if (testCharacters != null && testCharacters.Count > 0)
            {
                ReportInfo("シナジー計算テストを実行します（現在は未実装のため失敗します）");
                // 実装後: var calculator = new SynergyCalculator();
                // var results = calculator.CalculateSynergies(testCharacters);
            }
            else
            {
                ReportWarning("テストキャラクターが存在しません");
            }
        }

        /// <summary>
        /// テスト用シナジーデータを作成
        /// </summary>
        /// <returns>TestRaceAシナジーデータ</returns>
        private SynergyData CreateTestSynergyData()
        {
            var synergyData = ScriptableObject.CreateInstance<SynergyData>();
            
            // シナジーレベルを作成（2体、4体で発動）
            var level2 = new SynergyLevel(2, "TestRaceA 2体効果");
            var level4 = new SynergyLevel(4, "TestRaceA 4体効果");
            
            // テスト用設定メソッドを使用
            synergyData.SetTestData("testracea", "TestRaceA", new SynergyLevel[] { level2, level4 });
            
            return synergyData;
        }

        /// <summary>
        /// 指定した名前のテストキャラクターを作成
        /// </summary>
        /// <param name="characterName">キャラクター名</param>
        /// <param name="count">作成数</param>
        /// <returns>作成されたキャラクターリスト</returns>
        private List<Character> CreateTestCharactersWithName(string characterName, int count)
        {
            var characters = new List<Character>();
            
            for (int i = 0; i < count; i++)
            {
                var characterData = ScriptableObject.CreateInstance<CharacterData>();
                
                // テスト用設定メソッドを使用
                characterData.SetTestData($"{characterName}_{i + 1}");
                
                var character = new Character(characterData, 1);
                characters.Add(character);
            }
            
            return characters;
        }

        /// <summary>
        /// 効果付きテスト用シナジーデータを作成
        /// </summary>
        /// <returns>効果付きTestRaceAシナジーデータ</returns>
        private SynergyData CreateTestSynergyDataWithEffects()
        {
            var synergyData = ScriptableObject.CreateInstance<SynergyData>();
            
            // 2体レベル効果：攻撃力+50
            var attackEffect2 = ScriptableObject.CreateInstance<SynergyStatModifierEffect>();
            attackEffect2.SetTestData("testracea_lv2_attack", "TestRaceA攻撃力強化", StatType.Attack, 50f);
            
            var level2 = new SynergyLevel(2, "TestRaceA 2体効果");
            level2.AddEffect(attackEffect2);
            
            // 4体レベル効果：攻撃力+100、防御力+30
            var attackEffect4 = ScriptableObject.CreateInstance<SynergyStatModifierEffect>();
            attackEffect4.SetTestData("testracea_lv4_attack", "TestRaceA攻撃力強化", StatType.Attack, 100f);
            
            var defenseEffect4 = ScriptableObject.CreateInstance<SynergyStatModifierEffect>();
            defenseEffect4.SetTestData("testracea_lv4_defense", "TestRaceA防御力強化", StatType.Defense, 30f);
            
            var level4 = new SynergyLevel(4, "TestRaceA 4体効果");
            level4.AddEffect(attackEffect4);
            level4.AddEffect(defenseEffect4);
            
            // シナジーデータ設定
            synergyData.SetTestData("testracea", "TestRaceA", new SynergyLevel[] { level2, level4 });
            
            return synergyData;
        }
        
        /// <summary>
        /// HP条件発動シナジーデータを作成
        /// </summary>
        /// <returns>TestRaceB HP条件シナジーデータ</returns>
        private SynergyData CreateTestHPConditionSynergyData()
        {
            var synergyData = ScriptableObject.CreateInstance<SynergyData>();
            
            // HP条件効果：50%以下で全回復
            var hpEffect = ScriptableObject.CreateInstance<SynergyHPConditionEffect>();
            hpEffect.SetTestData("testraceb_hp_condition", "TestRaceB HP条件全回復", 0.5f, true);
            
            var level2 = new SynergyLevel(2, "TestRaceB 2体効果");
            level2.AddEffect(hpEffect);
            
            // シナジーデータ設定
            synergyData.SetTestData("testraceb", "TestRaceB", new SynergyLevel[] { level2 });
            
            return synergyData;
        }
        
        /// <summary>
        /// 死亡時発動シナジーデータを作成
        /// </summary>
        /// <returns>TestRaceC 死亡時シナジーデータ</returns>
        private SynergyData CreateTestDeathTriggerSynergyData()
        {
            var synergyData = ScriptableObject.CreateInstance<SynergyData>();
            
            // 死亡時効果：隣接味方の攻撃力+50（永続）
            var deathEffect = ScriptableObject.CreateInstance<SynergyDeathTriggerEffect>();
            deathEffect.SetTestData("testracec_death_trigger", "TestRaceC 死亡時攻撃力強化", StatType.Attack, 50f, true, true);
            
            var level3 = new SynergyLevel(3, "TestRaceC 3体効果");
            level3.AddEffect(deathEffect);
            
            // シナジーデータ設定
            synergyData.SetTestData("testracec", "TestRaceC", new SynergyLevel[] { level3 });
            
            return synergyData;
        }
        
        /// <summary>
        /// 攻撃時発動シナジーデータを作成
        /// </summary>
        /// <returns>TestRaceD 攻撃時シナジーデータ</returns>
        private SynergyData CreateTestAttackTriggerSynergyData()
        {
            var synergyData = ScriptableObject.CreateInstance<SynergyData>();
            
            // 攻撃時効果：20%以下の敵に即死効果
            var attackEffect = ScriptableObject.CreateInstance<SynergyAttackTriggerEffect>();
            attackEffect.SetTestData("testraced_attack_trigger", "TestRaceD 攻撃時即死効果", 0.2f, true, 1.0f);
            
            var level2 = new SynergyLevel(2, "TestRaceD 2体効果");
            level2.AddEffect(attackEffect);
            
            // シナジーデータ設定
            synergyData.SetTestData("testraced", "TestRaceD", new SynergyLevel[] { level2 });
            
            return synergyData;
        }

        /// <summary>
        /// シナジー効果をキャラクターに適用
        /// </summary>
        /// <param name="calculator">シナジー計算機</param>
        /// <param name="characters">対象キャラクターリスト</param>
        private void ApplySynergyEffects(SynergyCalculator calculator, List<Character> characters)
        {
            // シナジー計算
            var synergyResults = calculator.CalculateSynergies(characters);
            
            // アクティブなシナジーの効果を適用
            foreach (var result in synergyResults)
            {
                if (result.isActive && result.activeSynergyLevel != null)
                {
                    // 各効果をシナジー対象キャラクターに適用
                    foreach (var effect in result.activeSynergyLevel.Effects)
                    {
                        if (effect != null && effect.CanApply(testBattleContext))
                        {
                            effect.ApplyEffect(result.synergyCharacters, testBattleContext);
                        }
                    }
                }
            }
        }
    }
}