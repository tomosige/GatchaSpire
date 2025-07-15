using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクター合成操作
    /// ベースキャラクターに素材キャラクターを合成してレベルアップやステータス強化を行う
    /// </summary>
    [Serializable]
    public class FuseOperation : CharacterOperation
    {
        [Header("合成対象")]
        [SerializeField] private string baseCharacterId = "";
        [SerializeField] private List<string> materialCharacterIds = new List<string>();

        [Header("合成設定")]
        [SerializeField] private bool preserveBaseLevel = true;
        [SerializeField] private bool transferSkills = false;
        [SerializeField] private bool allowSameCharacterFusion = true;
        [SerializeField] private bool confirmFusion = false;

        [Header("効果設定")]
        [SerializeField] private float expConversionRate = 1.0f;
        [SerializeField] private float skillTransferRate = 0.5f;
        [SerializeField] private int maxMaterialCount = 10;

        // プロパティ
        public string BaseCharacterId => baseCharacterId;
        public List<string> MaterialCharacterIds => new List<string>(materialCharacterIds);
        public bool PreserveBaseLevel => preserveBaseLevel;
        public bool TransferSkills => transferSkills;
        public bool AllowSameCharacterFusion => allowSameCharacterFusion;
        public bool ConfirmFusion => confirmFusion;
        public float ExpConversionRate => expConversionRate;
        public float SkillTransferRate => skillTransferRate;
        public int MaxMaterialCount => maxMaterialCount;

        /// <summary>
        /// 合成操作のコンストラクタ
        /// </summary>
        /// <param name="baseId">ベースキャラクターID</param>
        /// <param name="materialIds">素材キャラクターIDリスト</param>
        /// <param name="requester">リクエスター名</param>
        public FuseOperation(string baseId, List<string> materialIds, string requester = "system") : base(requester)
        {
            baseCharacterId = baseId ?? "";
            materialCharacterIds = new List<string>(materialIds ?? new List<string>());
        }

        /// <summary>
        /// 単一素材合成のコンストラクタ
        /// </summary>
        /// <param name="baseId">ベースキャラクターID</param>
        /// <param name="materialId">素材キャラクターID</param>
        /// <param name="requester">リクエスター名</param>
        public FuseOperation(string baseId, string materialId, string requester = "system") : base(requester)
        {
            baseCharacterId = baseId ?? "";
            if (!string.IsNullOrEmpty(materialId))
            {
                materialCharacterIds.Add(materialId);
            }
        }

        /// <summary>
        /// ベースキャラクターを設定
        /// </summary>
        /// <param name="characterId">ベースキャラクターID</param>
        public void SetBaseCharacter(string characterId)
        {
            baseCharacterId = characterId ?? "";
        }

        /// <summary>
        /// 素材キャラクターを追加
        /// </summary>
        /// <param name="characterId">追加する素材キャラクターID</param>
        public void AddMaterialCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return;
            }

            if (materialCharacterIds.Contains(characterId))
            {
                return;
            }

            if (materialCharacterIds.Count >= maxMaterialCount)
            {
                return;
            }

            // ベースキャラクターと同じIDは追加しない
            if (characterId == baseCharacterId)
            {
                return;
            }

            materialCharacterIds.Add(characterId);
        }

        /// <summary>
        /// 素材キャラクターを削除
        /// </summary>
        /// <param name="characterId">削除する素材キャラクターID</param>
        public void RemoveMaterialCharacter(string characterId)
        {
            materialCharacterIds.Remove(characterId);
        }

        /// <summary>
        /// 全ての素材キャラクターをクリア
        /// </summary>
        public void ClearMaterialCharacters()
        {
            materialCharacterIds.Clear();
        }

        /// <summary>
        /// ベースレベル保持設定
        /// </summary>
        /// <param name="preserve">保持する場合true</param>
        public void SetPreserveBaseLevel(bool preserve)
        {
            preserveBaseLevel = preserve;
        }

        /// <summary>
        /// スキル継承設定
        /// </summary>
        /// <param name="transfer">継承する場合true</param>
        public void SetTransferSkills(bool transfer)
        {
            transferSkills = transfer;
        }

        /// <summary>
        /// 同キャラクター合成許可設定
        /// </summary>
        /// <param name="allow">許可する場合true</param>
        public void SetAllowSameCharacterFusion(bool allow)
        {
            allowSameCharacterFusion = allow;
        }

        /// <summary>
        /// 合成確認設定
        /// </summary>
        /// <param name="confirm">確認済みの場合true</param>
        public void SetConfirmFusion(bool confirm)
        {
            confirmFusion = confirm;
        }

        /// <summary>
        /// 経験値変換率を設定
        /// </summary>
        /// <param name="rate">変換率</param>
        public void SetExpConversionRate(float rate)
        {
            expConversionRate = Mathf.Max(0f, rate);
        }

        /// <summary>
        /// スキル継承率を設定
        /// </summary>
        /// <param name="rate">継承率</param>
        public void SetSkillTransferRate(float rate)
        {
            skillTransferRate = Mathf.Clamp01(rate);
        }

        /// <summary>
        /// 最大素材数を設定
        /// </summary>
        /// <param name="maxCount">最大素材数</param>
        public void SetMaxMaterialCount(int maxCount)
        {
            maxMaterialCount = Mathf.Max(1, maxCount);
            
            // 現在の素材数が上限を超えている場合は削除
            while (materialCharacterIds.Count > maxMaterialCount)
            {
                materialCharacterIds.RemoveAt(materialCharacterIds.Count - 1);
            }
        }

        /// <summary>
        /// 操作の有効性を検証
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            // ベースキャラクターが設定されているか
            if (string.IsNullOrEmpty(baseCharacterId))
            {
                return false;
            }

            // 素材キャラクターが存在するか
            if (!materialCharacterIds.Any())
            {
                return false;
            }

            // ベースキャラクターが素材に含まれていないか
            if (materialCharacterIds.Contains(baseCharacterId))
            {
                return false;
            }

            // 重複素材がないか
            var uniqueMaterials = new HashSet<string>(materialCharacterIds);
            if (uniqueMaterials.Count != materialCharacterIds.Count)
            {
                return false;
            }

            // 素材数が上限以下か
            if (materialCharacterIds.Count > maxMaterialCount)
            {
                return false;
            }

            // 変換率が有効範囲か
            if (expConversionRate < 0 || skillTransferRate < 0 || skillTransferRate > 1)
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
            info += $"\n=== 合成設定 ===\n";
            info += $"ベースキャラクター: {baseCharacterId}\n";
            info += $"素材キャラクター数: {materialCharacterIds.Count}\n";
            info += $"ベースレベル保持: {preserveBaseLevel}\n";
            info += $"スキル継承: {transferSkills}\n";
            info += $"同キャラクター合成許可: {allowSameCharacterFusion}\n";
            info += $"合成確認済み: {confirmFusion}\n";
            info += $"経験値変換率: {expConversionRate:F2}\n";
            info += $"スキル継承率: {skillTransferRate:F2}\n";
            info += $"最大素材数: {maxMaterialCount}\n";

            if (materialCharacterIds.Any())
            {
                info += "\n=== 素材キャラクター ===\n";
                info += string.Join("\n", materialCharacterIds.Select((id, index) => $"{index + 1}. {id}")) + "\n";
            }

            return info;
        }

        /// <summary>
        /// 操作の簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"合成操作({materialCharacterIds.Count}素材) - {operationId}";
        }
    }
}