using System.Collections;
using UnityEngine;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// テスト排他制御用の基底クラス
    /// 共通の実装を提供し、必要に応じてオーバーライド可能
    /// GameSystemBaseを継承してUnityGameSystemCoordinatorによる管理を受ける
    /// </summary>
    public abstract class TestExclusiveBase : GameSystemBase, ITestExclusive
    {
        [Header("テスト設定")]
        [SerializeField] protected bool runTestsOnStart = true;
        [SerializeField] protected bool showDetailedLogs = true;
        
        #region ITestExclusive 実装
        
        /// <summary>
        /// テストクラス名（デフォルトは型名）
        /// </summary>
        public virtual string TestClassName => GetType().Name;
        
        /// <summary>
        /// 最大実行時間（デフォルトは2分）
        /// </summary>
        public virtual float MaxExecutionTimeSeconds => 120f;
        
        /// <summary>
        /// テスト準備処理
        /// </summary>
        public virtual void OnTestPrepare()
        {
            LogTestPhase("準備処理");
        }
        
        /// <summary>
        /// テスト後処理
        /// </summary>
        public virtual void OnTestCleanup()
        {
            LogTestPhase("後処理");
        }
        
        /// <summary>
        /// テスト強制終了処理
        /// </summary>
        public virtual void OnTestForceTerminated()
        {
            LogTestPhase("強制終了されました", true);
        }
        
        #endregion
        
        #region 抽象メソッド
        
        /// <summary>
        /// 各テストクラスで実装が必要なメインテストメソッド
        /// </summary>
        public abstract IEnumerator RunAllTests();
        
        #endregion
        
        #region 共通ユーティリティ
        
        /// <summary>
        /// テストフェーズのログ出力
        /// </summary>
        protected virtual void LogTestPhase(string phase, bool isWarning = false)
        {
            var message = $"=== {TestClassName} {phase} ===";
            
            if (isWarning)
            {
                ReportWarning(message);
            }
            else
            {
                ReportInfo(message);
            }
        }
        
        /// <summary>
        /// 条件付きデバッグログ出力
        /// </summary>
        protected virtual void LogDebug(string message)
        {
            if (showDetailedLogs)
            {
                ReportInfo($"[{TestClassName}] {message}");
            }
        }
        
        /// <summary>
        /// テスト結果のログ出力
        /// </summary>
        protected virtual void LogTestResult(string result, bool isSuccess = true)
        {
            var prefix = isSuccess ? "✓" : "✗";
            var message = $"[{TestClassName}] {prefix} {result}";
            
            if (isSuccess)
            {
                ReportInfo(message);
            }
            else
            {
                ReportError(message);
            }
        }
        
        /// <summary>
        /// 手動テスト実行（エディタ用）
        /// </summary>
        [ContextMenu("Run Tests")]
        public virtual void RunTestsManually()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunAllTests());
            }
            else
            {
                ReportWarning($"[{TestClassName}] テストは実行時にのみ動作します");
            }
        }
        
        #endregion
        
        #region GameSystemBase 実装
        
        /// <summary>
        /// システム名（デフォルトは型名）
        /// </summary>
        protected override string SystemName => GetType().Name;
        
        /// <summary>
        /// システム初期化
        /// </summary>
        protected override void OnSystemInitialize()
        {
            priority = SystemPriority.Lowest; // テストは最低優先度
        }
        
        /// <summary>
        /// システム開始
        /// </summary>
        protected override void OnSystemStart()
        {
            // TestSequentialRunnerによって制御されるため、個別の自動実行は無効化
            // 必要に応じてサブクラスでオーバーライド
        }
        
        /// <summary>
        /// Unity Awake
        /// </summary>
        private void Awake()
        {
            OnAwake();
        }
        
        #endregion
    }
}