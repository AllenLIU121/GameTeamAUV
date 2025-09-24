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

    [Header("节点预制体和父物体")]
    public GameObject nodePrefab;
    [SerializeField] private GameObject parent;

    private DisasterSystem disasterSystem;
    private Node[,] grid;
    private NodeView[,] nodeViewGrid;
    private Node currentPlayerNode;
    private int turnCycle = 0;

    public bool IsInitialized { get; private set; } = false;

    public Node GetPlayerNode() => currentPlayerNode;
    private Node currentShopNode;
    private Node currentShelterNode;

    private PlayerController playerController;
    private bool isMoving = false;
    public bool IsMoving => isMoving;
    private WaitForSeconds waitForOneSecond;

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
        InitializePlayerPos();
        waitForOneSecond = new WaitForSeconds(1f);

        ProcessTurnCycle();
        IsInitialized = true;
    }

    private void InitializePlayerPos()
    {
        // 玩家初始节点位置
        currentPlayerNode = GetNodeAt(7, 0);

        if (PlayerController.Instance != null)
        {
            playerController = PlayerController.Instance;
            Vector3 initalPos = new Vector3(currentPlayerNode.gridPosition.x, currentPlayerNode.gridPosition.y, 0);
            playerController.transform.position = parent.transform.TransformPoint(initalPos);
        }
    }

    #region --------- 游戏核心流程 ---------

    public IEnumerator PlayerDidMove(Node newNode)
    {
        if (isMoving) yield break;
        isMoving = true;

        Vector3 targetPosition = new Vector3(newNode.gridPosition.x, newNode.gridPosition.y, 0);
        Vector3 worldPosition = parent.transform.TransformPoint(targetPosition);
        playerController.MoveTo(worldPosition);
        yield return waitForOneSecond;

        currentPlayerNode = newNode;
        Debug.Log($"Player moved to the new node: ({currentPlayerNode.gridPosition.x}, {currentPlayerNode.gridPosition.y})");

        ProcessTurnCycle();
        isMoving = false;
    }

    private void ProcessTurnCycle()
    {
        ProcessNodeContent(currentPlayerNode);

        if (turnCycle == 2)
        {
            bool isPlayerSafe = IsPlayerInSafeZone(currentPlayerNode);
            if (!isPlayerSafe)
            {
                GameStateManager.Instance.Character.ReduceAllAliveCharactersStamina();
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
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.IsInSelectingMode = false;
                    PlayerController.Instance.enabled = false;
                }

                SceneController.Instance.LoadSceneAsync(GameConstants.SceneName.FinalScene);
                break;
        }

        if (node.isStore)
        {
            // 到达商店后的逻辑
            node.isStore = false;
            nodeViewGrid[node.gridPosition.x, node.gridPosition.y].UpdateVisuals();
        }
        else if (node.isSafeZone)
        {
            // 到达躲避点后的逻辑
            node.isSafeZone = false;
            nodeViewGrid[node.gridPosition.x, node.gridPosition.y].UpdateVisuals();
        }
    }

    private void RefreshWorldState(Node playerNode)
    {
        Debug.Log($"[MapController] Refreshing world state...");

        // 清理动态修改的节点数据
        if (currentShopNode != null)
        {
            currentShopNode.isStore = false;
            nodeViewGrid[currentShopNode.gridPosition.x, currentShopNode.gridPosition.y].UpdateVisuals();
        }
        if (currentShelterNode != null)
        {
            currentShelterNode.isSafeZone = false;
            nodeViewGrid[currentShelterNode.gridPosition.x, currentShelterNode.gridPosition.y].UpdateVisuals();
        }

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
            currentShopNode = shopNode;
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
            currentShelterNode = shelterNode;
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

    // 存储地图数据
    public List<NodeRuntimeData> GetMapDataToSave()
    {
        var mapState = new List<NodeRuntimeData>();
        if (grid == null) return mapState;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Node node = grid[x, y];
                if (node != null)
                {
                    var nodeData = new NodeRuntimeData
                    {
                        gridPosition = node.gridPosition,
                        nodeType = node.nodeType,
                        isStore = node.isStore,
                        isSafeZone = node.isSafeZone
                    };
                    mapState.Add(nodeData);
                }
            }
        }
        return mapState;
    }

    // 恢复地图数据
    public void RestoreMapData(List<NodeRuntimeData> savedNodes)
    {
        if (grid == null || nodeViewGrid == null)
        {
            Debug.LogError($"[MapController] Grid or nodeViewGrid is null]");
        }

        foreach (var savedNode in savedNodes)
        {
            int x = savedNode.gridPosition.x;
            int y = savedNode.gridPosition.y;

            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                Node node = grid[x, y];
                node.nodeType = savedNode.nodeType;
                node.isStore = savedNode.isStore;
                node.isSafeZone = savedNode.isSafeZone;

                nodeViewGrid[x, y].UpdateVisuals();
            }
        }
        Debug.Log("[MapController] Map restored.");

        InitializePlayerPos();

        turnCycle = 0;
        ProcessTurnCycle();
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
                    GameObject nodeObject = Instantiate(nodePrefab, new Vector3(x, y, 0), Quaternion.identity, parent.transform);
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

        parent.transform.localScale = new Vector3(0.495f, 0.495f, 0.495f);
        parent.transform.position = new Vector3(1.24f, -1.03f, 0f);
        Debug.Log("[MapController] Map generated successfully!");
    }

    // 获取指定坐标节点数据
    public Node GetNodeAt(int x, int y)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            return grid[x, y];
        return null;
    }

    public bool IsAdjacent(Node node1, Node node2)
    {
        if (node1 == null || node2 == null) return false;

        int distance = Mathf.Abs(node1.gridPosition.x - node2.gridPosition.x) + Mathf.Abs(node1.gridPosition.y - node2.gridPosition.y);
        return distance == 1;
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
