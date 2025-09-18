using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class ClickToOpenUI : MonoBehaviour
{
    [Header("交互设置")]
    public GameObject interactionButton; // 交互按钮
    public float interactDistance = 1.5f; // 可交互距离

    [Header("对话设置")]
    public GameObject dialogueUI;       // 对话UI对象
    public string dialogueID;           // 对话ID，用于区分不同物体触发的对话
    public int startDialogueIndex = 0;  // 对话开始的索引

    [Header("事件回调")]
    public UnityEvent onDialogueStart;  // 对话开始时的事件
    public UnityEvent onDialogueEnd;    // 对话结束时的事件

    private GameObject player;          // 玩家对象引用
    private bool isPlayerNear = false;  // 玩家是否在附近的标志
    private DialogueManager dialogueManager; // 对话管理器引用

    private void Start()
    {
        // 初始时隐藏交互按钮和对话UI
        if (interactionButton != null)
        {
            interactionButton.SetActive(false);
        }
        
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
            
            // 获取对话管理器组件
            dialogueManager = dialogueUI.GetComponent<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("[ClickToOpenUI] 对话UI中未找到DialogueManager组件");
            }
        }
        
        // 查找玩家对象（可以通过标签、名称或特定组件查找）
        FindPlayer();
    }

    private void Update()
    {
        // 检测玩家是否在附近
        CheckPlayerDistance();
        
        // 如果玩家在附近且按下E键，触发对话
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && dialogueUI != null && !dialogueUI.activeSelf)
        {
            TriggerDialogue();
        }
    }

    // 通过标签或其他方式查找玩家
    private void FindPlayer()
    {
        // 尝试通过标签查找玩家（如果玩家有特定标签）
        player = GameObject.FindWithTag("Player");
        
        // 如果没有找到带Player标签的对象，尝试通过名称查找
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        // 如果还是没找到，记录警告
        if (player == null)
        {
            Debug.LogWarning("[ClickToOpenUI] 未找到玩家对象，请确保玩家对象正确命名或有Player标签");
        }
    }

    // 检查玩家与物体的距离
    private void CheckPlayerDistance()
    {
        if (player == null || interactionButton == null)
            return;
        
        float distance = Vector2.Distance(transform.position, player.transform.position);
        bool shouldShowButton = distance <= interactDistance;
        
        if (shouldShowButton != isPlayerNear)
        {
            isPlayerNear = shouldShowButton;
            interactionButton.SetActive(isPlayerNear);
        }
    }

    // 如果使用触发器方式检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 确保只对玩家响应
        if (IsPlayer(other.gameObject))
        {
            isPlayerNear = true;
            if (interactionButton != null)
            {
                interactionButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 确保只对玩家响应
        if (IsPlayer(other.gameObject))
        {
            isPlayerNear = false;
            if (interactionButton != null)
            {
                interactionButton.SetActive(false);
            }
            
            // 如果玩家离开，关闭对话UI
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
                OnDialogueEnd();
            }
        }
    }

    // 判断游戏对象是否为玩家
    private bool IsPlayer(GameObject obj)
    {
        // 可以通过多种方式判断是否为玩家
        // 1. 通过标签
        if (obj.CompareTag("Player"))
            return true;
        
        // 2. 通过名称
        if (obj.name == "Player")
            return true;
        
        // 3. 通过特定组件或脚本
        // 可以根据项目中玩家特有的组件进行判断
        
        return false;
    }

    // 设置对话UI（可以在运行时动态修改要显示的对话UI）
    public void SetDialogueUI(GameObject newDialogueUI, string newDialogueID = null)
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
        
        dialogueUI = newDialogueUI;
        
        // 获取新的对话管理器引用
        if (dialogueUI != null)
        {
            dialogueManager = dialogueUI.GetComponent<DialogueManager>();
        }
        else
        {
            dialogueManager = null;
        }
        
        if (!string.IsNullOrEmpty(newDialogueID))
        {
            dialogueID = newDialogueID;
        }
    }

    // 触发对话的方法
    private void TriggerDialogue()
    {
        if (dialogueUI == null)
            return;
        
        dialogueUI.SetActive(true);
        Debug.Log("触发对话: " + dialogueID);
        
        // 触发对话开始事件
        onDialogueStart?.Invoke();
        
        // 如果有对话管理器组件，调用其方法开始对话
        if (dialogueManager != null)
        {
            // 根据对话ID或索引开始对话
            if (startDialogueIndex >= 0)
            {
                dialogueManager.StartDialogueAt(startDialogueIndex);
            }
            else
            {
                dialogueManager.StartDialogue();
            }
        }
    }

    // 当对话结束时调用
    private void OnDialogueEnd()
    {
        onDialogueEnd?.Invoke();
        Debug.Log("对话结束: " + dialogueID);
    }
}