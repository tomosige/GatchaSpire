using System;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジー効果の適用対象
    /// </summary>
    [Serializable]
    public enum SynergyTarget
    {
        Self = 1,           // 自分のみ
        SynergyOwners = 2,  // シナジー保有者のみ
        AllAllies = 3,      // 全味方
        AllEnemies = 4,     // 全敵
        All = 5             // 全体
    }

    /// <summary>
    /// シナジー効果の種類
    /// </summary>
    [Serializable]
    public enum SynergyEffectType
    {
        StatModifier = 1,   // ステータス修正型
        TriggerAbility = 2, // 発動能力型
        PassiveAbility = 3  // パッシブ能力型
    }

    /// <summary>
    /// ステータス修正の種類
    /// </summary>
    [Serializable]
    public enum StatModifierType
    {
        Additive = 1,       // 加算
        Multiplicative = 2  // 乗算
    }

    /// <summary>
    /// 発動条件の種類
    /// </summary>
    [Serializable]
    public enum TriggerCondition
    {
        BattleStart = 1,    // 戦闘開始時
        TimeElapsed = 2,    // 時間経過
        OnDamage = 3,       // ダメージ時
        OnHeal = 4,         // 回復時
        OnDeath = 5,        // 死亡時
        OnKill = 6,         // 撃破時
        OnCritical = 7,     // クリティカル時
        OnSkillUse = 8      // スキル使用時
    }
}