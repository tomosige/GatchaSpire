using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public void OnStartGameButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRun();
        }
        else
        {
            Debug.LogError("GameManagerが見つかりません。");
        }
    }
}
