using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクター売却操作
    /// 複数キャラクターの一括売却に対応
    /// </summary>
    [Serializable]
    public class SellOperation : CharacterOperation
    {
        [Header("売却対象")]
        [SerializeField] private List<string> targetCharacterIds = new List<string>();

        [Header("売却設定")]
        [SerializeField] private bool confirmSell = false;
        [SerializeField] private bool sellLocked = false;
        [SerializeField] private bool sellFavorites = false;

        [Header("価格設定")]
        [SerializeField] private float priceMultiplier = 1.0f;
        [SerializeField] private int minimumPrice = 1;

        // プロパティ
        public List<string> TargetCharacterIds => new List<string>(targetCharacterIds);
        public bool ConfirmSell => confirmSell;
        public bool SellLocked => sellLocked;
        public bool SellFavorites => sellFavorites;
        public float PriceMultiplier => priceMultiplier;
        public int MinimumPrice => minimumPrice;

        /// <summary>
        /// 売却操作のコンストラクタ
        /// </summary>
        /// <param name="characterIds">売却対象キャラクターID</param>
        /// <param name="requester">リクエスター名</param>
        public SellOperation(List<string> characterIds, string requester = "system") : base(requester)
        {
            targetCharacterIds = new List<string>(characterIds ?? new List<string>());
        }

        /// <summary>
        /// 単一キャラクター売却のコンストラクタ
        /// </summary>
        /// <param name="characterId">売却対象キャラクターID</param>
        /// <param name="requester">リクエスター名</param>
        public SellOperation(string characterId, string requester = "system") : base(requester)
        {
            if (!string.IsNullOrEmpty(characterId))
            {
                targetCharacterIds.Add(characterId);
            }
        }

        /// <summary>
        /// 売却対象キャラクターを追加
        /// </summary>
        /// <param name="characterId">追加するキャラクターID</param>
        public void AddTargetCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || targetCharacterIds.Contains(characterId))
            {
                return;
            }

            targetCharacterIds.Add(characterId);
        }

        /// <summary>
        /// 売却対象キャラクターを削除
        /// </summary>
        /// <param name="characterId">削除するキャラクターID</param>
        public void RemoveTargetCharacter(string characterId)
        {
            targetCharacterIds.Remove(characterId);
        }

        /// <summary>
        /// 売却確認フラグを設定
        /// </summary>
        /// <param name="confirm">確認済みの場合true</param>
        public void SetConfirmSell(bool confirm)
        {
            confirmSell = confirm;
        }

        /// <summary>
        /// ロック済みキャラクター売却許可を設定
        /// </summary>
        /// <param name="allowLocked">許可する場合true</param>
        public void SetSellLocked(bool allowLocked)
        {
            sellLocked = allowLocked;
        }

        /// <summary>
        /// お気に入りキャラクター売却許可を設定
        /// </summary>
        /// <param name="allowFavorites">許可する場合true</param>
        public void SetSellFavorites(bool allowFavorites)
        {
            sellFavorites = allowFavorites;
        }

        /// <summary>
        /// 価格倍率を設定
        /// </summary>
        /// <param name="multiplier">価格倍率</param>
        public void SetPriceMultiplier(float multiplier)
        {
            priceMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// 最低価格を設定
        /// </summary>
        /// <param name="minPrice">最低価格</param>
        public void SetMinimumPrice(int minPrice)
        {
            minimumPrice = Mathf.Max(0, minPrice);
        }

        /// <summary>
        /// 操作の有効性を検証
        /// </summary>
        /// <returns>有効な場合true</returns>
        public override bool IsValid()
        {
            // 売却対象が存在するか
            if (!targetCharacterIds.Any())
            {
                return false;
            }

            // 価格設定が有効か
            if (priceMultiplier < 0 || minimumPrice < 0)
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
            info += $"\n=== 売却設定 ===\n";
            info += $"対象キャラクター数: {targetCharacterIds.Count}\n";
            info += $"売却確認済み: {confirmSell}\n";
            info += $"ロック済み売却許可: {sellLocked}\n";
            info += $"お気に入り売却許可: {sellFavorites}\n";
            info += $"価格倍率: {priceMultiplier:F2}\n";
            info += $"最低価格: {minimumPrice}\n";

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
            return $"売却操作({targetCharacterIds.Count}件) - {operationId}";
        }
    }
}