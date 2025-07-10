namespace GatchaSpire.Core
{
    /// <summary>
    /// Unity特化ゲームシステムインターフェース
    /// MonoBehaviourライフサイクルとの統合を提供
    /// </summary>
    public interface IUnityGameSystem : IGameSystem
    {
        /// <summary>
        /// Unity Awake相当の初期化
        /// </summary>
        void OnAwake();

        /// <summary>
        /// Unity Start相当の初期化
        /// </summary>
        void OnStart();

        /// <summary>
        /// Unity OnDestroy相当の終了処理
        /// </summary>
        void OnDestroy();

        /// <summary>
        /// 毎フレーム更新が必要かどうか
        /// </summary>
        bool RequiresUpdate { get; }

        /// <summary>
        /// Update実行順序
        /// </summary>
        int ExecutionOrder { get; }
    }
}