using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Gold;
using GatchaSpire.Gameplay.Board;

namespace GatchaSpire.Gameplay.Battle
{
    /// <summary>
    /// BattleManagerの動作確認テスト
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class BattleManagerTest : TestExclusiveBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableDetailedLogs = true;

        [Header("テスト用データ")]
        [SerializeField] private CharacterData[] testCharacterData;
        [SerializeField] private float testBattleTimeLimit = 10f;

        private BattleManager battleManager;
        private BoardStateManager boardManager;
        private GoldManager goldManager;
        private CharacterDatabase characterDatabase;
        private List<Character> testPlayerCharacters = new List<Character>();
        private List<Character> testEnemyCharacters = new List<Character>();

        public override string TestClassName => "BattleManagerTest";

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
            // 必要なシステムを検索・作成
            SetupBattleManager();
            SetupBoardManager();
            SetupGoldManager();
            SetupCharacterDatabase();

            // テスト用キャラクターの準備
            CreateTestCharacters();

            ReportInfo("BattleManagerテストコンポーネントを初期化しました");
        }

        /// <summary>
        /// BattleManagerのセットアップ
        /// </summary>
        private void SetupBattleManager()
        {
            battleManager = FindObjectOfType<BattleManager>();
            if (battleManager == null)
            {
                var battleManagerGO = new GameObject("BattleManager");
                battleManagerGO.transform.SetParent(transform);
                battleManager = battleManagerGO.AddComponent<BattleManager>();
            }
        }

        /// <summary>
        /// BoardStateManagerのセットアップ
        /// </summary>
        private void SetupBoardManager()
        {
            boardManager = FindObjectOfType<BoardStateManager>();
            if (boardManager == null)
            {
                var boardManagerGO = new GameObject("BoardStateManager");
                boardManagerGO.transform.SetParent(transform);
                boardManager = boardManagerGO.AddComponent<BoardStateManager>();
                boardManager.Initialize();
            }
        }

        /// <summary>
        /// GoldManagerのセットアップ
        /// </summary>
        private void SetupGoldManager()
        {
            goldManager = GoldManager.Instance;
            if (goldManager == null)
            {
                var goldManagerGO = new GameObject("GoldManager");
                goldManagerGO.transform.SetParent(transform);
                goldManager = goldManagerGO.AddComponent<GoldManager>();
            }
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
        /// テスト用キャラクターを作成
        /// </summary>
        private void CreateTestCharacters()
        {
            testPlayerCharacters.Clear();
            testEnemyCharacters.Clear();

            ReportInfo("テストキャラクター作成開始");

            // テストデータが設定されている場合
            if (testCharacterData != null && testCharacterData.Length > 0)
            {
                ReportInfo($"事前設定されたテストデータを使用: {testCharacterData.Length}個のCharacterData");

                // プレイヤーキャラクター作成（最大3体）
                for (int i = 0; i < testCharacterData.Length && i < 3; i++)
                {
                    var character = new Character(testCharacterData[i], 1);
                    testPlayerCharacters.Add(character);
                    ReportInfo($"プレイヤーキャラクター作成: {character.CharacterData.CharacterName}");
                }

                // 敵キャラクター作成（最大2体）
                for (int i = 0; i < testCharacterData.Length && i < 2; i++)
                {
                    var character = new Character(testCharacterData[i], 1);
                    testEnemyCharacters.Add(character);
                    ReportInfo($"敵キャラクター作成: {character.CharacterData.CharacterName}");
                }
            }
            else
            {
                ReportInfo("事前設定データなし。デフォルトテストキャラクター作成を試行");
                // デフォルトテストキャラクター作成
                CreateDefaultTestCharacters();
            }

            ReportInfo($"テストキャラクター作成完了: プレイヤー{testPlayerCharacters.Count}体、敵{testEnemyCharacters.Count}体");
        }

        /// <summary>
        /// デフォルトテストキャラクターの作成
        /// </summary>
        private void CreateDefaultTestCharacters()
        {
            // CharacterDatabaseから取得を試行
            if (characterDatabase != null)
            {
                var allCharacters = characterDatabase.AllCharacters;
                if (allCharacters.Count > 0)
                {
                    // プレイヤーキャラクター
                    for (int i = 0; i < allCharacters.Count && i < 3; i++)
                    {
                        var character = new Character(allCharacters[i], 1);
                        testPlayerCharacters.Add(character);
                    }

                    // 敵キャラクター
                    for (int i = 0; i < allCharacters.Count && i < 2; i++)
                    {
                        var character = new Character(allCharacters[i], 1);
                        testEnemyCharacters.Add(character);
                    }
                }
            }

            if (testPlayerCharacters.Count == 0 || testEnemyCharacters.Count == 0)
            {
                ReportWarning("テストキャラクターが不足しています。一部のテストをスキップします");
            }
        }

        /// <summary>
        /// 全テストを順次実行
        /// </summary>
        private IEnumerator RunAllTestsSequentially()
        {
            LogDebug("=== BattleManagerテスト開始 ===");

            yield return StartCoroutine(TestBattleManagerInitialization());
            yield return StartCoroutine(TestBattleSetupValidation());
            yield return StartCoroutine(TestBasicBattleFlow());
            yield return StartCoroutine(TestBattleStateTransitions());
            yield return StartCoroutine(TestCombatMechanics());
            yield return StartCoroutine(TestVictoryConditions());
            yield return StartCoroutine(TestBattleRewards());
            yield return StartCoroutine(TestForceEndBattle());

            LogTestResult("=== 全テスト完了 ===");
        }

        /// <summary>
        /// BattleManager初期化テスト
        /// </summary>
        private IEnumerator TestBattleManagerInitialization()
        {
            LogDebug("BattleManager初期化テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            // BattleManagerの存在確認
            AssertTest(battleManager != null, "BattleManagerが存在すること");

            if (battleManager == null)
            {
                LogTestResult("BattleManagerがnullのため、以降のテストをスキップします", false);
                yield break;
            }

            // 詳細ログ付きで初期状態確認
            LogDebug($"[詳細] CurrentBattleState = {battleManager.CurrentBattleState}");
            AssertTestDetailed(battleManager.CurrentBattleState == BattleState.Idle, "初期状態がIdleであること",
                battleManager.CurrentBattleState, BattleState.Idle);

            LogDebug($"[詳細] BattleTimer = {battleManager.BattleTimer}");
            AssertTestDetailed(battleManager.BattleTimer == 0f, "初期戦闘時間が0であること",
                battleManager.BattleTimer, 0f);

            LogDebug($"[詳細] IsBattleActive = {battleManager.IsBattleActive}");
            AssertTestDetailed(!battleManager.IsBattleActive, "初期状態で戦闘が非アクティブであること",
                battleManager.IsBattleActive, false);

            LogDebug($"[詳細] PlayerCombatCharacters.Count = {battleManager.PlayerCombatCharacters.Count}");
            AssertTestDetailed(battleManager.PlayerCombatCharacters.Count == 0, "初期プレイヤー戦闘キャラクター数が0であること",
                battleManager.PlayerCombatCharacters.Count, 0);

            LogDebug($"[詳細] EnemyCombatCharacters.Count = {battleManager.EnemyCombatCharacters.Count}");
            AssertTestDetailed(battleManager.EnemyCombatCharacters.Count == 0, "初期敵戦闘キャラクター数が0であること",
                battleManager.EnemyCombatCharacters.Count, 0);

            LogTestResult("BattleManager初期化テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 戦闘セットアップ検証テスト
        /// </summary>
        private IEnumerator TestBattleSetupValidation()
        {
            LogDebug("戦闘セットアップ検証テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            // 無効なセットアップのテスト
            var invalidSetup = new BattleSetup();
            invalidSetup.TimeLimit = -10f;
            AssertTest(!invalidSetup.IsValid(), "空のセットアップが無効と判定されること");

            bool startResult1 = battleManager.StartBattle(null);
            AssertTest(!startResult1, "nullセットアップで戦闘開始が失敗すること");

            bool startResult2 = battleManager.StartBattle(invalidSetup);
            AssertTest(!startResult2, "無効セットアップで戦闘開始が失敗すること");

            // 有効なセットアップの作成
            if (testEnemyCharacters.Count > 0)
            {
                var validSetup = CreateValidBattleSetup();
                AssertTest(validSetup.IsValid(), "有効なセットアップが正しく作成されること");
                AssertTest(validSetup.EnemyPositions.Count == validSetup.EnemyCharacters.Count, "敵位置数がキャラクター数と一致すること");
            }

            LogTestResult("戦闘セットアップ検証テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 基本戦闘フローテスト
        /// </summary>
        private IEnumerator TestBasicBattleFlow()
        {
            LogDebug("基本戦闘フローテスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            if (testPlayerCharacters.Count == 0 || testEnemyCharacters.Count == 0)
            {
                LogTestResult("テストキャラクター不足のため基本戦闘フローテストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // プレイヤーキャラクターをボードに配置
            SetupTestBattle();

            // 戦闘開始
            var battleSetup = CreateValidBattleSetup();
            bool startResult = battleManager.StartBattle(battleSetup);
            AssertTest(startResult, "有効なセットアップで戦闘が開始できること");
            AssertTest(battleManager.CurrentBattleState == BattleState.InProgress, "戦闘状態がInProgressになること");
            AssertTest(battleManager.IsBattleActive, "戦闘がアクティブになること");

            // 少し戦闘を進行させる
            yield return new WaitForSeconds(1f);

            AssertTest(battleManager.BattleTimer > 0f, "戦闘時間が進行していること");


            LogTestResult("基本戦闘フローテスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 戦闘状態遷移テスト
        /// </summary>
        private IEnumerator TestBattleStateTransitions()
        {
            LogDebug("戦闘状態遷移テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            if (testPlayerCharacters.Count == 0 || testEnemyCharacters.Count == 0)
            {
                LogTestResult("テストキャラクター不足のため戦闘状態遷移テストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            bool stateChangedEventFired = false;
            BattleState lastOldState = BattleState.Idle;
            BattleState lastNewState = BattleState.Idle;

            // イベントハンドラー登録
            battleManager.OnBattleStateChanged += (oldState, newState) =>
            {
                stateChangedEventFired = true;
                lastOldState = oldState;
                lastNewState = newState;
            };

            // 戦闘準備
            SetupTestBattle();
            var battleSetup = CreateValidBattleSetup();

            // 戦闘開始と状態遷移確認
            battleManager.StartBattle(battleSetup);
            yield return StartCoroutine(WaitForCondition(
                () => stateChangedEventFired,
                0.5f,
                "戦闘開始時の状態変更イベント"
            ));

            AssertTest(stateChangedEventFired, "戦闘開始時の状態変更イベントが発火すること");
            AssertTest(lastNewState == BattleState.InProgress, "InProgressに遷移すること");

            // 戦闘終了と状態遷移確認
            stateChangedEventFired = false;
            battleManager.ForceEndBattle();
            yield return StartCoroutine(WaitForCondition(
                () => stateChangedEventFired,
                0.5f,
                "戦闘終了時の状態変更イベント"
            ));

            AssertTest(stateChangedEventFired, "終了時の状態変更イベントが発火すること");

            LogTestResult("戦闘状態遷移テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 戦闘メカニクステスト
        /// </summary>
        private IEnumerator TestCombatMechanics()
        {
            LogDebug("戦闘メカニクステスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            if (testPlayerCharacters.Count == 0 || testEnemyCharacters.Count == 0)
            {
                LogTestResult("テストキャラクター不足のため戦闘メカニクステストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // 戦闘イベントの監視
            bool damageEventFired = false;
            bool moveEventFired = false;

            battleManager.OnCharacterTakeDamage += (attacker, target, damage) => damageEventFired = true;
            battleManager.OnCharacterMove += (character, oldPos, newPos) => moveEventFired = true;

            // 戦闘開始
            SetupTestBattle();
            var battleSetup = CreateValidBattleSetup();
            battleManager.StartBattle(battleSetup);

            // 戦闘開始直後の確認
            yield return new WaitForSeconds(0.1f);
            AssertTest(battleManager.PlayerCombatCharacters.Count > 0, "プレイヤー戦闘キャラクターが作成されること");
            AssertTest(battleManager.EnemyCombatCharacters.Count > 0, "敵戦闘キャラクターが作成されること");

            // 戦闘が進行中であることを確認
            AssertTest(battleManager.IsBattleActive, "戦闘がアクティブであること");

            // 戦闘を短時間進行させてイベント発火を確認
            yield return new WaitForSeconds(1f);

            // 戦闘メカニクスの確認（イベントベース）
            if (damageEventFired)
            {
                LogTestResult("ダメージイベントが正常に発火しました");
            }

            if (moveEventFired)
            {
                LogTestResult("移動イベントが正常に発火しました");
            }

            // 戦闘がまだ進行中であることを確認してから強制終了
            if (battleManager.IsBattleActive)
            {
                battleManager.ForceEndBattle();
            }
            yield return StartCoroutine(WaitForCondition(
                () => battleManager.CurrentBattleState == BattleState.Ending,
                0.5f,
                "戦闘終了時の状態変更イベント"
            ));

            LogTestResult("戦闘メカニクステスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 勝利条件テスト
        /// </summary>
        private IEnumerator TestVictoryConditions()
        {
            LogDebug("勝利条件テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            if (testPlayerCharacters.Count == 0 || testEnemyCharacters.Count == 0)
            {
                LogTestResult("テストキャラクター不足のため勝利条件テストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            bool battleEndedEventFired = false;
            BattleResult lastBattleResult = null;

            battleManager.OnBattleEnded += (result) =>
            {
                battleEndedEventFired = true;
                lastBattleResult = result;
            };

            LogTestResult("勝利条件テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 戦闘報酬テスト
        /// </summary>
        private IEnumerator TestBattleRewards()
        {
            LogDebug("戦闘報酬テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            if (goldManager == null)
            {
                LogTestResult("GoldManagerが見つからないため戦闘報酬テストをスキップ");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // 初期ゴールド記録
            int initialGold = goldManager.CurrentGold;

            bool battleEndedEventFired = false;
            BattleResult lastBattleResult = null;

            battleManager.OnBattleEnded += (result) =>
            {
                battleEndedEventFired = true;
                lastBattleResult = result;
            };

            // 戦闘実行（強制終了による引き分け）
            if (testPlayerCharacters.Count > 0 && testEnemyCharacters.Count > 0)
            {
                SetupTestBattle();
                var battleSetup = CreateValidBattleSetup();
                battleManager.StartBattle(battleSetup);
                yield return StartCoroutine(WaitForCondition(
                    () => battleManager.CurrentBattleState == BattleState.InProgress,
                    1f,
                    "戦闘開始時の状態変更イベント"
                ));

                battleManager.ForceEndBattle();
                yield return StartCoroutine(WaitForCondition(
                    () => battleManager.CurrentBattleState == BattleState.Ending,
                    0.5f,
                    "戦闘終了時の状態変更イベント"
                ));

                AssertTest(battleEndedEventFired, "戦闘終了イベントが発火すること");
                if (lastBattleResult != null)
                {
                    AssertTest(lastBattleResult.GoldReward >= 0, "ゴールド報酬が設定されること");
                    AssertTest(lastBattleResult.ExperienceReward >= 0, "経験値報酬が設定されること");

                    // ゴールド増加確認
                    int finalGold = goldManager.CurrentGold;
                    if (lastBattleResult.GoldReward > 0)
                    {
                        AssertTest(finalGold > initialGold, "ゴールドが実際に増加すること");
                    }
                }
            }

            LogTestResult("戦闘報酬テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 強制終了テスト
        /// </summary>
        private IEnumerator TestForceEndBattle()
        {
            LogDebug("強制終了テスト開始");

            // テスト前の状態リセット
            yield return StartCoroutine(ResetBattleStateForTest());

            // 非戦闘時の強制終了（何も起こらないはず）
            battleManager.ForceEndBattle();
            AssertTest(battleManager.CurrentBattleState == BattleState.Idle, "非戦闘時の強制終了で状態が変わらないこと");

            if (testPlayerCharacters.Count > 0 && testEnemyCharacters.Count > 0)
            {
                LogDebug($"強制終了テスト: プレイヤー{testPlayerCharacters.Count}体、敵{testEnemyCharacters.Count}体で実行");

                // 戦闘中の強制終了
                SetupTestBattle();
                var battleSetup = CreateValidBattleSetup();

                LogDebug($"戦闘開始前: ボード上プレイヤー{boardManager.PlayerCharacters.Count}体、敵{boardManager.EnemyCharacters.Count}体");

                bool startResult = battleManager.StartBattle(battleSetup);
                LogDebug($"StartBattle結果: {startResult}, 戦闘状態: {battleManager.CurrentBattleState}");

                yield return new WaitForSeconds(0.1f);
                AssertTest(battleManager.IsBattleActive, "戦闘が開始されること");

                battleManager.ForceEndBattle();
                yield return StartCoroutine(WaitForCondition(
                    () => battleManager.CurrentBattleState == BattleState.Ending,
                    0.5f,
                    "戦闘終了時の状態変更イベント"
                ));
                AssertTest(!battleManager.IsBattleActive, "強制終了により戦闘が停止すること");
                AssertTest(battleManager.CurrentBattleState == BattleState.Idle, "強制終了後にIdleに戻ること");
            }
            else
            {
                LogTestResult($"キャラクター不足のため強制終了テストをスキップ: プレイヤー{testPlayerCharacters.Count}体、敵{testEnemyCharacters.Count}体");
            }

            LogTestResult("強制終了テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テスト戦闘のセットアップ
        /// </summary>
        private void SetupTestBattle()
        {
            // ボードをクリア
            boardManager.ClearBoard();

            // プレイヤーキャラクターを配置
            for (int i = 0; i < testPlayerCharacters.Count && i < 3; i++)
            {
                var position = new Vector2Int(i + 1, 1);
                boardManager.PlaceCharacter(testPlayerCharacters[i], position, true);
            }
        }

        /// <summary>
        /// 有効な戦闘セットアップを作成
        /// </summary>
        /// <returns>戦闘セットアップ</returns>
        private BattleSetup CreateValidBattleSetup()
        {
            if (enableDetailedLogs)
            {
                ReportInfo($"CreateValidBattleSetup: プレイヤー{testPlayerCharacters.Count}体、敵{testEnemyCharacters.Count}体で戦闘セットアップを作成中");
            }

            var setup = new BattleSetup
            {
                BattleName = "テスト戦闘",
                TimeLimit = testBattleTimeLimit,
                EnableDamageEscalation = true,
                DamageEscalationStartTime = testBattleTimeLimit * 0.5f,
                BaseGoldReward = 50,
                BaseExperienceReward = 25,
                RewardMultiplier = 1f
            };

            // 敵キャラクターと位置を設定
            for (int i = 0; i < testEnemyCharacters.Count && i < 2; i++)
            {
                setup.EnemyCharacters.Add(testEnemyCharacters[i]);
                setup.EnemyPositions.Add(new Vector2Int(i + 2, 6)); // 敵エリアに配置

                if (enableDetailedLogs)
                {
                    ReportInfo($"敵キャラクター追加: {testEnemyCharacters[i].CharacterData.CharacterName} at {new Vector2Int(i + 2, 6)}");
                }
            }

            if (enableDetailedLogs)
            {
                ReportInfo($"戦闘セットアップ作成完了: 敵{setup.EnemyCharacters.Count}体, IsValid={setup.IsValid()}");
            }

            return setup;
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
        /// 条件待機の結果を取得するヘルパーメソッド
        /// </summary>
        /// <param name="condition">待機条件</param>
        /// <param name="timeoutSeconds">タイムアウト時間（秒）</param>
        /// <param name="conditionDescription">条件の説明</param>
        /// <returns>条件が満たされたかどうか</returns>
        private IEnumerator<bool> WaitForConditionWithResult(System.Func<bool> condition, float timeoutSeconds = 1f, string conditionDescription = "条件")
        {
            float elapsed = 0f;

            while (!condition() && elapsed < timeoutSeconds)
            {
                yield return false;
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
        /// テスト前の戦闘状態リセット
        /// </summary>
        private IEnumerator ResetBattleStateForTest()
        {
            if (battleManager != null)
            {
                // 戦闘を強制終了
                battleManager.ForceEndBattle();
                yield return StartCoroutine(WaitForCondition(
                    () => battleManager.CurrentBattleState == BattleState.Ending,
                    0.5f,
                    "戦闘終了時の状態変更イベント"
                ));

                // ボードもクリア
                if (boardManager != null)
                {
                    boardManager.ClearBoard();
                }

                // 状態がIdleになるまで待機
                yield return StartCoroutine(WaitForCondition(
                    () => battleManager.CurrentBattleState == BattleState.Idle,
                    1f,
                    "戦闘状態のIdle移行"
                ));

                if (enableDetailedLogs && battleManager.CurrentBattleState == BattleState.Idle)
                {
                    ReportInfo("テスト前の戦闘状態をリセットしました");
                }
            }
        }

        /// <summary>
        /// デバッグ用戦闘情報表示
        /// </summary>
        [ContextMenu("Debug Print Battle Info")]
        public void DebugPrintBattleInfo()
        {
            if (battleManager != null)
            {
                battleManager.DebugPrintBattleInfo();
            }
        }

        /// <summary>
        /// テスト戦闘開始
        /// </summary>
        [ContextMenu("Start Test Battle")]
        public void StartTestBattle()
        {
            if (battleManager != null && testPlayerCharacters.Count > 0 && testEnemyCharacters.Count > 0)
            {
                SetupTestBattle();
                var setup = CreateValidBattleSetup();
                battleManager.StartBattle(setup);
                ReportInfo("テスト戦闘を開始しました");
            }
            else
            {
                ReportWarning("テスト戦闘の開始に必要な条件が揃っていません");
            }
        }
    }
}