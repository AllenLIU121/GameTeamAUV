using UnityEngine;

/// <summary>
/// 防灾手册拾取触发器 - 处理防灾手册的获取逻辑
/// 继承自ItemDialogueTrigger，扩展了选择获取时调用特定方法的功能
/// </summary>
public class DisasterManualPickup : ItemDialogueTrigger
{
    private EarthquakeFlowManager earthquakeFlowManager;

    // 标记是否已经获取了防灾手册
    private bool hasPickedUpManual = false;

    // 获取防灾手册的选择索引（从0开始）
    public int pickupChoiceIndex = 0;

    protected override void Start()
    {
        // 在Unity中，即使没有使用override关键字，base.Start()也可以调用父类的Start方法
        base.Start();
        
        Debug.Log("DisasterManualPickup: Start方法执行");
        
        // 查找地震流程管理器
        earthquakeFlowManager = FindObjectOfType<EarthquakeFlowManager>();
        
        if (earthquakeFlowManager == null)
        {
            Debug.LogWarning("在场景中找不到EarthquakeFlowManager组件，无法设置获取防灾手册的状态！");
        }
        
        // 设置默认对话文件
        dialogueCSVFileName = "disaster_manual_pickup.csv";
        // 设置为选择类型对话
        isChoiceTypeDialogue = true;
        // 设置为只触发一次
        triggerOnce = true;
        
        Debug.Log("DisasterManualPickup: 配置已设置完成");
    }

    /// <summary>
    /// 当玩家选择获取防灾手册时调用此方法
    /// 此方法应该在DialogueManager的选择处理逻辑中被调用
    /// </summary>
    public void OnManualPickedUp()
    {
        // 检查当前对话文件是否是disaster_manual_pickup.csv
        // 防止在其他对话文件中误触发
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null && dialogueManager.csvFileName == "disaster_manual_pickup.csv")
        {
            if (!hasPickedUpManual)
            {
                hasPickedUpManual = true;
                Debug.Log("玩家获取了防灾手册");
                
                // 调用EarthquakeFlowManager的SetPlayerHasDisasterManual方法
                if (earthquakeFlowManager != null)
                {
                    earthquakeFlowManager.SetPlayerHasDisasterManual();
                }
                else
                {
                    Debug.LogError("EarthquakeFlowManager不存在，无法设置获取防灾手册的状态！");
                }
            }
        }
        else
        {
            Debug.LogWarning("DisasterManualPickup: 当前对话文件不是disaster_manual_pickup.csv，不触发获取防灾手册事件");
        }
    }
}