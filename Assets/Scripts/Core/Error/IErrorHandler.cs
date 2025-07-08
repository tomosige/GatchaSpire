using System;
using System.Collections.Generic;

namespace GatchaSpire.Core.Error
{
    /// <summary>
    /// エラーハンドリングインターフェース
    /// システム全体のエラー処理を統一
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// エラーを処理
        /// </summary>
        /// <param name="error">処理するエラー</param>
        void HandleError(SystemError error);

        /// <summary>
        /// クリティカルエラーを処理
        /// </summary>
        /// <param name="error">処理するクリティカルエラー</param>
        void HandleCriticalError(SystemError error);

        /// <summary>
        /// エラーからの復旧を試行
        /// </summary>
        /// <param name="error">復旧を試行するエラー</param>
        /// <returns>復旧に成功した場合true</returns>
        bool TryRecover(SystemError error);

        /// <summary>
        /// カテゴリごとの復旧アクションを登録
        /// </summary>
        /// <param name="category">エラーカテゴリ</param>
        /// <param name="recoveryAction">復旧アクション</param>
        void RegisterRecoveryAction(ErrorCategory category, Func<SystemError, bool> recoveryAction);

        /// <summary>
        /// エラー履歴を取得
        /// </summary>
        /// <returns>エラー履歴のリスト</returns>
        List<SystemError> GetErrorHistory();

        /// <summary>
        /// エラー履歴をクリア
        /// </summary>
        void ClearErrorHistory();
    }

    /// <summary>
    /// エラー報告インターフェース
    /// 各システムがエラーを報告するために使用
    /// </summary>
    public interface IErrorReporter
    {
        /// <summary>
        /// エラーを報告（汎用）
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="severity">重要度</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外オブジェクト</param>
        /// <param name="isRecoverable">復旧可能かどうか</param>
        void ReportError(string systemName, ErrorSeverity severity, ErrorCategory category, 
                        string message, Exception exception = null, bool isRecoverable = true);

        /// <summary>
        /// 情報を報告
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="message">メッセージ</param>
        void ReportInfo(string systemName, string message);

        /// <summary>
        /// 警告を報告
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="message">メッセージ</param>
        void ReportWarning(string systemName, string message);

        /// <summary>
        /// エラーを報告
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外オブジェクト</param>
        void ReportError(string systemName, string message, Exception exception = null);

        /// <summary>
        /// クリティカルエラーを報告
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外オブジェクト</param>
        void ReportCritical(string systemName, string message, Exception exception = null);

        /// <summary>
        /// 致命的エラーを報告
        /// </summary>
        /// <param name="systemName">システム名</param>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外オブジェクト</param>
        void ReportFatal(string systemName, string message, Exception exception = null);
    }
}