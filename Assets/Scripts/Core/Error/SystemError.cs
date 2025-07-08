using System;
using UnityEngine;

namespace GatchaSpire.Core.Error
{
    /// <summary>
    /// エラーの重要度レベル
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>情報</summary>
        Info,
        /// <summary>警告</summary>
        Warning,
        /// <summary>エラー</summary>
        Error,
        /// <summary>クリティカルエラー</summary>
        Critical,
        /// <summary>致命的エラー</summary>
        Fatal
    }

    /// <summary>
    /// エラーのカテゴリ分類
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>システム関連</summary>
        System,
        /// <summary>ゲームプレイ関連</summary>
        Gameplay,
        /// <summary>UI関連</summary>
        UI,
        /// <summary>データ関連</summary>
        Data,
        /// <summary>ネットワーク関連</summary>
        Network,
        /// <summary>Unity関連</summary>
        Unity
    }

    /// <summary>
    /// システムエラー情報クラス
    /// 統一されたエラー管理のための包括的な情報を保持
    /// </summary>
    [System.Serializable]
    public class SystemError
    {
        [SerializeField] private string systemName;
        [SerializeField] private ErrorSeverity severity;
        [SerializeField] private ErrorCategory category;
        [SerializeField] private string message;
        [SerializeField] private string stackTrace;
        [SerializeField] private string timestamp;
        [SerializeField] private bool isRecoverable;

        /// <summary>エラーが発生したシステム名</summary>
        public string SystemName => systemName;

        /// <summary>エラーの重要度</summary>
        public ErrorSeverity Severity => severity;

        /// <summary>エラーのカテゴリ</summary>
        public ErrorCategory Category => category;

        /// <summary>エラーメッセージ</summary>
        public string Message => message;

        /// <summary>スタックトレース</summary>
        public string StackTrace => stackTrace;

        /// <summary>発生時刻</summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>復旧可能かどうか</summary>
        public bool IsRecoverable => isRecoverable;

        /// <summary>元の例外オブジェクト</summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// システムエラーを作成
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="severity">重要度</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外オブジェクト</param>
        /// <param name="isRecoverable">復旧可能かどうか</param>
        public SystemError(string systemName, ErrorSeverity severity, ErrorCategory category, 
                          string message, Exception exception = null, bool isRecoverable = true)
        {
            this.systemName = systemName;
            this.severity = severity;
            this.category = category;
            this.message = message;
            this.Exception = exception;
            this.isRecoverable = isRecoverable;
            this.Timestamp = DateTime.Now;
            this.timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            this.stackTrace = exception?.StackTrace ?? UnityEngine.StackTraceUtility.ExtractStackTrace();
        }

        /// <summary>
        /// フォーマットされたメッセージを取得
        /// </summary>
        public string GetFormattedMessage()
        {
            return $"[{timestamp}] [{severity}] [{category}] {systemName}: {message}";
        }

        /// <summary>
        /// 重要度に応じた色を取得
        /// </summary>
        public Color GetSeverityColor()
        {
            return severity switch
            {
                ErrorSeverity.Info => Color.white,
                ErrorSeverity.Warning => Color.yellow,
                ErrorSeverity.Error => Color.red,
                ErrorSeverity.Critical => Color.magenta,
                ErrorSeverity.Fatal => Color.black,
                _ => Color.gray
            };
        }

        /// <summary>
        /// 詳細情報を含む完全な文字列表現
        /// </summary>
        public string GetDetailedString()
        {
            var details = GetFormattedMessage();
            if (Exception != null)
            {
                details += $"\n例外: {Exception.GetType().Name}: {Exception.Message}";
            }
            if (!string.IsNullOrEmpty(stackTrace))
            {
                details += $"\nスタックトレース:\n{stackTrace}";
            }
            return details;
        }
    }
}