using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Drop2DSpirite : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //场景中的canvas
    public Canvas canv;

    RectTransform dragObjRect = null;
    //拖拽的ui
    public RectTransform dragUI;
    //是否能拖拽
    public bool canDrag = false;

    void Start()
    {
        dragObjRect = canv.transform as RectTransform;

    }
    ////开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter.tag == "item")
        {
            canDrag = true;
            dragUI = eventData.pointerEnter.GetComponent<RectTransform>();
            Debug.Log("获取到拖拽对象：" + dragUI); // 若输出为null，说明获取失败
        }
        else
        {
            canDrag = false;
        }
    }
    //拖拽中
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle
            (dragObjRect, eventData.position, eventData.pressEventCamera, out globalMousePos))
        {

            dragUI.position = globalMousePos;
            dragUI.rotation = dragObjRect.rotation;
            CheckPos();

        }
    }
    //拖拽结束
    public void OnEndDrag(PointerEventData eventData)
    {
        canDrag = false;
        dragUI = null;
    }

    //检查位置,如果拖拽超出指定区域，则重新赋值
    void CheckPos()
    {
        Vector2 pos = dragUI.anchoredPosition;
        if (pos.x <= -150)
        {
            pos.x = -150;
        }
        else if (pos.x >= 150)
        {
            pos.x = 150;
        }

        else if (pos.y >= 40)
        {
            pos.y = 40;
        }

        else if (pos.y <= -40)
        {
            pos.y = -40;
        }
        dragUI.anchoredPosition = pos;
    }

}




