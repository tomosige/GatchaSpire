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
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>適用可能かどうか</returns>
        public override bool CanApply(Character target, Character caster, BattleContext context)
        {
            if (target == null || caster == null)
                return false;

            // 対象が生存している場合のみ適用可能
            if (!target.IsAlive)
                return false;

            // 成功確率チェック
            if (SuccessChance < 1.0f)
            {
                float roll = UnityEngine.Random.value;
                if (roll > SuccessChance)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// ステータス修正効果を適用
        /// Character.AddTemporaryBoostメソッドを使用した実装
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void Apply(Character target, Character caster, BattleContext context)
        {
            if (!CanApply(target, caster, context))
            {
                Debug.Log($"[StatModifierEffect] {EffectName}の適用条件が満たされていません");
                return;
            }

            // 実際の修正値を計算
            float modifierValue = CalculateEffectiveValue(caster);
            
            // 修正タイプに応じて最終値を計算
            int finalModifier = 0;
            
            switch (modifierType)
            {
                case ModifierType.Additive:
                    finalModifier = Mathf.RoundToInt(modifierValue);
                    break;
                    
                case ModifierType.Multiplicative:
                    // 乗算の場合、現在値に対する増減として計算
                    int currentValue = target.CurrentStats.GetFinalStat(targetStat);
                    finalModifier = Mathf.RoundToInt(currentValue * (modifierValue - 1f));
                    break;
                    
                case ModifierType.Percentage:
                    // パーセンテージの場合、現在値に対する増減として計算
                    int currentValue2 = target.CurrentStats.GetFinalStat(targetStat);
                    finalModifier = Mathf.RoundToInt(currentValue2 * (modifierValue / 100f));
                    break;
                    
                case ModifierType.Override:
                    // 上書きの場合、現在値からの差分として計算
                    int currentValue3 = target.CurrentStats.GetFinalStat(targetStat);
                    finalModifier = Mathf.RoundToInt(modifierValue) - currentValue3;
                    break;
            }

            // Character.AddTemporaryBoostでステータス修正を適用
            target.AddTemporaryBoost(targetStat, finalModifier);

            // ログ出力
            string effectType = isDebuff ? "デバフ" : "バフ";
            string sign = finalModifier >= 0 ? "+" : "";
            Debug.Log($"[StatModifierEffect] {EffectName}: {target.CharacterData.CharacterName}の{targetStat}を{sign}{finalModifier}修正 ({effectType}, {modifierType})");

            // フローティングテキスト表示フラグ確認
            if (ShowFloatingText)
            {
                Debug.Log($"[StatModifierEffect] フローティングテキスト表示: {targetStat} {sign}{finalModifier}");
            }

            // 継続時間がある場合のログ
            if (Duration > 0f)
            {
                Debug.Log($"[StatModifierEffect] 継続時間: {Duration:F1}秒 (継続効果管理は将来実装予定)");
            }
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