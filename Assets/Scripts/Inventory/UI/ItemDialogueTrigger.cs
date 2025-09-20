using UnityEngine;

/// <summary>
/// 物品对话触发器 - 当玩家靠近物品时触发对话系统
/// 支持两种UI类型：选择类型和提示类型
/// </summary>
public class ItemDialogueTrigger : MonoBehaviour
{
    // 引用对话管理器
    private DialogueManager dialogueManager;

    [Header("对话配置")]
    // 要触发的对话CSV文件名
    public string dialogueCSVFileName = "dialogue_example.csv";
    
    // 对话起始索引
    public int dialogueStartIndex = 0;
    
    // 对话类型：true表示选择类型，false表示提示类型
    public bool isChoiceTypeDialogue = true;

    [Header("触发设置")]
    // 触发对话的范围
    public float triggerRange = 2f;
    
    // 玩家标签
    public string playerTag = "Player";
    
    [Tooltip("是否只触发一次")]
    public bool triggerOnce = false;
    
    [Tooltip("是否需要按下交互键才能触发对话")]
    public bool requireInteractionKey = true;
    
    [Tooltip("交互键")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("对话触发冷却时间(秒)，防止过于频繁触发")]
    public float triggerCooldown = 1.0f;

    [Header("UI设置")]
    // 交互提示UI（可选）
    public GameObject interactionPromptUI;
    
    // 注意：这些UI引用现在由DialogueManager直接管理
    // 选择类型对话UI（可选：如果需要在此处也可引用）
    [Tooltip("此引用是可选的，DialogueManager中已配置完整的UI引用")]
    public GameObject choiceTypeDialogueUI;
    
    // 提示类型对话UI
    [Tooltip("此引用是可选的，DialogueManager中已配置完整的UI引用")]
    public GameObject hintTypeDialogueUI;

    private bool isPlayerInRange = false;
    private bool hasTriggered = false;
    private bool isDialogueActive = false;
    private float lastTriggerTime = -Mathf.Infinity;

    void Start()
    {
        // 查找对话管理器
        dialogueManager = FindObjectOfType<DialogueManager>();
        
        if (dialogueManager == null)
        {
            Debug.LogError("在场景中找不到DialogueManager组件！请确保已添加对话管理器。");
        }
        
        // 初始隐藏交互提示
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
        
        // UI元素的初始状态现在由DialogueManager的UpdateDialogueUI方法管理
        // 如果需要在此处初始化UI，可以保留下面的代码
        /*
        if (choiceTypeDialogueUI != null)
        {
            choiceTypeDialogueUI.SetActive(false);
        }
        
        if (hintTypeDialogueUI != null)
        {
            hintTypeDialogueUI.SetActive(false);
        }
        */
    }

    void Update()
    {
        // 如果没有对话管理器，不执行逻辑
        if (dialogueManager == null)
        {
            return;
        }
        
        // 如果已经触发过且设置为只触发一次，则不执行后续逻辑
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        // 检查玩家是否在范围内
        CheckPlayerDistance();
        
        // 更新对话活动状态 - 使用DialogueManager提供的IsDialogueActive方法
        isDialogueActive = dialogueManager.IsDialogueActive();
        
        // 当玩家在范围内且满足触发条件时开始对话
        if (isPlayerInRange && CanTriggerDialogue() && !isDialogueActive)
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// 检查玩家与物品的距离
    /// </summary>
    private void CheckPlayerDistance()
    {
        // 查找玩家对象
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= triggerRange)
            {
                isPlayerInRange = true;
                ShowInteractionPrompt(true);
            }
            else
            {
                isPlayerInRange = false;
                ShowInteractionPrompt(false);
                // 如果对话正在进行但玩家离开范围，可以选择结束对话
                if (isDialogueActive && !requireInteractionKey)
                {
                    EndDialogue();
                }
            }
        }
    }

    /// <summary>
    /// 显示或隐藏交互提示
    /// </summary>
    private void ShowInteractionPrompt(bool show)
    {
        if (interactionPromptUI != null && requireInteractionKey)
        {
            interactionPromptUI.SetActive(show);
        }
    }

    /// <summary>
    /// 检查是否可以触发对话
    /// </summary>
    private bool CanTriggerDialogue()
    {
        // 首先检查是否过了冷却时间
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            return false;
        }
        
        if (!requireInteractionKey)
        {
            // 不需要按键，但确保对话还没有在进行中
            if (!isDialogueActive)
            {
                // 更新最后触发时间
                lastTriggerTime = Time.time;
                return true;
            }
            return false;
        }
        else
        {
            // 使用GetKeyDown确保只在按键按下的那一帧触发
            bool keyPressed = Input.GetKeyDown(interactionKey);
            if (keyPressed)
            {
                // 更新最后触发时间
                lastTriggerTime = Time.time;
            }
            return keyPressed;
        }
    }

    /// <summary>
    /// 触发对话
    /// </summary>
    private void TriggerDialogue()
    {
        if (dialogueManager != null && !string.IsNullOrEmpty(dialogueCSVFileName))
        {
            // 确保对话没有正在进行
            if (dialogueManager.IsDialogueActive())
            {
                Debug.LogWarning("[ItemDialogueTrigger] 对话已经在进行中，忽略触发: " + gameObject.name);
                return;
            }
            
            Debug.Log("触发物品对话: " + gameObject.name + "，文件: " + dialogueCSVFileName);
            
            // 设置对话文件
            dialogueManager.SetDialogueCSVFile(dialogueCSVFileName);
            
            // 设置对话类型
            dialogueManager.SetDialogueType(isChoiceTypeDialogue);
            
            // 从指定索引开始对话
            dialogueManager.StartDialogueAt(dialogueStartIndex);
            
            // 标记为已触发
            if (triggerOnce)
            {
                hasTriggered = true;
            }
            
            // 隐藏交互提示
            ShowInteractionPrompt(false);
        }
        else
        {
            Debug.LogWarning("[ItemDialogueTrigger] 无法触发对话 - 对话管理器不存在或对话文件名未设置: " + gameObject.name);
        }
    }

    /// <summary>
    /// 显示对应的对话UI
    /// </summary>
    private void ShowDialogueUI()
    {
        // 这个方法现在由DialogueManager中的UpdateDialogueUI方法处理
        // 这里保留空方法，以防未来需要扩展
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        // 这里可以根据需要添加结束对话的逻辑
        // 例如，隐藏UI元素等
    }

    // 绘制触发范围的Gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}