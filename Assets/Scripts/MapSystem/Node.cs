using UnityEngine;

public enum NodeType
{
    OpenSpace,
    Mountain,
    Sea,
    Embassy,
}

public class Node
{
    public Vector2Int gridPosition;
    public NodeType nodeType;

    // 动态状态
    public bool isStore;
    public bool isSafeZone;
    public bool isDisasterZone;

    // 场景中节点GameObject
    public GameObject visualObject;

    public Node(int x, int y, NodeType type)
    {
        gridPosition = new Vector2Int(x, y);
        nodeType = type;

        ClearDynamicStates();
    }

    public void ClearDynamicStates()
    {
        isStore = false;
        isSafeZone = false;
        isDisasterZone = false;
    }
}
