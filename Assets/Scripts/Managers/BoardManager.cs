using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    private const int BOARD_SIZE = 5;
    public CharacterData[,] board = new CharacterData[BOARD_SIZE, BOARD_SIZE];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 指定した位置にキャラクターを配置する
    public void PlaceCharacter(CharacterData character, int x, int y)
    {
        if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
        {
            Debug.LogError("無効なボードの位置です。");
            return;
        }
        board[x, y] = character;
    }

    // 指定した位置のキャラクターを取得する
    public CharacterData GetCharacterAt(int x, int y)
    {
        if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
        {
            return null;
        }
        return board[x, y];
    }
}
