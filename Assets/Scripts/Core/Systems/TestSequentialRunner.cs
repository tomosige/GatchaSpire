using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// テストを順次実行する制御システム
    /// 優先度順にキューに積み、一つずつ確実に実行する
    /// </summary>
    public class TestSequentialRunner : MonoBehaviour
    {
        [Header("実行制御設定")]
        [SerializeField] private bool autoRunOnStart = true;
        [SerializeField] private float delayBetweenTests = 1f;
        [SerializeField] private bool showDetailedLogs = true;

        // テスト実行管理
        private Queue<ITestExclusive> testQueue = new Queue<ITestExclusive>();
        private ITestExclusive currentTest;
        private bool isRunning = false;
        private int totalTests = 0;
        private int completedTests = 0;

        // シングルトンパターン
        private static TestSequentialRunner instance;
        public static TestSequentialRunner Instance => instance;

        /// <summary>
        /// 現在実行中のテスト
        /// </summary>
        public ITestExclusive CurrentTest => currentTest;

        /// <summary>
        /// テスト実行中かどうか
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// テスト進捗状況
        /// </summary>
        public string GetProgress() => $"{completedTests}/{totalTests}";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[TestSequentialRunner] テスト順次実行システムを初期化しました");
            }
            else if (instance != this)
            {
                Debug.LogWarning("[TestSequentialRunner] 重複インスタンスを破棄します");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (autoRunOnStart)
            {
                StartCoroutine(InitializeAndRunTests());
            }
        }

        /// <summary>
        /// テストを初期化して順次実行
        /// </summary>
        private IEnumerator InitializeAndRunTests()
        {
            // 少し待ってから開始（他のシステムの初期化を待つ）
            yield return new WaitForSeconds(0.5f);

            InitializeTestQueue();

            if (testQueue.Count > 0)
            {
                yield return StartCoroutine(RunTestsSequentially());
            }
            else
            {
                Debug.LogWarning("[TestSequentialRunner] 実行可能なテストが見つかりませんでした");
            }
        }

        /// <summary>
        /// すべてのテストを取得してキューに追加
        /// </summary>
        private void InitializeTestQueue()
        {
            Debug.Log("[TestSequentialRunner] テストクラスを収集中...");

            // すべてのITestExclusiveを実装したコンポーネントを取得
            var allTestComponents = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is ITestExclusive)
                .Cast<ITestExclusive>()
                .ToList();

            if (showDetailedLogs)
            {
                Debug.Log($"[TestSequentialRunner] 発見されたテストクラス: {allTestComponents.Count}個");
                foreach (var test in allTestComponents)
                {
                    Debug.Log($"  - {test.TestClassName} (最大実行時間: {test.MaxExecutionTimeSeconds}秒)");
                }
            }

            // DefaultExecutionOrder順にソート
            allTestComponents.Sort((a, b) => GetExecutionOrder(a).CompareTo(GetExecutionOrder(b)));

            // キューに追加
            testQueue.Clear();
            foreach (var test in allTestComponents)
            {
                testQueue.Enqueue(test);
            }

            totalTests = testQueue.Count;
            completedTests = 0;

            Debug.Log($"[TestSequentialRunner] テストキューを初期化しました: {totalTests}個のテストが登録されました");
        }

        /// <summary>
        /// テストを順次実行
        /// </summary>
        private IEnumerator RunTestsSequentially()
        {
            isRunning = true;
            Debug.Log($"[TestSequentialRunner] テスト順次実行を開始します ({totalTests}個のテスト)");

            var overallStartTime = Time.time;

            while (testQueue.Count > 0)
            {
                currentTest = testQueue.Dequeue();

                Debug.Log($"[TestSequentialRunner] テスト実行開始: {currentTest.TestClassName} ({completedTests + 1}/{totalTests})");

                // テスト実行
                yield return StartCoroutine(RunSingleTest(currentTest));

                completedTests++;

                Debug.Log($"[TestSequentialRunner] テスト実行完了: {currentTest.TestClassName} ({completedTests}/{totalTests})");

                // テスト間の待機
                if (delayBetweenTests > 0 && testQueue.Count > 0)
                {
                    yield return new WaitForSeconds(delayBetweenTests);
                }

                currentTest = null;
            }

            var totalTime = Time.time - overallStartTime;
            Debug.Log($"[TestSequentialRunner] すべてのテストが完了しました！総実行時間: {totalTime:F2}秒");

            isRunning = false;
        }

        /// <summary>
        /// 単一のテストを実行
        /// </summary>
        private IEnumerator RunSingleTest(ITestExclusive test)
        {
            var startTime = Time.time;
            var maxTime = test.MaxExecutionTimeSeconds > 0 ? test.MaxExecutionTimeSeconds : 300f;

            // テスト準備
            test.OnTestPrepare();

            // テスト実行（MonoBehaviourのメソッドを呼び出す）
            yield return StartCoroutine(ExecuteTestMethod(test));

            // テスト後処理
            test.OnTestCleanup();

            var executionTime = Time.time - startTime;
            if (showDetailedLogs)
            {
                Debug.Log($"[TestSequentialRunner] {test.TestClassName} 実行時間: {executionTime:F2}秒");
            }

            // タイムアウトチェック
            if (Time.time - startTime > maxTime)
            {
                Debug.LogError($"[TestSequentialRunner] テストがタイムアウトしました: {test.TestClassName}");
                test.OnTestForceTerminated();
            }
        }

        /// <summary>
        /// テストメソッドを実行
        /// </summary>
        private IEnumerator ExecuteTestMethod(ITestExclusive test)
        {
            var monoBehaviour = test as MonoBehaviour;
            if (monoBehaviour == null)
            {
                Debug.LogError($"[TestSequentialRunner] テストクラスがMonoBehaviourを継承していません: {test.TestClassName}");
                yield break;
            }

            // TestExclusiveBaseを継承しているかチェック
            var testBase = monoBehaviour as TestExclusiveBase;
            if (testBase != null)
            {
                // TestExclusiveBaseのRunAllTestsメソッドを実行
                yield return StartCoroutine(testBase.RunAllTests());
            }
            else
            {
                // リフレクションで繋ぎ打ち実行
                var type = monoBehaviour.GetType();

                // 一般的なテストメソッド名を試行
                var testMethodNames = new[] {
                    "RunAllTests",
                    "RunAllTestsCoroutine",
                    "RunBasicTests",
                    "CheckFoundationSystems"
                };

                System.Reflection.MethodInfo testMethod = null;
                foreach (var methodName in testMethodNames)
                {
                    testMethod = type.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (testMethod != null)
                    {
                        break;
                    }
                }

                if (testMethod != null)
                {
                    // コルーチンかどうかチェック
                    if (testMethod.ReturnType == typeof(IEnumerator))
                    {
                        var coroutine = (IEnumerator)testMethod.Invoke(monoBehaviour, null);
                        yield return StartCoroutine(coroutine);
                    }
                    else
                    {
                        testMethod.Invoke(monoBehaviour, null);
                        yield return null;
                    }
                }
                else
                {
                    Debug.LogWarning($"[TestSequentialRunner] 実行可能なテストメソッドが見つかりませんでした: {test.TestClassName}");
                }
            }
        }

        /// <summary>
        /// DefaultExecutionOrderを取得
        /// </summary>
        private int GetExecutionOrder(ITestExclusive test)
        {
            var monoBehaviour = test as MonoBehaviour;
            if (monoBehaviour == null) return 0;

            var type = monoBehaviour.GetType();
            var attribute = type.GetCustomAttributes(typeof(DefaultExecutionOrder), false).FirstOrDefault() as DefaultExecutionOrder;

            return attribute?.order ?? 0;
        }

        /// <summary>
        /// 手動でテストを実行
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTestsManually()
        {
            if (isRunning)
            {
                Debug.LogWarning("[TestSequentialRunner] テストは既に実行中です");
                return;
            }

            if (Application.isPlaying)
            {
                StartCoroutine(InitializeAndRunTests());
            }
            else
            {
                Debug.LogWarning("[TestSequentialRunner] テストは実行時にのみ動作します");
            }
        }

        /// <summary>
        /// テスト実行を停止
        /// </summary>
        [ContextMenu("Stop Tests")]
        public void StopTests()
        {
            if (!isRunning)
            {
                Debug.LogWarning("[TestSequentialRunner] テストは実行されていません");
                return;
            }

            StopAllCoroutines();

            if (currentTest != null)
            {
                currentTest.OnTestForceTerminated();
                currentTest = null;
            }

            testQueue.Clear();
            isRunning = false;

            Debug.Log("[TestSequentialRunner] テスト実行を停止しました");
        }

        /// <summary>
        /// 現在の実行状況を取得
        /// </summary>
        public string GetStatus()
        {
            if (!isRunning)
            {
                return "待機中";
            }

            if (currentTest != null)
            {
                return $"実行中: {currentTest.TestClassName} ({completedTests + 1}/{totalTests})";
            }

            return $"進行中 ({completedTests}/{totalTests})";
        }
    }
}