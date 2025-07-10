namespace GatchaSpire.Core
{
    /// <summary>
    /// リセット可能システムインターフェース
    /// ランごとの完全リセット機能を提供
    /// </summary>
    public interface IResettableSystem
    {
        /// <summary>
        /// システムの完全リセット
        /// ランの開始時に全データを初期状態に戻す
        /// </summary>
        void ResetSystem();

        /// <summary>
        /// リセット後の状態確認
        /// </summary>
        bool IsResetToDefault();

        /// <summary>
        /// リセット可能かどうかの確認
        /// </summary>
        bool CanReset();
    }

    /// <summary>
    /// Unity特化リセット可能システムインターフェース
    /// </summary>
    public interface IUnityResettable : IResettableSystem
    {
        /// <summary>
        /// Unityオブジェクトの状態リセット
        /// </summary>
        void ResetUnityState();

        /// <summary>
        /// シーンロード時に保持するかどうか
        /// </summary>
        bool PreserveDuringSceneLoad { get; }
    }
}