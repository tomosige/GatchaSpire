using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// HP条件発動型シナジー効果
    /// HPが一定以下になった時に発動するシナジー効果
    /// TestRaceB用: HPが50%以下になった時に全回復
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyHPConditionEffect", menuName = "GatchaSpire/Synergy/HP Condition Effect")]
    public class SynergyHPConditionEffect : SynergyTriggerAbilityEffect
    {
        [Header("HP条件設定")]
        [SerializeField] private float hpThreshold = 0.5f; // HP閾値（0.5 = 50%）
        [SerializeField] private bool healToFull = true; // 全回復するか
        [SerializeField] private int healAmount = 100; // 固定回復量（healToFullがfalseの場合）
        [SerializeField] private bool restoreMP = false; // MPも回復するか
        
        // HP監視用
        private Dictionary<Character, float> lastHPValues = new Dictionary<Character, float>();
        
        public float HPThreshold => hpThreshold;
        public bool HealToFull => healToFull;
        public int HealAmount => healAmount;
        public bool RestoreMP => restoreMP;
        
        /// <summary>
        /// ダメージリスナーを登録（HP変化を監視）
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void RegisterDamageListener(List<Character> targets, BattleContext context)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    // 初期HP値を記録
                    lastHPValues[target] = target.CurrentHP;
                    
                    // HPが変化した時の処理をCharacterに登録
                    // 実際の実装では、Characterクラスに適切なイベントシステムが必要
                    RegisterHPChangeListener(target, context);
                }
            }
        }
        
        /// <summary>
        /// HP変化リスナーを登録
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        private void RegisterHPChangeListener(Character character, BattleContext context)
        {
            // 現在の実装では、戦闘システムから定期的にチェックする方式を採用
            // 将来的には、CharacterクラスにOnHPChangedイベントを追加することを推奨
            
            // BattleContextに定期チェック機能を追加する想定
            if (context.BattleManager != null)
            {
                // 戦闘システムに定期チェック処理を登録
                context.BattleManager.RegisterHPConditionCheck(character, this);
            }
        }
        
        /// <summary>
        /// HP条件をチェック（戦闘システムから呼び出される）
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>発動条件を満たしている場合true</returns>
        public bool CheckHPCondition(Character character, BattleContext context)
        {
            if (character == null || !CanTrigger(character, context))
            {
                return false;
            }
            
            // 現在のHP割合を計算
            float currentHPRatio = character.CurrentHP / (float)character.MaxHP;
            
            // 閾値以下になった場合に発動
            if (currentHPRatio <= hpThreshold)
            {
                float lastHPRatio = lastHPValues.GetValueOrDefault(character, 1.0f) / (float)character.MaxHP;
                
                // 前回のチェック時より下がった場合のみ発動
                if (lastHPRatio > hpThreshold)
                {
                    Debug.Log($"[SynergyHPCondition] {character.CharacterData.CharacterName}のHPが{hpThreshold * 100}%以下になりました");
                    return true;
                }
            }
            
            // HP値を更新
            lastHPValues[character] = character.CurrentHP;
            return false;
        }
        
        /// <summary>
        /// 具体的な効果実装（全回復）
        /// </summary>
        /// <param name="character">発動キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void ExecuteSpecificEffect(Character character, BattleContext context)
        {
            if (character == null)
            {
                return;
            }
            
            int healedAmount = 0;
            
            if (healToFull)
            {
                // 全回復
                healedAmount = character.MaxHP - character.CurrentHP;
                character.Heal(healedAmount);
                Debug.Log($"[SynergyHPCondition] {character.CharacterData.CharacterName}が全回復しました ({healedAmount}回復)");
            }
            else
            {
                // 固定量回復
                healedAmount = Mathf.Min(healAmount, character.MaxHP - character.CurrentHP);
                character.Heal(healedAmount);
                Debug.Log($"[SynergyHPCondition] {character.CharacterData.CharacterName}が{healedAmount}回復しました");
            }
            
            // MP回復
            if (restoreMP)
            {
                int mpRestored = character.MaxMP - character.CurrentMP;
                character.RecoverMP(mpRestored);
                Debug.Log($"[SynergyHPCondition] {character.CharacterData.CharacterName}のMPが{mpRestored}回復しました");
            }
            
            // HP値を更新
            lastHPValues[character] = character.CurrentHP;
        }
        
        /// <summary>
        /// 発動条件の登録を解除
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void UnregisterTriggerCondition(List<Character> targets, BattleContext context)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    lastHPValues.Remove(target);
                    
                    // 戦闘システムから登録を解除
                    if (context.BattleManager != null)
                    {
                        context.BattleManager.UnregisterHPConditionCheck(target, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// 効果の有効性チェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            return base.IsValid() && 
                   hpThreshold > 0f && hpThreshold <= 1f &&
                   (healToFull || healAmount > 0);
        }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // HP閾値の範囲チェック
            hpThreshold = Mathf.Clamp01(hpThreshold);
            
            // 回復量の範囲チェック
            if (healAmount < 0)
            {
                healAmount = 0;
            }
            
            // 発動条件をOnDamageに固定
            if (Application.isPlaying && TriggerCondition != TriggerCondition.OnDamage)
            {
                Debug.LogWarning($"[SynergyHPCondition] {name}: HP条件効果はOnDamage発動条件である必要があります", this);
            }
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="threshold">HP閾値</param>
        /// <param name="fullHeal">全回復するか</param>
        public void SetTestData(string id, string name, float threshold, bool fullHeal)
        {
            // 基底クラスの設定
            SetTestData(id, name, TriggerCondition.OnDamage, threshold);
            
            // HP条件効果固有の設定
            hpThreshold = threshold;
            healToFull = fullHeal;
            healAmount = fullHeal ? 0 : 100;
            restoreMP = false;
        }
#endif
    }
}