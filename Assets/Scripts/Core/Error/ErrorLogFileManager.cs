using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace GatchaSpire.Core.Error
{
    /// <summary>
    /// エラーログのファイル保存管理
    /// </summary>
    public class ErrorLogFileManager
    {
        private readonly string logDirectory;
        private readonly string currentLogFile;
        private readonly int maxLogFiles = 10;
        private readonly long maxLogFileSize = 10 * 1024 * 1024; // 10MB

        public ErrorLogFileManager()
        {
            logDirectory = Path.Combine(Application.persistentDataPath, "ErrorLogs");
            currentLogFile = Path.Combine(logDirectory, $"error_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            
            EnsureLogDirectoryExists();
            CleanupOldLogFiles();
        }

        /// <summary>
        /// エラーをファイルに保存
        /// </summary>
        public void SaveErrorToFile(SystemError error)
        {
            try
            {
                EnsureLogDirectoryExists();
                
                var logEntry = FormatLogEntry(error);
                
                // ファイルサイズチェック
                if (File.Exists(currentLogFile) && new FileInfo(currentLogFile).Length > maxLogFileSize)
                {
                    RotateLogFile();
                }
                
                File.AppendAllText(currentLogFile, logEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログファイルへの書き込みに失敗しました: {e.Message}");
            }
        }

        /// <summary>
        /// 複数エラーをバッチ保存
        /// </summary>
        public void SaveErrorsToFile(List<SystemError> errors)
        {
            try
            {
                EnsureLogDirectoryExists();
                
                var logEntries = new List<string>();
                foreach (var error in errors)
                {
                    logEntries.Add(FormatLogEntry(error));
                }
                
                File.AppendAllLines(currentLogFile, logEntries);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] バッチログ保存に失敗しました: {e.Message}");
            }
        }

        /// <summary>
        /// ログエントリのフォーマット
        /// </summary>
        private string FormatLogEntry(SystemError error)
        {
            var entry = $"[{error.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ";
            entry += $"[{error.Severity}] ";
            entry += $"[{error.Category}] ";
            entry += $"{error.SystemName}: {error.Message}";
            
            if (error.Exception != null)
            {
                entry += $"\n例外: {error.Exception.GetType().Name}: {error.Exception.Message}";
            }
            
            if (!string.IsNullOrEmpty(error.StackTrace))
            {
                entry += $"\nスタックトレース:\n{error.StackTrace}";
            }
            
            entry += "\n" + new string('-', 80) + "\n";
            
            return entry;
        }

        /// <summary>
        /// ログディレクトリの存在確認・作成
        /// </summary>
        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        /// <summary>
        /// ログファイルのローテーション
        /// </summary>
        private void RotateLogFile()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var rotatedFile = Path.Combine(logDirectory, $"error_log_{timestamp}.txt");
            
            if (File.Exists(currentLogFile))
            {
                File.Move(currentLogFile, rotatedFile);
            }
            
            CleanupOldLogFiles();
        }

        /// <summary>
        /// 古いログファイルのクリーンアップ
        /// </summary>
        private void CleanupOldLogFiles()
        {
            try
            {
                var logFiles = Directory.GetFiles(logDirectory, "error_log_*.txt");
                if (logFiles.Length <= maxLogFiles) return;

                Array.Sort(logFiles, (x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));
                
                for (int i = 0; i < logFiles.Length - maxLogFiles; i++)
                {
                    File.Delete(logFiles[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログファイルのクリーンアップに失敗しました: {e.Message}");
            }
        }

        /// <summary>
        /// 全ログファイルのパスを取得
        /// </summary>
        public string[] GetAllLogFiles()
        {
            try
            {
                return Directory.GetFiles(logDirectory, "error_log_*.txt");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログファイル一覧の取得に失敗しました: {e.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// ログファイルの内容を読み込み
        /// </summary>
        public string ReadLogFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログファイルの読み込みに失敗しました: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 全ログファイルを削除
        /// </summary>
        public void ClearAllLogs()
        {
            try
            {
                var logFiles = Directory.GetFiles(logDirectory, "error_log_*.txt");
                foreach (var file in logFiles)
                {
                    File.Delete(file);
                }
                Debug.Log("[ErrorLogFileManager] 全ログファイルを削除しました");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログファイルの削除に失敗しました: {e.Message}");
            }
        }

        /// <summary>
        /// ログディレクトリのサイズを取得
        /// </summary>
        public long GetLogDirectorySize()
        {
            try
            {
                var logFiles = Directory.GetFiles(logDirectory, "error_log_*.txt");
                long totalSize = 0;
                foreach (var file in logFiles)
                {
                    totalSize += new FileInfo(file).Length;
                }
                return totalSize;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorLogFileManager] ログディレクトリサイズの取得に失敗しました: {e.Message}");
                return 0;
            }
        }
    }
}