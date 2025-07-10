using System;

namespace GatchaSpire.Core.Gacha
{
    /// <summary>
    /// ガチャアップグレードの効果タイプ
    /// </summary>
    [Serializable]
    public enum GachaUpgradeType
    {
        RarityRateIncrease = 1,     // レアリティ排出率増加
        CostReduction = 2,          // コスト削減
        SimultaneousPull = 3,       // 同時排出数増加
        GuaranteedRare = 4,         // レア保証
        BonusCharacter = 5,         // ボーナスキャラクター
        CeilingReduction = 6        // 天井回数削減
    }

    /// <summary>
    /// ガチャ結果の種類
    /// </summary>
    [Serializable]
    public enum GachaResultType
    {
        Normal = 1,                 // 通常
        Guaranteed = 2,             // 保証
        Ceiling = 3,                // 天井
        Pickup = 4,                 // ピックアップ
        Bonus = 5                   // ボーナス
    }

    /// <summary>
    /// ガチャ実行結果の状態
    /// </summary>
    [Serializable]
    public enum GachaStatus
    {
        Success = 1,                // 成功
        InsufficientGold = 2,       // ゴールド不足
        SystemError = 3,            // システムエラー
        InvalidData = 4,            // データ不正
        Disabled = 5                // 無効
    }

    /// <summary>
    /// ガチャプールの状態
    /// </summary>
    [Serializable]
    public enum GachaPoolStatus
    {
        Active = 1,                 // アクティブ
        Inactive = 2,               // 非アクティブ
        Maintenance = 3,            // メンテナンス中
        Expired = 4                 // 期限切れ
    }
}