using UnityEngine;
using System.Collections.Generic;
using System;
using GatchaSpire.Core.Systems;
using GatchaSpire.Core.Character;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Gameplay.Board
{
    /// <summary>
    /// 7x8戦闘フィールドの状態管理クラス
    /// TFT方式UI表示対応（配置時7x4、戦闘時7x8）
    /// </summary>
    [DefaultExecutionOrder(-50)] // UnityGameSystemCoordinatorの後に実行
    public class BoardStateManager : GameSystemBase
    {
        [Header("ボード設定")]
        [SerializeField] private int boardWidth = 7;
        [SerializeField] private int boardHeight = 8;
        [SerializeField] private int playerAreaHeight = 4;
        [SerializeField] private int maxCharactersPerTeam = 8;

        // ボード状態
        private Character[,] board;
        private HashSet<Vector2Int> playerArea;
        private HashSet<Vector2Int> enemyArea;
        private List<Character> playerCharacters = new List<Character>();
        private List<Character> enemyCharacters = new List<Character>();

        // イベント
        public event Action<Character, Vector2Int, Vector2Int> OnCharacterMoved;
        public event Action<Character, Vector2Int> OnCharacterPlaced;
        public event Action<Character, Vector2Int> OnCharacterRemoved;
        public event Action OnBoardStateChanged;

        // スキル・シナジーシステムインターフェース（後から実装）
        private ISkillSystem skillSystem;
        private ISynergySystem synergySystem;

        protected override string SystemName => "BoardStateManager";

        public int BoardWidth => boardWidth;
        public int BoardHeight => boardHeight;
        public int PlayerAreaHeight => playerAreaHeight;
        public int MaxCharactersPerTeam => maxCharactersPerTeam;
        public Character[,] Board => board;
        public List<Character> PlayerCharacters => new List<Character>(playerCharacters);
        public List<Character> EnemyCharacters => new List<Character>(enemyCharacters);

        /// <summary>
        /// Unity Awake - GameSystemBaseの自動登録処理を呼び出し
        /// </summary>
        private void Awake()
        {
            OnAwake();
        }

        protected override void OnSystemInitialize()
        {
            InitializeBoard();
            InitializeAreas();
            
            // 将来実装されるシステムの参照を取得（nullチェック付き）
            skillSystem = GetComponent<ISkillSystem>();
            synergySystem = GetComponent<ISynergySystem>();

            ReportInfo("ボードシステムを初期化しました");
        }

        /// <summary>
        /// ボードを初期化
        /// </summary>
        private void InitializeBoard()
        {
            board = new Character[boardWidth, boardHeight];
            playerCharacters.Clear();
            enemyCharacters.Clear();
        }

        /// <summary>
        /// プレイヤーエリアと敵エリアを定義
        /// </summary>
        private void InitializeAreas()
        {
            playerArea = new HashSet<Vector2Int>();
            enemyArea = new HashSet<Vector2Int>();

            for (int x = 0; x < boardWidth; x++)
            {
                // プレイヤーエリア（Y=0～3）
                for (int y = 0; y < playerAreaHeight; y++)
                {
                    playerArea.Add(new Vector2Int(x, y));
                }

                // 敵エリア（Y=4～7）
                for (int y = playerAreaHeight; y < boardHeight; y++)
                {
                    enemyArea.Add(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// 指定位置にキャラクターを配置
        /// </summary>
        /// <param name="character">配置するキャラクター</param>
        /// <param name="position">配置位置</param>
        /// <param name="isPlayerCharacter">プレイヤーキャラクターかどうか</param>
        /// <returns>配置に成功したかどうか</returns>
        public bool PlaceCharacter(Character character, Vector2Int position, bool isPlayerCharacter = true)
        {
            try
            {
                if (!IsValidPlacement(character, position, isPlayerCharacter))
                    return false;

                // 既存位置からの移動の場合、元の位置をクリア
                RemoveCharacterFromBoard(character);

                // 新しい位置に配置
                board[position.x, position.y] = character;

                // キャラクターリストを更新
                if (isPlayerCharacter)
                {
                    if (!playerCharacters.Contains(character))
                        playerCharacters.Add(character);
                }
                else
                {
                    if (!enemyCharacters.Contains(character))
                        enemyCharacters.Add(character);
                }

                // シナジー再計算（システムが利用可能な場合）
                synergySystem?.RecalculateSynergies(isPlayerCharacter ? playerCharacters : enemyCharacters);

                OnCharacterPlaced?.Invoke(character, position);
                OnBoardStateChanged?.Invoke();

                ReportInfo($"キャラクター {character.CharacterData.CharacterName} を位置 {position} に配置しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター配置エラー: {e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// キャラクターを移動
        /// </summary>
        /// <param name="character">移動するキャラクター</param>
        /// <param name="newPosition">新しい位置</param>
        /// <returns>移動に成功したかどうか</returns>
        public bool MoveCharacter(Character character, Vector2Int newPosition)
        {
            try
            {
                Vector2Int oldPosition = GetCharacterPosition(character);
                if (oldPosition == new Vector2Int(-1, -1))
                {
                    ReportWarning("移動対象のキャラクターがボード上に見つかりません");
                    return false;
                }

                bool isPlayerCharacter = playerCharacters.Contains(character);
                if (!IsValidPlacement(character, newPosition, isPlayerCharacter))
                    return false;

                // 移動実行
                board[oldPosition.x, oldPosition.y] = null;
                board[newPosition.x, newPosition.y] = character;

                // シナジー再計算
                synergySystem?.RecalculateSynergies(isPlayerCharacter ? playerCharacters : enemyCharacters);

                OnCharacterMoved?.Invoke(character, oldPosition, newPosition);
                OnBoardStateChanged?.Invoke();

                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター移動エラー: {e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// キャラクターをボードから除去
        /// </summary>
        /// <param name="character">除去するキャラクター</param>
        /// <returns>除去に成功したかどうか</returns>
        public bool RemoveCharacter(Character character)
        {
            try
            {
                Vector2Int position = GetCharacterPosition(character);
                if (position == new Vector2Int(-1, -1))
                    return false;

                board[position.x, position.y] = null;
                
                bool isPlayerCharacter = playerCharacters.Contains(character);
                if (isPlayerCharacter)
                    playerCharacters.Remove(character);
                else
                    enemyCharacters.Remove(character);

                // シナジー再計算
                synergySystem?.RecalculateSynergies(isPlayerCharacter ? playerCharacters : enemyCharacters);

                OnCharacterRemoved?.Invoke(character, position);
                OnBoardStateChanged?.Invoke();

                ReportInfo($"キャラクター {character.CharacterData.CharacterName} を除去しました");
                return true;
            }
            catch (Exception e)
            {
                ReportError($"キャラクター除去エラー: {e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// 指定位置のキャラクターを取得
        /// </summary>
        /// <param name="position">取得位置</param>
        /// <returns>キャラクター（存在しない場合はnull）</returns>
        public Character GetCharacterAt(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return null;

            return board[position.x, position.y];
        }

        /// <summary>
        /// キャラクターの現在位置を取得
        /// </summary>
        /// <param name="character">検索するキャラクター</param>
        /// <returns>位置（見つからない場合は(-1, -1)）</returns>
        public Vector2Int GetCharacterPosition(Character character)
        {
            if (character == null)
            {
                return new Vector2Int(-1, -1);
            }


            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board[x, y] == character)
                    {
                        return new Vector2Int(x, y);
                    }
                        
                }
            }

            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// 配置検証
        /// </summary>
        /// <param name="character">配置するキャラクター</param>
        /// <param name="position">配置位置</param>
        /// <param name="isPlayerCharacter">プレイヤーキャラクターかどうか</param>
        /// <returns>配置可能かどうか</returns>
        public bool IsValidPlacement(Character character, Vector2Int position, bool isPlayerCharacter)
        {
            if (character == null)
            {
                return false;
            }
                

            if (!IsValidPosition(position))
            {
                return false;
            }
                

            // エリア制限チェック
            if (isPlayerCharacter && !playerArea.Contains(position))
            {
                return false;
            }
                

            if (!isPlayerCharacter && !enemyArea.Contains(position))
            {
                return false;
            }


            // 占有状況チェック
            if (board[position.x, position.y] != null && board[position.x, position.y] != character)
            {
                return false;
            }

            // チーム最大数チェック
            var teamCharacters = isPlayerCharacter ? playerCharacters : enemyCharacters;
            if (!teamCharacters.Contains(character) && teamCharacters.Count >= maxCharactersPerTeam)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 位置の有効性チェック
        /// </summary>
        /// <param name="position">チェックする位置</param>
        /// <returns>有効な位置かどうか</returns>
        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < boardWidth &&
                   position.y >= 0 && position.y < boardHeight;
        }

        /// <summary>
        /// プレイヤーエリアかどうかチェック
        /// </summary>
        /// <param name="position">チェックする位置</param>
        /// <returns>プレイヤーエリアかどうか</returns>
        public bool IsPlayerArea(Vector2Int position)
        {
            return playerArea.Contains(position);
        }

        /// <summary>
        /// 敵エリアかどうかチェック
        /// </summary>
        /// <param name="position">チェックする位置</param>
        /// <returns>敵エリアかどうか</returns>
        public bool IsEnemyArea(Vector2Int position)
        {
            return enemyArea.Contains(position);
        }

        /// <summary>
        /// ボードをクリア
        /// </summary>
        public void ClearBoard()
        {
            try
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    for (int y = 0; y < boardHeight; y++)
                    {
                        board[x, y] = null;
                    }
                }

                playerCharacters.Clear();
                enemyCharacters.Clear();

                OnBoardStateChanged?.Invoke();
                ReportInfo("ボードをクリアしました");
            }
            catch (Exception e)
            {
                ReportError($"ボードクリアエラー: {e.Message}", e);
            }
        }

        /// <summary>
        /// ボードからキャラクターを検索して除去（内部処理用）
        /// </summary>
        /// <param name="character">除去するキャラクター</param>
        private void RemoveCharacterFromBoard(Character character)
        {
            Vector2Int position = GetCharacterPosition(character);
            if (position != new Vector2Int(-1, -1))
            {
                board[position.x, position.y] = null;
            }
        }

        /// <summary>
        /// デバッグ用ボード状態表示
        /// </summary>
        public void DebugPrintBoard()
        {
            if (!enableDebugLogs)
            {
                return;
            }
               

            string boardState = "=== ボード状態 ===\n";
            for (int y = boardHeight - 1; y >= 0; y--)
            {
                string row = $"Y{y}: ";
                for (int x = 0; x < boardWidth; x++)
                {
                    Character character = board[x, y];
                    if (character != null)
                    {
                        row += $"[{character.CharacterData.CharacterName.Substring(0, 1)}] ";
                    }
                    else
                    {
                        row += "[ ] ";
                    }
                }
                boardState += row + "\n";
            }
            boardState += $"プレイヤー: {playerCharacters.Count}/{maxCharactersPerTeam}\n";
            boardState += $"敵: {enemyCharacters.Count}/{maxCharactersPerTeam}";

            Debug.Log(boardState);
        }

        protected override void OnSystemReset()
        {
            ClearBoard();
        }
    }

    /// <summary>
    /// スキルシステムインターフェース（将来実装）
    /// </summary>
    public interface ISkillSystem
    {
        void UpdateSkillCooldowns(float deltaTime);
        bool CanUseSkill(Character character, string skillId);
        void UseSkill(Character character, string skillId, Vector2Int targetPosition);
    }

    /// <summary>
    /// シナジーシステムインターフェース（将来実装）
    /// </summary>
    public interface ISynergySystem
    {
        void RecalculateSynergies(List<Character> characters);
        void ApplySynergyEffects(Character character);
        void RemoveSynergyEffects(Character character);
    }
}