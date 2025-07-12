using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Board
{
    /// <summary>
    /// キャラクター配置ルールの検証を行うクラス
    /// </summary>
    public class PlacementValidator : MonoBehaviour
    {
        [Header("配置ルール設定")]
        [SerializeField] private bool enableDebugLogs = true;

        private BoardStateManager boardManager;
        private List<IPlacementRule> placementRules = new List<IPlacementRule>();

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="boardManager">ボード状態管理クラス</param>
        public void Initialize(BoardStateManager boardManager)
        {
            this.boardManager = boardManager;
            InitializeDefaultRules();
        }

        /// <summary>
        /// デフォルトの配置ルールを初期化
        /// </summary>
        private void InitializeDefaultRules()
        {
            placementRules.Clear();
            
            // 基本的な配置ルール
            placementRules.Add(new BasicPlacementRule(boardManager));
            placementRules.Add(new TeamAreaRule(boardManager));
            placementRules.Add(new MaxCharacterRule(boardManager));
            placementRules.Add(new OccupancyRule(boardManager));

            if (enableDebugLogs)
                Debug.Log($"[PlacementValidator] {placementRules.Count}個の配置ルールを初期化しました");
        }

        /// <summary>
        /// 配置の有効性を検証
        /// </summary>
        /// <param name="character">配置するキャラクター</param>
        /// <param name="position">配置位置</param>
        /// <param name="isPlayerCharacter">プレイヤーキャラクターかどうか</param>
        /// <returns>検証結果</returns>
        public ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            if (boardManager == null)
            {
                return ValidationResult.CreateError("ボード管理クラスが初期化されていません");
            }

            // 全ルールを順次適用
            foreach (var rule in placementRules)
            {
                var result = rule.ValidatePlacement(character, position, isPlayerCharacter);
                if (!result.IsValid)
                {
                    if (enableDebugLogs)
                        Debug.LogWarning($"[PlacementValidator] 配置検証失敗: {result.ErrorMessage}");
                    return result;
                }
            }

            return ValidationResult.CreateSuccess("配置可能です");
        }

        /// <summary>
        /// 配置可能な位置を取得
        /// </summary>
        /// <param name="character">配置するキャラクター</param>
        /// <param name="isPlayerCharacter">プレイヤーキャラクターかどうか</param>
        /// <returns>配置可能な位置のリスト</returns>
        public List<Vector2Int> GetValidPositions(Character character, bool isPlayerCharacter)
        {
            var validPositions = new List<Vector2Int>();

            for (int x = 0; x < boardManager.BoardWidth; x++)
            {
                for (int y = 0; y < boardManager.BoardHeight; y++)
                {
                    var position = new Vector2Int(x, y);
                    var result = ValidatePlacement(character, position, isPlayerCharacter);
                    if (result.IsValid)
                    {
                        validPositions.Add(position);
                    }
                }
            }

            return validPositions;
        }

        /// <summary>
        /// カスタム配置ルールを追加
        /// </summary>
        /// <param name="rule">追加するルール</param>
        public void AddRule(IPlacementRule rule)
        {
            if (rule != null && !placementRules.Contains(rule))
            {
                placementRules.Add(rule);
                if (enableDebugLogs)
                    Debug.Log($"[PlacementValidator] カスタムルールを追加しました: {rule.GetType().Name}");
            }
        }

        /// <summary>
        /// 配置ルールを除去
        /// </summary>
        /// <param name="rule">除去するルール</param>
        public void RemoveRule(IPlacementRule rule)
        {
            if (placementRules.Remove(rule))
            {
                if (enableDebugLogs)
                    Debug.Log($"[PlacementValidator] ルールを除去しました: {rule.GetType().Name}");
            }
        }
    }

    /// <summary>
    /// 配置ルールのインターフェース
    /// </summary>
    public interface IPlacementRule
    {
        ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter);
        string GetRuleName();
    }

    /// <summary>
    /// 基本配置ルール（位置の有効性チェック）
    /// </summary>
    public class BasicPlacementRule : IPlacementRule
    {
        private readonly BoardStateManager boardManager;

        public BasicPlacementRule(BoardStateManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            if (character == null)
            {
                return ValidationResult.CreateError("キャラクターがnullです");
            }

            if (!boardManager.IsValidPosition(position))
            {
                return ValidationResult.CreateError($"無効な位置です: {position}");
            }

            return ValidationResult.CreateSuccess();
        }

        public string GetRuleName() => "基本配置ルール";
    }

    /// <summary>
    /// チームエリア制限ルール
    /// </summary>
    public class TeamAreaRule : IPlacementRule
    {
        private readonly BoardStateManager boardManager;

        public TeamAreaRule(BoardStateManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            if (isPlayerCharacter && !boardManager.IsPlayerArea(position))
            {
                return ValidationResult.CreateError("プレイヤーキャラクターは自軍エリア（Y=0～3）にのみ配置できます");
            }

            if (!isPlayerCharacter && !boardManager.IsEnemyArea(position))
            {
                return ValidationResult.CreateError("敵キャラクターは敵軍エリア（Y=4～7）にのみ配置できます");
            }

            return ValidationResult.CreateSuccess();
        }

        public string GetRuleName() => "チームエリア制限ルール";
    }

    /// <summary>
    /// 最大キャラクター数制限ルール
    /// </summary>
    public class MaxCharacterRule : IPlacementRule
    {
        private readonly BoardStateManager boardManager;

        public MaxCharacterRule(BoardStateManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            var teamCharacters = isPlayerCharacter ? boardManager.PlayerCharacters : boardManager.EnemyCharacters;
            
            // 既に配置されているキャラクターの場合は制限対象外
            if (teamCharacters.Contains(character))
            {
                return ValidationResult.CreateSuccess();
            }

            if (teamCharacters.Count >= boardManager.MaxCharactersPerTeam)
            {
                string teamName = isPlayerCharacter ? "プレイヤー" : "敵";
                return ValidationResult.CreateError($"{teamName}チームの最大配置数（{boardManager.MaxCharactersPerTeam}体）に達しています");
            }

            return ValidationResult.CreateSuccess();
        }

        public string GetRuleName() => "最大キャラクター数制限ルール";
    }

    /// <summary>
    /// 占有状況チェックルール
    /// </summary>
    public class OccupancyRule : IPlacementRule
    {
        private readonly BoardStateManager boardManager;

        public OccupancyRule(BoardStateManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public ValidationResult ValidatePlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            var occupyingCharacter = boardManager.GetCharacterAt(position);
            
            // 何も配置されていない場合はOK
            if (occupyingCharacter == null)
            {
                return ValidationResult.CreateSuccess();
            }

            // 同じキャラクターの場合（移動）はOK
            if (occupyingCharacter == character)
            {
                return ValidationResult.CreateSuccess();
            }

            // 別のキャラクターが占有している場合はNG
            return ValidationResult.CreateError($"位置 {position} は既に {occupyingCharacter.CharacterData.CharacterName} によって占有されています");
        }

        public string GetRuleName() => "占有状況チェックルール";
    }

    /// <summary>
    /// 検証結果クラス
    /// </summary>
    [System.Serializable]
    public class ValidationResult
    {
        [SerializeField] private bool isValid;
        [SerializeField] private string errorMessage;
        [SerializeField] private string successMessage;

        public bool IsValid => isValid;
        public string ErrorMessage => errorMessage;
        public string SuccessMessage => successMessage;
        public string Message => isValid ? successMessage : errorMessage;

        private ValidationResult(bool isValid, string message, bool isError = true)
        {
            this.isValid = isValid;
            if (isError)
                this.errorMessage = message;
            else
                this.successMessage = message;
        }

        public static ValidationResult CreateSuccess(string message = "成功")
        {
            return new ValidationResult(true, message, false);
        }

        public static ValidationResult CreateError(string message)
        {
            return new ValidationResult(false, message, true);
        }

        public override string ToString()
        {
            return $"ValidationResult(Valid: {isValid}, Message: {Message})";
        }
    }
}