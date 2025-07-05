using UnityEngine;

public class PreparationUI : MonoBehaviour
{
    // このメソッドをUnityエディタ上でUIボタンのOnClickイベントに登録する
    public void OnStartBattleButtonClicked()
    {
        // GameManagerのStartBattleメソッドを呼び出す
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartBattle();
        }
    }
}
