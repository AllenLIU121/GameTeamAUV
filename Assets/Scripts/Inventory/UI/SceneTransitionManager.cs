using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;
using DialogueSystem;
using System.Reflection;

/// <summary>
/// 场景转换管理器 - 处理场景之间的转换逻辑
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    // 对话管理器引用
    private DialogueManager dialogueManager;
    // 玩家GameObject引用
    private GameObject player;
    // 玩家Transform引用（用于直接设置位置）
    private Transform playerTransform;
    // 角色移动组件引用（如果存在，用于更新其位置数据）
    // private CharacterMove characterMove;

    // 成功逃离场景的标志性对话文本
    private const string ESCAPE_SUCCESS_DIALOGUE = "终于逃出来了！快去大使馆吧！";
    // 下一个场景的对话文件名
    private const string NEXT_SCENE_DIALOGUE_FILE = "store_711.csv";
    // 目标位置坐标
    private readonly Vector3 TARGET_POSITION = new Vector3(20f, 1f, 0f);

    // 标记是否需要在对话结束后执行场景转换
    private bool _needSceneTransition = false;

    private void Start()
    {
        // 获取对话管理器实例
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("SceneTransitionManager: 在场景中找不到DialogueManager组件！");
        }

        // 直接通过Player标签查找玩家GameObject
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            // characterMove = player.GetComponent<CharacterMove>();
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager: 找不到玩家对象！");
        }

        // 注册到对话事件
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.AddListener(OnDialogueEnd);
            dialogueManager.onDialogueEntryShown.AddListener(OnDialogueEntryShown);
        }
    }

    /// <summary>
    /// 对话条目显示完成时的回调函数
    /// </summary>
    private void OnDialogueEntryShown()
    {
        // 检查是否已触发死亡状态（存在死亡面板）
        if (HasDeathPanelActive())
            return;

        // 检查当前对话是否包含逃离成功的标志性文本
        if (IsEscapeSuccessDialogue())
        {
            _needSceneTransition = true;
        }
    }

    /// <summary>
    /// 对话结束时的回调函数
    /// </summary>
    private void OnDialogueEnd()
    {

        // 检查是否已触发死亡状态（存在死亡面板）
        if (HasDeathPanelActive())
            return;

        // 如果之前标记了需要场景转换，则执行场景转换
        if (_needSceneTransition)
        {
            ExecuteSceneTransition();
            _needSceneTransition = false;
        }
    }

    /// <summary>
    /// 检查是否有死亡面板在场景中激活
    /// </summary>
    /// <returns>是否有激活的死亡面板</returns>
    private bool HasDeathPanelActive()
    {
        GameObject finalCanvas = GameObject.Find("Final Canvas");
        if (finalCanvas != null)
        {
            foreach (Transform child in finalCanvas.transform)
            {
                if (child.gameObject.activeInHierarchy &&
                   (child.name.Contains("DeadPanel") || child.name.Contains("死亡")))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查当前对话是否是表示逃离成功的对话
    /// </summary>
    /// <returns>是否是逃离成功对话</returns>
    private bool IsEscapeSuccessDialogue()
    {
        // 首先检查是否是特定的几个文件
        string currentCSVFile = GetCurrentCSVFileName();
        if (currentCSVFile != "fire_outside.csv" &&
            currentCSVFile != "fire_room_choice.csv" &&
            currentCSVFile != "typhoon_inside.csv" &&
            currentCSVFile != "typhoon_outside.csv")
            return false;

        // 通过反射获取DialogueManager中的对话信息
        try
        {
            var dialoguesField = typeof(DialogueManager).GetField("_dialogues",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var currentIndexField = typeof(DialogueManager).GetField("_currentDialogueIndex",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (dialoguesField == null || currentIndexField == null)
                return false;

            var dialogues = dialoguesField.GetValue(dialogueManager) as List<DialogueEntry>;
            int currentIndex = (int)currentIndexField.GetValue(dialogueManager);

            if (dialogues == null || dialogues.Count <= 0 || currentIndex < 0 || currentIndex >= dialogues.Count)
                return false;

            DialogueEntry lastSeenEntry = dialogues[currentIndex];
            return lastSeenEntry != null && lastSeenEntry.dialogueText.Contains(ESCAPE_SUCCESS_DIALOGUE);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取当前对话的CSV文件名
    /// </summary>
    /// <returns>当前对话的CSV文件名</returns>
    private string GetCurrentCSVFileName()
    {
        try
        {
            FieldInfo currentCsvPathField = typeof(DialogueSystem.DialogueManager).GetField("_currentCSVPath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentCsvPathField != null)
            {
                string currentCSVFile = currentCsvPathField.GetValue(dialogueManager) as string;
                if (!string.IsNullOrEmpty(currentCSVFile))
                {
                    return System.IO.Path.GetFileName(currentCSVFile);
                }
            }
        }
        catch { }

        return "";
    }

    /// <summary>
    /// 执行场景转换逻辑
    /// </summary>
    private void ExecuteSceneTransition()
    {
        // 重置地震计数，使场景切换后不再受之前地震次数的影响
        ResetEarthquakeCount();

        // 设置玩家位置（瞬移效果）
        bool positionSetSuccess = SetPlayerPosition();

        // 触发下一个场景的对话
        TriggerNextSceneDialogue(positionSetSuccess);
    }

    /// <summary>
    /// 设置玩家位置
    /// </summary>
    private bool SetPlayerPosition()
    {
        if (playerTransform != null)
        {
            playerTransform.position = TARGET_POSITION;
        }
        else
        {
            playerTransform = player.GetComponent<Transform>();
            if (playerTransform != null)
            {
                playerTransform.position = TARGET_POSITION;
                return true;
            }
        }

        // 最后的尝试：直接通过标签查找
        try
        {
            playerTransform = GameObject.FindWithTag("Player")?.GetComponent<Transform>();
            if (playerTransform != null)
            {
                playerTransform.position = TARGET_POSITION;
                return true;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// 触发下一个场景的对话
    /// </summary>
    private void TriggerNextSceneDialogue(bool positionSetSuccess)
    {
        // 尝试获取dialogueManager（如果之前为空）
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        if (dialogueManager != null)
        {
            // 确保对话管理器不在活跃状态
            if (!dialogueManager.IsDialogueActive())
            {
                TriggerNextSceneDialogueDelayed();
            }
            else
            {
                // 如果对话正在进行中，延迟触发
                Invoke("TriggerNextSceneDialogueDelayed", 1f);
            }
        }
        else
        {
            // 最后的备用方案：创建临时DialogueManager实例
            CreateTempDialogueManager();
        }
    }

    /// <summary>
    /// 创建临时DialogueManager实例
    /// </summary>
    private void CreateTempDialogueManager()
    {
        try
        {
            GameObject dialogueManagerObj = new GameObject("TempDialogueManager");
            DialogueManager tempDialogueManager = dialogueManagerObj.AddComponent<DialogueManager>();

            bool isChoiceDialogue = NEXT_SCENE_DIALOGUE_FILE.Contains("choice") ||
                                  NEXT_SCENE_DIALOGUE_FILE.Contains("earthquake_father_smoking");

            // 使用反射设置对话类型和启动对话
            MethodInfo setDialogueTypeMethod = typeof(DialogueManager).GetMethod("SetDialogueType",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo startDialogueMethod = typeof(DialogueManager).GetMethod("StartDialogue",
                BindingFlags.Public | BindingFlags.Instance);

            if (setDialogueTypeMethod != null)
            {
                setDialogueTypeMethod.Invoke(tempDialogueManager, new object[] { isChoiceDialogue });
            }

            if (startDialogueMethod != null)
            {
                startDialogueMethod.Invoke(tempDialogueManager, new object[] { NEXT_SCENE_DIALOGUE_FILE });
            }

            // 对话结束后删除临时对象
            Destroy(dialogueManagerObj, 10f);
        }
        catch { }
    }

    /// <summary>
    /// 重置地震动画状态
    /// </summary>
    private void ResetEarthquakeCount()
    {
        foreach (AnimationTrigger trigger in FindObjectsOfType<AnimationTrigger>())
        {
            trigger.StopAnimationCycle();
            trigger.SetEarthquakeCount(0);
            trigger.enabled = false;
        }
    }

    /// <summary>
    /// 延迟触发下一个场景对话的方法
    /// </summary>
    private void TriggerNextSceneDialogueDelayed()
    {
        if (dialogueManager == null || dialogueManager.IsDialogueActive())
            return;

        // 根据对话文件名动态设置对话类型
        bool isChoiceDialogue = NEXT_SCENE_DIALOGUE_FILE.Contains("choice") ||
                               NEXT_SCENE_DIALOGUE_FILE.Contains("earthquake_father_smoking");
        dialogueManager.SetDialogueType(isChoiceDialogue);

        try
        {
            dialogueManager.StartDialogue(NEXT_SCENE_DIALOGUE_FILE);
        }
        catch { }
    }

    private void OnDestroy()
    {
        // 移除事件监听，防止内存泄漏
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.RemoveListener(OnDialogueEnd);
            dialogueManager.onDialogueEntryShown.RemoveListener(OnDialogueEntryShown);
        }
    }
}