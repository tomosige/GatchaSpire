using UnityEngine;
using System;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// ダメージ効果クラス
    /// 物理・魔法・属性ダメージとクリティカル処理を管理
    /// 仕様書3.2のDamageEffectに基づく実装
    /// </summary>
    [Serializable]
    public class DamageEffect : SkillEffect
    {
        [Header("ダメージ設定")]
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool ignoreDefense = false;
        [SerializeField] private float criticalChance = 0f;
        [SerializeField] private float criticalMultiplier = 2.0f;

        /// <summary>
        /// ダメージの種類（物理/魔法/属性）
        /// </summary>
        public DamageType DamageType
        {
            get => damageType;
            set => damageType = value;
        }

        /// <summary>
        /// 防御無視フラグ
        /// </summary>
        public bool IgnoreDefense
        {
            get => ignoreDefense;
            set => ignoreDefense = value;
        }

        /// <summary>
        /// クリティカル確率（0.0-1.0）
        /// </summary>
        public float CriticalChance
        {
            get => criticalChance;
            set => criticalChance = Mathf.Clamp01(value);
        }

        /// <summary>
        /// クリティカル倍率
        /// </summary>
        public float CriticalMultiplier
        {
            get => criticalMultiplier;
            set => criticalMultiplier = Mathf.Max(1.0f, value);
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public DamageEffect() : base()
        {
            EffectType = SkillEffectType.Damage;
            TargetType = SkillTargetType.SingleEnemy;
            damageType = DamageType.Physical;
            ignoreDefense = false;
            criticalChance = 0f;
            criticalMultiplier = 2.0f;
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">効果名</param>
        /// <param name="desc">説明</param>
        /// <param name="baseDamage">基本ダメージ</param>
        /// <param name="type">ダメージタイプ</param>
        public DamageEffect(string name, string desc, float baseDamage, DamageType type = DamageType.Physical)
            : base(name, desc, SkillEffectType.Damage, baseDamage)
        {
            TargetType = SkillTargetType.SingleEnemy;
            damageType = type;
            ignoreDefense = false;
            criticalChance = 0f;
            criticalMultiplier = 2.0f;
        }

        /// <summary>
        /// ダメージ効果が適用可能かどうかを判定
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
        /// ダメージ効果を適用
        /// 今回はプロパティテスト用なので空実装
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        public override void Apply(Character target, Character caster)
        {
            // 実際のダメージ適用処理は将来のPhaseで実装
            // 今回はTestSkillEffectPropertiesのためのクラス存在確認が目的
            
            if (!CanApply(target, caster))
                return;

            // プレースホルダー実装
            Debug.Log($"[DamageEffect] {EffectName}によるダメージ効果を{target.CharacterData.CharacterName}に適用 (実装予定)");
        }

        /// <summary>
        /// 最終ダメージ値を計算（クリティカル、防御考慮）
        /// </summary>
        /// <param name="caster">発動者</param>
        /// <param name="target">対象</param>
        /// <returns>最終ダメージ値</returns>
        public virtual float CalculateFinalDamage(Character caster, Character target)
        {
            if (caster == null || target == null)
                return 0f;

            float baseDamage = CalculateEffectiveValue(caster);
            float finalDamage = baseDamage;

            // クリティカル判定（簡易実装）
            if (criticalChance > 0f && UnityEngine.Random.value <= criticalChance)
            {
                finalDamage *= criticalMultiplier;
            }

            // 防御力適用（簡易実装）
            if (!ignoreDefense && damageType == DamageType.Physical)
            {
                float defense = target.CurrentStats.GetFinalStat(StatType.Defense);
                finalDamage = Mathf.Max(1f, finalDamage - defense * 0.5f);
            }
            else if (!ignoreDefense && damageType == DamageType.Magical)
            {
                float resistance = target.CurrentStats.GetFinalStat(StatType.Resistance);
                finalDamage = Mathf.Max(1f, finalDamage - resistance * 0.5f);
            }

            return Mathf.Max(0f, finalDamage);
        }

        /// <summary>
        /// ダメージ効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public override string GetDetailedInfo()
        {
            var info = base.GetDetailedInfo();
            info += $"\nダメージタイプ: {damageType}";
            
            if (ignoreDefense)
            {
                info += "\n防御無視: あり";
            }
            
            if (criticalChance > 0f)
            {
                info += $"\nクリティカル: {criticalChance * 100:F0}% (×{criticalMultiplier:F1})";
            }
            
            return info;
        }

        /// <summary>
        /// ダメージ効果の文字列表現
        /// </summary>
        /// <returns>効果名とダメージ情報</returns>
        public override string ToString()
        {
            return $"{EffectName} ({damageType}ダメージ, 値:{BaseValue})";
        }
    }
}