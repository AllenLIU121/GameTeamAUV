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
            AudioManager.Instance.PlaySFX("SFX_LandSlide");
            ActiveDisaster = landslideDisaster;     // 触发山体滑坡
        }
        else if (isPlayerInSea)
        {
            AudioManager.Instance.PlaySFX("SFX_Tsunami");
            ActiveDisaster = tsunamiDisaster;   // 触发海啸
        }
        else
        {
            ActiveDisaster = mapWideDisasters[Random.Range(0, mapWideDisasters.Count)]; // 空旷地区随机触发全图灾害
            if (ActiveDisaster.disasterID == "earthquake")
            {
                AudioManager.Instance.PlaySFX("SFX_Earthquake");
            }
            else if (ActiveDisaster.disasterID == "fire")
            {
                AudioManager.Instance.PlaySFX("SFX_Fire");
            }
            else if (ActiveDisaster.disasterID == "flood")
            {
                AudioManager.Instance.PlaySFX("SFX_Flood");
            }
            else if (ActiveDisaster.disasterID == "typhoon")
            {
                AudioManager.Instance.PlaySFX("SFX_Typhoon");
            }
        }

        currentDisasterName.text = $"当前灾害: {ActiveDisaster.disasterName}";
    }
}
