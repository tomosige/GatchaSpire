using System;

namespace GatchaSpire.Core
{
    /// <summary>
    /// ゲームシステムの基本インターフェース
    /// 全てのゲームシステムが実装する必要がある共通機能を定義
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>
        /// システム名を取得
        /// </summary>
        string GetSystemName();

        /// <summary>
        /// システムの初期化
        /// </summary>
        void Initialize();

        /// <summary>
        /// システムの更新処理
        /// </summary>
        void Update();

        /// <summary>
        /// システムの終了処理
        /// </summary>
        void Shutdown();

        /// <summary>
        /// システムが初期化済みかどうか
        /// </summary>
        bool IsInitialized();

        /// <summary>
        /// システムの優先度（初期化順序）
        /// </summary>
        SystemPriority Priority { get; }
    }

    /// <summary>
    /// システムの優先度定義
    /// 数値が小さいほど先に初期化される
    /// </summary>
    public enum SystemPriority
    {
        Critical = 0,   // システム管理、エラーハンドリング
        High = 100,     // データベース、設定管理
        Medium = 200,   // ゲームロジック
        Low = 300,      // UI、エフェクト
        Lowest = 400    // デバッグ、統計
    }
}