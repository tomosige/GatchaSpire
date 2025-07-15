using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// 攻撃時発動型シナジー効果
    /// 低HPの敵を攻撃時に即死効果を発動するシナジー効果
    /// TestRaceD用: 20%以下の敵を攻撃時に即死効果
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyAttackTriggerEffect", menuName = "GatchaSpire/Synergy/Attack Trigger Effect")]
    public class SynergyAttackTriggerEffect : SynergyTriggerAbilityEffect
    {
        [Header("攻撃時効果設定")]
        [SerializeField] private float targetHPThreshold = 0.2f; // 対象HP閾値（0.2 = 20%）
        [SerializeField] private bool isInstantKill = true; // 即死効果か
        [SerializeField] private float bonusDamage = 0f; // 追加ダメージ（即死でない場合）
        [SerializeField] private float activationRate = 1.0f; // 発動確率（1.0 = 100%）
        [SerializeField] private bool targetEnemiesOnly = true; // 敵のみ対象か
        
        // 攻撃イベント監視用
        private Dictionary<Character, bool> attackListeners = new Dictionary<Character, bool>();
        
        public float TargetHPThreshold => targetHPThreshold;
        public bool IsInstantKill => isInstantKill;
        public float BonusDamage => bonusDamage;
        public float ActivationRate => activationRate;
        public bool TargetEnemiesOnly => targetEnemiesOnly;
        
        /// <summary>
        /// 攻撃イベントリスナーを登録
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void RegisterDamageListener(List<Character> targets, BattleContext context)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    attackListeners[target] = true;
                    
                    // 攻撃イベントを戦闘システムに登録
                    if (context.BattleManager != null)
                    {
                        context.BattleManager.RegisterAttackListener(target, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// 攻撃イベントが発生した時の処理（戦闘システムから呼び出される）
        /// </summary>
        /// <param name="attacker">攻撃者</param>
        /// <param name="target">攻撃対象</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>追加効果が発動した場合true</returns>
        public bool OnAttackEvent(Character attacker, Character target, int damage, BattleContext context)
        {
            if (attacker == null || target == null || !CanTrigger(attacker, context))
            {
                return false;
            }
            
            // 攻撃者がこのシナジーの対象かチェック
            if (!attackListeners.ContainsKey(attacker))
            {
                return false;
            }
            
            // 対象が条件を満たしているかチェック
            if (!CheckTargetConditions(target))
            {
                return false;
            }
            
            // 発動確率チェック
            if (Random.value > activationRate)
            {
                return false;
            }
            
            Debug.Log($"[SynergyAttackTrigger] {attacker.CharacterData.CharacterName}が{target.CharacterData.CharacterName}への攻撃で特殊効果を発動");
            
            // 攻撃時効果を実行
            ExecuteAttackEffect(attacker, target, damage, context);
            return true;
        }
        
        /// <summary>
        /// 対象が条件を満たしているかチェック
        /// </summary>
        /// <param name="target">攻撃対象</param>
        /// <returns>条件を満たしている場合true</returns>
        private bool CheckTargetConditions(Character target)
        {
            if (target == null || target.IsDead)
            {
                return false;
            }
            
            // 敵のみ対象かチェック（現在の実装では簡略化）
            if (targetEnemiesOnly)
            {
                // 実際の実装では、戦闘システムから敵味方判定を行う
                // 現在は全てのキャラクターを対象とする
            }
            
            // HP閾値チェック
            float currentHPRatio = target.CurrentHP / (float)target.MaxHP;
            return currentHPRatio <= targetHPThreshold;
        }
        
        /// <summary>
        /// 攻撃時効果を実行
        /// </summary>
        /// <param name="attacker">攻撃者</param>
        /// <param name="target">攻撃対象</param>
        /// <param name="originalDamage">元のダメージ</param>
        /// <param name="context">戦闘コンテキスト</param>
        private void ExecuteAttackEffect(Character attacker, Character target, int originalDamage, BattleContext context)
        {
            // 発動回数を記録
            ExecuteTriggerEffect(attacker, context);
            
            if (isInstantKill)
            {
                // 即死効果
                int killDamage = target.CurrentHP;
                target.TakeDamage(killDamage);
                Debug.Log($"[SynergyAttackTrigger] {attacker.CharacterData.CharacterName}の攻撃で{target.CharacterData.CharacterName}が即死しました");
            }
            else
            {
                // 追加ダメージ
                int additionalDamage = Mathf.RoundToInt(bonusDamage);
                target.TakeDamage(additionalDamage);
                Debug.Log($"[SynergyAttackTrigger] {attacker.CharacterData.CharacterName}の攻撃で{target.CharacterData.CharacterName}に追加ダメージ{additionalDamage}");
            }
        }
        
        /// <summary>
        /// 具体的な効果実装
        /// </summary>
        /// <param name="character">発動キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void ExecuteSpecificEffect(Character character, BattleContext context)
        {
            // 攻撃時効果は OnAttackEvent で実装されるため、ここでは何もしない
            // 発動回数のカウントのみ行う
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
                    attackListeners.Remove(target);
                    
                    // 戦闘システムから登録を解除
                    if (context.BattleManager != null)
                    {
                        context.BattleManager.UnregisterAttackListener(target, this);
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
                   targetHPThreshold > 0f && targetHPThreshold <= 1f &&
                   activationRate > 0f && activationRate <= 1f &&
                   (isInstantKill || bonusDamage > 0f);
        }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // HP閾値の範囲チェック
            targetHPThreshold = Mathf.Clamp01(targetHPThreshold);
            
            // 発動確率の範囲チェック
            activationRate = Mathf.Clamp01(activationRate);
            
            // 追加ダメージの範囲チェック
            if (bonusDamage < 0)
            {
                bonusDamage = 0;
            }
            
            // 発動条件をOnDamageに固定（攻撃時として使用）
            if (Application.isPlaying && TriggerCondition != TriggerCondition.OnDamage)
            {
                Debug.LogWarning($"[SynergyAttackTrigger] {name}: 攻撃時効果はOnDamage発動条件である必要があります", this);
            }
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="hpThreshold">対象HP閾値</param>
        /// <param name="instantKill">即死効果か</param>
        /// <param name="rate">発動確率</param>
        public void SetTestData(string id, string name, float hpThreshold, bool instantKill, float rate)
        {
            // 基底クラスの設定
            SetTestData(id, name, TriggerCondition.OnDamage, hpThreshold);
            
            // 攻撃時効果固有の設定
            targetHPThreshold = hpThreshold;
            isInstantKill = instantKill;
            bonusDamage = instantKill ? 0f : 100f;
            activationRate = rate;
            targetEnemiesOnly = true;
        }
#endif
    }
}