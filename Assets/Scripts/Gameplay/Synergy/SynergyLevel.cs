using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジーの効果レベル
    /// 特定の必要数で発動する効果の定義
    /// </summary>
    [System.Serializable]
    public class SynergyLevel
    {
        [Header("発動条件")]
        [SerializeField] private int requiredCount = 0; // シナジー発動に必要なキャラクター数（0=未設定）
        
        [Header("効果リスト")]
        [SerializeField] private List<SynergyEffect> effects = new List<SynergyEffect>();
        
        [Header("表示設定")]
        [SerializeField] private string levelDescription = "";
        [SerializeField] private Color levelColor = Color.white;
        
        // プロパティ
        public int RequiredCount => requiredCount;
        public List<SynergyEffect> Effects => effects;
        public string LevelDescription => levelDescription;
        public Color LevelColor => levelColor;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SynergyLevel()
        {
            // Unity Inspector用のデフォルトコンストラクタ
        }
        
        /// <summary>
        /// コンストラクタ（パラメータ指定）
        /// </summary>
        /// <param name="requiredCount">必要キャラクター数</param>
        /// <param name="levelDescription">レベル説明</param>
        public SynergyLevel(int requiredCount, string levelDescription)
        {
            this.requiredCount = requiredCount;
            this.levelDescription = levelDescription;
            this.effects = new List<SynergyEffect>();
            this.levelColor = Color.white;
        }
        
        /// <summary>
        /// 効果を追加
        /// </summary>
        /// <param name="effect">追加する効果</param>
        public void AddEffect(SynergyEffect effect)
        {
            if (effect != null && !effects.Contains(effect))
            {
                effects.Add(effect);
            }
        }
        
        /// <summary>
        /// 効果を削除
        /// </summary>
        /// <param name="effect">削除する効果</param>
        public void RemoveEffect(SynergyEffect effect)
        {
            if (effect != null)
            {
                effects.Remove(effect);
            }
        }
        
        /// <summary>
        /// 指定タイプの効果を取得
        /// </summary>
        /// <param name="effectType">効果タイプ</param>
        /// <returns>該当する効果のリスト</returns>
        public List<SynergyEffect> GetEffectsByType(SynergyEffectType effectType)
        {
            var result = new List<SynergyEffect>();
            
            foreach (var effect in effects)
            {
                if (effect != null && effect.EffectType == effectType)
                {
                    result.Add(effect);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// このレベルが有効かどうか
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            return requiredCount > 0 && effects != null; // 必要数1以上かつ効果リストが存在
        }
        
        /// <summary>
        /// レベル情報を文字列で取得
        /// </summary>
        /// <returns>レベル情報文字列</returns>
        public override string ToString()
        {
            return $"必要数{requiredCount}体: {levelDescription} (効果数: {effects?.Count ?? 0})";
        }
        
        /// <summary>
        /// レベルの詳細情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public string GetDetailedInfo()
        {
            var info = $"=== レベル {requiredCount} ===\n";
            info += $"説明: {levelDescription}\n";
            info += $"効果数: {effects?.Count ?? 0}\n";
            
            if (effects != null && effects.Count > 0)
            {
                info += "効果一覧:\n";
                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect != null)
                    {
                        info += $"  {i + 1}. {effect.EffectName} ({effect.EffectType})\n";
                    }
                }
            }
            
            return info;
        }
    }
}