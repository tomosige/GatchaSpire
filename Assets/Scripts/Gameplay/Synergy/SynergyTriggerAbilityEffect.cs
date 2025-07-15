using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;
using System;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// 発動能力型シナジー効果の抽象クラス
    /// 特定条件で動的に発動するシナジー効果の基底クラス
    /// </summary>
    public abstract class SynergyTriggerAbilityEffect : SynergyEffect
    {
        [Header("発動条件設定")]
        [SerializeField] private TriggerCondition triggerCondition = TriggerCondition.BattleStart;
        [SerializeField] private float triggerValue = 0f; // 発動に必要な値（HP％、時間など）
        [SerializeField] private int maxTriggerCount = 1; // 最大発動回数（-1=無制限）
        
        [Header("効果設定")]
        [SerializeField] private float effectDuration = 0f; // 効果持続時間（0=即座、-1=永続）
        [SerializeField] private bool isOneTimeOnly = true; // 一度だけの発動か
        
        // 発動回数管理
        private Dictionary<Character, int> triggerCounts = new Dictionary<Character, int>();
        private HashSet<Character> triggeredCharacters = new HashSet<Character>();
        
        // プロパティ
        public TriggerCondition TriggerCondition => triggerCondition;
        public float TriggerValue => triggerValue;
        public int MaxTriggerCount => maxTriggerCount;
        public float EffectDuration => effectDuration;
        public bool IsOneTimeOnly => isOneTimeOnly;
        
        /// <summary>
        /// 基本的な効果適用（SynergyEffectの実装）
        /// 発動能力型は戦闘開始時に発動条件を登録
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void ApplyEffect(List<Character> targets, BattleContext context)
        {
            if (targets == null || !CanApply(context))
            {
                return;
            }
            
            // 発動条件を戦闘システムに登録
            RegisterTriggerCondition(targets, context);
            
            // 戦闘開始時発動の場合は即座に実行
            if (triggerCondition == TriggerCondition.BattleStart)
            {
                foreach (var target in targets)
                {
                    if (target != null && CanTrigger(target, context))
                    {
                        ExecuteTriggerEffect(target, context);
                    }
                }
            }
        }
        
        /// <summary>
        /// 効果を除去（SynergyEffectの実装）
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void RemoveEffect(List<Character> targets, BattleContext context)
        {
            if (targets == null)
            {
                return;
            }
            
            // 発動条件の登録を解除
            UnregisterTriggerCondition(targets, context);
            
            // 発動状態をリセット
            foreach (var target in targets)
            {
                if (target != null)
                {
                    triggeredCharacters.Remove(target);
                    triggerCounts.Remove(target);
                }
            }
        }
        
        /// <summary>
        /// 発動条件を戦闘システムに登録
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected virtual void RegisterTriggerCondition(List<Character> targets, BattleContext context)
        {
            // 各発動条件に応じてイベントリスナーを登録
            switch (triggerCondition)
            {
                case TriggerCondition.OnDamage:
                    RegisterDamageListener(targets, context);
                    break;
                case TriggerCondition.OnHeal:
                    RegisterHealListener(targets, context);
                    break;
                case TriggerCondition.OnDeath:
                    RegisterDeathListener(targets, context);
                    break;
                case TriggerCondition.OnKill:
                    RegisterKillListener(targets, context);
                    break;
                case TriggerCondition.OnCritical:
                    RegisterCriticalListener(targets, context);
                    break;
                case TriggerCondition.OnSkillUse:
                    RegisterSkillUseListener(targets, context);
                    break;
                case TriggerCondition.TimeElapsed:
                    RegisterTimeElapsedListener(targets, context);
                    break;
                default:
                    Debug.LogWarning($"[SynergyTriggerAbility] 未対応の発動条件: {triggerCondition}");
                    break;
            }
        }
        
        /// <summary>
        /// 発動条件の登録を解除
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected virtual void UnregisterTriggerCondition(List<Character> targets, BattleContext context)
        {
            // 具象クラスでイベントリスナーの解除を実装
        }
        
        /// <summary>
        /// 発動条件をチェック
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>発動可能な場合true</returns>
        protected virtual bool CanTrigger(Character character, BattleContext context)
        {
            if (character == null || !context.IsValid())
            {
                return false;
            }
            
            // 一度だけの発動で既に発動済みの場合
            if (isOneTimeOnly && triggeredCharacters.Contains(character))
            {
                return false;
            }
            
            // 最大発動回数をチェック
            if (maxTriggerCount > 0)
            {
                int currentCount = triggerCounts.GetValueOrDefault(character, 0);
                if (currentCount >= maxTriggerCount)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 発動効果を実行
        /// </summary>
        /// <param name="character">発動キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        public virtual void ExecuteTriggerEffect(Character character, BattleContext context)
        {
            if (!CanTrigger(character, context))
            {
                return;
            }
            
            // 発動回数を記録
            if (isOneTimeOnly)
            {
                triggeredCharacters.Add(character);
            }
            
            if (maxTriggerCount > 0)
            {
                triggerCounts[character] = triggerCounts.GetValueOrDefault(character, 0) + 1;
            }
            
            // 具象クラスで実際の効果を実装
            ExecuteSpecificEffect(character, context);
            
            // ログ出力
            Debug.Log($"[SynergyTriggerAbility] {character.CharacterData.CharacterName}で{EffectName}が発動");
        }
        
        /// <summary>
        /// 具体的な効果実装（具象クラスでオーバーライド）
        /// </summary>
        /// <param name="character">発動キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected abstract void ExecuteSpecificEffect(Character character, BattleContext context);
        
        /// <summary>
        /// 効果適用可能かどうか判定
        /// </summary>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>適用可能な場合true</returns>
        public override bool CanApply(BattleContext context)
        {
            return IsValid() && context != null && context.IsValid();
        }
        
        /// <summary>
        /// 効果の有効性チェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            return base.IsValid() && 
                   triggerCondition != 0 && 
                   maxTriggerCount != 0;
        }
        
        // 各種イベントリスナー登録メソッド（具象クラスでオーバーライド）
        protected virtual void RegisterDamageListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterHealListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterDeathListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterKillListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterCriticalListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterSkillUseListener(List<Character> targets, BattleContext context) { }
        protected virtual void RegisterTimeElapsedListener(List<Character> targets, BattleContext context) { }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // 発動条件のバリデーション
            if (triggerCondition == 0 && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyTriggerAbility] {name}: 発動条件が設定されていません", this);
            }
            
            // 発動回数のバリデーション
            if (maxTriggerCount == 0)
            {
                Debug.LogWarning($"[SynergyTriggerAbility] {name}: 最大発動回数が0です", this);
            }
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="condition">発動条件</param>
        /// <param name="value">発動値</param>
        public void SetTestData(string id, string name, TriggerCondition condition, float value)
        {
            // 基底クラスの設定
            SetTestDataBase(id, name, SynergyTarget.SynergyOwners, SynergyEffectType.TriggerAbility);
            
            // 発動能力型固有の設定
            triggerCondition = condition;
            triggerValue = value;
            maxTriggerCount = 1;
            isOneTimeOnly = true;
            effectDuration = 0f;
        }
#endif
    }
}