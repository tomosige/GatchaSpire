using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GatchaSpire.Core.Error;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core
{
    /// <summary>
    /// Unity特化ゲームシステムコーディネーター
    /// 全てのゲームシステムの統合管理とUnityライフサイクルとの連携を提供
    /// </summary>
    [DefaultExecutionOrder(-100)] // 他のシステムより先に実行
    public class UnityGameSystemCoordinator : MonoBehaviour, IErrorHandler
    {
        [Header("Unity設定")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool autoInitializeOnAwake = true;
        [SerializeField] private bool handleApplicationFocus = true;

        [Header("デバッグ")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showSystemHealthInGUI = false;

        private Dictionary<string, IUnityGameSystem> unitySystems = new Dictionary<string, IUnityGameSystem>();
        private List<IUnityGameSystem> updateSystems = new List<IUnityGameSystem>();
        private UnityErrorHandler errorHandler;
        private DependencyResolver dependencyResolver;
        private bool isInitialized;
        private bool applicationPaused;
        private bool systemsStarted;

        private static UnityGameSystemCoordinator instance;
        
        /// <summary>シングルトンインスタンス</summary>
        public static UnityGameSystemCoordinator Instance => instance;

        /// <summary>初期化状態</summary>
        public bool IsInitialized => isInitialized;

        /// <summary>登録されているシステム数</summary>
        public int SystemCount => unitySystems?.Count ?? 0;


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

        /// <summary>
        /// コーディネーターの初期化
        /// </summary>
        private void InitializeCoordinator()
        {
            unitySystems = new Dictionary<string, IUnityGameSystem>();
            updateSystems = new List<IUnityGameSystem>();
            dependencyResolver = new DependencyResolver();
            
            // エラーハンドラーの初期化または検索
            errorHandler = FindObjectOfType<UnityErrorHandler>();
            if (errorHandler == null)
            {
                var errorHandlerGO = new GameObject("UnityErrorHandler");
                errorHandler = errorHandlerGO.AddComponent<UnityErrorHandler>();
                errorHandlerGO.transform.SetParent(transform);
            }

            isInitialized = true;

            if (enableDebugLogs)
                Debug.Log("[UnityGameSystemCoordinator] Unity特化コーディネーターを初期化しました");
        }

        /// <summary>
        /// Unityシステムを登録
        /// </summary>
        /// <param name="system">登録するシステム</param>
        public void RegisterUnitySystem(IUnityGameSystem system)
        {
            if (system == null)
            {
                errorHandler?.ReportError("UnityGameSystemCoordinator", "nullシステムは登録できません");
                return;
            }

            string systemName = system.GetSystemName();
            if (unitySystems.ContainsKey(systemName))
            {
                errorHandler?.ReportWarning("UnityGameSystemCoordinator", 
                    $"システム {systemName} は既に登録済みです");
                return;
            }

            unitySystems[systemName] = system;
            
            if (system.RequiresUpdate)
            {
                updateSystems.Add(system);
                updateSystems = updateSystems.OrderBy(s => s.ExecutionOrder).ToList();
            }

            // 既に初期化済みの場合、即座に初期化
            if (isInitialized)
            {
                try
                {
                    system.OnAwake();
                    system.Initialize();
                    
                    // Start済みの場合はOnStartも実行
                    if (systemsStarted && gameObject.activeInHierarchy)
                    {
                        system.OnStart();
                    }
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportCritical("UnityGameSystemCoordinator", 
                        $"システム {systemName} の初期化に失敗しました", e);
                }
            }

            if (enableDebugLogs)
                Debug.Log($"[UnityGameSystemCoordinator] システムを登録しました: {systemName}");
        }

        /// <summary>
        /// システムの登録を解除
        /// </summary>
        /// <param name="systemName">解除するシステム名</param>
        public void UnregisterSystem(string systemName)
        {
            if (unitySystems.TryGetValue(systemName, out var system))
            {
                try
                {
                    system.OnDestroy();
                    system.Shutdown();
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportError("UnityGameSystemCoordinator", 
                        $"システム {systemName} の終了処理でエラーが発生しました", e);
                }

                unitySystems.Remove(systemName);
                updateSystems.Remove(system);

                if (enableDebugLogs)
                    Debug.Log($"[UnityGameSystemCoordinator] システムの登録を解除しました: {systemName}");
            }
        }

        /// <summary>
        /// 依存関係を指定してUnityシステムを登録
        /// </summary>
        /// <param name="system">登録するシステム</param>
        /// <param name="dependsOn">依存するシステム名の配列</param>
        public void RegisterUnitySystem(IUnityGameSystem system, params string[] dependsOn)
        {
            RegisterUnitySystem(system);
            
            if (dependsOn != null && dependsOn.Length > 0)
            {
                dependencyResolver.RegisterSystem(system, dependsOn);
            }
            else
            {
                dependencyResolver.RegisterSystem(system);
            }
        }

        /// <summary>
        /// 全システムの初期化（依存関係解決版）
        /// </summary>
        public void InitializeAllSystems()
        {
            if (!isInitialized) return;

            try
            {
                // 依存関係の検証
                var validation = dependencyResolver.ValidateDependencies();
                if (!validation.IsValid)
                {
                    errorHandler?.ReportError("UnityGameSystemCoordinator", 
                        $"依存関係の問題が検出されました: {validation.GetSummary()}");
                }

                // 依存関係に基づいた初期化順序で実行
                var orderedSystems = dependencyResolver.ResolveInitializationOrder();

                foreach (var system in orderedSystems)
                {
                    try
                    {
                        system.OnAwake();
                        system.Initialize();
                    }
                    catch (System.Exception e)
                    {
                        errorHandler?.ReportCritical("UnityGameSystemCoordinator", 
                            $"システム {system.GetSystemName()} の初期化に失敗しました", e);
                    }
                }

                if (enableDebugLogs)
                {
                    Debug.Log($"[UnityGameSystemCoordinator] {orderedSystems.Count}個のシステムを依存関係順に初期化しました");
                    Debug.Log($"初期化順序: {string.Join(" -> ", orderedSystems.Select(s => s.GetSystemName()))}");
                }
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportCritical("UnityGameSystemCoordinator", 
                    "依存関係解決に失敗しました", e);
                
                // フォールバック：優先度順で初期化
                var sortedSystems = unitySystems.Values.OrderBy(s => s.Priority).ToList();
                foreach (var system in sortedSystems)
                {
                    try
                    {
                        if (!system.IsInitialized())
                        {
                            system.OnAwake();
                            system.Initialize();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        errorHandler?.ReportCritical("UnityGameSystemCoordinator", 
                            $"フォールバック初期化でもシステム {system.GetSystemName()} が失敗しました", ex);
                    }
                }
            }
        }

        /// <summary>
        /// システムを名前で取得
        /// </summary>
        /// <typeparam name="T">システムの型</typeparam>
        /// <param name="systemName">システム名</param>
        /// <returns>見つかったシステム、またはnull</returns>
        public T GetSystem<T>(string systemName) where T : class, IUnityGameSystem
        {
            return unitySystems.TryGetValue(systemName, out var system) ? system as T : null;
        }

        /// <summary>
        /// 全システムの状態確認
        /// </summary>
        /// <returns>全システムが正常に初期化されている場合true</returns>
        public bool AreAllSystemsHealthy()
        {
            return unitySystems.Values.All(system => system.IsInitialized());
        }

        private void Start()
        {
            systemsStarted = true;
            
            foreach (var system in unitySystems.Values)
            {
                try
                {
                    system.OnStart();
                }
                catch (System.Exception e)
                {
                    errorHandler?.ReportError("UnityGameSystemCoordinator", 
                        $"システム {system.GetSystemName()} のStart処理でエラーが発生しました", e);
                }
            }

            if (enableDebugLogs)
                Debug.Log("[UnityGameSystemCoordinator] 全システムのStart処理が完了しました");
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
                        $"システム {system.GetSystemName()} のUpdate処理でエラーが発生しました", e);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!handleApplicationFocus) return;

            applicationPaused = pauseStatus;
            
            foreach (var system in unitySystems.Values)
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
                            $"システム {system.GetSystemName()} のアプリケーションライフサイクル処理でエラーが発生しました", e);
                    }
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!handleApplicationFocus) return;

            foreach (var system in unitySystems.Values)
            {
                if (system is IApplicationLifecycle lifecycle)
                {
                    try
                    {
                        lifecycle.OnApplicationFocus(hasFocus);
                    }
                    catch (System.Exception e)
                    {
                        errorHandler?.ReportError("UnityGameSystemCoordinator", 
                            $"システム {system.GetSystemName()} のフォーカス処理でエラーが発生しました", e);
                    }
                }
            }
        }

        /// <summary>
        /// 致命的エラー時の安全なリセット
        /// </summary>
        /// <param name="error">発生した致命的エラー</param>
        public void HandleFatalError(SystemError error)
        {
            Debug.LogError($"[UnityGameSystemCoordinator] 致命的エラーを処理中: {error.Message}");
            
            try
            {
                ResetAllUnitySystemsToSafeState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UnityGameSystemCoordinator] 安全状態へのリセットに失敗しました: {e.Message}");
                
                // 最後の手段：シーンリロード
                if (Application.isPlaying)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
            }
        }

        /// <summary>
        /// 全システムを安全な状態にリセット
        /// </summary>
        private void ResetAllUnitySystemsToSafeState()
        {
            foreach (var system in unitySystems.Values)
            {
                if (system is IUnityResettable resettable)
                {
                    try
                    {
                        resettable.ResetUnityState();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"システム {system.GetSystemName()} のリセットに失敗しました: {e.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var system in unitySystems.Values)
            {
                try
                {
                    system.OnDestroy();
                    system.Shutdown();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"システム {system.GetSystemName()} の破棄処理でエラーが発生しました: {e.Message}");
                }
            }

            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnGUI()
        {
            if (!showSystemHealthInGUI || !enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("システム状態", GUI.skin.box);
            
            foreach (var system in unitySystems.Values)
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
}