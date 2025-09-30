using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    private Node nodeData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 5;
    }

    public void Initialize(Node nodeData)
    {
        this.nodeData = nodeData;
        UpdateVisuals();
    }

    // 让外部获取节点数据
    public Node GetNodeData() => nodeData;

    // 更新视觉变化
    public void UpdateVisuals()
    {
        if (spriteRenderer == null || nodeData == null) return;

        switch (nodeData.nodeType)
        {
        // case NodeType.Embassy:
        //     spriteRenderer.sprite = Color.gray;
        //     break;
        //     case NodeType.Mountain:
        //         spriteRenderer.color = Color.green;
        //         break;
        //     case NodeType.Sea:
        //         spriteRenderer.color = Color.blue;
        //         break;
        //     case NodeType.OpenSpace:
        //         spriteRenderer.color = Color.white;
        //         break;
        }

        if (nodeData.isStore)
        {
            spriteRenderer.sprite = MapController.Instance.storeIcon;
        }
        else if (nodeData.isSafeZone)
        {
            spriteRenderer.sprite = MapController.Instance.shelterIcon;
        }
        else
        {
            spriteRenderer.sprite = null;
        }
    }
}
