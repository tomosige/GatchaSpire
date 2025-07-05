using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("ドラッグ開始");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグ中はマウスカーソルに追従させる
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("ドラッグ終了");
        // ここで、ドロップされた位置に応じてBoardManagerにキャラクターを配置する処理を呼び出す
    }
}
