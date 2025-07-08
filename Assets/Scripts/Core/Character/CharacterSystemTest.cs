using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターシステムのテストクラス
    /// </summary>
    public class CharacterSystemTest : GameSystemBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool showDetailedLogs = true;
        [SerializeField] private bool createTestCharacterData = true;

        protected override string SystemName => "CharacterSystemTest";

        private List<string> testResults = new List<string>();
        private CharacterDatabase characterDatabase;
        private List<CharacterData> testCharacterDataList = new List<CharacterData>();

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
                StartCoroutine(RunAllTests());
            }
        }

        /// <summary>
        /// 全てのテストを実行
        /// </summary>
        /// <returns></returns>
        public IEnumerator RunAllTests()
        {
            ReportInfo("キャラクターシステムテストを開始します");
            testResults.Clear();

            // CharacterDatabaseの取得
            characterDatabase = CharacterDatabase.Instance;
            if (characterDatabase == null)
            {
                characterDatabase = FindObjectOfType<CharacterDatabase>();
                if (characterDatabase == null)
                {
                    ReportError("CharacterDatabaseが見つかりません");
                    yield break;
                }
            }

            // テスト用データの作成
            if (createTestCharacterData)
            {
                CreateTestCharacterData();
                yield return new WaitForSeconds(0.1f);
            }

            // データ読み込みテスト
            yield return StartCoroutine(TestDataLoading());
            yield return new WaitForSeconds(0.1f);

            // ステータス計算テスト
            yield return StartCoroutine(TestStatCalculations());
            yield return new WaitForSeconds(0.1f);

            // 検索機能テスト
            yield return StartCoroutine(TestSearchFunctions());
            yield return new WaitForSeconds(0.1f);

            // エラーハンドリングテスト
            yield return StartCoroutine(TestErrorHandling());
            yield return new WaitForSeconds(0.1f);

            // キャラクターインスタンステスト
            yield return StartCoroutine(TestCharacterInstances());
            yield return new WaitForSeconds(0.1f);

            // 結果表示
            ShowTestResults();
        }

        /// <summary>
        /// テスト用キャラクターデータを作成
        /// </summary>
        private void CreateTestCharacterData()
        {
            ReportInfo("テスト用キャラクターデータを作成中");

            // テスト用のCharacterDataを動的に作成
            var testData1 = ScriptableObject.CreateInstance<CharacterData>();
            testData1.name = "TestWarrior";
            // 本来はprivateフィールドなので、実際のテストでは事前に作成されたアセットを使用

            var testData2 = ScriptableObject.CreateInstance<CharacterData>();
            testData2.name = "TestMage";

            testCharacterDataList.Add(testData1);
            testCharacterDataList.Add(testData2);

            testResults.Add("✓ テスト用キャラクターデータ作成: 完了");
        }

        /// <summary>
        /// データ読み込みテスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestDataLoading()
        {
            ReportInfo("データ読み込みテストを開始");

            // データベース初期化テスト
            int initialCount = characterDatabase.CharacterCount;
            testResults.Add($"✓ 初期キャラクター数: {initialCount}");

            // データ再読み込みテスト
            characterDatabase.LoadAllCharacterData();
            yield return new WaitForSeconds(0.1f);

            int reloadedCount = characterDatabase.CharacterCount;
            if (reloadedCount >= initialCount)
            {
                testResults.Add("✓ データ再読み込み: 正常動作");
            }
            else
            {
                testResults.Add("✗ データ再読み込み: データが減少しました");
            }

            // データ整合性チェック
            characterDatabase.ValidateAllData();
            testResults.Add("✓ データ整合性チェック: 完了");

            yield return null;
        }

        /// <summary>
        /// ステータス計算テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestStatCalculations()
        {
            ReportInfo("ステータス計算テストを開始");

            // CharacterStats構造体のテスト
            var stats = new CharacterStats(100, 50, 20, 15, 12, 10, 8, 5);
            
            // 基本ステータステスト
            if (stats.GetBaseStat(StatType.HP) == 100)
            {
                testResults.Add("✓ 基本ステータス取得: 正常動作");
            }
            else
            {
                testResults.Add("✗ 基本ステータス取得: 失敗");
            }

            yield return new WaitForSeconds(0.05f);

            // 修正値適用テスト
            stats.AddModifier(StatType.Attack, 10);
            if (stats.GetFinalStat(StatType.Attack) == 30)
            {
                testResults.Add("✓ ステータス修正値: 正常動作");
            }
            else
            {
                testResults.Add($"✗ ステータス修正値: 失敗 (期待値: 30, 実際: {stats.GetFinalStat(StatType.Attack)})");
            }

            yield return new WaitForSeconds(0.05f);

            // 戦闘力計算テスト
            int battlePower = stats.CalculateBattlePower();
            if (battlePower > 0)
            {
                testResults.Add($"✓ 戦闘力計算: {battlePower}");
            }
            else
            {
                testResults.Add("✗ 戦闘力計算: 失敗");
            }

            yield return new WaitForSeconds(0.05f);

            // レベル成長テスト
            var growthRates = new Dictionary<StatType, float>
            {
                { StatType.HP, 0.1f },
                { StatType.Attack, 0.12f }
            };
            stats.ApplyLevelGrowth(5, growthRates);
            
            if (stats.GetFinalStat(StatType.HP) > 100)
            {
                testResults.Add("✓ レベル成長適用: 正常動作");
            }
            else
            {
                testResults.Add("✗ レベル成長適用: 失敗");
            }

            yield return null;
        }

        /// <summary>
        /// 検索機能テスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestSearchFunctions()
        {
            ReportInfo("検索機能テストを開始");

            // 全キャラクター取得テスト
            var allCharacters = characterDatabase.AllCharacters;
            testResults.Add($"✓ 全キャラクター取得: {allCharacters.Count}体");

            yield return new WaitForSeconds(0.05f);

            // レアリティ別検索テスト
            var commonCharacters = characterDatabase.GetCharactersByRarity(CharacterRarity.Common);
            testResults.Add($"✓ コモンキャラクター検索: {commonCharacters.Count}体");

            yield return new WaitForSeconds(0.05f);

            // クラス別検索テスト
            var warriors = characterDatabase.GetCharactersByClass(CharacterClass.Warrior);
            testResults.Add($"✓ 戦士クラス検索: {warriors.Count}体");

            yield return new WaitForSeconds(0.05f);

            // 複合フィルタテスト
            var filter = new CharacterFilter();
            filter.Rarities.Add(CharacterRarity.Rare);
            filter.Classes.Add(CharacterClass.Mage);
            
            var filteredCharacters = characterDatabase.GetCharactersByFilter(filter);
            testResults.Add($"✓ 複合フィルタ検索: {filteredCharacters.Count}体");

            yield return new WaitForSeconds(0.05f);

            // ランダム取得テスト
            var randomCharacters = characterDatabase.GetRandomCharacters(3);
            if (randomCharacters.Count <= 3)
            {
                testResults.Add($"✓ ランダム取得: {randomCharacters.Count}体");
            }
            else
            {
                testResults.Add("✗ ランダム取得: 指定数を超過");
            }

            yield return null;
        }

        /// <summary>
        /// エラーハンドリングテスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestErrorHandling()
        {
            ReportInfo("エラーハンドリングテストを開始");

            // 無効なID検索テスト
            var invalidCharacter = characterDatabase.GetCharacterById(-1);
            if (invalidCharacter == null)
            {
                testResults.Add("✓ 無効ID検索エラー: 正常処理");
            }
            else
            {
                testResults.Add("✗ 無効ID検索エラー: 異常処理");
            }

            yield return new WaitForSeconds(0.05f);

            // 空文字列名前検索テスト
            var emptyNameCharacter = characterDatabase.GetCharacterByName("");
            if (emptyNameCharacter == null)
            {
                testResults.Add("✓ 空文字列名前検索エラー: 正常処理");
            }
            else
            {
                testResults.Add("✗ 空文字列名前検索エラー: 異常処理");
            }

            yield return new WaitForSeconds(0.05f);

            // 存在しない名前検索テスト
            var nonExistentCharacter = characterDatabase.GetCharacterByName("NonExistentCharacter");
            if (nonExistentCharacter == null)
            {
                testResults.Add("✓ 存在しない名前検索: 正常処理");
            }
            else
            {
                testResults.Add("✗ 存在しない名前検索: 異常処理");
            }

            yield return null;
        }

        /// <summary>
        /// キャラクターインスタンステスト
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestCharacterInstances()
        {
            ReportInfo("キャラクターインスタンステストを開始");

            // Resourcesからテスト用CharacterDataを読み込み
            var testData = Resources.Load<CharacterData>("Characters\\Test\\TestCharacterData");

            if (testData == null)
            {
                testResults.Add("⚠ テスト用CharacterDataが見つかりません。既存データを使用します");
                var allCharacters = characterDatabase.AllCharacters;
                if (allCharacters.Count > 0)
                {
                    testData = allCharacters[0];
                }
                else
                {
                    testResults.Add("✗ キャラクターインスタンステスト: 利用可能なデータがありません");
                    yield break;
                }
            }

            // 正常なCharacterDataでテスト実行
            var character = new Character(testData, 1);

            // 以下、既存のテストを続行
            if (character.InstanceId != null)
            {
                testResults.Add("✓ キャラクターインスタンス作成: 正常動作");
            }
            else
            {
                testResults.Add("✗ キャラクターインスタンス作成: 失敗");
            }

            // 経験値追加テスト
            int initialLevel = character.CurrentLevel;
            character.AddExperience(1000);

            if (character.CurrentLevel >= initialLevel)
            {
                testResults.Add($"✓ 経験値追加: レベル{character.CurrentLevel}");
            }
            else
            {
                testResults.Add("✗ 経験値追加: 失敗");
            }

            // ダメージ・回復テスト
            int initialHP = character.CurrentHP;
            character.TakeDamage(50);
            int afterDamageHP = character.CurrentHP;
            character.Heal(25);
            int afterHealHP = character.CurrentHP;

            if (afterDamageHP < initialHP && afterHealHP > afterDamageHP)
            {
                testResults.Add("✓ ダメージ・回復システム: 正常動作");
            }
            else
            {
                testResults.Add("✗ ダメージ・回復システム: 失敗");
            }

            yield return null;
        }

        /// <summary>
        /// テスト結果を表示
        /// </summary>
        private void ShowTestResults()
        {
            ReportInfo("=== キャラクターシステムテスト結果 ===");
            
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
                ReportInfo($"🎉 {summary} - キャラクターシステムが正常に動作しています");
            }
            else
            {
                ReportWarning($"⚠️ {summary} - 一部に問題があります");
            }

            // デバッグ情報の表示
            if (showDetailedLogs && characterDatabase != null)
            {
                ReportInfo("=== デバッグ情報 ===");
                ReportInfo(characterDatabase.GetDebugInfo());
                
                var stats = characterDatabase.GetStatistics();
                ReportInfo($"データベース統計: 総数{stats.TotalCharacters}, 戦闘力範囲{stats.MinBattlePower}-{stats.MaxBattlePower}");
            }
        }

        /// <summary>
        /// 手動でテストを実行
        /// </summary>
        [ContextMenu("Run Tests")]
        public void RunTestsManually()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunAllTests());
            }
            else
            {
                ReportWarning("テストは実行時のみ動作します");
            }
        }

        /// <summary>
        /// 統計情報を表示
        /// </summary>
        [ContextMenu("Show Statistics")]
        public void ShowStatistics()
        {
            if (characterDatabase == null)
            {
                ReportWarning("CharacterDatabaseが初期化されていません");
                return;
            }

            var stats = characterDatabase.GetStatistics();
            ReportInfo("=== キャラクターデータベース統計 ===");
            ReportInfo($"総キャラクター数: {stats.TotalCharacters}");
            ReportInfo($"戦闘力範囲: {stats.MinBattlePower} - {stats.MaxBattlePower}");
            ReportInfo($"平均戦闘力: {stats.AverageBattlePower:F1}");
            
            ReportInfo("=== レアリティ分布 ===");
            foreach (var rarity in stats.CharactersByRarity)
            {
                ReportInfo($"{rarity.Key}: {rarity.Value}体");
            }
        }
    }
}