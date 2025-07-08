using System;

namespace GatchaSpire.Core
{
    /// <summary>
    /// 永続化システムインターフェース
    /// 永続的なデータ管理機能を提供（解放要素など）
    /// </summary>
    public interface IPersistentSystem
    {
        /// <summary>
        /// データの保存
        /// </summary>
        void SaveData();

        /// <summary>
        /// データの読み込み
        /// </summary>
        void LoadData();

        /// <summary>
        /// 保存データが存在するかどうか
        /// </summary>
        bool HasSaveData();

        /// <summary>
        /// 保存データの削除
        /// </summary>
        void DeleteSaveData();
    }

    /// <summary>
    /// アプリケーションライフサイクル管理インターフェース
    /// </summary>
    public interface IApplicationLifecycle
    {
        /// <summary>
        /// アプリケーション一時停止時
        /// </summary>
        void OnApplicationPause();

        /// <summary>
        /// アプリケーション復帰時
        /// </summary>
        void OnApplicationResume();

        /// <summary>
        /// アプリケーションフォーカス変更時
        /// </summary>
        void OnApplicationFocus(bool hasFocus);
    }
}