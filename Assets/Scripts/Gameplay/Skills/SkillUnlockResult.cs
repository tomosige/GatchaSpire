using UnityEngine;
using System;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// スキル習得結果の管理クラス
    /// レベルアップ時のスキル習得情報を保持し、ログ出力やUI表示に使用
    /// </summary>
    [Serializable]
    public class SkillUnlockResult
    {
        [Header("習得情報")]
        [SerializeField] private int unlockLevel;
        [SerializeField] private int skillSlot;
        [SerializeField] private Skill unlockedSkill;
        [SerializeField] private string characterName;

        /// <summary>
        /// スキル習得レベル
        /// </summary>
        public int UnlockLevel 
        { 
            get => unlockLevel; 
            set => unlockLevel = value; 
        }

        /// <summary>
        /// スキルスロット番号（0から開始）
        /// </summary>
        public int SkillSlot 
        { 
            get => skillSlot; 
            set => skillSlot = value; 
        }

        /// <summary>
        /// 習得したスキル
        /// </summary>
        public Skill UnlockedSkill 
        { 
            get => unlockedSkill; 
            set => unlockedSkill = value; 
        }

        /// <summary>
        /// キャラクター名
        /// </summary>
        public string CharacterName 
        { 
            get => characterName; 
            set => characterName = value ?? ""; 
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public SkillUnlockResult()
        {
            unlockLevel = 0;
            skillSlot = -1;
            unlockedSkill = null;
            characterName = "";
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="level">習得レベル</param>
        /// <param name="slot">スキルスロット番号</param>
        /// <param name="skill">習得したスキル</param>
        /// <param name="charName">キャラクター名</param>
        public SkillUnlockResult(int level, int slot, Skill skill, string charName)
        {
            unlockLevel = level;
            skillSlot = slot;
            unlockedSkill = skill;
            characterName = charName ?? "";
        }

        /// <summary>
        /// スキル習得結果の妥当性を確認
        /// </summary>
        /// <returns>妥当性</returns>
        public bool IsValid()
        {
            return unlockLevel > 0 && 
                   skillSlot >= 0 && 
                   unlockedSkill != null && 
                   !string.IsNullOrEmpty(characterName);
        }

        /// <summary>
        /// スキル習得結果の文字列表現
        /// 仕様書4.2の形式に準拠
        /// </summary>
        /// <returns>習得結果メッセージ</returns>
        public override string ToString()
        {
            if (!IsValid())
            {
                return "無効なスキル習得結果";
            }

            return $"{characterName}がレベル{unlockLevel}でスキル「{unlockedSkill.SkillName}」を習得しました";
        }

        /// <summary>
        /// 詳細情報付きの文字列表現
        /// </summary>
        /// <returns>詳細習得結果メッセージ</returns>
        public string ToDetailedString()
        {
            if (!IsValid())
            {
                return "無効なスキル習得結果（詳細情報なし）";
            }

            return $"{characterName}がレベル{unlockLevel}でスキルスロット{skillSlot}にスキル「{unlockedSkill.SkillName}」（ID:{unlockedSkill.SkillId}）を習得しました";
        }

        /// <summary>
        /// スキル習得結果の比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価かどうか</returns>
        public override bool Equals(object obj)
        {
            if (obj is SkillUnlockResult other)
            {
                return unlockLevel == other.unlockLevel &&
                       skillSlot == other.skillSlot &&
                       characterName == other.characterName &&
                       ((unlockedSkill == null && other.unlockedSkill == null) ||
                        (unlockedSkill != null && other.unlockedSkill != null && 
                         unlockedSkill.SkillId == other.unlockedSkill.SkillId));
            }
            return false;
        }

        /// <summary>
        /// ハッシュコードの取得
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(unlockLevel, skillSlot, characterName, unlockedSkill?.SkillId ?? 0);
        }
    }
}