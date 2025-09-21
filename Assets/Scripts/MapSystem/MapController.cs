using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeConfiguration
{
    public Vector2Int gridPosition;
    public NodeType nodeType;
}

public class MapController : Singleton<MapController>
{
    [Header("地图尺寸")]
    public int mapWidth = 7;
    public int mapHeight = 7;
    public List<NodeConfiguration> initialLayout;

    [Header("节点预制体")]
    public GameObject nodePrefab;

    private DisasterSystem disasterSystem;
    private Node[,] grid;
    private NodeView[,] nodeViewGrid;
    private Node currentPlayerNode;
    private int turnCycle = 0;

    protected override void Awake()
    {
        base.Awake();

        GenerateMapData();
        // PrintMap();

        disasterSystem = GetComponent<DisasterSystem>();
        if (disasterSystem == null)
        {
            Debug.LogError($"[MapController] DisasterSystem not found");
            return;
        }
    }

    private void Start()
    {
        // 玩家初始节点位置
        currentPlayerNode = GetNodeAt(5, 0);
        ProcessTurnCycle();
    }

    #region --------- 游戏核心流程 ---------

    public void PlayerDidMove(Node newNode)
    {
        currentPlayerNode = newNode;
        Debug.Log($"Player moved to the new node: ({currentPlayerNode.gridPosition.x}, {currentPlayerNode.gridPosition.y})");

        ProcessTurnCycle();
    }

    private void ProcessTurnCycle()
    {
        ProcessNodeContent(currentPlayerNode);

        if (turnCycle == 2)
        {
            bool isPlayerSafe = IsPlayerInSafeZone(currentPlayerNode);
            if (!isPlayerSafe)
            {
                Debug.LogWarning($"Player is not safe, -30% stamina");
            }
            else
            {
                Debug.Log($"Player is safe");
            }

            turnCycle = 0;
        }

        if (turnCycle == 0)
        {
            Debug.Log("--- A New Turn Cycle Started ---");
            RefreshWorldState(currentPlayerNode);
            turnCycle = 1;
        }
        else
        {
            turnCycle = 2;
        }
    }

    // 根据当前点位类型判断要进行的逻辑
    private void ProcessNodeContent(Node node)
    {
        switch (node.nodeType)
        {
            case NodeType.Embassy:
                // 胜利的逻辑
                break;
        }

        if (node.isStore)
        {
            // 到达商店后的逻辑
            node.isStore = false;
        }
        else if (node.isSafeZone)
        {
            // 到达躲避点后的逻辑
            node.isSafeZone = false;
        }
    }

    private void RefreshWorldState(Node playerNode)
    {
        Debug.Log($"[MapController] Refreshing world state...");

        ClearDynamicObjects();

        disasterSystem.TriggerNewDisaster(playerNode);
        SpawnShop(playerNode);
        SpawnShelter(playerNode);
    }
    #endregion

    private bool IsPlayerInSafeZone(Node playerNode)
    {
        DisasterSO currentDisaster = disasterSystem.ActiveDisaster;
        if (currentDisaster == null) return true;

        switch (currentDisaster.safeCondition)
        {
            case SafeZoneCondition.InNode:
                return playerNode.nodeType == currentDisaster.requiredNodeType;
            case SafeZoneCondition.AwayFromNode:
                return playerNode.nodeType != currentDisaster.awayFromNodeType;
                // return !IsNodeAdjacentToType(playerNode, currentDisaster.awayFromNodeType);
            case SafeZoneCondition.InShelter:
                return playerNode.isSafeZone;
        }
        return false;
    }

    // 以玩家位置为中心, 在前后左右节点位置 随机挑选一个生成商店
    private void SpawnShop(Node playerNode)
    {
        Vector2Int playerPos = playerNode.gridPosition;
        Vector2Int[] adjacents = {                                  
            new Vector2Int(playerPos.x - 1, playerPos.y),
            new Vector2Int(playerPos.x + 1, playerPos.y),
            new Vector2Int(playerPos.x, playerPos.y - 1),
            new Vector2Int(playerPos.x, playerPos.y + 1)
        };

        var validNodes = new List<Node>();
        foreach (var adjacentPos in adjacents)
        {
            Node node = GetNodeAt(adjacentPos.x, adjacentPos.y);
            if (node != null)
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count > 0)
        {
            Node shopNode = validNodes[Random.Range(0, validNodes.Count)];
            shopNode.isStore = true;
            nodeViewGrid[shopNode.gridPosition.x, shopNode.gridPosition.y].UpdateVisuals();
            Debug.Log($"[MapController] Spawned shop at ({shopNode.gridPosition.x}, {shopNode.gridPosition.y})");
        }
    }

    // 以玩家位置为中心,在三个角上(除掉右下角)的节点位置 随机挑选一个生成躲避点
    private void SpawnShelter(Node playerNode)
    {
        Vector2Int playerPos = playerNode.gridPosition;
        Vector2Int[] vertices = {                                  // 排除掉右下角的节点
            new Vector2Int(playerPos.x - 1, playerPos.y + 1),
            new Vector2Int(playerPos.x + 1, playerPos.y + 1),
            new Vector2Int(playerPos.x - 1, playerPos.y - 1)
        };

        var validNodes = new List<Node>();
        foreach (var verticePos in vertices)
        {
            Node node = GetNodeAt(verticePos.x, verticePos.y);
            if (node != null)
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count > 0)
        {
            Node shelterNode = validNodes[Random.Range(0, validNodes.Count)];
            shelterNode.isSafeZone = true;
            nodeViewGrid[shelterNode.gridPosition.x, shelterNode.gridPosition.y].UpdateVisuals();
            Debug.Log($"[MapController] Spawned shelter at ({shelterNode.gridPosition.x}, {shelterNode.gridPosition.y})");
        }
    }

    // // 检查玩家周围有没有特定类型的节点
    // public bool IsNodeAdjacentToType(Node node, NodeType typeToCheck)
    // {
    //     List<Node> neighbours = GetAdjacentNodes(node);
    //     foreach (var neighbour in neighbours)
    //     {
    //         if (neighbour.nodeType == typeToCheck)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    // // 获取指定节点的相邻节点
    // private List<Node> GetAdjacentNodes(Node node)
    // {
    //     List<Node> adjacentNodes = new List<Node>();
    //     if (node == null) return adjacentNodes;

    //     Vector2Int playerPos = node.gridPosition;
    //     Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    //     foreach (var dir in directions)
    //     {
    //         Node adjacentNode = GetNodeAt(playerPos.x + dir.x, playerPos.y + dir.y);
    //         if (adjacentNode != null)
    //         {
    //             adjacentNodes.Add(adjacentNode);
    //         }
    //     }
    //     return adjacentNodes;
    // }

    private void ClearDynamicObjects()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                GetNodeAt(x, y)?.ClearDynamicStates();
            }
        }
    }

    // 生成节点地图数据
    private void GenerateMapData()
    {
        grid = new Node[mapWidth, mapHeight];
        nodeViewGrid = new NodeView[mapWidth, mapHeight];

        Dictionary<Vector2Int, NodeType> layoutDict = new Dictionary<Vector2Int, NodeType>();
        foreach (var config in initialLayout)
        {
            if (!layoutDict.ContainsKey(config.gridPosition))
            {
                layoutDict[config.gridPosition] = config.nodeType;
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                NodeType nodeType = NodeType.OpenSpace;

                if (layoutDict.ContainsKey(position))
                {
                    nodeType = layoutDict[position];
                }

                grid[x, y] = new Node(x, y, nodeType);
                if (nodePrefab != null)
                {
                    GameObject nodeObject = Instantiate(nodePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                    nodeObject.name = $"Node ({x}, {y})";

                    NodeView nodeView = nodeObject.GetComponent<NodeView>();
                    if (nodeView != null)
                    {
                        nodeView.Initialize(grid[x, y]);
                        nodeViewGrid[x, y] = nodeView;
                    }
                    else
                    {
                        Debug.LogError($"[MapController] NodeView component not found in {nodeObject.name}.");
                    }
                }
            }
        }

        Debug.Log("[MapController] Map generated successfully!");
    }

    // 获取指定坐标节点数据
    public Node GetNodeAt(int x, int y)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            return grid[x, y];
        return null;
    }

    // // Test
    // public void PrintMap()
    // {
    //     var mapBuilder = new System.Text.StringBuilder();

    //     mapBuilder.AppendLine("\n--- 7x7 Map NodeType Layout --- ");
    //     for (int y = mapHeight - 1; y >= 0; y--)
    //     {
    //         for (int x = 0; x < mapWidth; x++)
    //         {
    //             Node node = GetNodeAt(x, y);
    //             if (node != null)
    //             {
    //                 string nodeTypeStr = node.nodeType.ToString().PadRight(10);
    //                 mapBuilder.Append(nodeTypeStr);
    //                 mapBuilder.Append(" | ");
    //             }
    //         }
    //         mapBuilder.AppendLine();
    //     }
    //     Debug.Log(mapBuilder.ToString());
    // }
}
