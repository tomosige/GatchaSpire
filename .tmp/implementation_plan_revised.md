# GatchaSpire 実装計画（修正版）

## 修正のポイント
1. **Unity特化設計パターンの統合**
2. **統一エラーハンドリングシステムの追加**
3. **ScriptableObject中心のデータ管理**
4. **エディタ拡張とデバッグ支援の強化**

---

## Phase 0: Unity基盤とエラーハンドリング

### 目標
Unity特化の基盤システムとエラーハンドリングの確立

### 実装内容

#### Step 0.1: プロジェクト構造セットアップ（修正版）
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Interfaces/       # 共通インターフェース
│   │   ├── Systems/          # システム管理
│   │   ├── Error/            # エラーハンドリング（新規）
│   │   ├── Unity/            # Unity特化基盤（新規）
│   │   └── Data/             # データクラス
│   ├── Gameplay/             # ゲームロジック
│   ├── UI/                   # UIシステム  
│   ├── Utils/                # ユーティリティ
│   └── Editor/               # エディタ拡張（新規）
├── Data/                     # ScriptableObject
│   ├── Characters/
│   ├── Gacha/
│   ├── Balance/
│   └── Settings/             # 開発設定（新規）
├── Scenes/
│   ├── Main/                 # メインシーン
│   ├── Tests/                # テストシーン
│   └── Development/          # 開発専用シーン（新規）
└── Tests/                    # テストコード
```

#### Step 0.2: Unity特化インターフェース実装
**ファイル**: `Scripts/Core/Interfaces/`

```csharp
// Unity特化システムインターフェース
public interface IUnityGameSystem : IGameSystem
{
    void OnAwake();
    void OnStart();
    void OnDestroy();
    bool RequiresUpdate { get; }
    int ExecutionOrder { get; }
}

// ScriptableObject検証インターフェース
public interface IValidatable
{
    ValidationResult Validate();
    void OnValidate();
}

// エディタプレビューインターフェース
public interface IEditorPreviewable
{
    void PreviewInEditor();
    Texture2D GetPreviewTexture();
    string GetPreviewDescription();
}

// Unity特化リセットインターフェース
public interface IUnityResettable : IResettableSystem
{
    void ResetUnityState();
    bool PreserveDuringSceneLoad { get; }
}
```

#### Step 0.3: エラーハンドリング基盤実装
**ファイル**: `Scripts/Core/Error/`

**SystemError.cs**:
```csharp
using System;
using UnityEngine;

namespace GatchaSpire.Core.Error
{
    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical,
        Fatal
    }

    public enum ErrorCategory
    {
        System,
        Gameplay,
        UI,
        Data,
        Network,
        Unity
    }

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

        public string SystemName => systemName;
        public ErrorSeverity Severity => severity;
        public ErrorCategory Category => category;
        public string Message => message;
        public string StackTrace => stackTrace;
        public DateTime Timestamp { get; private set; }
        public bool IsRecoverable => isRecoverable;
        public Exception Exception { get; private set; }

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

        public string GetFormattedMessage()
        {
            return $"[{timestamp}] [{severity}] [{category}] {systemName}: {message}";
        }

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
    }
}
```

**IErrorHandler.cs**:
```csharp
using System;
using System.Collections.Generic;

namespace GatchaSpire.Core.Error
{
    public interface IErrorHandler
    {
        void HandleError(SystemError error);
        void HandleCriticalError(SystemError error);
        bool TryRecover(SystemError error);
        void RegisterRecoveryAction(ErrorCategory category, Func<SystemError, bool> recoveryAction);
        List<SystemError> GetErrorHistory();
        void ClearErrorHistory();
    }

    public interface IErrorReporter
    {
        void ReportError(string systemName, ErrorSeverity severity, ErrorCategory category, 
                        string message, Exception exception = null, bool isRecoverable = true);
        void ReportInfo(string systemName, string message);
        void ReportWarning(string systemName, string message);
        void ReportError(string systemName, string message, Exception exception = null);
        void ReportCritical(string systemName, string message, Exception exception = null);
        void ReportFatal(string systemName, string message, Exception exception = null);
    }
}
```

**UnityErrorHandler.cs**:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GatchaSpire.Core.Error
{
    public class UnityErrorHandler : MonoBehaviour, IErrorHandler, IErrorReporter
    {
        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool pauseOnCriticalError = true;
        [SerializeField] private int maxErrorHistory = 100;

        [Header("Unity Integration")]
        [SerializeField] private bool handleUnityLogMessages = true;
        [SerializeField] private bool reportUncaughtExceptions = true;

        private List<SystemError> errorHistory;
        private Dictionary<ErrorCategory, Func<SystemError, bool>> recoveryActions;
        
        private static UnityErrorHandler instance;
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

        private void InitializeErrorHandler()
        {
            errorHistory = new List<SystemError>();
            recoveryActions = new Dictionary<ErrorCategory, Func<SystemError, bool>>();

            if (handleUnityLogMessages)
            {
                Application.logMessageReceived += OnUnityLogMessage;
            }

            if (reportUncaughtExceptions)
            {
                Application.logMessageReceivedThreaded += OnUnityLogMessageThreaded;
            }

            Debug.Log("[UnityErrorHandler] Error handling system initialized");
        }

        private void OnUnityLogMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                var severity = type == LogType.Exception ? ErrorSeverity.Critical : ErrorSeverity.Error;
                var error = new SystemError("Unity", severity, ErrorCategory.Unity, logString);
                HandleError(error);
            }
        }

        private void OnUnityLogMessageThreaded(string logString, string stackTrace, LogType type)
        {
            // スレッドセーフなエラー処理
            if (type == LogType.Exception)
            {
                var error = new SystemError("Unity", ErrorSeverity.Fatal, ErrorCategory.Unity, logString);
                // メインスレッドで処理するためキューに追加
                EnqueueErrorForMainThread(error);
            }
        }

        private Queue<SystemError> mainThreadErrorQueue = new Queue<SystemError>();
        
        private void EnqueueErrorForMainThread(SystemError error)
        {
            lock (mainThreadErrorQueue)
            {
                mainThreadErrorQueue.Enqueue(error);
            }
        }

        private void Update()
        {
            // メインスレッドでのエラー処理
            lock (mainThreadErrorQueue)
            {
                while (mainThreadErrorQueue.Count > 0)
                {
                    var error = mainThreadErrorQueue.Dequeue();
                    HandleError(error);
                }
            }
        }

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
                Debug.Log($"<color=#{color}>{error.GetFormattedMessage()}</color>");
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

        public void HandleCriticalError(SystemError error)
        {
            Debug.LogError($"CRITICAL ERROR: {error.GetFormattedMessage()}");
            
            if (pauseOnCriticalError && Application.isEditor)
            {
                Debug.Break();
            }

            // 致命的エラーの場合、システム全体のリセットを試行
            if (error.Severity == ErrorSeverity.Fatal)
            {
                var coordinator = GameSystemCoordinator.Instance;
                coordinator?.HandleFatalError(error);
            }
        }

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
                        Debug.Log($"[UnityErrorHandler] Successfully recovered from error: {error.Message}");
                    }
                    return recovered;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityErrorHandler] Recovery action failed: {e.Message}");
                    return false;
                }
            }

            return false;
        }

        public void RegisterRecoveryAction(ErrorCategory category, Func<SystemError, bool> recoveryAction)
        {
            recoveryActions[category] = recoveryAction;
        }

        public List<SystemError> GetErrorHistory() => new List<SystemError>(errorHistory);

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

        public void ReportInfo(string systemName, string message) =>
            ReportError(systemName, ErrorSeverity.Info, ErrorCategory.System, message);

        public void ReportWarning(string systemName, string message) =>
            ReportError(systemName, ErrorSeverity.Warning, ErrorCategory.System, message);

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
```

#### Step 0.4: Unity特化システム管理基盤
**ファイル**: `Scripts/Core/Unity/UnityGameSystemCoordinator.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Core
{
    [DefaultExecutionOrder(-100)] // 他のシステムより先に実行
    public class UnityGameSystemCoordinator : MonoBehaviour, IErrorHandler
    {
        [Header("Unity Settings")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool autoInitializeOnAwake = true;
        [SerializeField] private bool handleApplicationFocus = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showSystemHealthInGUI = false;

        private Dictionary<string, IUnityGameSystem> unitySystem;
        private List<IUnityGameSystem> updateSystems;
        private UnityErrorHandler errorHandler;
        private bool isInitialized;
        private bool applicationPaused;

        private static UnityGameSystemCoordinator instance;
        public static UnityGameSystemCoordinator Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }

                InitializeCoordinator();

                if (autoInitializeOnAwake)
                {
                    InitializeAllSystems();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeCoordinator()
        {
            unitySystem = new Dictionary<string, IUnityGameSystem>();
            updateSystems = new List<IUnityGameSystem>();
            
            // エラーハンドラーの初期化
            errorHandler = FindObjectOfType<UnityErrorHandler>();
            if (errorHandler == null)
            {
                var errorHandlerGO = new GameObject("UnityErrorHandler");
                errorHandler = errorHandlerGO.AddComponent<UnityErrorHandler>();
                errorHandlerGO.transform.SetParent(transform);
            }

            isInitialized = true;

            if (enableDebugLogs)
                Debug.Log("[UnityGameSystemCoordinator] Unity-specific coordinator initialized");
        }

        public void RegisterUnitySystem(IUnityGameSystem system)
        {
            if (system == null)
            {
                errorHandler?.ReportError("UnityGameSystemCoordinator", "Cannot register null Unity system");
                return;
            }

            string systemName = system.GetSystemName();
            if (unitySystem.ContainsKey(systemName))
            {
                errorHandler?.ReportWarning("UnityGameSystemCoordinator", 
                    $"Unity system {systemName} is already registered");
                return;
            }

            unitySystem[systemName] = system;
            
            if (system.RequiresUpdate)
            {
                updateSystems.Add(system);
                updateSystems = updateSystems.OrderBy(s => s.ExecutionOrder).ToList();
            }

            // システム初期化（既に全体初期化済みの場合）
            if (isInitialized)
            {
                try
                {
                    system.OnAwake();
                    system.Initialize();
                    if (gameObject.activeInHierarchy)
                    {
                        system.OnStart();
                    }
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportCritical("UnityGameSystemCoordinator", 
                        $"Failed to initialize Unity system {systemName}", e);
                }
            }

            if (enableDebugLogs)
                Debug.Log($"[UnityGameSystemCoordinator] Registered Unity system: {systemName}");
        }

        private void Start()
        {
            foreach (var system in unitySystem.Values)
            {
                try
                {
                    system.OnStart();
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportError("UnityGameSystemCoordinator", 
                        $"Failed to start Unity system {system.GetSystemName()}", e);
                }
            }
        }

        private void Update()
        {
            if (applicationPaused) return;

            foreach (var system in updateSystems)
            {
                try
                {
                    system.Update();
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportError("UnityGameSystemCoordinator", 
                        $"Error in Unity system update {system.GetSystemName()}", e);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!handleApplicationFocus) return;

            applicationPaused = pauseStatus;
            
            foreach (var system in unitySystem.Values)
            {
                if (system is IApplicationLifecycle lifecycle)
                {
                    try
                    {
                        if (pauseStatus)
                            lifecycle.OnApplicationPause();
                        else
                            lifecycle.OnApplicationResume();
                    }
                    catch (System.Exception e)
                    {
                        errorHandler?.ReportError("UnityGameSystemCoordinator", 
                            $"Error in application lifecycle {system.GetSystemName()}", e);
                    }
                }
            }
        }

        public void HandleFatalError(SystemError error)
        {
            Debug.LogError($"[UnityGameSystemCoordinator] Handling fatal error: {error.Message}");
            
            // 安全な状態へのリセット試行
            try
            {
                ResetAllUnitySystemsToSafeState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UnityGameSystemCoordinator] Failed to reset to safe state: {e.Message}");
                
                // 最後の手段：シーンリロード
                if (Application.isPlaying)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
            }
        }

        private void ResetAllUnitySystemsToSafeState()
        {
            foreach (var system in unitySystem.Values)
            {
                if (system is IUnityResettable resettable)
                {
                    try
                    {
                        resettable.ResetUnityState();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to reset Unity system {system.GetSystemName()}: {e.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var system in unitySystem.Values)
            {
                try
                {
                    system.OnDestroy();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error destroying Unity system {system.GetSystemName()}: {e.Message}");
                }
            }
        }

        private void OnGUI()
        {
            if (!showSystemHealthInGUI || !enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("System Health", GUI.skin.box);
            
            foreach (var system in unitySystem.Values)
            {
                var color = system.IsInitialized() ? Color.green : Color.red;
                var colorText = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{system.GetSystemName()}</color>";
                GUILayout.Label(colorText);
            }
            
            GUILayout.EndArea();
        }

        // IErrorHandler implementation
        public void HandleError(SystemError error) => errorHandler?.HandleError(error);
        public void HandleCriticalError(SystemError error) => errorHandler?.HandleCriticalError(error);
        public bool TryRecover(SystemError error) => errorHandler?.TryRecover(error) ?? false;
        public void RegisterRecoveryAction(ErrorCategory category, System.Func<SystemError, bool> recoveryAction) =>
            errorHandler?.RegisterRecoveryAction(category, recoveryAction);
        public List<SystemError> GetErrorHistory() => errorHandler?.GetErrorHistory() ?? new List<SystemError>();
        public void ClearErrorHistory() => errorHandler?.ClearErrorHistory();
    }

    // アプリケーションライフサイクル管理用インターフェース
    public interface IApplicationLifecycle
    {
        void OnApplicationPause();
        void OnApplicationResume();
        void OnApplicationFocus(bool hasFocus);
    }
}
```

#### Step 0.5: 開発設定システム
**ファイル**: `Assets/Data/Settings/DevelopmentSettings.asset`（ScriptableObject）

```csharp
using UnityEngine;

namespace GatchaSpire.Core
{
    [CreateAssetMenu(fileName = "DevelopmentSettings", menuName = "GatchaSpire/Settings/Development Settings")]
    public class DevelopmentSettings : ScriptableObject, IValidatable
    {
        [Header("Debug")]
        public bool enableAllDebugLogs = true;
        public bool showSystemHealth = false;
        public bool enableGodMode = false;
        public bool pauseOnCriticalError = true;

        [Header("Performance")]
        public bool skipAnimations = false;
        public bool fastBattles = false;
        public bool disableParticles = false;
        public int targetFrameRate = 60;

        [Header("Balance Testing")]
        [Range(0.1f, 10.0f)]
        public float globalGoldMultiplier = 1.0f;
        [Range(0.1f, 10.0f)]
        public float globalExpMultiplier = 1.0f;
        [Range(0.1f, 10.0f)]
        public float battleSpeedMultiplier = 1.0f;

        [Header("Testing")]
        public bool unlockAllCharacters = false;
        public bool infiniteGold = false;
        public bool maxGachaLevel = false;

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (globalGoldMultiplier <= 0)
                result.AddError("Global gold multiplier must be greater than 0");
            if (globalExpMultiplier <= 0)
                result.AddError("Global exp multiplier must be greater than 0");
            if (targetFrameRate < 30)
                result.AddWarning("Target frame rate below 30 may cause poor user experience");

            return result;
        }

        public void OnValidate()
        {
            var validation = Validate();
            if (!validation.IsValid)
            {
                Debug.LogWarning($"[DevelopmentSettings] Validation issues: {validation.GetSummary()}");
            }
        }

        public void ApplySettings()
        {
            Application.targetFrameRate = targetFrameRate;
            
            if (enableAllDebugLogs)
                Debug.Log("[DevelopmentSettings] Development settings applied");
        }
    }

    [System.Serializable]
    public class ValidationResult
    {
        [SerializeField] private List<string> errors = new List<string>();
        [SerializeField] private List<string> warnings = new List<string>();

        public bool IsValid => errors.Count == 0;
        public IReadOnlyList<string> Errors => errors;
        public IReadOnlyList<string> Warnings => warnings;

        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);

        public string GetSummary()
        {
            var summary = "";
            if (errors.Count > 0)
                summary += $"Errors: {string.Join(", ", errors)}";
            if (warnings.Count > 0)
                summary += $"Warnings: {string.Join(", ", warnings)}";
            return summary;
        }
    }
}
```

---

## Phase 0 完了チェックリスト（修正版）

### Unity特化基盤
- [ ] UnityGameSystemCoordinator が正常動作
- [ ] IUnityGameSystem インターフェースが機能
- [ ] ScriptableObject検証システムが動作
- [ ] DontDestroyOnLoad が適切に設定
- [ ] Script Execution Order が適切

### エラーハンドリング
- [ ] UnityErrorHandler が Unity ログを捕捉
- [ ] SystemError の重要度別処理が動作
- [ ] 致命的エラー時の安全なリセットが機能
- [ ] エラー履歴の記録・表示が正常
- [ ] 復旧アクションの登録・実行が動作

### 開発支援
- [ ] DevelopmentSettings の検証が動作
- [ ] エディタでの設定変更が即座に反映
- [ ] デバッグ情報の表示が適切
- [ ] パフォーマンス設定が機能

### 統合確認
- [ ] 全システムがエラーなく初期化
- [ ] Unity ライフサイクルとの適切な連携
- [ ] シーン遷移時の状態保持が正常
- [ ] メモリリークの発生なし

この修正により、Unity特化の堅牢な基盤とエラーハンドリングが確立されます。