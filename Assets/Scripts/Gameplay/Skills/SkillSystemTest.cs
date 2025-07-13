using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// スキルシステムの動作確認テスト
    /// TDDアプローチによる段階的実装
    /// </summary>
    [DefaultExecutionOrder(110)]
    public class SkillSystemTest : TestExclusiveBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableDetailedLogs = true;

        [Header("テスト用データ")]
        [SerializeField] private CharacterData[] testCharacterData;

        private CharacterDatabase characterDatabase;
        private Character testCharacter;

        public override string TestClassName => "SkillSystemTest";

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
            CreateTestCharacter();
            ReportInfo("SkillSystemTestコンポーネントを初期化しました");
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
        private void CreateTestCharacter()
        {
            if (testCharacterData != null && testCharacterData.Length > 0)
            {
                testCharacter = new Character(testCharacterData[0], 1);
                ReportInfo($"テストキャラクター作成: {testCharacter.CharacterData.CharacterName}");
            }
            else if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                testCharacter = new Character(characterDatabase.AllCharacters[0], 1);
                ReportInfo($"デフォルトテストキャラクター作成: {testCharacter.CharacterData.CharacterName}");
            }
            else
            {
                ReportWarning("テストキャラクターが作成できませんでした");
            }
        }

        /// <summary>
        /// 全テストを順次実行
        /// </summary>
        private IEnumerator RunAllTestsSequentially()
        {
            LogDebug("=== SkillSystemテスト開始 ===");

            // Phase 1: 基本スキルシステム（実装済み）
            yield return StartCoroutine(TestSkillProgressionInitialization());
            yield return StartCoroutine(TestSkillUnlockLevels());
            yield return StartCoroutine(TestLevelUpSkillAcquisition());
            yield return StartCoroutine(TestJumpLevelUpSkillAcquisition());
            
            // Phase 2: 未実装部分（コメントアウト）
            // yield return StartCoroutine(TestSkillUnlockResultValidation());
            // yield return StartCoroutine(TestErrorCaseHandling());
            // yield return StartCoroutine(TestSkillCooldownManager());
            // yield return StartCoroutine(TestSkillCooldownRealtime());
            // yield return StartCoroutine(TestBasicSkillEffects());
            // yield return StartCoroutine(TestSkillEffectProperties());
            // yield return StartCoroutine(TestCharacterInventoryIntegration());

            LogTestResult("=== 実装済みテスト完了 ===");
        }

        /// <summary>
        /// スキル進行システム初期化テスト
        /// </summary>
        private IEnumerator TestSkillProgressionInitialization()
        {
            LogDebug("スキル進行システム初期化テスト開始");

            // テストケース専用のCharacterSkillProgressionを作成
            var skillProgression = new CharacterSkillProgression(1);

            // 期待値: CharacterSkillProgressionクラスが存在すること
            AssertTest(skillProgression != null, "CharacterSkillProgressionクラスの存在確認");

            if (skillProgression == null)
            {
                LogTestResult("CharacterSkillProgressionが作成できないため、以降のテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // 期待値: スキル習得レベルが定義されていること
            var unlockLevels = CharacterSkillProgression.SKILL_UNLOCK_LEVELS;
            AssertTest(unlockLevels != null, "SKILL_UNLOCK_LEVELSの定義確認");
            AssertTest(unlockLevels.Length == 3, "スキル習得レベル数が3であること");
            AssertTestDetailed(unlockLevels[0] == 3, "第1スキル習得レベルが3であること", unlockLevels[0], 3);
            AssertTestDetailed(unlockLevels[1] == 6, "第2スキル習得レベルが6であること", unlockLevels[1], 6);
            AssertTestDetailed(unlockLevels[2] == 10, "第3スキル習得レベルが10であること", unlockLevels[2], 10);

            // 期待値: 初期レベル1でスキル習得数が0であること
            AssertTestDetailed(skillProgression.Level == 1, "初期レベルが1であること", skillProgression.Level, 1);
            AssertTestDetailed(skillProgression.UnlockedSkillCount == 0, "初期スキル習得数が0であること", skillProgression.UnlockedSkillCount, 0);
            AssertTestDetailed(skillProgression.MaxSkillSlots == 3, "最大スキルスロット数が3であること", skillProgression.MaxSkillSlots, 3);

            LogTestResult("スキル進行システム初期化テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキル習得レベルテスト
        /// </summary>
        private IEnumerator TestSkillUnlockLevels()
        {
            LogDebug("スキル習得レベルテスト開始");

            // テストケース専用のCharacterSkillProgressionを作成
            var skillProgression = new CharacterSkillProgression(1);

            // 期待値: レベル3、6、10でスキル習得可能であること
            AssertTest(skillProgression.CanUnlockSkill(3), "レベル3でのスキル習得可能性確認");
            AssertTest(skillProgression.CanUnlockSkill(6), "レベル6でのスキル習得可能性確認");
            AssertTest(skillProgression.CanUnlockSkill(10), "レベル10でのスキル習得可能性確認");

            // 期待値: その他のレベルでスキル習得不可であること
            AssertTest(!skillProgression.CanUnlockSkill(2), "レベル2でのスキル習得不可確認");
            AssertTest(!skillProgression.CanUnlockSkill(5), "レベル5でのスキル習得不可確認");
            AssertTest(!skillProgression.CanUnlockSkill(7), "レベル7でのスキル習得不可確認");
            AssertTest(!skillProgression.CanUnlockSkill(1), "レベル1でのスキル習得不可確認");

            // 期待値: スキルスロット番号の正しい取得
            AssertTestDetailed(skillProgression.GetSkillSlot(3) == 0, "レベル3のスキルスロットが0であること", skillProgression.GetSkillSlot(3), 0);
            AssertTestDetailed(skillProgression.GetSkillSlot(6) == 1, "レベル6のスキルスロットが1であること", skillProgression.GetSkillSlot(6), 1);
            AssertTestDetailed(skillProgression.GetSkillSlot(10) == 2, "レベル10のスキルスロットが2であること", skillProgression.GetSkillSlot(10), 2);
            AssertTestDetailed(skillProgression.GetSkillSlot(5) == -1, "レベル5のスキルスロットが-1であること", skillProgression.GetSkillSlot(5), -1);

            LogTestResult("スキル習得レベルテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// レベルアップスキル習得テスト（仕様書4.3の具体例に基づく）
        /// </summary>
        private IEnumerator TestLevelUpSkillAcquisition()
        {
            LogDebug("レベルアップスキル習得テスト開始");

            // テストケース専用のCharacterSkillProgressionを作成
            var skillProgression = new CharacterSkillProgression(1);

            // テスト用スキルの作成
            var testSkill3 = new Skill("テストスキル3", "レベル3で習得", 1, 3);
            var testSkill6 = new Skill("テストスキル6", "レベル6で習得", 2, 6);

            // レベル2→3のシミュレーション
            skillProgression.Level = 2;
            skillProgression.ResetAllSkills();
            
            // レベル3に上昇
            skillProgression.Level = 3;
            bool unlockResult = skillProgression.UnlockSkill(0, testSkill3); // スロット0（レベル3）
            
            AssertTest(unlockResult, "レベル2→3でのスキル習得が成功すること");
            AssertTestDetailed(skillProgression.UnlockedSkillCount == 1, "習得スキル数が1であること", skillProgression.UnlockedSkillCount, 1);
            AssertTest(skillProgression.IsSkillUnlocked(0), "スキルスロット0が習得済みであること");
            AssertTest(skillProgression.GetSkill(0) == testSkill3, "習得したスキルがtestSkill3であること");

            // レベル2→4のシミュレーション（レベル3をスキップしない）
            skillProgression.Level = 2;
            skillProgression.ResetAllSkills();
            
            // レベル4に上昇（レベル3のスキルが習得可能）
            skillProgression.Level = 4;
            var availableSkills = skillProgression.GetAvailableSkillLevels();
            
            AssertTest(availableSkills.Contains(3), "レベル4到達時にレベル3スキルが習得可能であること");
            AssertTest(!availableSkills.Contains(6), "レベル4到達時にレベル6スキルは習得不可能であること");

            LogTestResult("レベルアップスキル習得テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ジャンプレベルアップスキル習得テスト（仕様書4.3の具体例に基づく）
        /// </summary>
        private IEnumerator TestJumpLevelUpSkillAcquisition()
        {
            LogDebug("ジャンプレベルアップスキル習得テスト開始");

            // テストケース専用のCharacterSkillProgressionを作成
            var skillProgression = new CharacterSkillProgression(1);

            // テスト用スキルの作成
            var testSkill3 = new Skill("テストスキル3", "レベル3で習得", 1, 3);
            var testSkill6 = new Skill("テストスキル6", "レベル6で習得", 2, 6);
            var testSkill10 = new Skill("テストスキル10", "レベル10で習得", 3, 10);

            // 仕様書例: レベル1→7（スキル2個習得可能）
            skillProgression.Level = 1;
            skillProgression.ResetAllSkills();
            skillProgression.Level = 7;
            
            var availableSkills = skillProgression.GetAvailableSkillLevels();
            AssertTestDetailed(availableSkills.Count == 2, "レベル1→7で2個のスキルが習得可能", availableSkills.Count, 2);
            AssertTest(availableSkills.Contains(3), "レベル3スキルが習得可能であること");
            AssertTest(availableSkills.Contains(6), "レベル6スキルが習得可能であること");
            AssertTest(!availableSkills.Contains(10), "レベル10スキルは習得不可能であること");

            // 実際に習得テスト
            bool unlock3 = skillProgression.UnlockSkill(0, testSkill3);
            bool unlock6 = skillProgression.UnlockSkill(1, testSkill6);
            
            AssertTest(unlock3, "レベル3スキルの習得が成功すること");
            AssertTest(unlock6, "レベル6スキルの習得が成功すること");
            AssertTestDetailed(skillProgression.UnlockedSkillCount == 2, "習得スキル数が2であること", skillProgression.UnlockedSkillCount, 2);

            // 仕様書例: レベル1→10（スキル3個習得可能）
            var skillProgression2 = new CharacterSkillProgression(1);
            skillProgression2.Level = 10;
            
            var allAvailableSkills = skillProgression2.GetAvailableSkillLevels();
            AssertTestDetailed(allAvailableSkills.Count == 3, "レベル1→10で3個のスキルが習得可能", allAvailableSkills.Count, 3);
            AssertTest(allAvailableSkills.Contains(3), "レベル3スキルが習得可能であること");
            AssertTest(allAvailableSkills.Contains(6), "レベル6スキルが習得可能であること");
            AssertTest(allAvailableSkills.Contains(10), "レベル10スキルが習得可能であること");

            // 仕様書例: レベル5→10（スキル2個習得可能）
            var skillProgression3 = new CharacterSkillProgression(5);
            skillProgression3.Level = 10;
            
            var partialAvailableSkills = skillProgression3.GetAvailableSkillLevels();
            AssertTestDetailed(partialAvailableSkills.Count == 2, "レベル5→10で2個のスキルが習得可能", partialAvailableSkills.Count, 2);
            AssertTest(!partialAvailableSkills.Contains(3), "レベル3スキルは習得不可能であること（既に通過）");
            AssertTest(partialAvailableSkills.Contains(6), "レベル6スキルが習得可能であること");
            AssertTest(partialAvailableSkills.Contains(10), "レベル10スキルが習得可能であること");

            LogTestResult("ジャンプレベルアップスキル習得テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキルクールダウン管理テスト
        /// </summary>
        private IEnumerator TestSkillCooldownManager()
        {
            LogDebug("スキルクールダウン管理テスト開始");

            // 期待値: CharacterSkillManagerクラスが存在すること
            AssertTest(false, "CharacterSkillManagerクラスの存在確認（未実装のため失敗予定）");

            // 期待値: 個別クールダウン管理機能
            AssertTest(false, "個別スキルクールダウン管理確認（未実装のため失敗予定）");

            // 期待値: クールダウン準備完了判定
            AssertTest(false, "スキル使用準備完了判定確認（未実装のため失敗予定）");

            // 期待値: クールダウン時間の経過処理
            AssertTest(false, "クールダウン経過時間処理確認（未実装のため失敗予定）");

            LogTestResult("スキルクールダウン管理テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 基本スキル効果テスト
        /// </summary>
        private IEnumerator TestBasicSkillEffects()
        {
            LogDebug("基本スキル効果テスト開始");

            // 期待値: SkillEffectクラスが存在すること
            AssertTest(false, "SkillEffectクラスの存在確認（未実装のため失敗予定）");

            // 期待値: DamageEffectクラスが存在すること
            AssertTest(false, "DamageEffectクラスの存在確認（未実装のため失敗予定）");

            // 期待値: HealEffectクラスが存在すること
            AssertTest(false, "HealEffectクラスの存在確認（未実装のため失敗予定）");

            // 期待値: スキル効果適用処理
            AssertTest(false, "スキル効果適用処理確認（未実装のため失敗予定）");

            LogTestResult("基本スキル効果テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// SkillUnlockResult検証テスト（仕様書4.2対応）
        /// </summary>
        private IEnumerator TestSkillUnlockResultValidation()
        {
            LogDebug("SkillUnlockResult検証テスト開始");

            // 期待値: SkillUnlockResultクラスの存在
            AssertTest(false, "SkillUnlockResultクラスの存在確認（未実装のため失敗予定）");

            // 期待値: SkillUnlockResultのプロパティ検証
            AssertTest(false, "UnlockLevelプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "SkillSlotプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "UnlockedSkillプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "CharacterNameプロパティの確認（未実装のため失敗予定）");

            // 期待値: ToString()メソッドの出力フォーマット検証
            // "{CharacterName}がレベル{UnlockLevel}でスキル「{UnlockedSkill.SkillName}」を習得しました"
            AssertTest(false, "ToString()メソッドの出力フォーマット確認（未実装のため失敗予定）");

            LogTestResult("SkillUnlockResult検証テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// エラーケース処理テスト
        /// </summary>
        private IEnumerator TestErrorCaseHandling()
        {
            LogDebug("エラーケース処理テスト開始");

            if (testCharacter == null)
            {
                LogTestResult("テストキャラクターが存在しないため、エラーケース処理テストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // 期待値: 無効レベル（負の値）への対応
            AssertTest(false, "負のレベルへのレベルアップ拒否確認（未実装のため失敗予定）");

            // 期待値: 現在レベル以下への対応
            AssertTest(false, "現在レベル以下へのレベルアップ拒否確認（未実装のため失敗予定）");

            // 期待値: 既に習得済みスキルの重複習得防止
            AssertTest(false, "既習得スキルの重複習得防止確認（未実装のため失敗予定）");

            // 期待値: スキルデータが見つからない場合の処理
            AssertTest(false, "スキルデータ未発見時のエラー処理確認（未実装のため失敗予定）");

            LogTestResult("エラーケース処理テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキルクールダウンリアルタイム処理テスト
        /// </summary>
        private IEnumerator TestSkillCooldownRealtime()
        {
            LogDebug("スキルクールダウンリアルタイム処理テスト開始");

            // 期待値: IsSkillReadyメソッドの動作確認
            AssertTest(false, "IsSkillReadyメソッドの初回呼び出し確認（未実装のため失敗予定）");
            AssertTest(false, "クールダウン中のIsSkillReady=false確認（未実装のため失敗予定）");
            AssertTest(false, "クールダウン完了後のIsSkillReady=true確認（未実装のため失敗予定）");

            // 期待値: UseSkillメソッドの時刻記録
            AssertTest(false, "UseSkillメソッドの時刻記録確認（未実装のため失敗予定）");
            AssertTest(false, "SkillLastUsedTimeの更新確認（未実装のため失敗予定）");
            AssertTest(false, "SkillCooldownsの設定確認（未実装のため失敗予定）");

            // 期待値: Time.timeを使用したリアルタイム処理
            AssertTest(false, "Time.time基準のクールダウン計算確認（未実装のため失敗予定）");
            AssertTest(false, "0.1秒間隔処理での経過時間計算確認（未実装のため失敗予定）");

            LogTestResult("スキルクールダウンリアルタイム処理テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキル効果詳細プロパティテスト
        /// </summary>
        private IEnumerator TestSkillEffectProperties()
        {
            LogDebug("スキル効果詳細プロパティテスト開始");

            // 期待値: 基本プロパティの存在確認
            AssertTest(false, "BaseValueプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "ScalingRatioプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "ScalingSourceプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "TargetTypeプロパティの確認（未実装のため失敗予定）");

            // 期待値: 継続効果プロパティの確認
            AssertTest(false, "Durationプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "IsPermanentプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "TickIntervalプロパティの確認（未実装のため失敗予定）");

            // 期待値: 確率・条件プロパティの確認
            AssertTest(false, "SuccessChanceプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "TriggerConditionsプロパティの確認（未実装のため失敗予定）");

            // 期待値: スタック・重複プロパティの確認
            AssertTest(false, "CanStackプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "MaxStacksプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "StackTypeプロパティの確認（未実装のため失敗予定）");

            // 期待値: 視覚効果プロパティの確認
            AssertTest(false, "EffectAnimationIdプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "EffectColorプロパティの確認（未実装のため失敗予定）");
            AssertTest(false, "ShowFloatingTextプロパティの確認（未実装のため失敗予定）");

            LogTestResult("スキル効果詳細プロパティテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// CharacterInventoryManager統合テスト（仕様書5.1対応）
        /// </summary>
        private IEnumerator TestCharacterInventoryIntegration()
        {
            LogDebug("CharacterInventoryManager統合テスト開始");

            // 期待値: FuseCharactersメソッドとの統合
            AssertTest(false, "FuseCharactersメソッドの存在確認（未実装のため失敗予定）");

            // 期待値: 合成レベル計算とスキル習得の連携
            AssertTest(false, "合成レベル計算後のスキル習得確認（未実装のため失敗予定）");

            // 期待値: CharacterFusionResultの作成
            AssertTest(false, "CharacterFusionResultクラスの存在確認（未実装のため失敗予定）");
            AssertTest(false, "SkillUnlocksプロパティの設定確認（未実装のため失敗予定）");

            // 期待値: 複数レベル上昇による複数スキル習得の統合処理
            AssertTest(false, "合成による複数レベル上昇でのスキル習得確認（未実装のため失敗予定）");

            LogTestResult("CharacterInventoryManager統合テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テスト用アサーションメソッド
        /// </summary>
        /// <param name="condition">テスト条件</param>
        /// <param name="message">テストメッセージ</param>
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
        /// <param name="condition">テスト条件</param>
        /// <param name="message">テストメッセージ</param>
        /// <param name="actualValue">実際の値</param>
        /// <param name="expectedValue">期待値</param>
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
        /// <param name="condition">待機条件</param>
        /// <param name="timeoutSeconds">タイムアウト時間（秒）</param>
        /// <param name="conditionDescription">条件の説明</param>
        /// <returns>条件が満たされたかどうか</returns>
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
        /// デバッグ用スキル情報表示
        /// </summary>
        [ContextMenu("Debug Print Skill Info")]
        public void DebugPrintSkillInfo()
        {
            if (testCharacter != null)
            {
                ReportInfo($"テストキャラクター: {testCharacter.CharacterData.CharacterName}, レベル: {testCharacter.CurrentLevel}");
            }
            else
            {
                ReportWarning("テストキャラクターが存在しません");
            }
        }

        /// <summary>
        /// テストスキル習得の実行
        /// </summary>
        [ContextMenu("Test Skill Unlock")]
        public void TestSkillUnlock()
        {
            if (testCharacter != null)
            {
                ReportInfo("スキル習得テストを実行します（現在は未実装のため失敗します）");
                // 実装後: var result = testCharacter.LevelUp(3);
            }
            else
            {
                ReportWarning("テストキャラクターが存在しません");
            }
        }
    }
}