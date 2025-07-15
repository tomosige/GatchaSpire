using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクターロック/アンロック操作
    /// キャラクターのロック状態を変更する
    /// </summary>
    [Serializable]
    public class LockOperation : CharacterOperation
    {
        [Header("操作対象")]
        [SerializeField] private List<string> targetCharacterIds = new List<string>();

        [Header("ロック設定")]
        [SerializeField] private bool lockState = true;
        [SerializeField] private bool overrideFavorites = false;
        [SerializeField] private bool confirmLockChange = false;

        [Header("一括設定")]
        [SerializeField] private bool applyToAll = false;
        [SerializeField] private CharacterRarity minRarityForBatch = CharacterRarity.Rare;

        // プロパティ
        public List<string> TargetCharacterIds => new List<string>(targetCharacterIds);
        public bool LockState => lockState;
        public bool OverrideFavorites => overrideFavorites;
        public bool ConfirmLockChange => confirmLockChange;
        public bool ApplyToAll => applyToAll;
        public CharacterRarity MinRarityForBatch => minRarityForBatch;

        /// <summary>
        /// ロック操作のコンストラクタ
        /// </summary>
        /// <param name="characterIds">対象キャラクターIDリスト</param>
        /// <param name="shouldLock">ロックする場合true、アンロックする場合false</param>
        /// <param name="requester">リクエスター名</param>
        public LockOperation(List<string> characterIds, bool shouldLock, string requester = "system") : base(requester)
        {
            targetCharacterIds = new List<string>(characterIds ?? new List<string>());
            lockState = shouldLock;
        }

        /// <summary>
        /// 単一キャラクターロック操作のコンストラクタ
        /// </summary>
        /// <param name="characterId">対象キャラクターID</param>
        /// <param name="shouldLock">ロックする場合true、アンロックする場合false</param>
        /// <param name="requester">リクエスター名</param>
        public LockOperation(string characterId, bool shouldLock, string requester = "system") : base(requester)
        {
            if (!string.IsNullOrEmpty(characterId))
            {
                targetCharacterIds.Add(characterId);
            }
            lockState = shouldLock;
        }

        /// <summary>
        /// 対象キャラクターを追加
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

            targetCharacterIds.Add(characterId);
        }

        /// <summary>
        /// 対象キャラクターを削除
        /// </summary>
        /// <param name="characterId">削除するキャラクターID</param>
        public void RemoveTargetCharacter(string characterId)
        {
            targetCharacterIds.Remove(characterId);
        }

        /// <summary>
        /// ロック状態を設定
        /// </summary>
        /// <param name="shouldLock">ロックする場合true</param>
        public void SetLockState(bool shouldLock)
        {
            lockState = shouldLock;
        }

        /// <summary>
        /// お気に入り設定の上書き許可を設定
        /// </summary>
        /// <param name="allow">許可する場合true</param>
        public void SetOverrideFavorites(bool allow)
        {
            overrideFavorites = allow;
        }

        /// <summary>
        /// ロック変更確認設定
        /// </summary>
        /// <param name="confirm">確認済みの場合true</param>
        public void SetConfirmLockChange(bool confirm)
        {
            confirmLockChange = confirm;
        }

        /// <summary>
        /// 全キャラクター適用設定
        /// </summary>
        /// <param name="applyAll">全適用する場合true</param>
        public void SetApplyToAll(bool applyAll)
        {
            applyToAll = applyAll;
        }

        /// <summary>
        /// 一括処理時の最低レアリティを設定
        /// </summary>
        /// <param name="minRarity">最低レアリティ</param>
        public void SetMinRarityForBatch(CharacterRarity minRarity)
        {
            minRarityForBatch = minRarity;
        }

        /// <summary>
        /// 操作の有効性を検証
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            // 一括適用の場合は対象IDが空でも有効
            if (applyToAll)
            {
                return true;
            }

            // 通常の場合は対象が存在する必要がある
            if (!targetCharacterIds.Any())
            {
                return false;
            }

            // 重複IDがないか
            var uniqueIds = new HashSet<string>(targetCharacterIds);
            if (uniqueIds.Count != targetCharacterIds.Count)
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
            info += $"\n=== ロック操作設定 ===\n";
            info += $"ロック状態: {(lockState ? "ロック" : "アンロック")}\n";
            info += $"対象キャラクター数: {targetCharacterIds.Count}\n";
            info += $"お気に入り上書き: {overrideFavorites}\n";
            info += $"変更確認済み: {confirmLockChange}\n";
            info += $"全キャラクター適用: {applyToAll}\n";
            
            if (applyToAll)
            {
                info += $"一括処理最低レアリティ: {minRarityForBatch}\n";
            }

            if (targetCharacterIds.Any())
            {
                info += "\n=== 対象キャラクター ===\n";
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
            var action = lockState ? "ロック" : "アンロック";
            var count = applyToAll ? "全件" : $"{targetCharacterIds.Count}件";
            return $"{action}操作({count}) - {operationId}";
        }
    }
}