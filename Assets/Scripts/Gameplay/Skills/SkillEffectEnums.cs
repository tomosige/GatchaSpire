using UnityEngine;
using System;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// スキル効果のスケーリング元ステータス
    /// 既存StatTypeを拡張してスキル効果用の追加要素を含む
    /// </summary>
    [Serializable]
    public enum ScalingAttribute
    {
        // 基本ステータス（StatTypeと対応）
        HP = StatType.HP,
        MP = StatType.MP,
        Attack = StatType.Attack,
        Defense = StatType.Defense,
        Speed = StatType.Speed,
        Magic = StatType.Magic,
        Resistance = StatType.Resistance,
        Luck = StatType.Luck,
        Critical = StatType.Critical,
        Accuracy = StatType.Accuracy,
        
        // スキル効果専用のスケーリング要素
        MaxHP = 1001,          // 最大HP
        CurrentHP = 1002,      // 現在HP
        MissingHP = 1003,      // 失われたHP（MaxHP - CurrentHP）
        Level = 1004,          // キャラクターレベル
        MaxMP = 1005,          // 最大MP
        CurrentMP = 1006,      // 現在MP
        MissingMP = 1007,      // 失われたMP（MaxMP - CurrentMP）
        
        // 計算ステータス
        BattlePower = 1010,    // 戦闘力
        HPPercentage = 1011,   // HP割合（0.0-1.0）
        MPPercentage = 1012,   // MP割合（0.0-1.0）
        
        // 固定値（スケーリングなし）
        None = 0
    }

    /// <summary>
    /// スキル効果のスタック挙動定義
    /// 同一効果が重複適用された際の処理方式
    /// </summary>
    [Serializable]
    public enum StackBehavior
    {
        /// <summary>継続時間をリセット（効果値は変わらず、時間のみ最新に更新）</summary>
        Refresh = 1,
        
        /// <summary>継続時間を延長（既存の残り時間 + 新規時間）</summary>
        Extend = 2,
        
        /// <summary>効果値を増加（BaseValue * スタック数）</summary>
        Intensify = 3,
        
        /// <summary>独立効果（別々の効果として並行動作）</summary>
        Independent = 4,
        
        /// <summary>上書き（古い効果を削除して新規効果で置換）</summary>
        Override = 5
    }

    /// <summary>
    /// スキル効果の対象タイプ
    /// 効果が適用される対象の種類を定義
    /// </summary>
    [Serializable]
    public enum SkillTargetType
    {
        /// <summary>自分自身</summary>
        Self = 1,
        
        /// <summary>単体の味方</summary>
        SingleAlly = 2,
        
        /// <summary>単体の敵</summary>
        SingleEnemy = 3,
        
        /// <summary>全ての味方</summary>
        AllAllies = 4,
        
        /// <summary>全ての敵</summary>
        AllEnemies = 5,
        
        /// <summary>範囲内の味方（MaxTargetsで制限）</summary>
        AreaAllies = 6,
        
        /// <summary>範囲内の敵（MaxTargetsで制限）</summary>
        AreaEnemies = 7,
        
        /// <summary>ランダムな味方（MaxTargetsで制限）</summary>
        RandomAllies = 8,
        
        /// <summary>ランダムな敵（MaxTargetsで制限）</summary>
        RandomEnemies = 9,
        
        /// <summary>最も近い味方</summary>
        NearestAlly = 10,
        
        /// <summary>最も近い敵</summary>
        NearestEnemy = 11,
        
        /// <summary>HPが最も低い味方</summary>
        LowestHPAlly = 12,
        
        /// <summary>HPが最も低い敵</summary>
        LowestHPEnemy = 13,
        
        /// <summary>全ての対象（敵味方問わず）</summary>
        Everyone = 99
    }

    /// <summary>
    /// スキル効果の種類
    /// 効果の大分類を定義
    /// </summary>
    [Serializable]
    public enum SkillEffectType
    {
        /// <summary>ダメージ効果</summary>
        Damage = 1,
        
        /// <summary>回復効果</summary>
        Heal = 2,
        
        /// <summary>ステータス修正効果</summary>
        StatModifier = 3,
        
        /// <summary>状態異常効果</summary>
        StatusEffect = 4,
        
        /// <summary>位置操作効果</summary>
        Displacement = 5,
        
        /// <summary>シールド効果</summary>
        Shield = 6,
        
        /// <summary>特殊効果</summary>
        Special = 99
    }

    /// <summary>
    /// ダメージの種類
    /// 物理・魔法・属性等の区分
    /// </summary>
    [Serializable]
    public enum DamageType
    {
        /// <summary>物理ダメージ</summary>
        Physical = 1,
        
        /// <summary>魔法ダメージ</summary>
        Magical = 2,
        
        /// <summary>確定ダメージ（防御無視）</summary>
        True = 3,
        
        /// <summary>火属性ダメージ</summary>
        Fire = 4,
        
        /// <summary>水属性ダメージ</summary>
        Water = 5,
        
        /// <summary>土属性ダメージ</summary>
        Earth = 6,
        
        /// <summary>風属性ダメージ</summary>
        Air = 7,
        
        /// <summary>光属性ダメージ</summary>
        Light = 8,
        
        /// <summary>闇属性ダメージ</summary>
        Dark = 9,
        
        /// <summary>雷属性ダメージ</summary>
        Lightning = 10,
        
        /// <summary>氷属性ダメージ</summary>
        Ice = 11
    }

    /// <summary>
    /// 修正値の適用方式
    /// ステータス変更時の計算方法
    /// </summary>
    [Serializable]
    public enum ModifierType
    {
        /// <summary>加算修正（基本値 + 修正値）</summary>
        Additive = 1,
        
        /// <summary>乗算修正（基本値 * 修正値）</summary>
        Multiplicative = 2,
        
        /// <summary>パーセンテージ修正（基本値 * (1 + 修正値/100)）</summary>
        Percentage = 3,
        
        /// <summary>固定値設定（修正値で上書き）</summary>
        Override = 4
    }
}