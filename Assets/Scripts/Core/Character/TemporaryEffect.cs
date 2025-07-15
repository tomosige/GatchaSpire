using System;
using UnityEngine;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// 一時的な効果を管理するクラス
    /// シナジー、スキル、アイテムなどによる一時的なステータス修正を個別に管理
    /// </summary>
    [Serializable]
    public class TemporaryEffect
    {
        [SerializeField] private string effectId;   // 効果識別子
        [SerializeField] private StatType statType; // 対象ステータス
        [SerializeField] private int value;         // 修正値
        
        // プロパティ
        public string EffectId => effectId;
        public StatType StatType => statType;
        public int Value => value;
        
        /// <summary>
        /// デフォルトコンストラクタ（シリアライゼーション用）
        /// </summary>
        public TemporaryEffect()
        {
            effectId = "";
            statType = StatType.Attack;
            value = 0;
        }
        
        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="effectId">効果識別子</param>
        /// <param name="statType">対象ステータス</param>
        /// <param name="value">修正値</param>
        public TemporaryEffect(string effectId, StatType statType, int value)
        {
            this.effectId = effectId ?? "";
            this.statType = statType;
            this.value = value;
        }
        
        /// <summary>
        /// 効果が有効かどうか
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(effectId) && value != 0;
        }
        
        /// <summary>
        /// 効果情報を文字列で取得
        /// </summary>
        /// <returns>効果情報文字列</returns>
        public override string ToString()
        {
            return $"{effectId}: {statType} {(value >= 0 ? "+" : "")}{value}";
        }
        
        /// <summary>
        /// 効果の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public string GetDetailedInfo()
        {
            return $"効果ID: {effectId}, ステータス: {statType}, 修正値: {(value >= 0 ? "+" : "")}{value}";
        }
        
        /// <summary>
        /// 同じ効果IDかどうかを判定
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>同じ効果IDの場合true</returns>
        public bool HasSameId(TemporaryEffect other)
        {
            return other != null && effectId == other.effectId;
        }
        
        /// <summary>
        /// 同じ効果IDかどうかを判定
        /// </summary>
        /// <param name="id">効果ID</param>
        /// <returns>同じ効果IDの場合true</returns>
        public bool HasSameId(string id)
        {
            return effectId == id;
        }
        
        /// <summary>
        /// 同じステータス種類かどうかを判定
        /// </summary>
        /// <param name="stat">ステータス種類</param>
        /// <returns>同じステータス種類の場合true</returns>
        public bool AffectsStat(StatType stat)
        {
            return statType == stat;
        }
    }
}