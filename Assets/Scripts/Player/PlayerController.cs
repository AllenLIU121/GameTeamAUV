using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    private Camera mainCamera;
    public bool IsInSelectingMode;

    public float moveSpeed = 4f;
    private MapController mapController;
    private Coroutine moveCoroutine;


    void Start()
    {
        mainCamera = Camera.main;
        IsInSelectingMode = true;
        mapController = MapController.Instance;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        if (IsInSelectingMode && Input.GetMouseButtonDown(0))
        {
            HandleNodeSelection();
        }
    }

    private void HandleNodeSelection()
    {
        if (mapController.IsMoving) return;

        Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            NodeView clickedNodeView = hit.collider.GetComponent<NodeView>();
            if (clickedNodeView != null)
            {
                Node targetNode = clickedNodeView.GetNodeData();
                Node currentNode = MapController.Instance.GetPlayerNode();
                Debug.Log($"[PlayerController] currentNode: ({currentNode.gridPosition.x}, {currentNode.gridPosition.y}); targetNode: ({targetNode.gridPosition.x}, {targetNode.gridPosition.y})");

                if (MapController.Instance.IsAdjacent(currentNode, targetNode))
                {
                    Debug.Log($"[PlayerController] Valid move to ({targetNode.gridPosition.x}, {targetNode.gridPosition.y})");
                    StartCoroutine(MapController.Instance.PlayerDidMove(targetNode));
                }
                else
                {
                    Debug.LogWarning($"[PlayerController] Invalid move. Target node is not adjacent.");
                }
            }
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        moveCoroutine = null;
    }
}
