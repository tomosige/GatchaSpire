using UnityEngine;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// Phase 0基盤システムの動作確認用ランナー
    /// </summary>
    public class FoundationTestRunner : MonoBehaviour
    {
        [Header("確認設定")]
        [SerializeField] private bool autoRunOnStart = true;
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            if (autoRunOnStart)
            {
                CheckFoundationSystems();
            }
        }

        /// <summary>
        /// 基盤システムの動作確認
        /// </summary>
        [ContextMenu("Check Foundation Systems")]
        public void CheckFoundationSystems()
        {
            Debug.Log("=== Phase 0 基盤システム動作確認 ===");

            // 1. エラーハンドラーの確認
            CheckErrorHandler();

            // 2. システムコーディネーターの確認
            CheckSystemCoordinator();

            // 3. 開発設定の確認
            CheckDevelopmentSettings();

            // 4. ファイル管理の確認
            CheckFileManagement();

            Debug.Log("=== 動作確認完了 ===");
        }

        private void CheckErrorHandler()
        {
            var handler = UnityErrorHandler.Instance;
            if (handler != null)
            {
                Debug.Log("✓ UnityErrorHandler: 正常に初期化されています");
                
                // テストメッセージを送信
                handler.ReportInfo("FoundationTestRunner", "動作確認用テストメッセージ");
                handler.ReportWarning("FoundationTestRunner", "動作確認用警告メッセージ");
                
                var history = handler.GetErrorHistory();
                Debug.Log($"✓ エラー履歴: {history.Count}件のログが記録されています");
            }
            else
            {
                Debug.LogError("✗ UnityErrorHandler: インスタンスが見つかりません");
            }
        }

        private void CheckSystemCoordinator()
        {
            var coordinator = UnityGameSystemCoordinator.Instance;
            if (coordinator != null)
            {
                Debug.Log($"✓ UnityGameSystemCoordinator: 正常に初期化されています");
                Debug.Log($"  - 初期化状態: {coordinator.IsInitialized}");
                Debug.Log($"  - 登録システム数: {coordinator.SystemCount}");
                Debug.Log($"  - 全システム健康状態: {coordinator.AreAllSystemsHealthy()}");
            }
            else
            {
                Debug.LogError("✗ UnityGameSystemCoordinator: インスタンスが見つかりません");
            }
        }

        private void CheckDevelopmentSettings()
        {
            var settings = Resources.Load<DevelopmentSettings>("Settings\\DevelopmentSettings");
            
            if (settings != null)
            {
                Debug.Log("✓ DevelopmentSettings: 正常に読み込まれています");
                
                var validation = settings.Validate();
                if (validation.IsValid)
                {
                    Debug.Log("✓ 設定バリデーション: 全て正常です");
                }
                else
                {
                    Debug.LogWarning($"⚠ 設定バリデーション: {validation.Errors.Count}個のエラー, {validation.Warnings.Count}個の警告");
                    if (showDebugLogs)
                    {
                        Debug.Log($"詳細: {validation.GetSummary()}");
                    }
                }

                Debug.Log($"  - デバッグログ有効: {settings.EnableAllDebugLogs}");
                Debug.Log($"  - 目標フレームレート: {settings.TargetFrameRate}");
                Debug.Log($"  - ゴールド倍率: {settings.GlobalGoldMultiplier}x");
            }
            else
            {
                Debug.LogError("✗ DevelopmentSettings: 設定ファイルが見つかりません");
            }
        }

        private void CheckFileManagement()
        {
            var fileManager = new ErrorLogFileManager();
            
            // テストエラーを保存
            var testError = new SystemError("FoundationTestRunner", ErrorSeverity.Info, ErrorCategory.System, "動作確認用テストエラー");
            fileManager.SaveErrorToFile(testError);

            // ログファイルの確認
            var logFiles = fileManager.GetAllLogFiles();
            Debug.Log($"✓ ファイル管理: {logFiles.Length}個のログファイルが管理されています");
            
            var dirSize = fileManager.GetLogDirectorySize();
            Debug.Log($"  - ログディレクトリサイズ: {dirSize / 1024f:F1} KB");

            if (showDebugLogs && logFiles.Length > 0)
            {
                Debug.Log($"  - 最新ログファイル: {System.IO.Path.GetFileName(logFiles[logFiles.Length - 1])}");
            }
        }

        /// <summary>
        /// エラーハンドリングのテスト
        /// </summary>
        [ContextMenu("Test Error Handling")]
        public void TestErrorHandling()
        {
            var handler = UnityErrorHandler.Instance;
            if (handler == null)
            {
                Debug.LogError("エラーハンドラーが見つかりません");
                return;
            }

            Debug.Log("=== エラーハンドリングテスト開始 ===");

            // 各レベルのエラーをテスト
            handler.ReportInfo("TestRunner", "情報レベルのテストメッセージ");
            handler.ReportWarning("TestRunner", "警告レベルのテストメッセージ");
            handler.ReportError("TestRunner", "エラーレベルのテストメッセージ");

            // 復旧アクションのテスト
            bool recoveryTestPassed = false;
            handler.RegisterRecoveryAction(ErrorCategory.System, (error) => {
                Debug.Log($"復旧アクションが実行されました: {error.Message}");
                recoveryTestPassed = true;
                return true;
            });

            var recoverableError = new SystemError("TestRunner", ErrorSeverity.Error, ErrorCategory.System, "復旧テスト用エラー");
            handler.HandleError(recoverableError);

            if (recoveryTestPassed)
            {
                Debug.Log("✓ 復旧アクション: 正常に動作しました");
            }
            else
            {
                Debug.LogError("✗ 復旧アクション: 実行されませんでした");
            }

            Debug.Log("=== エラーハンドリングテスト完了 ===");
        }

        /// <summary>
        /// システムの詳細情報を表示
        /// </summary>
        [ContextMenu("Show System Details")]
        public void ShowSystemDetails()
        {
            Debug.Log("=== システム詳細情報 ===");

            var coordinator = UnityGameSystemCoordinator.Instance;
            if (coordinator != null)
            {
                Debug.Log($"システムコーディネーター詳細:");
                Debug.Log($"  - インスタンスID: {coordinator.GetInstanceID()}");
                Debug.Log($"  - ゲームオブジェクト名: {coordinator.gameObject.name}");
                Debug.Log($"  - DontDestroyOnLoad: {coordinator.gameObject.scene.name == "DontDestroyOnLoad"}");
            }

            var errorHandler = UnityErrorHandler.Instance;
            if (errorHandler != null)
            {
                var history = errorHandler.GetErrorHistory();
                Debug.Log($"エラーハンドラー詳細:");
                Debug.Log($"  - エラー履歴数: {history.Count}");
                Debug.Log($"  - インスタンスID: {errorHandler.GetInstanceID()}");
                Debug.Log($"  - ゲームオブジェクト名: {errorHandler.gameObject.name}");
            }
        }
    }
}