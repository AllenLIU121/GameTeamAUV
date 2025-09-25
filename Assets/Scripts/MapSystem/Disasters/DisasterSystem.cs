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
            AudioManager.Instance.PlaySFX("山体滑坡循环0925_01.wav");
            ActiveDisaster = landslideDisaster;     // 触发山体滑坡
        }
        else if (isPlayerInSea)
        {
            AudioManager.Instance.PlaySFX("海啸循环0925_01.wav");
            ActiveDisaster = tsunamiDisaster;   // 触发海啸
        }
        else
        {
            ActiveDisaster = mapWideDisasters[Random.Range(0, mapWideDisasters.Count)]; // 空旷地区随机触发全图灾害
            if (ActiveDisaster.disasterName == "\\u5730\\u9707")
            {
                AudioManager.Instance.PlaySFX("地震循环0925_01.wav");
            }
            else if (ActiveDisaster.disasterName == "\\u706B\\u707E")
            {
                AudioManager.Instance.PlaySFX("火灾循环0925_01.wav");
            }
            else if (ActiveDisaster.disasterName == "\\u6D2A\\u6C34")
            {
                AudioManager.Instance.PlaySFX("洪水循环0925_01.wav");
            }
            else if (ActiveDisaster.disasterName == "\\u53F0\\u98CE")
            {
                AudioManager.Instance.PlaySFX("台风循环0925_01.wav");
            }
        }

        currentDisasterName.text = $"当前灾害: {ActiveDisaster.disasterName}";
    }
}
