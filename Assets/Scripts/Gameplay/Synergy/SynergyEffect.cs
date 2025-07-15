using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;
using GatchaSpire.Gameplay.Skills;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジー効果の基底クラス
    /// 具体的な効果実装のベースとなる抽象クラス
    /// </summary>
    public abstract class SynergyEffect : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private string effectId = ""; // 効果識別子（空文字列=未設定）
        [SerializeField] private string effectName = ""; // 効果名
        [SerializeField] private string effectDescription = ""; // 効果説明
        
        [Header("適用設定")]
        [SerializeField] private SynergyTarget targetType = SynergyTarget.SynergyOwners; // 適用対象
        [SerializeField] private SynergyEffectType effectType = SynergyEffectType.StatModifier; // 効果種類
        
        // プロパティ
        public string EffectId => effectId;
        public string EffectName => effectName;
        public string EffectDescription => effectDescription;
        public SynergyTarget TargetType => targetType;
        public SynergyEffectType EffectType => effectType;
        
        /// <summary>
        /// 効果を適用
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public abstract void ApplyEffect(List<Character> targets, BattleContext context);
        
        /// <summary>
        /// 効果を除去
        /// </summary>
        /// <param name="targets">対象キャラクターリスト</param>
        /// <param name="context">戦闘コンテキスト</param>
        public abstract void RemoveEffect(List<Character> targets, BattleContext context);
        
        /// <summary>
        /// 効果適用可能かどうか判定
        /// </summary>
        /// <param name="context">戦闘コンテキスト</param>
        /// <returns>適用可能な場合true</returns>
        public abstract bool CanApply(BattleContext context);
        
        /// <summary>
        /// 効果が有効かどうか
        /// </summary>
        /// <returns>有効な場合true</returns>
        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(effectId) && !string.IsNullOrEmpty(effectName);
        }
        
        /// <summary>
        /// 効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public virtual string GetDetailedInfo()
        {
            var info = $"=== {effectName} ===\n";
            info += $"ID: {effectId}\n";
            info += $"種類: {effectType}\n";
            info += $"対象: {targetType}\n";
            info += $"説明: {effectDescription}\n";
            return info;
        }
        
        /// <summary>
        /// 効果情報を文字列で取得
        /// </summary>
        /// <returns>効果情報文字列</returns>
        public override string ToString()
        {
            return $"{effectName} ({effectType}, {targetType})";
        }
        
        /// <summary>
        /// Unity OnValidate
        /// </summary>
        protected virtual void OnValidate()
        {
            // ID文字列の正規化
            if (!string.IsNullOrEmpty(effectId))
            {
                effectId = effectId.Trim().ToLower();
            }
            
            // 基本バリデーション
            if (string.IsNullOrEmpty(effectId) && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyEffect] {name}: 効果IDが設定されていません", this);
            }
            
            if (string.IsNullOrEmpty(effectName) && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyEffect] {name}: 効果名が設定されていません", this);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド（基底クラス）
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <param name="name">効果名</param>
        /// <param name="target">適用対象</param>
        /// <param name="type">効果種類</param>
        public void SetTestDataBase(string id, string name, SynergyTarget target, SynergyEffectType type)
        {
            effectId = id;
            effectName = name;
            targetType = target;
            effectType = type;
        }
#endif
    }
}