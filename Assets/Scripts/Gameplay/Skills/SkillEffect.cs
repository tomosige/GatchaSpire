using UnityEngine;
using System;
using System.Collections.Generic;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// スキル効果の基底抽象クラス
    /// 全てのスキル効果に共通するプロパティとメソッドを定義
    /// 仕様書3.1に基づく実装
    /// </summary>
    [Serializable]
    public abstract class SkillEffect
    {
        [Header("基本プロパティ")]
        [SerializeField] private SkillEffectType effectType = SkillEffectType.Special;
        [SerializeField] private string effectName = "";
        [SerializeField] private string description = "";

        [Header("数値効果")]
        [SerializeField] private float baseValue = 0f;
        [SerializeField] private float scalingRatio = 0f;
        [SerializeField] private ScalingAttribute scalingSource = ScalingAttribute.None;

        [Header("対象・範囲")]
        [SerializeField] private SkillTargetType targetType = SkillTargetType.SingleEnemy;
        [SerializeField] private int maxTargets = 1;

        [Header("継続効果")]
        [SerializeField] private float duration = 0f;
        [SerializeField] private bool isPermanent = false;
        [SerializeField] private float tickInterval = 1f;

        [Header("確率・条件")]
        [SerializeField] private float successChance = 1.0f;

        [Header("スタック・重複")]
        [SerializeField] private bool canStack = false;
        [SerializeField] private int maxStacks = 1;
        [SerializeField] private StackBehavior stackType = StackBehavior.Refresh;

        [Header("視覚効果")]
        [SerializeField] private string effectAnimationId = "";
        [SerializeField] private Color effectColor = Color.white;
        [SerializeField] private bool showFloatingText = true;

        // 基本プロパティ
        /// <summary>
        /// 効果の種類
        /// </summary>
        public SkillEffectType EffectType
        {
            get => effectType;
            set => effectType = value;
        }

        /// <summary>
        /// 効果名
        /// </summary>
        public string EffectName
        {
            get => effectName;
            set => effectName = value ?? "";
        }

        /// <summary>
        /// 効果の説明
        /// </summary>
        public string Description
        {
            get => description;
            set => description = value ?? "";
        }

        // 数値効果プロパティ
        /// <summary>
        /// 基本効果値
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set => baseValue = value;
        }

        /// <summary>
        /// スケーリング係数
        /// </summary>
        public float ScalingRatio
        {
            get => scalingRatio;
            set => scalingRatio = Mathf.Max(0f, value);
        }

        /// <summary>
        /// スケール元ステータス
        /// </summary>
        public ScalingAttribute ScalingSource
        {
            get => scalingSource;
            set => scalingSource = value;
        }

        // 対象・範囲プロパティ
        /// <summary>
        /// 対象タイプ
        /// </summary>
        public SkillTargetType TargetType
        {
            get => targetType;
            set => targetType = value;
        }

        /// <summary>
        /// 最大対象数
        /// </summary>
        public int MaxTargets
        {
            get => maxTargets;
            set => maxTargets = Mathf.Max(1, value);
        }

        // 継続効果プロパティ
        /// <summary>
        /// 効果継続時間（秒）
        /// </summary>
        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 永続効果かどうか
        /// </summary>
        public bool IsPermanent
        {
            get => isPermanent;
            set => isPermanent = value;
        }

        /// <summary>
        /// DoT/HoTの間隔（秒）
        /// </summary>
        public float TickInterval
        {
            get => tickInterval;
            set => tickInterval = Mathf.Max(0.1f, value);
        }

        // 確率・条件プロパティ
        /// <summary>
        /// 成功確率（0.0-1.0）
        /// </summary>
        public float SuccessChance
        {
            get => successChance;
            set => successChance = Mathf.Clamp01(value);
        }

        // スタック・重複プロパティ
        /// <summary>
        /// 効果重複可能か
        /// </summary>
        public bool CanStack
        {
            get => canStack;
            set => canStack = value;
        }

        /// <summary>
        /// 最大スタック数
        /// </summary>
        public int MaxStacks
        {
            get => maxStacks;
            set => maxStacks = Mathf.Max(1, value);
        }

        /// <summary>
        /// スタック時の挙動
        /// </summary>
        public StackBehavior StackType
        {
            get => stackType;
            set => stackType = value;
        }

        // 視覚効果プロパティ
        /// <summary>
        /// エフェクトアニメーションID
        /// </summary>
        public string EffectAnimationId
        {
            get => effectAnimationId;
            set => effectAnimationId = value ?? "";
        }

        /// <summary>
        /// エフェクト色
        /// </summary>
        public Color EffectColor
        {
            get => effectColor;
            set => effectColor = value;
        }

        /// <summary>
        /// ダメージ表示等のフローティングテキストを表示するか
        /// </summary>
        public bool ShowFloatingText
        {
            get => showFloatingText;
            set => showFloatingText = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        protected SkillEffect()
        {
            effectType = SkillEffectType.Special;
            effectName = "";
            description = "";
            baseValue = 0f;
            scalingRatio = 0f;
            scalingSource = ScalingAttribute.None;
            targetType = SkillTargetType.SingleEnemy;
            maxTargets = 1;
            duration = 0f;
            isPermanent = false;
            tickInterval = 1f;
            successChance = 1.0f;
            canStack = false;
            maxStacks = 1;
            stackType = StackBehavior.Refresh;
            effectAnimationId = "";
            effectColor = Color.white;
            showFloatingText = true;
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">効果名</param>
        /// <param name="desc">説明</param>
        /// <param name="type">効果タイプ</param>
        /// <param name="value">基本効果値</param>
        protected SkillEffect(string name, string desc, SkillEffectType type, float value) : this()
        {
            effectName = name ?? "";
            description = desc ?? "";
            effectType = type;
            baseValue = value;
        }

        /// <summary>
        /// スケーリングを適用した実効果値を計算
        /// </summary>
        /// <param name="caster">効果を発動するキャラクター</param>
        /// <returns>実効果値</returns>
        public virtual float CalculateEffectiveValue(Character caster)
        {
            if (caster == null || scalingSource == ScalingAttribute.None || scalingRatio <= 0f)
            {
                return baseValue;
            }

            float scalingValue = GetScalingValue(caster, scalingSource);
            return baseValue + (scalingValue * scalingRatio);
        }

        /// <summary>
        /// 指定ステータスからスケーリング値を取得
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <param name="attribute">スケーリング属性</param>
        /// <returns>スケーリング値</returns>
        protected virtual float GetScalingValue(Character character, ScalingAttribute attribute)
        {
            if (character == null)
                return 0f;

            return attribute switch
            {
                ScalingAttribute.HP => character.CurrentStats.GetFinalStat(StatType.HP),
                ScalingAttribute.MP => character.CurrentStats.GetFinalStat(StatType.MP),
                ScalingAttribute.Attack => character.CurrentStats.GetFinalStat(StatType.Attack),
                ScalingAttribute.Defense => character.CurrentStats.GetFinalStat(StatType.Defense),
                ScalingAttribute.Speed => character.CurrentStats.GetFinalStat(StatType.Speed),
                ScalingAttribute.Magic => character.CurrentStats.GetFinalStat(StatType.Magic),
                ScalingAttribute.Resistance => character.CurrentStats.GetFinalStat(StatType.Resistance),
                ScalingAttribute.Luck => character.CurrentStats.GetFinalStat(StatType.Luck),
                ScalingAttribute.MaxHP => character.MaxHP,
                ScalingAttribute.CurrentHP => character.CurrentHP,
                ScalingAttribute.MissingHP => character.MaxHP - character.CurrentHP,
                ScalingAttribute.Level => character.CurrentLevel,
                ScalingAttribute.MaxMP => character.MaxMP,
                ScalingAttribute.CurrentMP => character.CurrentMP,
                ScalingAttribute.MissingMP => character.MaxMP - character.CurrentMP,
                ScalingAttribute.BattlePower => character.BattlePower,
                ScalingAttribute.HPPercentage => character.HPPercentage,
                ScalingAttribute.MPPercentage => character.MPPercentage,
                _ => 0f
            };
        }

        /// <summary>
        /// 効果が適用可能かどうかを判定
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        /// <returns>適用可能かどうか</returns>
        public abstract bool CanApply(Character target, Character caster);

        /// <summary>
        /// 効果を適用
        /// 各具象クラスで実装（今回は空実装でOK）
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="caster">発動者</param>
        public abstract void Apply(Character target, Character caster);

        /// <summary>
        /// 効果の詳細情報を取得
        /// </summary>
        /// <returns>効果情報の文字列</returns>
        public virtual string GetDetailedInfo()
        {
            var info = $"=== {effectName} ===\n";
            info += $"種類: {effectType}\n";
            info += $"基本値: {baseValue}\n";
            
            if (scalingSource != ScalingAttribute.None && scalingRatio > 0f)
            {
                info += $"スケーリング: {scalingSource} × {scalingRatio:F2}\n";
            }
            
            info += $"対象: {targetType}";
            if (maxTargets > 1)
            {
                info += $" (最大{maxTargets}体)";
            }
            info += "\n";
            
            if (duration > 0f)
            {
                info += $"継続時間: {duration:F1}秒\n";
            }
            else if (isPermanent)
            {
                info += "継続時間: 永続\n";
            }
            
            if (successChance < 1.0f)
            {
                info += $"成功率: {successChance * 100:F0}%\n";
            }
            
            if (canStack && maxStacks > 1)
            {
                info += $"スタック: 最大{maxStacks}回 ({stackType})\n";
            }
            
            if (!string.IsNullOrEmpty(description))
            {
                info += $"\n{description}";
            }
            
            return info;
        }

        /// <summary>
        /// 効果の文字列表現
        /// </summary>
        /// <returns>効果名と基本情報</returns>
        public override string ToString()
        {
            return $"{effectName} ({effectType}, 値:{baseValue})";
        }
    }
}