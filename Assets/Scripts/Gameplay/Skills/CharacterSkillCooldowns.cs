using System;
using System.Collections.Generic;
using UnityEngine;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// キャラクター個別スキルクールダウン管理クラス
    /// 軽量なデータクラスとして各Characterインスタンスに内包される
    /// </summary>
    [Serializable]
    public class CharacterSkillCooldowns
    {
        [Header("クールダウン管理")]
        [SerializeField] private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();
        [SerializeField] private Dictionary<int, float> skillLastUsedTime = new Dictionary<int, float>();

        /// <summary>
        /// スキルクールダウン時間管理（スキルスロット → クールダウン時間）
        /// </summary>
        public Dictionary<int, float> SkillCooldowns => new Dictionary<int, float>(skillCooldowns);

        /// <summary>
        /// スキル最終使用時刻管理（スキルスロット → 最終使用時刻）
        /// </summary>
        public Dictionary<int, float> SkillLastUsedTime => new Dictionary<int, float>(skillLastUsedTime);

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CharacterSkillCooldowns()
        {
            skillCooldowns = new Dictionary<int, float>();
            skillLastUsedTime = new Dictionary<int, float>();
        }

        /// <summary>
        /// 指定スキルスロットが使用準備完了かどうかを判定
        /// 仕様書2.1に基づく実装
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>使用可能かどうか</returns>
        public bool IsSkillReady(int skillSlot, float currentTime)
        {
            // 初回使用の場合は使用可能
            if (!skillLastUsedTime.ContainsKey(skillSlot))
            {
                return true;
            }

            // クールダウン時間を取得（設定されていない場合は0）
            float cooldown = skillCooldowns.GetValueOrDefault(skillSlot, 0f);
            float lastUsedTime = skillLastUsedTime[skillSlot];
            float elapsedTime = currentTime - lastUsedTime;

            return elapsedTime >= cooldown;
        }

        /// <summary>
        /// スキルを使用してクールダウンを開始
        /// 仕様書2.1に基づく実装
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="currentTime">現在時刻</param>
        /// <param name="cooldownTime">クールダウン時間</param>
        public void UseSkill(int skillSlot, float currentTime, float cooldownTime)
        {
            if (skillSlot < 0)
            {
                Debug.LogError($"[CharacterSkillCooldowns] 無効なスキルスロット: {skillSlot}");
                return;
            }

            if (cooldownTime < 0f)
            {
                Debug.LogWarning($"[CharacterSkillCooldowns] 負のクールダウン時間が指定されました: {cooldownTime}秒。0秒に修正します。");
                cooldownTime = 0f;
            }

            // 使用時刻とクールダウン時間を記録
            skillLastUsedTime[skillSlot] = currentTime;
            skillCooldowns[skillSlot] = cooldownTime;
        }

        /// <summary>
        /// 指定スキルスロットの残りクールダウン時間を取得
        /// </summary>
        /// <param name="skillSlot">スキルスロット番号</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>残りクールダウン時間（0以下の場合は使用可能）</returns>
        public float GetRemainingCooldown(int skillSlot, float currentTime)
        {
            if (!skillLastUsedTime.ContainsKey(skillSlot))
            {
                return 0f; // 初回使用の場合は残り0
            }

            float cooldown = skillCooldowns.GetValueOrDefault(skillSlot, 0f);
            float lastUsedTime = skillLastUsedTime[skillSlot];
            float elapsedTime = currentTime - lastUsedTime;

            return Mathf.Max(0f, cooldown - elapsedTime);
        }

        /// <summary>
        /// 全スキルのクールダウンをリセット（テスト・デバッグ用）
        /// </summary>
        public void ResetAllCooldowns()
        {
            skillLastUsedTime.Clear();
            skillCooldowns.Clear();
        }

        /// <summary>
        /// 指定スキルスロットのクールダウンをリセット（テスト・デバッグ用）
        /// </summary>
        /// <param name="skillSlot">リセットするスキルスロット番号</param>
        public void ResetSkillCooldown(int skillSlot)
        {
            skillLastUsedTime.Remove(skillSlot);
            skillCooldowns.Remove(skillSlot);
        }

        /// <summary>
        /// スキルクールダウン状況の文字列表現
        /// </summary>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>クールダウン状況</returns>
        public string GetCooldownStatus(float currentTime)
        {
            if (skillLastUsedTime.Count == 0)
            {
                return "使用済みスキルなし";
            }

            var statusList = new List<string>();
            foreach (var kvp in skillLastUsedTime)
            {
                int skillSlot = kvp.Key;
                float remaining = GetRemainingCooldown(skillSlot, currentTime);
                bool isReady = IsSkillReady(skillSlot, currentTime);

                string status = isReady ? "使用可能" : $"残り{remaining:F1}秒";
                statusList.Add($"スロット{skillSlot}: {status}");
            }

            return string.Join(", ", statusList);
        }

        /// <summary>
        /// 使用中スキル数を取得
        /// </summary>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>クールダウン中のスキル数</returns>
        public int GetActiveCooldownCount(float currentTime)
        {
            int count = 0;
            foreach (int skillSlot in skillLastUsedTime.Keys)
            {
                if (!IsSkillReady(skillSlot, currentTime))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 全スキルが使用可能かどうかを判定
        /// </summary>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>全スキルが使用可能かどうか</returns>
        public bool AreAllSkillsReady(float currentTime)
        {
            foreach (int skillSlot in skillLastUsedTime.Keys)
            {
                if (!IsSkillReady(skillSlot, currentTime))
                {
                    return false;
                }
            }
            return true;
        }
    }
}