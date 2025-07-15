using UnityEngine;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// ゲームシステムの基底クラス
    /// IUnityGameSystemの基本実装を提供
    /// </summary>
    public abstract class GameSystemBase : MonoBehaviour, IUnityGameSystem, IUnityResettable
    {
        [Header("システム設定")]
        [SerializeField] protected bool enableDebugLogs = true;
        [SerializeField] protected SystemPriority priority = SystemPriority.Medium;
        [SerializeField] protected bool requiresUpdate = false;
        [SerializeField] protected int executionOrder = 0;

        private bool isInitialized = false;
        protected UnityErrorHandler errorHandler;

        /// <summary>システム名</summary>
        protected abstract string SystemName { get; }

        // IGameSystem implementation
        public virtual string GetSystemName() => SystemName;
        public virtual SystemPriority Priority => priority;
        public virtual bool IsInitialized() => isInitialized;

        // IUnityGameSystem implementation
        public virtual bool RequiresUpdate => requiresUpdate;
        public virtual int ExecutionOrder => executionOrder;

        // IUnityResettable implementation
        public virtual bool PreserveDuringSceneLoad => true;

        /// <summary>
        /// Unity Awake - システム登録とエラーハンドラー取得
        /// </summary>
        public virtual void OnAwake()
        {
            errorHandler = UnityErrorHandler.Instance;
            
            // システムコーディネーターに自動登録
            var coordinator = UnityGameSystemCoordinator.Instance;
            if (coordinator != null)
            {
                coordinator.RegisterUnitySystem(this);
            }
            else if (enableDebugLogs)
            {
                Debug.LogWarning($"[{SystemName}] UnityGameSystemCoordinatorが見つかりません");
            }

            OnSystemAwake();
        }

        /// <summary>
        /// システム初期化
        /// </summary>
        public virtual void Initialize()
        {
            try
            {
                OnSystemInitialize();
                isInitialized = true;

                if (enableDebugLogs)
                {
                    Debug.Log($"[{SystemName}] システムを初期化しました");
                }
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportCritical(SystemName, "システムの初期化に失敗しました", e);
                throw;
            }
        }

        /// <summary>
        /// Unity Start相当
        /// </summary>
        public virtual void OnStart()
        {
            try
            {
                OnSystemStart();
                
                if (enableDebugLogs)
                    Debug.Log($"[{SystemName}] システムを開始しました");
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "システムの開始処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void Update()
        {
            if (!isInitialized) return;

            try
            {
                OnSystemUpdate();
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "システムの更新処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// システム終了
        /// </summary>
        public virtual void Shutdown()
        {
            try
            {
                OnSystemShutdown();
                isInitialized = false;
                
                if (enableDebugLogs)
                    Debug.Log($"[{SystemName}] システムを終了しました");
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "システムの終了処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// Unity OnDestroy相当
        /// </summary>
        public virtual void OnDestroy()
        {
            try
            {
                OnSystemDestroy();
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "システムの破棄処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// システムリセット
        /// </summary>
        public virtual void ResetSystem()
        {
            try
            {
                OnSystemReset();
                
                if (enableDebugLogs)
                    Debug.Log($"[{SystemName}] システムをリセットしました");
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "システムのリセット処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// Unityオブジェクトの状態リセット
        /// </summary>
        public virtual void ResetUnityState()
        {
            try
            {
                OnUnityStateReset();
            }
            catch (System.Exception e)
            {
                errorHandler?.ReportError(SystemName, "Unityオブジェクトのリセット処理でエラーが発生しました", e);
            }
        }

        /// <summary>
        /// リセット可能状態の確認
        /// </summary>
        public virtual bool CanReset() => isInitialized;

        /// <summary>
        /// デフォルト状態への復帰確認
        /// </summary>
        public virtual bool IsResetToDefault() => true;

        // 継承クラスで実装する仮想メソッド
        protected virtual void OnSystemAwake() { }
        protected virtual void OnSystemInitialize() { }
        protected virtual void OnSystemStart() { }
        protected virtual void OnSystemUpdate() { }
        protected virtual void OnSystemShutdown() { }
        protected virtual void OnSystemDestroy() { }
        protected virtual void OnSystemReset() { }
        protected virtual void OnUnityStateReset() { }

        /// <summary>
        /// エラーを報告する便利メソッド
        /// </summary>
        protected void ReportError(string message, System.Exception exception = null)
        {
            errorHandler?.ReportError(SystemName, message, exception);
        }

        /// <summary>
        /// 警告を報告する便利メソッド
        /// </summary>
        protected void ReportWarning(string message)
        {
            errorHandler?.ReportWarning(SystemName, message);
        }

        /// <summary>
        /// 情報を報告する便利メソッド
        /// </summary>
        protected void ReportInfo(string message)
        {
            errorHandler?.ReportInfo(SystemName, message);
        }

        /// <summary>
        /// クリティカルエラーを報告する便利メソッド
        /// </summary>
        protected void ReportCritical(string message, System.Exception exception = null)
        {
            errorHandler?.ReportCritical(SystemName, message, exception);
        }
    }
}