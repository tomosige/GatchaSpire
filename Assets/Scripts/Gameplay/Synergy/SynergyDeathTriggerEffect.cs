using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// 死亡時発動型シナジー効果
    /// キャラクターが死亡時に隣接味方を強化するシナジー効果
    /// TestRaceC用: 死亡時に隣接味方が永続強化される
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyDeathTriggerEffect", menuName = "GatchaSpire/Synergy/Death Trigger Effect")]
    public class SynergyDeathTriggerEffect : SynergyTriggerAbilityEffect
    {
        [Header("死亡時効果設定")]
        [SerializeField] private StatType boostStatType = StatType.Attack; // 強化ステータス
        [SerializeField] private float boostAmount = 50f; // 強化量
        [SerializeField] private StatModifierType boostType = StatModifierType.Additive; // 強化タイプ
        [SerializeField] private bool isPermanent = true; // 永続効果か
        [SerializeField] private bool onlyAdjacent = true; // 隣接キャラクターのみ対象か
        
        // 死亡イベント監視用
        private Dictionary<Character, bool> deathListeners = new Dictionary<Character, bool>();
        
        public StatType BoostStatType => boostStatType;
        public float BoostAmount => boostAmount;
        public StatModifierType BoostType => boostType;
        public bool IsPermanent => isPermanent;
        public bool OnlyAdjacent => onlyAdjacent;
        
        /// <summary>
        /// 死亡リスナーを登録
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void RegisterDeathListener(List<Character> targets, BattleContext context)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    deathListeners[target] = true;
                    
                    // 死亡イベントを戦闘システムに登録
                    if (context.BattleManager != null)
                    {
                        context.BattleManager.RegisterDeathListener(target, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// 死亡イベントが発生した時の処理（戦闘システムから呼び出される）
        /// </summary>
        /// <param name="deadCharacter">死亡したキャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        public void OnCharacterDeath(Character deadCharacter, BattleContext context)
        {
            if (deadCharacter == null || !CanTrigger(deadCharacter, context))
            {
                return;
            }
            
            // 死亡したキャラクターがこのシナジーの対象かチェック
            if (!deathListeners.ContainsKey(deadCharacter))
            {
                return;
            }
            
            Debug.Log($"[SynergyDeathTrigger] {deadCharacter.CharacterData.CharacterName}が死亡しました");
            
            // 死亡時効果を実行
            ExecuteTriggerEffect(deadCharacter, context);
        }
        
        /// <summary>
        /// 具体的な効果実装（隣接味方の強化）
        /// </summary>
        /// <param name="character">死亡したキャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        protected override void ExecuteSpecificEffect(Character character, BattleContext context)
        {
            if (character == null || context.BattleManager == null)
            {
                return;
            }
            
            // 対象キャラクターを取得
            List<Character> targets = GetTargetCharacters(character, context);
            
            if (targets.Count == 0)
            {
                Debug.Log($"[SynergyDeathTrigger] {character.CharacterData.CharacterName}の死亡時効果の対象が見つかりません");
                return;
            }
            
            // 各対象キャラクターを強化
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    ApplyBoostEffect(target, character);
                }
            }
            
            Debug.Log($"[SynergyDeathTrigger] {character.CharacterData.CharacterName}の死亡により{targets.Count}体のキャラクターが強化されました");
        }
        
        /// <summary>
        /// 対象キャラクターを取得
        /// </summary>
        /// <param name="deadCharacter">死亡したキャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>対象キャラクターリスト</returns>
        private List<Character> GetTargetCharacters(Character deadCharacter, BattleContext context)
        {
            var targets = new List<Character>();
            
            if (onlyAdjacent)
            {
                // 隣接キャラクターのみ対象
                targets = GetAdjacentAllies(deadCharacter, context);
            }
            else
            {
                // 全味方を対象
                targets = GetAllAllies(deadCharacter, context);
            }
            
            return targets;
        }
        
        /// <summary>
        /// 隣接味方を取得
        /// </summary>
        /// <param name="character">基準キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>隣接味方リスト</returns>
        private List<Character> GetAdjacentAllies(Character character, BattleContext context)
        {
            var adjacentAllies = new List<Character>();
            
            // 戦闘システムから隣接位置を取得
            if (context.BattleManager != null)
            {
                var allCharacters = context.BattleManager.GetPlayerCharacters();
                var characterPosition = context.BattleManager.GetCharacterPosition(character);
                
                foreach (var ally in allCharacters)
                {
                    if (ally != null && ally != character && !ally.IsDead)
                    {
                        var allyPosition = context.BattleManager.GetCharacterPosition(ally);
                        
                        // 隣接判定（上下左右および斜め）
                        if (IsAdjacent(characterPosition, allyPosition))
                        {
                            adjacentAllies.Add(ally);
                        }
                    }
                }
            }
            
            return adjacentAllies;
        }
        
        /// <summary>
        /// 全味方を取得
        /// </summary>
        /// <param name="character">基準キャラクター</param>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>全味方リスト</returns>
        private List<Character> GetAllAllies(Character character, BattleContext context)
        {
            var allAllies = new List<Character>();
            
            if (context.BattleManager != null)
            {
                var allCharacters = context.BattleManager.GetPlayerCharacters();
                
                foreach (var ally in allCharacters)
                {
                    if (ally != null && ally != character && !ally.IsDead)
                    {
                        allAllies.Add(ally);
                    }
                }
            }
            
            return allAllies;
        }
        
        /// <summary>
        /// 隣接判定
        /// </summary>
        /// <param name="pos1">位置1</param>
        /// <param name="pos2">位置2</param>
        /// <returns>隣接している場合true</returns>
        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            int deltaX = Mathf.Abs(pos1.x - pos2.x);
            int deltaY = Mathf.Abs(pos1.y - pos2.y);
            
            // 上下左右および斜めを隣接とみなす
            return deltaX <= 1 && deltaY <= 1 && (deltaX + deltaY) > 0;
        }
        
        /// <summary>
        /// 強化効果を適用
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        /// <param name="deadCharacter">死亡したキャラクター</param>
        private void ApplyBoostEffect(Character target, Character deadCharacter)
        {
            if (target == null)
            {
                return;
            }
            
            // 一時的効果として強化を適用
            string effectId = $"{EffectId}_{deadCharacter.CharacterData.CharacterName}";
            int boostValue = Mathf.RoundToInt(boostAmount);
            
            target.AddTemporaryEffect(effectId, boostStatType, boostValue);
            
            Debug.Log($"[SynergyDeathTrigger] {target.CharacterData.CharacterName}に{deadCharacter.CharacterData.CharacterName}の死亡による{boostStatType}強化を適用: {boostStatType} +{boostAmount}");
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
                    deathListeners.Remove(target);
                    
                    // 戦闘システムから登録を解除
                    if (context.BattleManager != null)
                    {
                        context.BattleManager.UnregisterDeathListener(target, this);
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
                   boostAmount > 0f;
        }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // 強化量の範囲チェック
            if (boostAmount < 0)
            {
                boostAmount = 0;
            }
            
            // 発動条件をOnDeathに固定
            if (Application.isPlaying && TriggerCondition != TriggerCondition.OnDeath)
            {
                Debug.LogWarning($"[SynergyDeathTrigger] {name}: 死亡時効果はOnDeath発動条件である必要があります", this);
            }
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="statType">強化ステータス</param>
        /// <param name="amount">強化量</param>
        /// <param name="permanent">永続効果か</param>
        /// <param name="adjacentOnly">隣接のみ対象か</param>
        public void SetTestData(string id, string name, StatType statType, float amount, bool permanent, bool adjacentOnly)
        {
            // 基底クラスの設定
            SetTestData(id, name, TriggerCondition.OnDeath, 0f);
            
            // 死亡時効果固有の設定
            boostStatType = statType;
            boostAmount = amount;
            boostType = StatModifierType.Additive;
            isPermanent = permanent;
            onlyAdjacent = adjacentOnly;
        }
#endif
    }
}