using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterSystem : MonoBehaviour
{
    [Header("灾害配置")]
    public List<DisasterSO> mapWideDisasters;
    public DisasterSO landslideDisaster;
    public DisasterSO tsunamiDisaster;

    public DisasterSO ActiveDisaster { get; private set; }

    // 触发一次新的灾难
    public void TriggerNewDisaster(Node playerNode)
    {
        bool isPlayerInMountain = playerNode.nodeType == NodeType.Mountain;
        bool isPlayerInSea = playerNode.nodeType == NodeType.Sea;

        if (isPlayerInMountain)
        {
            ActiveDisaster = landslideDisaster;     // 触发山体滑坡
        }
        else if (isPlayerInSea)
        {
            ActiveDisaster = tsunamiDisaster;   // 触发海啸
        }
        else
        {
            ActiveDisaster = mapWideDisasters[Random.Range(0, mapWideDisasters.Count)]; // 空旷地区随机触发全图灾害
        }

        Debug.Log($"[DisasterSystem] Triggered new disaster: {ActiveDisaster.disasterName}");
    }
}
