using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// ステータス修正型シナジー効果
    /// 戦闘開始時にキャラクターのステータスを修正する（戦闘中永続）
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyStatModifierEffect", menuName = "GatchaSpire/Synergy/Stat Modifier Effect")]
    public class SynergyStatModifierEffect : SynergyEffect
    {
        [Header("ステータス修正")]
        [SerializeField] private StatType statType = StatType.Attack; // 修正するステータス
        [SerializeField] private StatModifierType modifierType = StatModifierType.Additive; // 固定値/割合
        [SerializeField] private float modifierValue = 0f; // 修正値（0=未設定）
        
        // プロパティ
        public StatType StatType => statType;
        public StatModifierType ModifierType => modifierType;
        public float ModifierValue => modifierValue;
        
        /// <summary>
        /// 効果を適用
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void ApplyEffect(List<Character> targets, BattleContext context)
        {
            if (targets == null || !CanApply(context))
            {
                return;
            }
            
            foreach (var target in targets)
            {
                if (target == null) continue;
                
                ApplyStatModification(target);
            }
        }
        
        /// <summary>
        /// 効果を除去
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public override void RemoveEffect(List<Character> targets, BattleContext context)
        {
            // ステータス修正型は戦闘中永続だが、シナジー構成変更時には除去が必要
            if (targets == null)
            {
                return;
            }
            
            foreach (var target in targets)
            {
                if (target == null) continue;
                
                int removedCount = target.RemoveTemporaryEffect(EffectId);
                if (removedCount > 0)
                {
                    Debug.Log($"[SynergyStatModifier] {target.CharacterData.CharacterName}から{EffectName}を除去");
                }
            }
        }
        
        /// <summary>
        /// 効果適用可能かどうか判定
        /// </summary>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>適用可能な場合true</returns>
        public override bool CanApply(BattleContext context)
        {
            return IsValid() && context != null && context.IsValid() && modifierValue != 0f; // 修正値が0でない
        }
        
        /// <summary>
        /// 効果が有効かどうか
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            return base.IsValid() && modifierValue != 0f; // 基底クラス条件 + 修正値が0でない
        }
        
        /// <summary>
        /// ステータス修正を適用
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        private void ApplyStatModification(Character character)
        {
            int finalValue = CalculateFinalModifierValue(character);
            
            // シナジー効果IDを使用して一時的効果を適用
            character.AddTemporaryEffect(EffectId, statType, finalValue);
            
            Debug.Log($"[SynergyStatModifier] {character.CharacterData.CharacterName}に{EffectName}を適用: {statType} {(modifierType == StatModifierType.Additive ? "+" : "x")}{finalValue}");
        }
        
        /// <summary>
        /// 最終修正値を計算
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <returns>最終修正値</returns>
        private int CalculateFinalModifierValue(Character character)
        {
            if (modifierType == StatModifierType.Additive)
            {
                return Mathf.RoundToInt(modifierValue); // 固定値を整数に丸める
            }
            else // Multiplicative
            {
                // 割合計算：現在値 * 修正率
                int currentValue = character.CurrentStats.GetFinalStat(statType);
                float calculatedValue = currentValue * (modifierValue / 100f); // modifierValueは%で指定
                return Mathf.RoundToInt(calculatedValue);
            }
        }
        
        /// <summary>
        /// 効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public override string GetDetailedInfo()
        {
            var info = base.GetDetailedInfo();
            info += $"ステータス: {statType}\n";
            info += $"修正タイプ: {modifierType}\n";
            info += $"修正値: {modifierValue}\n";
            info += $"効果: 戦闘中永続\n";
            return info;
        }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // 修正値の妥当性チェック
            if (modifierValue == 0f && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyStatModifierEffect] {name}: 修正値が0に設定されています", this);
            }
            
            // 割合修正の場合の値範囲チェック
            if (modifierType == StatModifierType.Multiplicative && (modifierValue < -100f || modifierValue > 1000f) && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyStatModifierEffect] {name}: 割合修正値が異常な範囲です({modifierValue}%)", this);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="stat">ステータス種類</param>
        /// <param name="value">修正値</param>
        public void SetTestData(string id, string name, StatType stat, float value)
        {
            // 基底クラスの設定メソッドを使用
            SetTestDataBase(id, name, SynergyTarget.SynergyOwners, SynergyEffectType.StatModifier);
            
            // 自身のフィールドを設定
            statType = stat;
            modifierValue = value;
            modifierType = StatModifierType.Additive;
        }
#endif
    }
}