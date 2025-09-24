using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisasterSystem : MonoBehaviour
{
    [Header("灾害配置")]
    public List<DisasterSO> mapWideDisasters;
    public DisasterSO landslideDisaster;
    public DisasterSO tsunamiDisaster;

    [SerializeField] private Text currentDisasterName;

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

        currentDisasterName.text = $"当前灾害: {ActiveDisaster.disasterName}";
    }
}
