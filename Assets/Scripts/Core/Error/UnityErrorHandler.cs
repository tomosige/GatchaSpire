using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace GatchaSpire.Core.Error
{
    /// <summary>
    /// Unity統合エラーハンドラー
    /// MonoBehaviourベースでUnityライフサイクルと連携したエラー処理を提供
    /// </summary>
    public class UnityErrorHandler : MonoBehaviour, IErrorHandler, IErrorReporter
    {
        [Header("設定")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool pauseOnCriticalError = true;
        [SerializeField] private int maxErrorHistory = 100;

        [Header("Unity統合")]
        [SerializeField] private bool handleUnityLogMessages = true;
        [SerializeField] private bool reportUncaughtExceptions = true;

        [Header("ファイル保存")]
        [SerializeField] private bool enableFileSaving = true;
        [SerializeField] private bool autoSaveOnCritical = true;

        private List<SystemError> errorHistory;
        private Dictionary<ErrorCategory, Func<SystemError, bool>> recoveryActions;
        private ConcurrentQueue<SystemError> mainThreadErrorQueue;
        private ErrorLogFileManager fileManager;
        
        private static UnityErrorHandler instance;
        
        /// <summary>シングルトンインスタンス</summary>
        public static UnityErrorHandler Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeErrorHandler();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// エラーハンドラーの初期化
        /// </summary>
        private void InitializeErrorHandler()
        {
            errorHistory = new List<SystemError>();
            recoveryActions = new Dictionary<ErrorCategory, Func<SystemError, bool>>();
            mainThreadErrorQueue = new ConcurrentQueue<SystemError>();

            // ファイルマネージャーの初期化
            if (enableFileSaving)
            {
                fileManager = new ErrorLogFileManager();
            }

            if (handleUnityLogMessages)
            {
                Application.logMessageReceived += OnUnityLogMessage;
            }

            if (reportUncaughtExceptions)
            {
                Application.logMessageReceivedThreaded += OnUnityLogMessageThreaded;
            }

            Debug.Log("[UnityErrorHandler] エラーハンドリングシステムを初期化しました");
        }

        /// <summary>
        /// Unityログメッセージの処理
        /// </summary>
        private void OnUnityLogMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                var severity = type == LogType.Exception ? ErrorSeverity.Critical : ErrorSeverity.Error;
                var error = new SystemError("Unity", severity, ErrorCategory.Unity, logString);
                HandleError(error);
            }
        }

        /// <summary>
        /// スレッドセーフなUnityログメッセージ処理
        /// </summary>
        private void OnUnityLogMessageThreaded(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                var error = new SystemError("Unity", ErrorSeverity.Fatal, ErrorCategory.Unity, logString);
                mainThreadErrorQueue.Enqueue(error);
            }
        }

        private void Update()
        {
            // メインスレッドでのエラー処理
            while (mainThreadErrorQueue.TryDequeue(out SystemError error))
            {
                HandleError(error);
            }
        }

        /// <summary>
        /// エラーを処理
        /// </summary>
        public void HandleError(SystemError error)
        {
            // エラー履歴に追加
            errorHistory.Add(error);
            if (errorHistory.Count > maxErrorHistory)
            {
                errorHistory.RemoveAt(0);
            }

            // ログ出力
            if (enableDebugLogs)
            {
                var color = ColorUtility.ToHtmlStringRGB(error.GetSeverityColor());
                switch (error.Severity)
                {
                    case ErrorSeverity.Critical:
                    case ErrorSeverity.Fatal:
                    case ErrorSeverity.Error:
                        Debug.LogError($"<color=#{color}>{error.GetFormattedMessage()}</color>");
                        break;
                    case ErrorSeverity.Warning:
                        Debug.LogWarning($"<color=#{color}>{error.GetFormattedMessage()}</color>");
                        break;
                    case ErrorSeverity.Info:
                        Debug.Log($"<color=#{color}>{error.GetFormattedMessage()}</color>");
                        break;
                    default:
                        // 未知のSeverityの場合はErrorとして出力
                        Debug.LogError($"<color=#{color}>[UNKNOWN SEVERITY] {error.GetFormattedMessage()}</color>");
                        break;
                }
            }

            // ファイル保存
            if (enableFileSaving && fileManager != null)
            {
                if (autoSaveOnCritical && (error.Severity == ErrorSeverity.Critical || error.Severity == ErrorSeverity.Fatal))
                {
                    fileManager.SaveErrorToFile(error);
                }
                else if (error.Severity >= ErrorSeverity.Error)
                {
                    fileManager.SaveErrorToFile(error);
                }
            }

            // 重要度による処理分岐
            switch (error.Severity)
            {
                case ErrorSeverity.Critical:
                case ErrorSeverity.Fatal:
                    HandleCriticalError(error);
                    break;
                case ErrorSeverity.Error:
                    TryRecover(error);
                    break;
            }
        }

        /// <summary>
        /// クリティカルエラーを処理
        /// </summary>
        public void HandleCriticalError(SystemError error)
        {
            Debug.LogError($"クリティカルエラー: {error.GetFormattedMessage()}");
            
            if (pauseOnCriticalError && Application.isEditor)
            {
                Debug.Break();
            }

            // 致命的エラーの場合、システム全体のリセットを試行
            if (error.Severity == ErrorSeverity.Fatal)
            {
                var coordinator = FindObjectOfType<MonoBehaviour>(); // GameSystemCoordinatorに後で変更
                // coordinator?.HandleFatalError(error);
            }
        }

        /// <summary>
        /// エラーからの復旧を試行
        /// </summary>
        public bool TryRecover(SystemError error)
        {
            if (!error.IsRecoverable) return false;

            if (recoveryActions.TryGetValue(error.Category, out var recoveryAction))
            {
                try
                {
                    bool recovered = recoveryAction(error);
                    if (recovered && enableDebugLogs)
                    {
                        Debug.Log($"[UnityErrorHandler] エラーから復旧しました: {error.Message}");
                    }
                    return recovered;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityErrorHandler] 復旧アクションが失敗しました: {e.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 復旧アクションを登録
        /// </summary>
        public void RegisterRecoveryAction(ErrorCategory category, Func<SystemError, bool> recoveryAction)
        {
            recoveryActions[category] = recoveryAction;
        }

        /// <summary>
        /// エラー履歴を取得
        /// </summary>
        public List<SystemError> GetErrorHistory() => new List<SystemError>(errorHistory);

        /// <summary>
        /// エラー履歴をクリア
        /// </summary>
        public void ClearErrorHistory()
        {
            errorHistory.Clear();
        }

        // IErrorReporter implementation
        public void ReportError(string systemName, ErrorSeverity severity, ErrorCategory category, 
                               string message, Exception exception = null, bool isRecoverable = true)
        {
            var error = new SystemError(systemName, severity, category, message, exception, isRecoverable);
            HandleError(error);
        }

        public void ReportWarning(string systemName, ErrorSeverity severity, ErrorCategory category,
                               string message, Exception exception = null, bool isRecoverable = true)
        {
            var error = new SystemError(systemName, severity, category, message, exception, isRecoverable);
            HandleError(error);
        }

        public void ReportInfo(string systemName, ErrorSeverity severity, ErrorCategory category,
                               string message, Exception exception = null, bool isRecoverable = true)
        {
            var error = new SystemError(systemName, severity, category, message, exception, isRecoverable);
            HandleError(error);
        }

        public void ReportInfo(string systemName, string message) =>
            ReportInfo(systemName, ErrorSeverity.Info, ErrorCategory.System, message);

        public void ReportWarning(string systemName, string message) =>
            ReportWarning(systemName, ErrorSeverity.Warning, ErrorCategory.System, message);

        public void ReportError(string systemName, string message, Exception exception = null) =>
            ReportError(systemName, ErrorSeverity.Error, ErrorCategory.System, message, exception);

        public void ReportCritical(string systemName, string message, Exception exception = null) =>
            ReportError(systemName, ErrorSeverity.Critical, ErrorCategory.System, message, exception, false);

        public void ReportFatal(string systemName, string message, Exception exception = null) =>
            ReportError(systemName, ErrorSeverity.Fatal, ErrorCategory.System, message, exception, false);

        private void OnDestroy()
        {
            if (handleUnityLogMessages)
            {
                Application.logMessageReceived -= OnUnityLogMessage;
                Application.logMessageReceivedThreaded -= OnUnityLogMessageThreaded;
            }
        }
    }
}