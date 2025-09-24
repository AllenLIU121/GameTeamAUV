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
            case NodeType.Embassy:
                spriteRenderer.color = Color.yellow;
                break;
            case NodeType.Mountain:
                spriteRenderer.color = Color.green;
                break;
            case NodeType.Sea:
                spriteRenderer.color = Color.blue;
                break;
            case NodeType.OpenSpace:
                spriteRenderer.color = Color.white;
                break;
        }

        if (nodeData.isStore)
        {
            spriteRenderer.color = Color.red;
        }
        else if (nodeData.isSafeZone)
        {
            spriteRenderer.color = Color.cyan;
        }
    }
}
