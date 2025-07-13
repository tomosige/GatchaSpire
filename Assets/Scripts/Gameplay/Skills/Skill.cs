using UnityEngine;
using System;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// 基本スキルクラス
    /// スキルの基本情報とメタデータを管理
    /// </summary>
    [Serializable]
    public class Skill
    {
        [Header("基本情報")]
        [SerializeField] private string skillName = "";
        [SerializeField] private string description = "";
        [SerializeField] private int skillId = 0;

        [Header("スキル分類")]
        [SerializeField] private SkillType skillType = SkillType.Active;
        [SerializeField] private SkillCategory category = SkillCategory.Attack;
        [SerializeField] private int unlockLevel = 3;

        [Header("使用設定")]
        [SerializeField] private float cooldownTime = 3.0f;
        [SerializeField] private int manaCost = 10;
        [SerializeField] private int maxUses = -1; // -1は無制限

        // プロパティ
        /// <summary>
        /// スキル名
        /// </summary>
        public string SkillName => skillName;

        /// <summary>
        /// スキルの説明
        /// </summary>
        public string Description => description;

        /// <summary>
        /// スキルID
        /// </summary>
        public int SkillId => skillId;

        /// <summary>
        /// スキルタイプ
        /// </summary>
        public SkillType SkillType => skillType;

        /// <summary>
        /// スキルカテゴリ
        /// </summary>
        public SkillCategory Category => category;

        /// <summary>
        /// 習得レベル
        /// </summary>
        public int UnlockLevel => unlockLevel;

        /// <summary>
        /// クールダウン時間（秒）
        /// </summary>
        public float CooldownTime => cooldownTime;

        /// <summary>
        /// マナ消費量
        /// </summary>
        public int ManaCost => manaCost;

        /// <summary>
        /// 最大使用回数（-1は無制限）
        /// </summary>
        public int MaxUses => maxUses;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Skill()
        {
            skillName = "新規スキル";
            description = "";
            skillId = 0;
            skillType = SkillType.Active;
            category = SkillCategory.Attack;
            unlockLevel = 3;
            cooldownTime = 3.0f;
            manaCost = 10;
            maxUses = -1;
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="desc">説明</param>
        /// <param name="id">スキルID</param>
        /// <param name="level">習得レベル</param>
        public Skill(string name, string desc, int id, int level)
        {
            skillName = name ?? "新規スキル";
            description = desc ?? "";
            skillId = id;
            skillType = SkillType.Active;
            category = SkillCategory.Attack;
            unlockLevel = Math.Max(1, level);
            cooldownTime = 3.0f;
            manaCost = 10;
            maxUses = -1;
        }

        /// <summary>
        /// スキル情報の文字列表現
        /// </summary>
        /// <returns>スキル情報</returns>
        public override string ToString()
        {
            return $"[{SkillId}] {SkillName} (Lv{UnlockLevel}) - {Description}";
        }
    }

    /// <summary>
    /// スキルタイプ
    /// </summary>
    public enum SkillType
    {
        /// <summary>能動スキル</summary>
        Active,
        /// <summary>受動スキル</summary>
        Passive,
        /// <summary>切り替えスキル</summary>
        Toggle,
        /// <summary>自動発動スキル</summary>
        Auto
    }

    /// <summary>
    /// スキルカテゴリ
    /// </summary>
    public enum SkillCategory
    {
        /// <summary>攻撃系</summary>
        Attack,
        /// <summary>防御系</summary>
        Defense,
        /// <summary>回復系</summary>
        Heal,
        /// <summary>支援系</summary>
        Support,
        /// <summary>妨害系</summary>
        Debuff,
        /// <summary>移動系</summary>
        Movement,
        /// <summary>特殊系</summary>
        Special
    }
}