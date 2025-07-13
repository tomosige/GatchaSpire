using UnityEngine;
using System;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// ステータス修正効果クラス
    /// 一時的なステータス増減（バフ/デバフ）を管理
    /// 仕様書3.2のStatModifierEffectに基づく実装
    /// </summary>
    [Serializable]
    public class StatModifierEffect : SkillEffect
    {
        [Header("ステータス修正設定")]
        [SerializeField] private StatType targetStat = StatType.Attack;
        [SerializeField] private ModifierType modifierType = ModifierType.Additive;
        [SerializeField] private bool isDebuff = false;

        /// <summary>
        /// 対象ステータス
        /// </summary>
        public StatType TargetStat
        {
            get => targetStat;
            set => targetStat = value;
        }

        /// <summary>
        /// 修正値の適用方式（加算/乗算/パーセンテージ/上書き）
        /// </summary>
        public ModifierType ModifierType
        {
            get => modifierType;
            set => modifierType = value;
        }

        /// <summary>
        /// デバフかどうか（効果の色や表示に影響）
        /// </summary>
        public bool IsDebuff
        {
            get => isDebuff;
            set => isDebuff = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public StatModifierEffect() : base()
        {
            EffectType = SkillEffectType.StatModifier;
            TargetType = SkillTargetType.SingleAlly;
            targetStat = StatType.Attack;
            modifierType = ModifierType.Additive;
            isDebuff = false;
            Duration = 30f; // デフォルト30秒継続
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">効果名</param>
        /// <param name="desc">説明</param>
        /// <param name="stat">対象ステータス</param>
        /// <param name="value">修正値</param>
        /// <param name="duration">継続時間</param>
        /// <param name="debuff">デバフかどうか</param>
        public StatModifierEffect(string name, string desc, StatType stat, float value, float duration = 30f, bool debuff = false)
            : base(name, desc, SkillEffectType.StatModifier, value)
        {
            TargetType = debuff ? SkillTargetType.SingleEnemy : SkillTargetType.SingleAlly;
            targetStat = stat;
            modifierType = ModifierType.Additive;
            isDebuff = debuff;
            Duration = duration;
            
            // デバフの場合は敵対象、バフの場合は味方対象
            if (debuff)
            {
                EffectColor = Color.red;
            }
            else
            {
                EffectColor = Color.green;
            }
        }

        /// <summary>
        /// ステータス修正効果が適用可能かどうかを判定
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        /// <returns>適用可能かどうか</returns>
        public override bool CanApply(Character target, Character caster)
        {
            if (target == null || caster == null)
                return false;

            // 対象が生存している場合のみ適用可能
            return target.IsAlive;
        }

        /// <summary>
        /// ステータス修正効果を適用
        /// 今回はプロパティテスト用なので空実装
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        public override void Apply(Character target, Character caster)
        {
            // 実際のステータス修正処理は将来のPhaseで実装
            // 今回はTestSkillEffectPropertiesのためのクラス存在確認が目的
            
            if (!CanApply(target, caster))
                return;

            // プレースホルダー実装
            string effectType = isDebuff ? "デバフ" : "バフ";
            Debug.Log($"[StatModifierEffect] {EffectName}による{targetStat}{effectType}を{target.CharacterData.CharacterName}に適用 (実装予定)");
        }

        /// <summary>
        /// 修正後のステータス値を計算
        /// </summary>
        /// <param name="originalValue">元のステータス値</param>
        /// <param name="caster">発動者</param>
        /// <returns>修正後のステータス値</returns>
        public virtual float CalculateModifiedValue(float originalValue, Character caster)
        {
            if (caster == null)
                return originalValue;

            float modifierValue = CalculateEffectiveValue(caster);

            return modifierType switch
            {
                ModifierType.Additive => originalValue + modifierValue,
                ModifierType.Multiplicative => originalValue * modifierValue,
                ModifierType.Percentage => originalValue * (1f + modifierValue / 100f),
                ModifierType.Override => modifierValue,
                _ => originalValue
            };
        }

        /// <summary>
        /// ステータス修正効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public override string GetDetailedInfo()
        {
            var info = base.GetDetailedInfo();
            info += $"\n対象ステータス: {targetStat}";
            info += $"\n修正方式: {modifierType}";
            info += $"\n効果種類: {(isDebuff ? "デバフ" : "バフ")}";
            
            return info;
        }

        /// <summary>
        /// ステータス修正効果の文字列表現
        /// </summary>
        /// <returns>効果名とステータス情報</returns>
        public override string ToString()
        {
            string sign = isDebuff ? "-" : "+";
            return $"{EffectName} ({targetStat}{sign}{BaseValue}, {modifierType})";
        }
    }
}