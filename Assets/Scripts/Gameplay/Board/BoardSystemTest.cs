using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Board
{
    /// <summary>
    /// ボードシステムの動作確認テスト
    /// </summary>
    public class BoardSystemTest : TestExclusiveBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableDetailedLogs = true;

        [Header("テスト用データ")]
        [SerializeField] private CharacterData[] testCharacterData;

        private BoardStateManager boardManager;
        private PlacementValidator validator;
        private List<Character> testCharacters = new List<Character>();

        public override string TestClassName => "BoardSystemTest";

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
            // BoardStateManagerを作成
            var boardManagerGO = new GameObject("BoardStateManager");
            boardManagerGO.transform.SetParent(transform);
            boardManager = boardManagerGO.AddComponent<BoardStateManager>();

            // PlacementValidatorを作成
            var validatorGO = new GameObject("PlacementValidator");
            validatorGO.transform.SetParent(transform);
            validator = validatorGO.AddComponent<PlacementValidator>();

            // システム初期化
            boardManager.Initialize();
            validator.Initialize(boardManager);

            // テスト用キャラクターデータの生成
            CreateTestCharacters();

            ReportInfo("テストコンポーネントを初期化しました");
        }

        /// <summary>
        /// テスト用キャラクターを作成
        /// </summary>
        private void CreateTestCharacters()
        {
            testCharacters.Clear();

            // テストデータが設定されていない場合はデフォルトを作成
            if (testCharacterData == null || testCharacterData.Length == 0)
            {
                ReportWarning("テストキャラクターデータが設定されていません。動的生成を試行します");
                CreateDynamicTestCharacters();
                return;
            }

            // 設定されたデータからキャラクターを作成
            for (int i = 0; i < testCharacterData.Length && i < 5; i++)
            {
                var character = new Character(testCharacterData[i], 1);
                testCharacters.Add(character);
            }

            ReportInfo($"{testCharacters.Count}体のテストキャラクターを作成しました");
        }

        /// <summary>
        /// 動的テストキャラクターの作成
        /// </summary>
        private void CreateDynamicTestCharacters()
        {
            // CharacterDatabaseからキャラクターを取得を試行
            var database = FindObjectOfType<CharacterDatabase>();
            if (database != null)
            {
                var allCharacters = database.AllCharacters;
                for (int i = 0; i < allCharacters.Count && i < 5; i++)
                {
                    var character = new Character(allCharacters[i], 1);
                    testCharacters.Add(character);
                }
                ReportInfo($"Databaseから{testCharacters.Count}体のキャラクターを取得しました");
            }
            else
            {
                ReportWarning("CharacterDatabaseが見つかりません。テストキャラクターなしで継続します");
            }
        }

        /// <summary>
        /// 全テストを順次実行
        /// </summary>
        private IEnumerator RunAllTestsSequentially()
        {
            ReportInfo("=== ボードシステムテスト開始 ===");

            yield return StartCoroutine(TestBoardInitialization());
            yield return StartCoroutine(TestCharacterPlacement());
            yield return StartCoroutine(TestCharacterMovement());
            yield return StartCoroutine(TestCharacterRemoval());
            yield return StartCoroutine(TestPlacementValidation());
            yield return StartCoroutine(TestAreaRestrictions());
            yield return StartCoroutine(TestMaxCharacterLimits());
            yield return StartCoroutine(TestBoardStateEvents());

            ReportInfo("=== 全テスト完了 ===");
        }

        /// <summary>
        /// ボード初期化テスト
        /// </summary>
        private IEnumerator TestBoardInitialization()
        {
            LogDebug("ボード初期化テスト開始");

            // 初期状態の確認
            AssertTest(boardManager.BoardWidth == 7, "ボード幅が7であること");
            AssertTest(boardManager.BoardHeight == 8, "ボード高さが8であること");
            AssertTest(boardManager.PlayerAreaHeight == 4, "プレイヤーエリア高さが4であること");
            AssertTest(boardManager.MaxCharactersPerTeam == 8, "最大キャラクター数が8であること");
            AssertTest(boardManager.PlayerCharacters.Count == 0, "初期プレイヤーキャラクター数が0であること");
            AssertTest(boardManager.EnemyCharacters.Count == 0, "初期敵キャラクター数が0であること");

            // エリア判定テスト
            AssertTest(boardManager.IsPlayerArea(new Vector2Int(3, 0)), "Y=0はプレイヤーエリア");
            AssertTest(boardManager.IsPlayerArea(new Vector2Int(3, 3)), "Y=3はプレイヤーエリア");
            AssertTest(boardManager.IsEnemyArea(new Vector2Int(3, 4)), "Y=4は敵エリア");
            AssertTest(boardManager.IsEnemyArea(new Vector2Int(3, 7)), "Y=7は敵エリア");

            LogTestResult("ボード初期化テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// キャラクター配置テスト
        /// </summary>
        private IEnumerator TestCharacterPlacement()
        {
            LogDebug("キャラクター配置テスト開始");

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];
                var position = new Vector2Int(3, 1);

                // 正常配置
                bool placed = boardManager.PlaceCharacter(character, position, true);
                AssertTest(placed, "キャラクターが正常に配置されること");
                AssertTest(boardManager.GetCharacterAt(position) == character, "指定位置にキャラクターが存在すること");
                AssertTest(boardManager.PlayerCharacters.Contains(character), "プレイヤーキャラクターリストに追加されること");

                // 位置取得テスト
                var foundPosition = boardManager.GetCharacterPosition(character);
                AssertTest(foundPosition == position, "キャラクター位置が正確に取得できること");
            }
            else
            {
                ReportWarning("テストキャラクターがないため配置テストをスキップします");
            }

            LogTestResult("キャラクター配置テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// キャラクター移動テスト
        /// </summary>
        private IEnumerator TestCharacterMovement()
        {
            LogDebug("キャラクター移動テスト開始");

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];
                var oldPosition = new Vector2Int(3, 1);
                var newPosition = new Vector2Int(2, 2);

                // 事前配置
                boardManager.PlaceCharacter(character, oldPosition, true);

                // 移動実行
                bool moved = boardManager.MoveCharacter(character, newPosition);
                AssertTest(moved, "キャラクターが正常に移動すること");
                AssertTest(boardManager.GetCharacterAt(oldPosition) == null, "元の位置が空になること");
                AssertTest(boardManager.GetCharacterAt(newPosition) == character, "新しい位置にキャラクターが存在すること");
            }
            else
            {
                ReportWarning("テストキャラクターがないため移動テストをスキップします");
            }

            LogTestResult("キャラクター移動テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// キャラクター除去テスト
        /// </summary>
        private IEnumerator TestCharacterRemoval()
        {
            LogDebug("キャラクター除去テスト開始");

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];
                var position = new Vector2Int(3, 1);

                // 事前配置
                boardManager.PlaceCharacter(character, position, true);
                
                // 除去実行
                bool removed = boardManager.RemoveCharacter(character);
                AssertTest(removed, "キャラクターが正常に除去されること");
                AssertTest(boardManager.GetCharacterAt(position) == null, "位置が空になること");
                AssertTest(!boardManager.PlayerCharacters.Contains(character), "プレイヤーキャラクターリストから除去されること");
            }
            else
            {
                ReportWarning("テストキャラクターがないため除去テストをスキップします");
            }

            LogTestResult("キャラクター除去テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 配置検証テスト
        /// </summary>
        private IEnumerator TestPlacementValidation()
        {
            LogDebug("配置検証テスト開始");

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];

                // 有効な配置
                var validResult = validator.ValidatePlacement(character, new Vector2Int(3, 1), true);
                AssertTest(validResult.IsValid, "有効な位置で配置可能と判定されること");

                // 無効な配置（範囲外）
                var invalidResult1 = validator.ValidatePlacement(character, new Vector2Int(-1, 0), true);
                AssertTest(!invalidResult1.IsValid, "範囲外位置で配置不可と判定されること");

                // 無効な配置（敵エリアへのプレイヤー配置）
                var invalidResult2 = validator.ValidatePlacement(character, new Vector2Int(3, 5), true);
                AssertTest(!invalidResult2.IsValid, "敵エリアにプレイヤーキャラクターを配置不可と判定されること");

                // null キャラクター
                var nullResult = validator.ValidatePlacement(null, new Vector2Int(3, 1), true);
                AssertTest(!nullResult.IsValid, "nullキャラクターで配置不可と判定されること");
            }
            else
            {
                ReportWarning("テストキャラクターがないため検証テストをスキップします");
            }

            LogTestResult("配置検証テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// エリア制限テスト
        /// </summary>
        private IEnumerator TestAreaRestrictions()
        {
            LogDebug("エリア制限テスト開始");

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];

                // プレイヤーエリア内配置
                bool playerAreaPlacement = boardManager.PlaceCharacter(character, new Vector2Int(0, 0), true);
                AssertTest(playerAreaPlacement, "プレイヤーエリア内にプレイヤーキャラクターを配置可能");

                boardManager.RemoveCharacter(character);

                // プレイヤーキャラクターを敵エリアに配置（失敗するはず）
                bool invalidPlacement = boardManager.PlaceCharacter(character, new Vector2Int(0, 7), true);
                AssertTest(!invalidPlacement, "プレイヤーキャラクターを敵エリアに配置不可");
            }

            LogTestResult("エリア制限テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 最大キャラクター数制限テスト
        /// </summary>
        private IEnumerator TestMaxCharacterLimits()
        {
            LogDebug("最大キャラクター数制限テスト開始");

            // ボードをクリア
            boardManager.ClearBoard();

            // 利用可能なキャラクター数に応じてテスト
            int charactersToPlace = Mathf.Min(testCharacters.Count, 3);
            
            for (int i = 0; i < charactersToPlace; i++)
            {
                var position = new Vector2Int(i, 0);
                bool placed = boardManager.PlaceCharacter(testCharacters[i], position, true);
                AssertTest(placed, $"キャラクター{i+1}が正常に配置されること");
            }

            AssertTest(boardManager.PlayerCharacters.Count == charactersToPlace, 
                      $"プレイヤーキャラクター数が{charactersToPlace}であること");

            LogTestResult("最大キャラクター数制限テスト完了");
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ボード状態イベントテスト
        /// </summary>
        private IEnumerator TestBoardStateEvents()
        {
            LogDebug("ボード状態イベントテスト開始");

            bool placedEventFired = false;
            bool movedEventFired = false;
            bool removedEventFired = false;
            bool stateChangedEventFired = false;

            // イベントハンドラーを登録
            boardManager.OnCharacterPlaced += (character, position) => placedEventFired = true;
            boardManager.OnCharacterMoved += (character, oldPos, newPos) => movedEventFired = true;
            boardManager.OnCharacterRemoved += (character, position) => removedEventFired = true;
            boardManager.OnBoardStateChanged += () => stateChangedEventFired = true;

            if (testCharacters.Count > 0)
            {
                var character = testCharacters[0];
                
                // ボードをクリア
                boardManager.ClearBoard();
                stateChangedEventFired = false;

                // 配置イベントテスト
                boardManager.PlaceCharacter(character, new Vector2Int(3, 1), true);
                AssertTest(placedEventFired, "配置イベントが発火すること");
                AssertTest(stateChangedEventFired, "状態変更イベントが発火すること");

                stateChangedEventFired = false;

                // 移動イベントテスト
                boardManager.MoveCharacter(character, new Vector2Int(2, 1));
                AssertTest(movedEventFired, "移動イベントが発火すること");
                AssertTest(stateChangedEventFired, "状態変更イベントが発火すること");

                stateChangedEventFired = false;

                // 除去イベントテスト
                boardManager.RemoveCharacter(character);
                AssertTest(removedEventFired, "除去イベントが発火すること");
                AssertTest(stateChangedEventFired, "状態変更イベントが発火すること");
            }
            else
            {
                ReportWarning("テストキャラクターがないためイベントテストをスキップします");
            }

            LogTestResult("ボード状態イベントテスト完了");
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
        /// ボード状態をリセット
        /// </summary>
        [ContextMenu("Reset Board")]
        public void ResetBoard()
        {
            if (boardManager != null)
            {
                boardManager.ClearBoard();
                ReportInfo("ボードをリセットしました");
            }
        }

        /// <summary>
        /// デバッグ用ボード表示
        /// </summary>
        [ContextMenu("Debug Print Board")]
        public void DebugPrintBoard()
        {
            if (boardManager != null)
            {
                boardManager.DebugPrintBoard();
            }
        }
    }
}