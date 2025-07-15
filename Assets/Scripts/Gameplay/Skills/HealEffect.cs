using UnityEngine;
using System;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// 回復効果クラス
    /// HP/MP回復と最大値超過回復を管理
    /// 仕様書3.2のHealEffectに基づく実装
    /// </summary>
    [Serializable]
    public class HealEffect : SkillEffect
    {
        [Header("回復設定")]
        [SerializeField] private bool healMP = false;
        [SerializeField] private bool canOverheal = false;
        [SerializeField] private float overhealDecayRate = 0.1f;

        /// <summary>
        /// MP回復かどうか（falseの場合はHP回復）
        /// </summary>
        public bool HealMP
        {
            get => healMP;
            set => healMP = value;
        }

        /// <summary>
        /// 最大HP/MP超過回復が可能か
        /// </summary>
        public bool CanOverheal
        {
            get => canOverheal;
            set => canOverheal = value;
        }

        /// <summary>
        /// 超過HP/MPの減衰率（1秒あたりの減少割合）
        /// </summary>
        public float OverhealDecayRate
        {
            get => overhealDecayRate;
            set => overhealDecayRate = Mathf.Clamp01(value);
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public HealEffect() : base()
        {
            EffectType = SkillEffectType.Heal;
            TargetType = SkillTargetType.SingleAlly;
            healMP = false;
            canOverheal = false;
            overhealDecayRate = 0.1f;
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">効果名</param>
        /// <param name="desc">説明</param>
        /// <param name="baseHeal">基本回復量</param>
        /// <param name="targetMP">MP回復かどうか</param>
        public HealEffect(string name, string desc, float baseHeal, bool targetMP = false)
            : base(name, desc, SkillEffectType.Heal, baseHeal)
        {
            TargetType = SkillTargetType.SingleAlly;
            healMP = targetMP;
            canOverheal = false;
            overhealDecayRate = 0.1f;
        }

        /// <summary>
        /// 回復効果が適用可能かどうかを判定
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

            // 既に最大値の場合、オーバーヒール可能かチェック
            if (healMP)
            {
                return canOverheal || target.CurrentMP < target.MaxMP;
            }
            else
            {
                return canOverheal || target.CurrentHP < target.MaxHP;
            }
        }

        /// <summary>
        /// 回復効果を適用
        /// Character.HealまたはRecoverMPメソッドを使用した実装
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void Apply(Character target, Character caster, BattleContext context)
        {
            if (!CanApply(target, caster, context))
            {
                Debug.Log($"[HealEffect] {EffectName}の適用条件が満たされていません");
                return;
            }

            // 実際の回復量を計算
            float actualHealAmount = CalculateActualHeal(caster, target);
            int healInt = Mathf.RoundToInt(actualHealAmount);

            int recoveredAmount = 0;
            string healType = "";

            if (healMP)
            {
                // MP回復処理
                recoveredAmount = target.RecoverMP(healInt);
                healType = "MP";
            }
            else
            {
                // HP回復処理
                recoveredAmount = target.Heal(healInt);
                healType = "HP";
            }

            // ログ出力
            Debug.Log($"[HealEffect] {EffectName}: {target.CharacterData.CharacterName}の{healType}を{recoveredAmount}回復");

            // フローティングテキスト表示フラグ確認
            if (ShowFloatingText)
            {
                Debug.Log($"[HealEffect] フローティングテキスト表示: +{recoveredAmount} {healType}");
            }

            // オーバーヒール処理（将来実装予定）
            if (canOverheal && healInt > recoveredAmount)
            {
                int overheal = healInt - recoveredAmount;
                Debug.Log($"[HealEffect] オーバーヒール発生: +{overheal} {healType} (将来実装予定)");
            }
        }

        /// <summary>
        /// 実際の回復量を計算（上限考慮）
        /// </summary>
        /// <param name="caster">発動者</param>
        /// <param name="target">対象</param>
        /// <returns>実際の回復量</returns>
        public virtual float CalculateActualHeal(Character caster, Character target)
        {
            if (caster == null || target == null)
                return 0f;

            float baseHeal = CalculateEffectiveValue(caster);
            
            if (canOverheal)
            {
                // オーバーヒール可能な場合は制限なし
                return baseHeal;
            }
            else
            {
                // 通常回復の場合は最大値で制限
                if (healMP)
                {
                    return Mathf.Min(baseHeal, target.MaxMP - target.CurrentMP);
                }
                else
                {
                    return Mathf.Min(baseHeal, target.MaxHP - target.CurrentHP);
                }
            }
        }

        /// <summary>
        /// 回復効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public override string GetDetailedInfo()
        {
            var info = base.GetDetailedInfo();
            info += $"\n回復対象: {(healMP ? "MP" : "HP")}";
            
            if (canOverheal)
            {
                info += $"\nオーバーヒール: あり (減衰率:{overhealDecayRate * 100:F0}%/秒)";
            }
            
            return info;
        }

        /// <summary>
        /// 回復効果の文字列表現
        /// </summary>
        /// <returns>効果名と回復情報</returns>
        public override string ToString()
        {
            string healType = healMP ? "MP" : "HP";
            return $"{EffectName} ({healType}回復, 値:{BaseValue})";
        }
    }
}