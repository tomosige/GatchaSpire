using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData characterData;
    public int currentHp;

    public void Setup(CharacterData data)
    {
        characterData = data;
        currentHp = characterData.hp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            // 死亡処理
            Debug.Log(characterData.characterName + " は倒れた。");
        }
    }
}
