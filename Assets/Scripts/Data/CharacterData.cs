using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "GatchaSpire/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public int hp;
    public int attack;
    // 今後、スキルや種族などのデータを追加
}
