using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクター経験値化操作
    /// 不要なキャラクターを経験値に変換して他のキャラクターに付与
    /// </summary>
    [Serializable]
    public class ConvertToExpOperation : CharacterOperation
    {
        [Header("変換対象")]
        [SerializeField] private List<string> targetCharacterIds = new List<string>();
        [SerializeField] private string receivingCharacterId = "";

        [Header("変換設定")]
        [SerializeField] private float conversionRate = 1.0f;
        [SerializeField] private bool convertLocked = false;
        [SerializeField] private bool convertFavorites = false;
        [SerializeField] private bool confirmConversion = false;

        [Header("効率設定")]
        [SerializeField] private float rarityBonusMultiplier = 1.0f;
        [SerializeField] private float levelBonusMultiplier = 0.1f;
        [SerializeField] private int minimumExpGain = 1;
        [SerializeField] private int maximumExpGain = 999999;

        // プロパティ
        public List<string> TargetCharacterIds => new List<string>(targetCharacterIds);
        public string ReceivingCharacterId => receivingCharacterId;
        public float ConversionRate => conversionRate;
        public bool ConvertLocked => convertLocked;
        public bool ConvertFavorites => convertFavorites;
        public bool ConfirmConversion => confirmConversion;
        public float RarityBonusMultiplier => rarityBonusMultiplier;
        public float LevelBonusMultiplier => levelBonusMultiplier;
        public int MinimumExpGain => minimumExpGain;
        public int MaximumExpGain => maximumExpGain;

        /// <summary>
        /// 経験値化操作のコンストラクタ
        /// </summary>
        /// <param name="targetIds">変換対象キャラクターIDリスト</param>
        /// <param name="receiverId">経験値受取キャラクターID</param>
        /// <param name="requester">リクエスター名</param>
        public ConvertToExpOperation(List<string> targetIds, string receiverId, string requester = "system") : base(requester)
        {
            targetCharacterIds = new List<string>(targetIds ?? new List<string>());
            receivingCharacterId = receiverId ?? "";
        }

        /// <summary>
        /// 単一キャラクター経験値化のコンストラクタ
        /// </summary>
        /// <param name="targetId">変換対象キャラクターID</param>
        /// <param name="receiverId">経験値受取キャラクターID</param>
        /// <param name="requester">リクエスター名</param>
        public ConvertToExpOperation(string targetId, string receiverId, string requester = "system") : base(requester)
        {
            if (!string.IsNullOrEmpty(targetId))
            {
                targetCharacterIds.Add(targetId);
            }
            receivingCharacterId = receiverId ?? "";
        }

        /// <summary>
        /// 経験値受取キャラクターを設定
        /// </summary>
        /// <param name="characterId">経験値受取キャラクターID</param>
        public void SetReceivingCharacter(string characterId)
        {
            receivingCharacterId = characterId ?? "";
        }

        /// <summary>
        /// 変換対象キャラクターを追加
        /// </summary>
        /// <param name="characterId">追加するキャラクターID</param>
        public void AddTargetCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return;
            }

            if (targetCharacterIds.Contains(characterId))
            {
                return;
            }

            // 受取キャラクターと同じIDは追加しない
            if (characterId == receivingCharacterId)
            {
                return;
            }

            targetCharacterIds.Add(characterId);
        }

        /// <summary>
        /// 変換対象キャラクターを削除
        /// </summary>
        /// <param name="characterId">削除するキャラクターID</param>
        public void RemoveTargetCharacter(string characterId)
        {
            targetCharacterIds.Remove(characterId);
        }

        /// <summary>
        /// 全ての変換対象キャラクターをクリア
        /// </summary>
        public void ClearTargetCharacters()
        {
            targetCharacterIds.Clear();
        }

        /// <summary>
        /// 変換率を設定
        /// </summary>
        /// <param name="rate">変換率</param>
        public void SetConversionRate(float rate)
        {
            conversionRate = Mathf.Max(0f, rate);
        }

        /// <summary>
        /// ロック済みキャラクター変換許可を設定
        /// </summary>
        /// <param name="allow">許可する場合true</param>
        public void SetConvertLocked(bool allow)
        {
            convertLocked = allow;
        }

        /// <summary>
        /// お気に入りキャラクター変換許可を設定
        /// </summary>
        /// <param name="allow">許可する場合true</param>
        public void SetConvertFavorites(bool allow)
        {
            convertFavorites = allow;
        }

        /// <summary>
        /// 変換確認設定
        /// </summary>
        /// <param name="confirm">確認済みの場合true</param>
        public void SetConfirmConversion(bool confirm)
        {
            confirmConversion = confirm;
        }

        /// <summary>
        /// レアリティボーナス倍率を設定
        /// </summary>
        /// <param name="multiplier">ボーナス倍率</param>
        public void SetRarityBonusMultiplier(float multiplier)
        {
            rarityBonusMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// レベルボーナス倍率を設定
        /// </summary>
        /// <param name="multiplier">ボーナス倍率</param>
        public void SetLevelBonusMultiplier(float multiplier)
        {
            levelBonusMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// 最小経験値獲得量を設定
        /// </summary>
        /// <param name="minExp">最小経験値</param>
        public void SetMinimumExpGain(int minExp)
        {
            minimumExpGain = Mathf.Max(0, minExp);
        }

        /// <summary>
        /// 最大経験値獲得量を設定
        /// </summary>
        /// <param name="maxExp">最大経験値</param>
        public void SetMaximumExpGain(int maxExp)
        {
            maximumExpGain = Mathf.Max(minimumExpGain, maxExp);
        }

        /// <summary>
        /// 操作の有効性を検証
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            // 変換対象が存在するか
            if (!targetCharacterIds.Any())
            {
                return false;
            }

            // 経験値受取キャラクターが設定されているか
            if (string.IsNullOrEmpty(receivingCharacterId))
            {
                return false;
            }

            // 受取キャラクターが変換対象に含まれていないか
            if (targetCharacterIds.Contains(receivingCharacterId))
            {
                return false;
            }

            // 重複IDがないか
            var uniqueIds = new HashSet<string>(targetCharacterIds);
            if (uniqueIds.Count != targetCharacterIds.Count)
            {
                return false;
            }

            // 変換率が有効範囲か
            if (conversionRate < 0)
            {
                return false;
            }

            // 経験値範囲が有効か
            if (minimumExpGain < 0 || maximumExpGain < minimumExpGain)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 操作の詳細情報を取得
        /// </summary>
        /// <returns>詳細情報</returns>
        public override string GetDetailedInfo()
        {
            var info = base.GetDetailedInfo();
            info += $"\n=== 経験値化設定 ===\n";
            info += $"変換対象キャラクター数: {targetCharacterIds.Count}\n";
            info += $"経験値受取キャラクター: {receivingCharacterId}\n";
            info += $"変換率: {conversionRate:F2}\n";
            info += $"ロック済み変換許可: {convertLocked}\n";
            info += $"お気に入り変換許可: {convertFavorites}\n";
            info += $"変換確認済み: {confirmConversion}\n";
            info += $"レアリティボーナス倍率: {rarityBonusMultiplier:F2}\n";
            info += $"レベルボーナス倍率: {levelBonusMultiplier:F2}\n";
            info += $"経験値範囲: {minimumExpGain} - {maximumExpGain}\n";

            if (targetCharacterIds.Any())
            {
                info += "\n=== 変換対象キャラクター ===\n";
                info += string.Join("\n", targetCharacterIds.Select((id, index) => $"{index + 1}. {id}")) + "\n";
            }

            return info;
        }

        /// <summary>
        /// 操作の簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"経験値化操作({targetCharacterIds.Count}件) - {operationId}";
        }
    }
}