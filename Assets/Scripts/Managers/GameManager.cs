using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Preparation,
        Battle
    }

    public enum Scene
    {
        Title,
        Game,
        Result
    }

    // ゲームデータ
    public int gold;
    public List<CharacterData> playerCharacters = new List<CharacterData>();
    public GameState currentState { get; private set; }

    // 初期設定値
    [SerializeField] private int initialGold = 10;
    [SerializeField] private List<CharacterData> initialCharacters;

    // ガチャ関連
    [SerializeField] private List<CharacterData> gachaPool;
    [SerializeField] private int gachaCost = 3;
    public int gachaLevel = 1;
    [SerializeField] private int gachaUpgradeCost = 5;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRun()
    {
        gold = initialGold;
        playerCharacters.Clear();
        playerCharacters.AddRange(initialCharacters);
        currentState = GameState.Preparation;
        gachaLevel = 1; // ラン開始時にガチャレベルをリセット
        LoadScene(Scene.Game);
    }

    public void StartBattle()
    {
        if (currentState == GameState.Preparation)
        {
            currentState = GameState.Battle;
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.StartBattle();
            }
        }
    }

    public void DrawGacha()
    {
        if (gold >= gachaCost)
        {
            gold -= gachaCost;
            if (gachaPool != null && gachaPool.Any())
            {
                CharacterData drawnCharacter = gachaPool[Random.Range(0, gachaPool.Count)];
                playerCharacters.Add(drawnCharacter);
                Debug.Log(drawnCharacter.characterName + " を引きました！");
            }
        }
        else
        {
            Debug.Log("ゴールドが足りません。");
        }
    }

    // ガチャレベルをアップグレードするメソッド
    public void UpgradeGacha()
    {
        if (gold >= gachaUpgradeCost)
        {
            gold -= gachaUpgradeCost;
            gachaLevel++;
            // ここで、レベルアップによる排出率の変化やコスト低減などのロジックを将来的に追加
            Debug.Log("ガチャレベルが " + gachaLevel + " になりました！");
        }
        else
        {
            Debug.Log("ゴールドが足りません。");
        }
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
