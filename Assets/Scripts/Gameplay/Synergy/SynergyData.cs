using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Core;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジーデータのScriptableObject
    /// シナジーの基本情報、発動条件、効果レベルを管理
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyData", menuName = "GatchaSpire/Synergy/Synergy Data")]
    public class SynergyData : ScriptableObject, IValidatable
    {
        [Header("基本情報")]
        [SerializeField] private string synergyId = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private string description = "";
        
        [Header("効果レベル")]
        [SerializeField] private SynergyLevel[] synergyLevels = new SynergyLevel[0];
        
        [Header("UI設定")]
        [SerializeField] private Sprite synergyIcon;
        [SerializeField] private Color synergyColor = Color.white;
        
        [Header("バランス設定")]
        [SerializeField] private int priority = 0;
        [SerializeField] private bool isActive = true;

        // プロパティ
        public string SynergyId => synergyId;
        public string DisplayName => displayName;
        public string Description => description;
        public SynergyLevel[] SynergyLevels => synergyLevels;
        public Sprite SynergyIcon => synergyIcon;
        public Color SynergyColor => synergyColor;
        public int Priority => priority;
        public bool IsActive => isActive;

        /// <summary>
        /// 指定した必要数に対応するシナジーレベルを取得
        /// </summary>
        /// <param name="RequiredCount">必要キャラクター数</param>
        /// <returns>対応するシナジーレベル、見つからない場合はnull</returns>
        public SynergyLevel GetSynergyLevel(int RequiredCount)
        {
            foreach (var level in synergyLevels)
            {
                if (level.RequiredCount == RequiredCount)
                {
                    return level;
                }
            }
            return null;
        }

        /// <summary>
        /// 指定した数で発動可能な最大シナジーレベルを取得
        /// </summary>
        /// <param name="currentCount">現在のキャラクター数</param>
        /// <returns>発動可能な最大シナジーレベル、発動不可の場合はnull</returns>
        public SynergyLevel GetMaxActiveSynergyLevel(int currentCount)
        {
            SynergyLevel maxLevel = null;
            int maxRequiredCount = 0;

            foreach (var level in synergyLevels)
            {
                if (level.RequiredCount <= currentCount && level.RequiredCount > maxRequiredCount)
                {
                    maxLevel = level;
                    maxRequiredCount = level.RequiredCount;
                }
            }

            return maxLevel;
        }

        /// <summary>
        /// 次のシナジーレベルを取得
        /// </summary>
        /// <param name="currentCount">現在のキャラクター数</param>
        /// <returns>次のシナジーレベル、存在しない場合はnull</returns>
        public SynergyLevel GetNextSynergyLevel(int currentCount)
        {
            SynergyLevel nextLevel = null;
            int minRequiredCount = int.MaxValue;

            foreach (var level in synergyLevels)
            {
                if (level.RequiredCount > currentCount && level.RequiredCount < minRequiredCount)
                {
                    nextLevel = level;
                    minRequiredCount = level.RequiredCount;
                }
            }

            return nextLevel;
        }

        /// <summary>
        /// 全シナジーレベルを取得（昇順ソート）
        /// </summary>
        /// <returns>昇順ソートされたシナジーレベル配列</returns>
        public SynergyLevel[] GetAllSynergyLevelsSorted()
        {
            var sortedLevels = new List<SynergyLevel>(synergyLevels);
            sortedLevels.Sort((a, b) => a.RequiredCount.CompareTo(b.RequiredCount));
            return sortedLevels.ToArray();
        }

        /// <summary>
        /// データの妥当性検証
        /// </summary>
        /// <returns>バリデーション結果</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            // 基本情報の検証
            if (string.IsNullOrEmpty(synergyId))
            {
                result.AddError("シナジーIDが設定されていません");
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                result.AddError("表示名が設定されていません");
            }
            
            if (string.IsNullOrEmpty(description))
            {
                result.AddWarning("説明が設定されていません");
            }

            // 発動条件の検証（synergyIdで代替）
            if (string.IsNullOrEmpty(synergyId))
            {
                result.AddError("シナジーIDが設定されていません（発動条件として使用）");
            }

            // 効果レベルの検証
            if (synergyLevels == null || synergyLevels.Length == 0)
            {
                result.AddError("効果レベルが設定されていません");
            }
            else
            {
                // 重複チェック
                var RequiredCounts = new HashSet<int>();
                foreach (var level in synergyLevels)
                {
                    if (RequiredCounts.Contains(level.RequiredCount))
                    {
                        result.AddError($"必要数 {level.RequiredCount} のレベルが重複しています");
                    }
                    else
                    {
                        RequiredCounts.Add(level.RequiredCount);
                    }

                    // レベル個別の検証
                    if (level.RequiredCount <= 0)
                    {
                        result.AddError($"必要数は1以上である必要があります（現在: {level.RequiredCount}）");
                    }
                }
            }

            // UI設定の検証
            if (synergyIcon == null)
            {
                result.AddWarning("シナジーアイコンが設定されていません");
            }

            // バランス設定の検証
            if (priority < 0)
            {
                result.AddWarning("優先度が負の値です");
            }

            return result;
        }

        /// <summary>
        /// エディタプレビュー用の情報を取得
        /// </summary>
        /// <returns>プレビュー情報</returns>
        public string GetPreviewInfo()
        {
            var info = $"=== {displayName} ===\n";
            info += $"ID: {synergyId}\n";
            info += $"優先度: {priority}\n";
            info += $"アクティブ: {isActive}\n";
            info += $"レベル数: {synergyLevels?.Length ?? 0}\n";
            
            if (synergyLevels != null && synergyLevels.Length > 0)
            {
                info += "\n効果レベル:\n";
                foreach (var level in GetAllSynergyLevelsSorted())
                {
                    info += $"  {level.RequiredCount}体: {level.LevelDescription}\n";
                }
            }
            
            return info;
        }

        /// <summary>
        /// Unity OnValidate
        /// </summary>
        private void OnValidate()
        {
            // 値の範囲制限
            priority = Mathf.Max(0, priority);
            
            // ID文字列の正規化
            if (!string.IsNullOrEmpty(synergyId))
            {
                synergyId = synergyId.Trim().ToLower();
            }

            // バリデーション実行
            var validation = Validate();
            if (!validation.IsValid && Application.isPlaying)
            {
                Debug.LogWarning($"[SynergyData] {name}: {validation.GetSummary()}", this);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// テスト用データ設定メソッド
        /// </summary>
        /// <param name="id">シナジーID</param>
        /// <param name="name">表示名</param>
        /// <param name="levels">シナジーレベル配列</param>
        public void SetTestData(string id, string name, SynergyLevel[] levels)
        {
            synergyId = id;
            displayName = name;
            synergyLevels = levels;
            isActive = true;
            priority = 0;
        }
#endif
    }
}