using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linqを使うために追加

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameObject characterPrefab; // キャラクターのプレハブ
    public GameObject enemyPrefab; // 敵のプレハブ

    [SerializeField] private List<EnemyData> currentStageEnemies; // 現在のステージで出現する敵のリスト

    private List<Character> playerUnits = new List<Character>();
    private List<Character> enemyUnits = new List<Character>();

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

    public void StartBattle()
    {
        Debug.Log("戦闘開始！");
        SpawnUnits();
        StartCoroutine(BattleCoroutine());
    }

    void SpawnUnits()
    {
        // プレイヤーの配置情報を取得して生成
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                CharacterData data = BoardManager.Instance.GetCharacterAt(x, y);
                if (data != null)
                {
                    GameObject charObj = Instantiate(characterPrefab, new Vector3(x, 0, y), Quaternion.identity);
                    Character character = charObj.GetComponent<Character>();
                    character.Setup(data);
                    playerUnits.Add(character);
                }
            }
        }

        // 敵ユニットの生成
        foreach (var enemyData in currentStageEnemies)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, new Vector3(Random.Range(0, 5), 0, Random.Range(0, 5)), Quaternion.identity); // 仮のランダムな位置
            Character enemy = enemyObj.GetComponent<Character>(); // Characterクラスを敵にも流用
            enemy.Setup(enemyData); // EnemyDataはCharacterDataを継承しているので問題なし
            enemyUnits.Add(enemy);
        }
    }

    IEnumerator BattleCoroutine()
    {
        Debug.Log("戦闘開始...");
        while (enemyUnits.Count > 0 && playerUnits.Count > 0)
        {
            // プレイヤーのターン
            foreach (var playerUnit in playerUnits.ToList()) // ToList()でコピーを作成し、コレクション変更時のエラーを回避
            {
                if (playerUnit == null || playerUnit.currentHp <= 0) continue; // 死亡したユニットはスキップ

                if (enemyUnits.Count > 0)
                {
                    Character target = enemyUnits[0]; // 最も近い敵などをターゲットにするロジックを後で追加
                    target.TakeDamage(playerUnit.characterData.attack);
                    if (target.currentHp <= 0)
                    {
                        enemyUnits.Remove(target);
                        Destroy(target.gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f); // 0.5秒待つ

            // 敵のターン
            foreach (var enemyUnit in enemyUnits.ToList()) // ToList()でコピーを作成
            {
                if (enemyUnit == null || enemyUnit.currentHp <= 0) continue; // 死亡したユニットはスキップ

                if (playerUnits.Count > 0)
                {
                    Character target = playerUnits[0]; // 最も近いプレイヤーなどをターゲットにするロジックを後で追加
                    target.TakeDamage(enemyUnit.characterData.attack);
                    if (target.currentHp <= 0)
                    {
                        playerUnits.Remove(target);
                        Destroy(target.gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f); // 0.5秒待つ
        }

        Debug.Log("戦闘終了！");
        // ここで戦闘結果の処理を呼び出す
    }
}
