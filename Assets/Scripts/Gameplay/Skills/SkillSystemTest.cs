using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;
using System.Linq;
using System;

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
            
            // Phase 2: 段階的実装
            yield return StartCoroutine(TestSkillUnlockResultValidation());
            
            // Phase 2: 段階的実装（続き）
            yield return StartCoroutine(TestCharacterSkillCooldown());
            
            // Phase 2: 段階的実装（続き）
            yield return StartCoroutine(TestErrorCaseHandling());
            
            // Phase 2: 段階的実装（続き）
            yield return StartCoroutine(TestSkillCooldownRealtime());
            
            // Phase 2: 段階的実装（続き）
            yield return StartCoroutine(TestCharacterInventoryIntegration());
            
            // Phase 2: 段階的実装（スキル効果プロパティ）
            yield return StartCoroutine(TestSkillEffectProperties());
            
            // 未実装部分（コメントアウト）
            // yield return StartCoroutine(TestBasicSkillEffects());

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
        /// スキルクールダウン管理テスト（Character内包方式、仕様書2.1対応）
        /// </summary>
        private IEnumerator TestCharacterSkillCooldown()
        {
            LogDebug("スキルクールダウン管理テスト開始");

            // テスト用キャラクター作成（レベル10で全スキル習得済み）
            var testCharacter = new Character();
            if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                testCharacter = new Character(characterDatabase.AllCharacters[0], 10);
            }

            // 期待値: CharacterSkillCooldownsクラスが存在すること
            AssertTest(testCharacter.SkillCooldowns != null, "CharacterSkillCooldownsクラスの存在確認");

            if (testCharacter.SkillCooldowns == null)
            {
                LogTestResult("CharacterSkillCooldownsが初期化されていないため、以降のテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            var skillCooldowns = testCharacter.SkillCooldowns;

            // 期待値: 初期状態では全スキルが使用可能
            float testTime = 0f;
            AssertTest(skillCooldowns.IsSkillReady(0, testTime), "初期状態でスキルスロット0が使用可能であること");
            AssertTest(skillCooldowns.IsSkillReady(1, testTime), "初期状態でスキルスロット1が使用可能であること");
            AssertTest(skillCooldowns.IsSkillReady(2, testTime), "初期状態でスキルスロット2が使用可能であること");

            // テスト開始前にクールダウンをリセット
            skillCooldowns.ResetAllCooldowns();

            // 期待値: スキル使用でクールダウン開始
            float cooldownTime = 3.0f;
            skillCooldowns.UseSkill(0, testTime, cooldownTime);
            
            AssertTest(!skillCooldowns.IsSkillReady(0, testTime), "スキル使用直後は使用不可であること");
            AssertTest(skillCooldowns.IsSkillReady(1, testTime), "他のスキルスロットは影響を受けないこと");

            // 期待値: クールダウン時間経過前は使用不可
            float halfCooldownTime = testTime + (cooldownTime / 2f);
            AssertTest(!skillCooldowns.IsSkillReady(0, halfCooldownTime), "クールダウン半分経過時点では使用不可であること");

            // 期待値: クールダウン時間経過後は使用可能
            float afterCooldownTime = testTime + cooldownTime + 0.1f;
            AssertTest(skillCooldowns.IsSkillReady(0, afterCooldownTime), "クールダウン完了後は使用可能であること");

            // 期待値: 複数スキルの個別クールダウン管理
            float currentTime = 10f;
            skillCooldowns.ResetAllCooldowns(); // テスト用リセット
            skillCooldowns.UseSkill(0, currentTime, 2.0f); // 2秒クールダウン
            skillCooldowns.UseSkill(1, currentTime, 5.0f); // 5秒クールダウン
            
            float checkTime1 = currentTime + 3.0f; // 3秒後
            AssertTest(skillCooldowns.IsSkillReady(0, checkTime1), "短いクールダウンは先に完了すること");
            AssertTest(!skillCooldowns.IsSkillReady(1, checkTime1), "長いクールダウンはまだ未完了であること");
            
            float checkTime2 = currentTime + 6.0f; // 6秒後
            AssertTest(skillCooldowns.IsSkillReady(0, checkTime2), "短いクールダウンは継続して使用可能であること");
            AssertTest(skillCooldowns.IsSkillReady(1, checkTime2), "長いクールダウンも完了して使用可能であること");

            // 期待値: 残りクールダウン時間の取得
            skillCooldowns.ResetAllCooldowns();
            skillCooldowns.UseSkill(2, 20f, 4.0f); // 4秒クールダウン
            
            float remaining1 = skillCooldowns.GetRemainingCooldown(2, 21f); // 1秒後
            AssertTestDetailed(remaining1 > 2.9f && remaining1 < 3.1f, "残りクールダウン時間が正確であること", remaining1, 3.0f);
            
            float remaining2 = skillCooldowns.GetRemainingCooldown(2, 25f); // 5秒後（完了後）
            AssertTestDetailed(remaining2 == 0f, "クールダウン完了後の残り時間が0であること", remaining2, 0f);

            // 期待値: Character統合メソッドの確認
            skillCooldowns.ResetAllCooldowns();
            AssertTest(testCharacter.CanUseSkill(0, 30f), "Character.CanUseSkillが正常に動作すること");
            
            // 期待値: Character.UseSkillの確認（実際にスキルが習得されている場合）
            if (testCharacter.SkillProgression.IsSkillUnlocked(0))
            {
                bool useResult = testCharacter.UseSkill(0, 30f);
                AssertTest(useResult, "Character.UseSkillが成功すること");
                AssertTest(!testCharacter.CanUseSkill(0, 30f), "スキル使用後はクールダウンが開始されること");
            }

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

            // テスト用スキル作成
            var testSkill = new Skill("テストスキル", "テスト用のスキル", 101, 3);

            // SkillUnlockResultクラスの存在確認
            var skillUnlockResult = new SkillUnlockResult(3, 0, testSkill, "テストキャラクター");
            AssertTest(skillUnlockResult != null, "SkillUnlockResultクラスの存在確認");

            if (skillUnlockResult == null)
            {
                LogTestResult("SkillUnlockResultが作成できないため、以降のテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // プロパティ検証
            AssertTestDetailed(skillUnlockResult.UnlockLevel == 3, "UnlockLevelプロパティの確認", skillUnlockResult.UnlockLevel, 3);
            AssertTestDetailed(skillUnlockResult.SkillSlot == 0, "SkillSlotプロパティの確認", skillUnlockResult.SkillSlot, 0);
            AssertTest(skillUnlockResult.UnlockedSkill == testSkill, "UnlockedSkillプロパティの確認");
            AssertTest(skillUnlockResult.CharacterName == "テストキャラクター", "CharacterNameプロパティの確認");

            // 妥当性確認
            AssertTest(skillUnlockResult.IsValid(), "SkillUnlockResultの妥当性確認");

            // ToString()メソッドの出力フォーマット検証
            // 仕様書4.2: "{CharacterName}がレベル{UnlockLevel}でスキル「{UnlockedSkill.SkillName}」を習得しました"
            string expectedFormat = "テストキャラクターがレベル3でスキル「テストスキル」を習得しました";
            string actualFormat = skillUnlockResult.ToString();
            AssertTest(actualFormat == expectedFormat, "ToString()メソッドの出力フォーマット確認");
            
            if (actualFormat != expectedFormat)
            {
                Debug.LogWarning($"[SkillSystemTest] フォーマット不一致 - 期待値: {expectedFormat}, 実際: {actualFormat}");
            }

            // LevelUpメソッドとの統合テスト
            var skillProgression = new CharacterSkillProgression(1);
            var levelUpResults = skillProgression.LevelUp(7, "統合テストキャラ");
            
            AssertTestDetailed(levelUpResults.Count == 2, "レベル1→7でのSkillUnlockResult数確認", levelUpResults.Count, 2);
            
            if (levelUpResults.Count >= 1)
            {
                var firstResult = levelUpResults[0];
                AssertTestDetailed(firstResult.UnlockLevel == 3, "最初の習得レベル確認", firstResult.UnlockLevel, 3);
                AssertTest(firstResult.CharacterName == "統合テストキャラ", "キャラクター名の設定確認");
                AssertTest(firstResult.IsValid(), "習得結果の妥当性確認");
            }

            if (levelUpResults.Count >= 2)
            {
                var secondResult = levelUpResults[1];
                AssertTestDetailed(secondResult.UnlockLevel == 6, "2番目の習得レベル確認", secondResult.UnlockLevel, 6);
            }

            LogTestResult("SkillUnlockResult検証テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// エラーケース処理テスト
        /// スキルシステム全体の堅牢性とエラーハンドリングを検証
        /// </summary>
        private IEnumerator TestErrorCaseHandling()
        {
            LogDebug("エラーケース処理テスト開始");

            // テスト用キャラクター作成
            var testCharacter = new Character();
            if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                testCharacter = new Character(characterDatabase.AllCharacters[0], 5);
            }

            // === CharacterSkillProgression エラーケース ===

            // 期待値: 無効レベルでのLevelUp拒否
            var skillProgression = new CharacterSkillProgression(5);
            var negativeResults = skillProgression.LevelUp(-1, "テストキャラ");
            AssertTestDetailed(negativeResults.Count == 0, "負のレベルへのレベルアップは拒否される", negativeResults.Count, 0);

            var sameResults = skillProgression.LevelUp(5, "テストキャラ");
            AssertTestDetailed(sameResults.Count == 0, "現在レベル以下へのレベルアップは拒否される", sameResults.Count, 0);

            var lowerResults = skillProgression.LevelUp(3, "テストキャラ");
            AssertTestDetailed(lowerResults.Count == 0, "現在レベルより低いレベルへのレベルアップは拒否される", lowerResults.Count, 0);

            // 期待値: 無効スキルスロットでのスキル習得拒否
            var testSkill = new Skill("無効テストスキル", "無効スロットテスト", 999, 3);
            bool invalidSlotResult1 = skillProgression.UnlockSkill(-1, testSkill);
            AssertTest(!invalidSlotResult1, "負のスキルスロットでのスキル習得は拒否される");

            bool invalidSlotResult2 = skillProgression.UnlockSkill(99, testSkill);
            AssertTest(!invalidSlotResult2, "存在しないスキルスロットでのスキル習得は拒否される");

            // 期待値: nullスキルでの習得拒否
            bool nullSkillResult = skillProgression.UnlockSkill(0, null);
            AssertTest(!nullSkillResult, "nullスキルでのスキル習得は拒否される");

            // === CharacterSkillCooldowns エラーケース ===

            var skillCooldowns = new CharacterSkillCooldowns();

            // 期待値: 無効スキルスロットでのクールダウン操作
            skillCooldowns.UseSkill(-1, 10f, 3f); // エラーログ出力されるが、クラッシュしない
            bool negativeSlotReady = skillCooldowns.IsSkillReady(-1, 11f);
            AssertTest(negativeSlotReady, "無効スキルスロットは常に使用可能として扱われる");

            // 期待値: 負のクールダウン時間の処理
            skillCooldowns.UseSkill(0, 10f, -5f); // 自動的に0に修正される
            bool negativeCooldownReady = skillCooldowns.IsSkillReady(0, 10f);
            AssertTest(negativeCooldownReady, "負のクールダウン時間は0に修正され即座に使用可能");

            // === Character統合メソッド エラーケース ===

            // 期待値: 未習得スキルの使用拒否
            testCharacter.SkillProgression.ResetAllSkills(); // 全スキルリセット
            bool unlockedSkillUse = testCharacter.UseSkill(0, 20f);
            AssertTest(!unlockedSkillUse, "未習得スキルの使用は拒否される");

            bool unlockedSkillCan = testCharacter.CanUseSkill(0, 20f);
            AssertTest(!unlockedSkillCan, "未習得スキルは使用不可として判定される");

            // 期待値: 無効スキルスロットでの操作
            bool invalidSlotCan = testCharacter.CanUseSkill(-1, 20f);
            AssertTest(!invalidSlotCan, "無効スキルスロットは使用不可として判定される");

            bool invalidSlotUse = testCharacter.UseSkill(99, 20f);
            AssertTest(!invalidSlotUse, "存在しないスキルスロットの使用は拒否される");

            // === SkillUnlockResult エラーケース ===

            // 期待値: 無効データでのSkillUnlockResult
            var invalidResult1 = new SkillUnlockResult(0, 0, null, "テストキャラ");
            AssertTest(!invalidResult1.IsValid(), "nullスキルを持つSkillUnlockResultは無効");

            var invalidResult2 = new SkillUnlockResult(-1, 0, testSkill, "テストキャラ");
            AssertTest(!invalidResult2.IsValid(), "負のレベルを持つSkillUnlockResultは無効");

            var invalidResult3 = new SkillUnlockResult(3, -1, testSkill, "テストキャラ");
            AssertTest(!invalidResult3.IsValid(), "負のスキルスロットを持つSkillUnlockResultは無効");

            var invalidResult4 = new SkillUnlockResult(3, 0, testSkill, "");
            AssertTest(!invalidResult4.IsValid(), "空の名前を持つSkillUnlockResultは無効");

            var invalidResult5 = new SkillUnlockResult(3, 0, testSkill, null);
            AssertTest(!invalidResult5.IsValid(), "null名前を持つSkillUnlockResultは無効");

            // 期待値: 無効SkillUnlockResultのToString()
            string invalidToString = invalidResult1.ToString();
            AssertTest(invalidToString == "無効なスキル習得結果", "無効SkillUnlockResultは適切なエラーメッセージを返す");

            // === 境界値テスト ===

            // 期待値: 最大レベル以上への対応
            if (testCharacter.CharacterData != null)
            {
                int maxLevel = testCharacter.CharacterData.MaxLevel;
                var overMaxResults = testCharacter.SkillProgression.LevelUp(maxLevel + 10, "境界値テスト");
                // レベル上限を超えても、有効な習得レベル範囲のスキルは習得される
                AssertTest(overMaxResults.Count >= 0, "最大レベル超過でもクラッシュせずに処理される");
            }

            // 期待値: ゼロ時間でのクールダウン処理
            skillCooldowns.ResetAllCooldowns();
            skillCooldowns.UseSkill(0, 30f, 0f); // 0秒クールダウン
            bool zeroCooldownReady = skillCooldowns.IsSkillReady(0, 30f);
            AssertTest(zeroCooldownReady, "0秒クールダウンは即座に使用可能");

            LogTestResult("エラーケース処理テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキルクールダウンリアルタイム処理テスト
        /// Time.timeを使用した実戦的なクールダウン動作を検証
        /// </summary>
        private IEnumerator TestSkillCooldownRealtime()
        {
            LogDebug("スキルクールダウンリアルタイム処理テスト開始");

            // テスト用キャラクター作成（レベル10で全スキル習得済み）
            var testCharacter = new Character();
            if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                testCharacter = new Character(characterDatabase.AllCharacters[0], 10);
            }

            // 期待値: Character統合での初期状態確認
            AssertTest(testCharacter.SkillCooldowns != null, "CharacterSkillCooldownsの初期化確認");
            AssertTest(testCharacter.SkillProgression != null, "CharacterSkillProgressionの初期化確認");

            if (testCharacter.SkillCooldowns == null)
            {
                LogTestResult("CharacterSkillCooldownsが初期化されていないため、リアルタイムテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // テスト前の状態リセット
            testCharacter.SkillCooldowns.ResetAllCooldowns();

            // === Time.timeベースでの初期状態確認 ===
            
            float testStartTime = Time.time;
            AssertTest(testCharacter.CanUseSkill(0), "Time.time使用時の初期状態でのスキル使用可能確認");
            AssertTest(testCharacter.SkillCooldowns.IsSkillReady(0, Time.time), "Time.time使用時のIsSkillReady初期状態確認");

            // === リアルタイムクールダウンテスト（テスト用スキル） ===

            // テスト用の短いクールダウンスキルを作成して手動設定
            var testSkill1sec = new Skill("1秒テストスキル", "リアルタイムテスト用", 9001, 3);
            testCharacter.SkillProgression.UnlockSkill(0, testSkill1sec); // スロット0に設定
            
            // テスト用クールダウン時間を直接使用（Character.UseSkillを迂回）
            float testCooldownTime = 1.0f; // テスト専用の1秒クールダウン
            testCharacter.SkillCooldowns.UseSkill(0, Time.time, testCooldownTime);
            
            LogDebug($"テスト用クールダウン開始: {testCooldownTime}秒");
            
            // 使用直後は使用不可
            AssertTest(!testCharacter.SkillCooldowns.IsSkillReady(0, Time.time), "スキル使用直後はクールダウン中で使用不可");
            
            float afterUseTime = Time.time;
            float remaining1 = testCharacter.SkillCooldowns.GetRemainingCooldown(0, afterUseTime);
            AssertTest(remaining1 > 0f, "使用直後は残りクールダウン時間が0より大きい");
            AssertTest(remaining1 <= testCooldownTime + 0.1f, "残りクールダウン時間がテスト設定値以下であること");

            LogDebug($"クールダウン開始: 残り時間 {remaining1:F2}秒");

            // 0.5秒待機（クールダウン途中）
            yield return new WaitForSeconds(0.5f);
            
            bool stillCooldown = !testCharacter.SkillCooldowns.IsSkillReady(0, Time.time);
            AssertTest(stillCooldown, "0.5秒経過時点ではまだクールダウン中");
            
            float midTime = Time.time;
            float remaining2 = testCharacter.SkillCooldowns.GetRemainingCooldown(0, midTime);
            AssertTest(remaining2 < remaining1, "時間経過により残りクールダウン時間が減少している");
            
            LogDebug($"0.5秒経過: 残り時間 {remaining2:F2}秒");

            // さらに0.7秒待機（クールダウン完了予定）
            yield return new WaitForSeconds(0.7f);
            
            bool cooldownComplete = testCharacter.SkillCooldowns.IsSkillReady(0, Time.time);
            AssertTest(cooldownComplete, "1.2秒経過後はクールダウンが完了して使用可能");
            
            float endTime = Time.time;
            float remaining3 = testCharacter.SkillCooldowns.GetRemainingCooldown(0, endTime);
            AssertTestDetailed(remaining3 == 0f, "クールダウン完了後の残り時間が0", remaining3, 0f);

            LogDebug($"1.2秒経過: 残り時間 {remaining3:F2}秒");

            // === 複数スキル同時クールダウンテスト ===

            testCharacter.SkillCooldowns.ResetAllCooldowns();
            
            float simultaneousStart = Time.time;
            
            // テスト用スキルを作成（異なるクールダウン時間）
            var testSkill1_5sec = new Skill("1.5秒テストスキル", "複数テスト用", 9002, 6);
            var testSkill2sec = new Skill("2秒テストスキル", "複数テスト用", 9003, 10);
            
            testCharacter.SkillProgression.UnlockSkill(1, testSkill1_5sec); // スロット1に設定
            testCharacter.SkillProgression.UnlockSkill(2, testSkill2sec);   // スロット2に設定
            
            // 複数スキルを手動でクールダウン開始（異なる時間）
            float cooldown1 = 1.5f;
            float cooldown2 = 2.0f;
            testCharacter.SkillCooldowns.UseSkill(1, simultaneousStart, cooldown1);
            testCharacter.SkillCooldowns.UseSkill(2, simultaneousStart, cooldown2);
            
            LogDebug($"複数クールダウン開始: スキル1={cooldown1}秒, スキル2={cooldown2}秒");
            
            // 両方ともクールダウン中
            AssertTest(!testCharacter.SkillCooldowns.IsSkillReady(1, Time.time), "同時使用後スキル1はクールダウン中");
            AssertTest(!testCharacter.SkillCooldowns.IsSkillReady(2, Time.time), "同時使用後スキル2はクールダウン中");

            // 1秒待機
            yield return new WaitForSeconds(1.0f);
            
            // まだ両方ともクールダウン中
            AssertTest(!testCharacter.SkillCooldowns.IsSkillReady(1, Time.time), "1秒経過でもスキル1はクールダウン中");
            AssertTest(!testCharacter.SkillCooldowns.IsSkillReady(2, Time.time), "1秒経過でもスキル2はクールダウン中");

            // さらに0.7秒待機（合計1.7秒、スキル1完了予定）
            yield return new WaitForSeconds(0.7f);
            
            bool skill1Ready = testCharacter.SkillCooldowns.IsSkillReady(1, Time.time);
            bool skill2Ready = testCharacter.SkillCooldowns.IsSkillReady(2, Time.time);
            
            AssertTest(skill1Ready, "1.7秒経過後スキル1（1.5秒CD）は使用可能");
            AssertTest(!skill2Ready, "1.7秒経過時点でスキル2（2秒CD）はまだクールダウン中");

            // さらに0.5秒待機（合計2.2秒、スキル2完了予定）
            yield return new WaitForSeconds(0.5f);
            
            AssertTest(testCharacter.SkillCooldowns.IsSkillReady(1, Time.time), "2.2秒経過後スキル1は継続して使用可能");
            AssertTest(testCharacter.SkillCooldowns.IsSkillReady(2, Time.time), "2.2秒経過後スキル2（2秒CD）も使用可能");

            // === 精度テスト（Time.deltaTimeとの整合性） ===

            testCharacter.SkillCooldowns.ResetAllCooldowns();
            
            float precisionStart = Time.time;
            testCharacter.SkillCooldowns.UseSkill(0, precisionStart, 1.0f); // 正確に1秒クールダウン
            
            // 0.1秒刻みで確認
            for (int i = 1; i <= 12; i++) // 1.2秒まで確認
            {
                yield return new WaitForSeconds(0.1f);
                
                float checkTime = Time.time;
                bool isReady = testCharacter.SkillCooldowns.IsSkillReady(0, checkTime);
                float elapsed = checkTime - precisionStart;
                
                if (elapsed < 1.0f)
                {
                    AssertTest(!isReady, $"{elapsed:F1}秒経過時点ではクールダウン中");
                }
                else
                {
                    AssertTest(isReady, $"{elapsed:F1}秒経過時点ではクールダウン完了");
                }
                
                LogDebug($"精度テスト {i}: 経過{elapsed:F2}秒, 使用可能: {isReady}");
            }

            // === Unity統合テスト（デフォルト引数動作） ===

            testCharacter.SkillCooldowns.ResetAllCooldowns();
            
            // テスト用短時間クールダウンスキルを使用
            var testSkillShort = new Skill("短時間テストスキル", "Unity統合テスト用", 9004, 3);
            testCharacter.SkillProgression.UnlockSkill(0, testSkillShort); // スロット0に再設定
            
            // CharacterSkillCooldowns.UseSkillで直接短時間クールダウンを設定
            float shortCooldown = 1.5f;
            testCharacter.SkillCooldowns.UseSkill(0, Time.time, shortCooldown);
            
            // Time.timeデフォルト引数の動作確認
            bool defaultTimeCan = testCharacter.CanUseSkill(0); // currentTime = -1f → Time.time使用
            AssertTest(!defaultTimeCan, "デフォルト引数（Time.time）でのクールダウン中判定");
            
            yield return new WaitForSeconds(shortCooldown + 0.2f); // クールダウン完了まで待機
            
            bool afterDefaultCan = testCharacter.CanUseSkill(0);
            AssertTest(afterDefaultCan, "デフォルト引数（Time.time）でのクールダウン完了判定");
            
            // 実際にCharacter.UseSkillでの使用テスト（但し、スキルのCooldownTimeプロパティは3秒）
            // NOTE: この部分は実際のスキルクールダウン時間（3秒）が使用される
            LogDebug("Character.UseSkillテストはスキルのCooldownTimeプロパティ（デフォルト3秒）を使用します");

            float totalTestTime = Time.time - testStartTime;
            LogDebug($"リアルタイムテスト総時間: {totalTestTime:F2}秒");

            LogTestResult("スキルクールダウンリアルタイム処理テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// スキル効果詳細プロパティテスト
        /// 仕様書3.1-3.2に基づくSkillEffectシステムのプロパティ検証
        /// </summary>
        private IEnumerator TestSkillEffectProperties()
        {
            LogDebug("スキル効果詳細プロパティテスト開始");

            // === 基本Enumの存在確認 ===
            
            // ScalingAttribute enum確認
            var scalingTest = ScalingAttribute.Attack;
            AssertTest(scalingTest == ScalingAttribute.Attack, "ScalingAttribute enumの存在確認");
            AssertTest(Enum.IsDefined(typeof(ScalingAttribute), ScalingAttribute.Level), "ScalingAttribute.Levelの定義確認");
            AssertTest(Enum.IsDefined(typeof(ScalingAttribute), ScalingAttribute.MaxHP), "ScalingAttribute.MaxHPの定義確認");

            // StackBehavior enum確認
            var stackTest = StackBehavior.Refresh;
            AssertTest(stackTest == StackBehavior.Refresh, "StackBehavior enumの存在確認");
            AssertTest(Enum.IsDefined(typeof(StackBehavior), StackBehavior.Intensify), "StackBehavior.Intensifyの定義確認");

            // SkillTargetType enum確認
            var targetTest = SkillTargetType.SingleEnemy;
            AssertTest(targetTest == SkillTargetType.SingleEnemy, "SkillTargetType enumの存在確認");
            AssertTest(Enum.IsDefined(typeof(SkillTargetType), SkillTargetType.AllAllies), "SkillTargetType.AllAlliesの定義確認");

            // DamageType enum確認
            var damageTest = DamageType.Physical;
            AssertTest(damageTest == DamageType.Physical, "DamageType enumの存在確認");
            AssertTest(Enum.IsDefined(typeof(DamageType), DamageType.Fire), "DamageType.Fireの定義確認");

            yield return new WaitForSeconds(0.1f);

            // === DamageEffect クラステスト ===
            
            LogDebug("DamageEffect プロパティテスト開始");
            
            var damageEffect = new DamageEffect("テストダメージ", "テスト用のダメージ効果", 100f, DamageType.Magical);
            AssertTest(damageEffect != null, "DamageEffectクラスの作成確認");
            
            if (damageEffect != null)
            {
                // 基本プロパティ確認
                AssertTestDetailed(damageEffect.EffectName == "テストダメージ", "DamageEffect.EffectNameプロパティ", damageEffect.EffectName, "テストダメージ");
                AssertTestDetailed(damageEffect.BaseValue == 100f, "DamageEffect.BaseValueプロパティ", damageEffect.BaseValue, 100f);
                AssertTestDetailed(damageEffect.EffectType == SkillEffectType.Damage, "DamageEffect.EffectTypeプロパティ", damageEffect.EffectType, SkillEffectType.Damage);
                AssertTestDetailed(damageEffect.DamageType == DamageType.Magical, "DamageEffect.DamageTypeプロパティ", damageEffect.DamageType, DamageType.Magical);
                
                // 対象・範囲プロパティ確認
                AssertTestDetailed(damageEffect.TargetType == SkillTargetType.SingleEnemy, "DamageEffect.TargetTypeプロパティ", damageEffect.TargetType, SkillTargetType.SingleEnemy);
                AssertTestDetailed(damageEffect.MaxTargets == 1, "DamageEffect.MaxTargetsプロパティ", damageEffect.MaxTargets, 1);
                
                // スケーリングプロパティ確認
                damageEffect.ScalingRatio = 0.5f;
                damageEffect.ScalingSource = ScalingAttribute.Attack;
                AssertTestDetailed(damageEffect.ScalingRatio == 0.5f, "DamageEffect.ScalingRatioプロパティ", damageEffect.ScalingRatio, 0.5f);
                AssertTestDetailed(damageEffect.ScalingSource == ScalingAttribute.Attack, "DamageEffect.ScalingSourceプロパティ", damageEffect.ScalingSource, ScalingAttribute.Attack);
                
                // ダメージ固有プロパティ確認
                damageEffect.CriticalChance = 0.2f;
                damageEffect.CriticalMultiplier = 2.5f;
                damageEffect.IgnoreDefense = true;
                AssertTestDetailed(damageEffect.CriticalChance == 0.2f, "DamageEffect.CriticalChanceプロパティ", damageEffect.CriticalChance, 0.2f);
                AssertTestDetailed(damageEffect.CriticalMultiplier == 2.5f, "DamageEffect.CriticalMultiplierプロパティ", damageEffect.CriticalMultiplier, 2.5f);
                AssertTest(damageEffect.IgnoreDefense, "DamageEffect.IgnoreDefenseプロパティ");
            }

            yield return new WaitForSeconds(0.1f);

            // === HealEffect クラステスト ===
            
            LogDebug("HealEffect プロパティテスト開始");
            
            var healEffect = new HealEffect("テスト回復", "テスト用の回復効果", 50f, false);
            AssertTest(healEffect != null, "HealEffectクラスの作成確認");
            
            if (healEffect != null)
            {
                // 基本プロパティ確認
                AssertTestDetailed(healEffect.EffectName == "テスト回復", "HealEffect.EffectNameプロパティ", healEffect.EffectName, "テスト回復");
                AssertTestDetailed(healEffect.BaseValue == 50f, "HealEffect.BaseValueプロパティ", healEffect.BaseValue, 50f);
                AssertTestDetailed(healEffect.EffectType == SkillEffectType.Heal, "HealEffect.EffectTypeプロパティ", healEffect.EffectType, SkillEffectType.Heal);
                AssertTestDetailed(healEffect.TargetType == SkillTargetType.SingleAlly, "HealEffect.TargetTypeプロパティ", healEffect.TargetType, SkillTargetType.SingleAlly);
                
                // 回復固有プロパティ確認
                AssertTest(!healEffect.HealMP, "HealEffect.HealMPプロパティ（HP回復）");
                healEffect.HealMP = true;
                AssertTest(healEffect.HealMP, "HealEffect.HealMPプロパティ設定後");
                
                healEffect.CanOverheal = true;
                healEffect.OverhealDecayRate = 0.15f;
                AssertTest(healEffect.CanOverheal, "HealEffect.CanOverhealプロパティ");
                AssertTestDetailed(healEffect.OverhealDecayRate == 0.15f, "HealEffect.OverhealDecayRateプロパティ", healEffect.OverhealDecayRate, 0.15f);
            }

            yield return new WaitForSeconds(0.1f);

            // === StatModifierEffect クラステスト ===
            
            LogDebug("StatModifierEffect プロパティテスト開始");
            
            var statEffect = new StatModifierEffect("攻撃力強化", "攻撃力を上昇させる", StatType.Attack, 25f, 60f, false);
            AssertTest(statEffect != null, "StatModifierEffectクラスの作成確認");
            
            if (statEffect != null)
            {
                // 基本プロパティ確認
                AssertTestDetailed(statEffect.EffectName == "攻撃力強化", "StatModifierEffect.EffectNameプロパティ", statEffect.EffectName, "攻撃力強化");
                AssertTestDetailed(statEffect.BaseValue == 25f, "StatModifierEffect.BaseValueプロパティ", statEffect.BaseValue, 25f);
                AssertTestDetailed(statEffect.EffectType == SkillEffectType.StatModifier, "StatModifierEffect.EffectTypeプロパティ", statEffect.EffectType, SkillEffectType.StatModifier);
                AssertTestDetailed(statEffect.Duration == 60f, "StatModifierEffect.Durationプロパティ", statEffect.Duration, 60f);
                
                // ステータス修正固有プロパティ確認
                AssertTestDetailed(statEffect.TargetStat == StatType.Attack, "StatModifierEffect.TargetStatプロパティ", statEffect.TargetStat, StatType.Attack);
                AssertTestDetailed(statEffect.ModifierType == ModifierType.Additive, "StatModifierEffect.ModifierTypeプロパティ", statEffect.ModifierType, ModifierType.Additive);
                AssertTest(!statEffect.IsDebuff, "StatModifierEffect.IsDebuffプロパティ（バフ）");
                
                // デバフテスト
                var debuffEffect = new StatModifierEffect("防御力低下", "防御力を低下させる", StatType.Defense, -15f, 30f, true);
                AssertTest(debuffEffect.IsDebuff, "StatModifierEffect.IsDebuffプロパティ（デバフ）");
                AssertTestDetailed(debuffEffect.TargetType == SkillTargetType.SingleEnemy, "デバフ対象タイプ", debuffEffect.TargetType, SkillTargetType.SingleEnemy);
            }

            yield return new WaitForSeconds(0.1f);

            // === 継続効果プロパティテスト ===
            
            LogDebug("継続効果プロパティテスト開始");
            
            var testEffect = new DamageEffect("継続ダメージ", "時間経過でダメージ", 10f);
            
            // 継続効果設定
            testEffect.Duration = 15f;
            testEffect.IsPermanent = false;
            testEffect.TickInterval = 2f;
            
            AssertTestDetailed(testEffect.Duration == 15f, "継続効果.Durationプロパティ", testEffect.Duration, 15f);
            AssertTest(!testEffect.IsPermanent, "継続効果.IsPermanentプロパティ");
            AssertTestDetailed(testEffect.TickInterval == 2f, "継続効果.TickIntervalプロパティ", testEffect.TickInterval, 2f);
            
            // 永続効果テスト
            testEffect.IsPermanent = true;
            AssertTest(testEffect.IsPermanent, "継続効果.IsPermanent設定後");

            yield return new WaitForSeconds(0.1f);

            // === 確率・条件プロパティテスト ===
            
            LogDebug("確率・条件プロパティテスト開始");
            
            testEffect.SuccessChance = 0.75f;
            AssertTestDetailed(testEffect.SuccessChance == 0.75f, "確率.SuccessChanceプロパティ", testEffect.SuccessChance, 0.75f);
            
            // 範囲外値のクランプテスト
            testEffect.SuccessChance = 1.5f; // 1.0でクランプされるべき
            AssertTestDetailed(testEffect.SuccessChance == 1.0f, "確率.SuccessChance上限クランプ", testEffect.SuccessChance, 1.0f);
            
            testEffect.SuccessChance = -0.5f; // 0.0でクランプされるべき
            AssertTestDetailed(testEffect.SuccessChance == 0.0f, "確率.SuccessChance下限クランプ", testEffect.SuccessChance, 0.0f);

            yield return new WaitForSeconds(0.1f);

            // === スタック・重複プロパティテスト ===
            
            LogDebug("スタック・重複プロパティテスト開始");
            
            testEffect.CanStack = true;
            testEffect.MaxStacks = 5;
            testEffect.StackType = StackBehavior.Intensify;
            
            AssertTest(testEffect.CanStack, "スタック.CanStackプロパティ");
            AssertTestDetailed(testEffect.MaxStacks == 5, "スタック.MaxStacksプロパティ", testEffect.MaxStacks, 5);
            AssertTestDetailed(testEffect.StackType == StackBehavior.Intensify, "スタック.StackTypeプロパティ", testEffect.StackType, StackBehavior.Intensify);
            
            // 無効値処理テスト
            testEffect.MaxStacks = 0; // 1で最小値制限されるべき
            AssertTestDetailed(testEffect.MaxStacks == 1, "スタック.MaxStacks最小値制限", testEffect.MaxStacks, 1);

            yield return new WaitForSeconds(0.1f);

            // === 視覚効果プロパティテスト ===
            
            LogDebug("視覚効果プロパティテスト開始");
            
            testEffect.EffectAnimationId = "explosion_effect";
            testEffect.EffectColor = Color.red;
            testEffect.ShowFloatingText = false;
            
            AssertTestDetailed(testEffect.EffectAnimationId == "explosion_effect", "視覚効果.EffectAnimationIdプロパティ", testEffect.EffectAnimationId, "explosion_effect");
            AssertTestDetailed(testEffect.EffectColor == Color.red, "視覚効果.EffectColorプロパティ", testEffect.EffectColor, Color.red);
            AssertTest(!testEffect.ShowFloatingText, "視覚効果.ShowFloatingTextプロパティ");

            yield return new WaitForSeconds(0.1f);

            // === 統合テスト（キャラクターとの連携） ===
            
            LogDebug("スキル効果統合テスト開始");
            
            // テストキャラクター作成
            var testCharacter = new Character();
            if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                testCharacter = new Character(characterDatabase.AllCharacters[0], 5);
            }
            
            // CanApplyテスト
            bool canApplyDamage = testEffect.CanApply(testCharacter, testCharacter);
            AssertTest(canApplyDamage, "SkillEffect.CanApplyメソッド（生存キャラクター）");
            
            // CalculateEffectiveValueテスト（スケーリング）
            testEffect.BaseValue = 50f;
            testEffect.ScalingRatio = 0.5f;
            testEffect.ScalingSource = ScalingAttribute.Level;
            
            float effectiveValue = testEffect.CalculateEffectiveValue(testCharacter);
            float expectedValue = 50f + (testCharacter.CurrentLevel * 0.5f);
            AssertTestDetailed(Mathf.Approximately(effectiveValue, expectedValue), "SkillEffect.CalculateEffectiveValueメソッド（スケーリング計算）", effectiveValue, expectedValue);

            LogTestResult("スキル効果詳細プロパティテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// CharacterInventoryManager統合テスト（仕様書5.1対応）
        /// </summary>
        private IEnumerator TestCharacterInventoryIntegration()
        {
            LogDebug("CharacterInventoryManager統合テスト開始");

            // CharacterInventoryManagerの取得
            var inventoryManager = FindObjectOfType<CharacterInventoryManager>();
            AssertTest(inventoryManager != null, "CharacterInventoryManagerクラスの存在確認");

            if (inventoryManager == null)
            {
                LogTestResult("CharacterInventoryManagerが見つからないため、統合テストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // === 合成前準備：テストキャラクター作成 ===

            // ベースキャラクター（レベル1）
            var baseCharacter = new Character();
            if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
            {
                baseCharacter = new Character(characterDatabase.AllCharacters[0], 1);
            }

            // 素材キャラクター（複数体）
            var materialCharacters = new List<Character>();
            for (int i = 0; i < 3; i++)
            {
                var material = new Character();
                if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
                {
                    material = new Character(characterDatabase.AllCharacters[0], 1);
                }
                materialCharacters.Add(material);
            }

            // インベントリに追加
            bool baseAdded = inventoryManager.AddCharacter(baseCharacter);
            AssertTest(baseAdded, "ベースキャラクターのインベントリ追加成功");

            var materialIds = new List<string>();
            foreach (var material in materialCharacters)
            {
                bool materialAdded = inventoryManager.AddCharacter(material);
                AssertTest(materialAdded, "素材キャラクターのインベントリ追加成功");
                materialIds.Add(material.InstanceId);
            }

            // === 合成前状態確認 ===

            int preLevel = baseCharacter.CurrentLevel;
            int preSkillCount = baseCharacter.SkillProgression.UnlockedSkillCount;
            
            LogDebug($"合成前状態: レベル{preLevel}, スキル{preSkillCount}個");

            // === キャラクター合成実行 ===

            bool fuseResult = inventoryManager.FuseCharacters(baseCharacter.InstanceId, materialIds, out Character resultCharacter);
            AssertTest(fuseResult, "FuseCharactersメソッドの実行成功");
            AssertTest(resultCharacter != null, "合成結果キャラクターの取得成功");

            if (resultCharacter == null)
            {
                LogTestResult("合成結果が取得できないため、以降のテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // === 合成後状態確認 ===

            int postLevel = resultCharacter.CurrentLevel;
            int postSkillCount = resultCharacter.SkillProgression.UnlockedSkillCount;
            
            LogDebug($"合成後状態: レベル{postLevel}, スキル{postSkillCount}個");

            // 期待値: レベルアップ発生
            AssertTest(postLevel > preLevel, "合成によりレベルアップが発生すること");

            // 期待値: スキル習得発生（レベル3以上到達時）
            if (postLevel >= 3)
            {
                AssertTest(postSkillCount > preSkillCount, "レベル3以上到達時はスキルが習得されること");
                
                // レベル別スキル習得確認
                if (postLevel >= 3)
                {
                    AssertTest(resultCharacter.SkillProgression.IsSkillUnlocked(0), "レベル3スキル（スロット0）が習得されていること");
                }
                if (postLevel >= 6)
                {
                    AssertTest(resultCharacter.SkillProgression.IsSkillUnlocked(1), "レベル6スキル（スロット1）が習得されていること");
                }
                if (postLevel >= 10)
                {
                    AssertTest(resultCharacter.SkillProgression.IsSkillUnlocked(2), "レベル10スキル（スロット2）が習得されていること");
                }
            }

            // === 複数レベル上昇テスト ===

            // 追加の大量経験値合成テスト
            var additionalMaterials = new List<Character>();
            for (int i = 0; i < 5; i++) // より多くの素材
            {
                var material = new Character();
                if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
                {
                    material = new Character(characterDatabase.AllCharacters[0], 1);
                }
                inventoryManager.AddCharacter(material);
                additionalMaterials.Add(material);
            }

            var additionalMaterialIds = additionalMaterials.Select(m => m.InstanceId).ToList();
            int preLevel2 = resultCharacter.CurrentLevel;
            int preSkillCount2 = resultCharacter.SkillProgression.UnlockedSkillCount;

            bool fuseResult2 = inventoryManager.FuseCharacters(resultCharacter.InstanceId, additionalMaterialIds, out Character finalResult);
            AssertTest(fuseResult2, "追加合成の実行成功");

            if (finalResult != null)
            {
                int finalLevel = finalResult.CurrentLevel;
                int finalSkillCount = finalResult.SkillProgression.UnlockedSkillCount;
                
                LogDebug($"最終状態: レベル{finalLevel}, スキル{finalSkillCount}個");

                // 期待値: 大量経験値による大幅レベルアップとスキル習得
                AssertTest(finalLevel > preLevel2, "追加合成により更なるレベルアップが発生すること");
                
                if (finalLevel >= 6 && preLevel2 < 6)
                {
                    AssertTest(finalSkillCount > preSkillCount2, "レベル6到達により新規スキルが習得されること");
                }

                // === 統合機能テスト ===

                // Character.AddExperience統合確認
                var testChar = new Character();
                if (characterDatabase != null && characterDatabase.AllCharacters.Count > 0)
                {
                    testChar = new Character(characterDatabase.AllCharacters[0], 1);
                }

                int preAddExpLevel = testChar.CurrentLevel;
                int preAddExpSkills = testChar.SkillProgression.UnlockedSkillCount;

                // 大量経験値追加（レベル3以上到達予定）
                int addedLevels = testChar.AddExperience(5000);
                
                AssertTest(addedLevels > 0, "AddExperienceメソッドによるレベルアップ成功");
                AssertTest(testChar.CurrentLevel > preAddExpLevel, "経験値追加によりレベルが上昇すること");
                
                if (testChar.CurrentLevel >= 3)
                {
                    AssertTest(testChar.SkillProgression.UnlockedSkillCount > preAddExpSkills, "AddExperience統合によりスキルが自動習得されること");
                }
            }

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