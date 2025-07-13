using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// キャラクタースキル習得レベル管理システム
    /// レベル3/6/10での段階的スキル習得とジャンプレベルアップ対応を提供
    /// </summary>
    [Serializable]
    public class CharacterSkillProgression
    {
        // 定数
        /// <summary>
        /// スキル習得レベル（固定）
        /// </summary>
        public static readonly int[] SKILL_UNLOCK_LEVELS = { 3, 6, 10 };

        [Header("基本情報")]
        [SerializeField] private int level = 1;
        [SerializeField] private Dictionary<int, Skill> unlockedSkills = new Dictionary<int, Skill>();

        // プロパティ
        /// <summary>
        /// 現在のレベル
        /// </summary>
        public int Level 
        { 
            get => level; 
            set => level = value; 
        }

        /// <summary>
        /// 習得済みスキル（読み取り専用コピー）
        /// </summary>
        public Dictionary<int, Skill> UnlockedSkills => new Dictionary<int, Skill>(unlockedSkills);

        /// <summary>
        /// 習得済みスキル数
        /// </summary>
        public int UnlockedSkillCount => unlockedSkills.Count;

        /// <summary>
        /// 最大習得可能スキル数
        /// </summary>
        public int MaxSkillSlots => SKILL_UNLOCK_LEVELS.Length;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CharacterSkillProgression()
        {
            level = 1;
            unlockedSkills = new Dictionary<int, Skill>();
        }

        /// <summary>
        /// レベル指定コンストラクタ
        /// レベルn以下で習得するスキルはすべて習得済みで作成される
        /// </summary>
        /// <param name="initialLevel">初期レベル</param>
        public CharacterSkillProgression(int initialLevel)
        {
            level = Math.Max(1, initialLevel);
            unlockedSkills = new Dictionary<int, Skill>();
            
            // 初期レベル以下で習得するスキルをすべて習得済みとして設定
            AutoUnlockSkillsForLevel(level);
        }

        /// <summary>
        /// 指定レベルでスキル習得が可能かどうかを判定
        /// </summary>
        /// <param name="checkLevel">確認するレベル</param>
        /// <returns>スキル習得可能かどうか</returns>
        public bool CanUnlockSkill(int checkLevel)
        {
            return SKILL_UNLOCK_LEVELS.Contains(checkLevel);
        }

        /// <summary>
        /// レベルからスキルスロット番号を取得
        /// </summary>
        /// <param name="unlockLevel">習得レベル</param>
        /// <returns>スキルスロット番号（0から開始、見つからない場合は-1）</returns>
        public int GetSkillSlot(int unlockLevel)
        {
            return Array.IndexOf(SKILL_UNLOCK_LEVELS, unlockLevel);
        }

        /// <summary>
        /// スキルスロットからレベルを取得
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <returns>習得レベル（無効なスロットの場合は-1）</returns>
        public int GetUnlockLevelBySlot(int skillSlot)
        {
            if (skillSlot < 0 || skillSlot >= SKILL_UNLOCK_LEVELS.Length)
            {
                return -1;
            }
            return SKILL_UNLOCK_LEVELS[skillSlot];
        }

        /// <summary>
        /// 指定スキルスロットが習得済みかどうかを確認
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <returns>習得済みかどうか</returns>
        public bool IsSkillUnlocked(int skillSlot)
        {
            return unlockedSkills.ContainsKey(skillSlot);
        }

        /// <summary>
        /// 指定レベルのスキルが習得済みかどうかを確認
        /// </summary>
        /// <param name="unlockLevel">習得レベル</param>
        /// <returns>習得済みかどうか</returns>
        public bool IsSkillUnlockedByLevel(int unlockLevel)
        {
            int skillSlot = GetSkillSlot(unlockLevel);
            return skillSlot >= 0 && IsSkillUnlocked(skillSlot);
        }

        /// <summary>
        /// 指定スキルスロットのスキルを取得
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <returns>スキル（見つからない場合はnull）</returns>
        public Skill GetSkill(int skillSlot)
        {
            return unlockedSkills.TryGetValue(skillSlot, out Skill skill) ? skill : null;
        }

        /// <summary>
        /// 現在レベルで習得可能なスキルレベルを取得
        /// </summary>
        /// <returns>習得可能なスキルレベルのリスト</returns>
        public List<int> GetAvailableSkillLevels()
        {
            return SKILL_UNLOCK_LEVELS.Where(unlockLevel => 
                unlockLevel <= level && !IsSkillUnlockedByLevel(unlockLevel)).ToList();
        }

        /// <summary>
        /// スキルを習得する（内部メソッド）
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="skill">習得するスキル</param>
        /// <returns>習得に成功したかどうか</returns>
        internal bool UnlockSkill(int skillSlot, Skill skill)
        {
            if (skill == null)
            {
                Debug.LogError("[CharacterSkillProgression] 習得するスキルがnullです");
                return false;
            }

            if (skillSlot < 0 || skillSlot >= SKILL_UNLOCK_LEVELS.Length)
            {
                Debug.LogError($"[CharacterSkillProgression] 無効なスキルスロット: {skillSlot}");
                return false;
            }

            if (IsSkillUnlocked(skillSlot))
            {
                Debug.LogWarning($"[CharacterSkillProgression] スキルスロット{skillSlot}は既に習得済みです");
                return false;
            }

            int requiredLevel = SKILL_UNLOCK_LEVELS[skillSlot];
            if (level < requiredLevel)
            {
                Debug.LogError($"[CharacterSkillProgression] レベル{requiredLevel}に達していません（現在レベル: {level}）");
                return false;
            }

            unlockedSkills[skillSlot] = skill;
            Debug.Log($"[CharacterSkillProgression] スキルスロット{skillSlot}にスキル「{skill.SkillName}」を習得しました");
            return true;
        }

        /// <summary>
        /// スキルの習得を取り消す（テスト・デバッグ用）
        /// </summary>
        /// <param name="skillSlot">取り消すスキルスロット番号</param>
        /// <returns>取り消しに成功したかどうか</returns>
        internal bool RemoveSkill(int skillSlot)
        {
            if (!IsSkillUnlocked(skillSlot))
            {
                Debug.LogWarning($"[CharacterSkillProgression] スキルスロット{skillSlot}は習得されていません");
                return false;
            }

            Skill removedSkill = unlockedSkills[skillSlot];
            unlockedSkills.Remove(skillSlot);
            Debug.Log($"[CharacterSkillProgression] スキル「{removedSkill.SkillName}」を取り消しました");
            return true;
        }

        /// <summary>
        /// 全スキルをリセットする（テスト・デバッグ用）
        /// </summary>
        public void ResetAllSkills()
        {
            int removedCount = unlockedSkills.Count;
            unlockedSkills.Clear();
            Debug.Log($"[CharacterSkillProgression] 全スキル（{removedCount}個）をリセットしました");
        }

        /// <summary>
        /// 指定レベル以下で習得するスキルを自動習得
        /// </summary>
        /// <param name="targetLevel">対象レベル</param>
        private void AutoUnlockSkillsForLevel(int targetLevel)
        {
            foreach (int unlockLevel in SKILL_UNLOCK_LEVELS)
            {
                if (unlockLevel <= targetLevel)
                {
                    int skillSlot = GetSkillSlot(unlockLevel);
                    if (skillSlot >= 0 && !IsSkillUnlocked(skillSlot))
                    {
                        // デフォルトスキルを作成して習得
                        var defaultSkill = CreateDefaultSkillForLevel(unlockLevel);
                        unlockedSkills[skillSlot] = defaultSkill;
                        
                        Debug.Log($"[CharacterSkillProgression] レベル{targetLevel}作成時に自動習得: スロット{skillSlot} (Lv{unlockLevel}) - {defaultSkill.SkillName}");
                    }
                }
            }
        }

        /// <summary>
        /// 指定レベル用のデフォルトスキルを作成
        /// </summary>
        /// <param name="unlockLevel">習得レベル</param>
        /// <returns>デフォルトスキル</returns>
        private Skill CreateDefaultSkillForLevel(int unlockLevel)
        {
            return new Skill(
                $"デフォルトスキルLv{unlockLevel}",
                $"レベル{unlockLevel}で自動習得されるスキル",
                unlockLevel * 100, // ID
                unlockLevel
            );
        }

        /// <summary>
        /// レベルアップ処理（合成による複数レベル上昇対応）
        /// 仕様書4.1に基づく実装
        /// </summary>
        /// <param name="targetLevel">目標レベル</param>
        /// <param name="characterName">キャラクター名（SkillUnlockResult用）</param>
        /// <returns>習得したスキルのリスト</returns>
        public List<SkillUnlockResult> LevelUp(int targetLevel, string characterName = "")
        {
            var unlockResults = new List<SkillUnlockResult>();
            
            if (targetLevel <= level)
            {
                Debug.LogWarning($"[CharacterSkillProgression] 対象レベル{targetLevel}は現在レベル{level}以下です");
                return unlockResults;
            }
            
            int oldLevel = level;
            level = targetLevel;
            
            // 旧レベルから新レベルまでの間で習得可能なスキルをチェック
            foreach (int unlockLevel in SKILL_UNLOCK_LEVELS)
            {
                // 旧レベルより上で、新レベル以下のスキル習得レベルをすべて処理
                if (unlockLevel > oldLevel && unlockLevel <= targetLevel)
                {
                    var result = TryUnlockSkillForLevelUp(unlockLevel, characterName);
                    if (result != null)
                    {
                        unlockResults.Add(result);
                    }
                }
            }
            
            Debug.Log($"[CharacterSkillProgression] レベル{oldLevel}→{targetLevel}で{unlockResults.Count}個のスキルを習得しました");
            return unlockResults;
        }

        /// <summary>
        /// 指定レベルでのスキル習得試行（レベルアップ用）
        /// </summary>
        /// <param name="unlockLevel">習得レベル</param>
        /// <param name="characterName">キャラクター名</param>
        /// <returns>習得結果（失敗時はnull）</returns>
        private SkillUnlockResult TryUnlockSkillForLevelUp(int unlockLevel, string characterName)
        {
            int skillSlot = GetSkillSlot(unlockLevel);
            
            if (skillSlot == -1)
            {
                Debug.LogError($"[CharacterSkillProgression] レベル{unlockLevel}は有効なスキル習得レベルではありません");
                return null;
            }
            
            // 既に習得済みかチェック
            if (IsSkillUnlocked(skillSlot))
            {
                Debug.LogWarning($"[CharacterSkillProgression] スキルスロット{skillSlot}は既に習得済みです");
                return null;
            }
            
            // レベルアップ用のスキルを作成（実際のゲームではCharacterDataから取得）
            var skillData = CreateSkillForLevelUp(unlockLevel);
            if (skillData == null)
            {
                Debug.LogError($"[CharacterSkillProgression] レベル{unlockLevel}のスキルデータが見つかりません");
                return null;
            }
            
            // スキル習得実行
            bool unlockSuccess = UnlockSkill(skillSlot, skillData);
            if (!unlockSuccess)
            {
                Debug.LogError($"[CharacterSkillProgression] スキル習得に失敗しました: レベル{unlockLevel}");
                return null;
            }
            
            // SkillUnlockResult作成
            return new SkillUnlockResult(unlockLevel, skillSlot, skillData, characterName);
        }

        /// <summary>
        /// レベルアップ用のスキルを作成
        /// 実際のゲームではCharacterDataから取得するが、テスト用に動的作成
        /// </summary>
        /// <param name="unlockLevel">習得レベル</param>
        /// <returns>作成されたスキル</returns>
        private Skill CreateSkillForLevelUp(int unlockLevel)
        {
            return new Skill(
                $"レベルアップスキルLv{unlockLevel}",
                $"レベル{unlockLevel}でレベルアップ時に習得されるスキル",
                unlockLevel * 1000 + 1, // レベルアップ用のID
                unlockLevel
            );
        }

        /// <summary>
        /// スキル進行状況をデバッグ出力
        /// </summary>
        public void DebugPrintSkillProgression()
        {
            Debug.Log($"[CharacterSkillProgression] レベル: {level}, 習得スキル数: {UnlockedSkillCount}/{MaxSkillSlots}");
            
            for (int i = 0; i < SKILL_UNLOCK_LEVELS.Length; i++)
            {
                int unlockLevel = SKILL_UNLOCK_LEVELS[i];
                bool isUnlocked = IsSkillUnlocked(i);
                string status = isUnlocked ? "習得済み" : (level >= unlockLevel ? "習得可能" : "レベル不足");
                string skillName = isUnlocked ? GetSkill(i).SkillName : "未習得";
                
                Debug.Log($"  スロット{i} (Lv{unlockLevel}): {status} - {skillName}");
            }
        }
    }
}