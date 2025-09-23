using UnityEngine;
using Inventory.Characters;
using System.Collections.Generic;
using DialogueSystem;

/// <summary>
/// 场景转换管理器 - 处理场景之间的转换逻辑
/// 当玩家触发特定对话（表示成功逃离当前场景）时，更新玩家位置并触发下一个场景的对话
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
    private CharacterMove characterMove;
    // 安全检查标记
    private bool hasTransitioned = false;

    // 成功逃离场景的标志性对话文本
    private const string ESCAPE_SUCCESS_DIALOGUE = "终于逃出来了！快去大使馆吧！";
    // 下一个场景的对话文件名
    private const string NEXT_SCENE_DIALOGUE_FILE = "store_711.csv";
    // 目标位置坐标
    private readonly Vector3 TARGET_POSITION = new Vector3(20f, 1f, 0f);

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
            // 同时查找CharacterMove组件以更新其内部位置数据
            characterMove = player.GetComponent<CharacterMove>();
            Debug.Log("SceneTransitionManager: 通过Player标签找到了玩家对象");
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager: 找不到玩家对象！");
        }

        // 注册到对话结束事件
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.AddListener(OnDialogueEnd);
        }
    }

    /// <summary>
    /// 对话结束时的回调函数
    /// </summary>
    private void OnDialogueEnd()
    {
        // 防止重复触发
        if (hasTransitioned)
        {
            return;
        }

        // 检查当前对话是否包含逃离成功的标志性文本
        if (dialogueManager != null && IsEscapeSuccessDialogue())
        {
            // 执行场景转换
            ExecuteSceneTransition();
        }
    }

    /// <summary>
    /// 检查当前对话是否是表示逃离成功的对话
    /// </summary>
    /// <returns>是否是逃离成功对话</returns>
    private bool IsEscapeSuccessDialogue()
    {
        // 获取当前CSV文件（使用反射获取私有字段）
        string currentCSVFile = "";
        try
        {
            System.Reflection.FieldInfo currentCsvPathField = typeof(DialogueSystem.DialogueManager).GetField("_currentCSVPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (currentCsvPathField != null)
            {
                currentCSVFile = currentCsvPathField.GetValue(dialogueManager) as string;
                // 如果路径不为空，提取文件名
                if (!string.IsNullOrEmpty(currentCSVFile))
                {
                    currentCSVFile = System.IO.Path.GetFileName(currentCSVFile);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("SceneTransitionManager: 获取当前对话文件失败: " + ex.Message);
        }

        Debug.Log("SceneTransitionManager: 当前对话文件: " + currentCSVFile);

        // 首先检查是否是特定的几个文件
        if (currentCSVFile == "fire_outside.csv" ||
            currentCSVFile == "fire_room_choice.csv" ||
            currentCSVFile == "typhoon_inside.csv" ||
            currentCSVFile == "typhoon_outside.csv")
        {
            // 使用反射获取DialogueManager中的最后一句对话内容
            try
            {
                // 假设DialogueManager有一个存储当前对话内容的字段或属性
                // 这里尝试获取DialogueManager中的对话条目列表
                var fieldInfo = typeof(DialogueManager).GetField("dialogues",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var dialogues = fieldInfo.GetValue(dialogueManager) as List<DialogueEntry>;
                    if (dialogues != null && dialogues.Count > 0)
                    {
                        // 检查最后一条对话是否包含标志性文本
                        DialogueEntry lastEntry = dialogues[dialogues.Count - 1];
                        if (lastEntry != null && lastEntry.dialogueText.Contains(ESCAPE_SUCCESS_DIALOGUE))
                        {
                            Debug.Log("SceneTransitionManager: 检测到标志性对话文本: " + lastEntry.dialogueText);
                            return true;
                        }
                        else
                        {
                            Debug.Log("SceneTransitionManager: 最后一条对话不包含标志性文本: " + (lastEntry != null ? lastEntry.dialogueText : "null"));
                            // 额外检查是否是最后一条对话（如果没有检测到标志性文本但确实是结束点，也应该触发转换）
                            if (lastEntry != null && lastEntry.isEndPoint)
                            {
                                Debug.Log("SceneTransitionManager: 检测到结束点对话，强制触发场景转换");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("SceneTransitionManager: dialogues列表为空或为null");
                    }
                }
                else
                {
                    Debug.LogWarning("SceneTransitionManager: 找不到dialogues字段");
                }

                // 如果无法通过反射获取，默认返回true（基于文件名的检查）
                Debug.LogWarning("SceneTransitionManager: 无法获取对话内容，使用文件名进行检测");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("SceneTransitionManager: 检查对话内容时发生错误: " + ex.Message);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 执行场景转换逻辑
    /// </summary>
    private void ExecuteSceneTransition()
    {
        // 标记已转换
        hasTransitioned = true;

        Debug.Log("SceneTransitionManager: 检测到玩家成功逃离当前场景，执行场景转换...");

        // 检查player和playerTransform的状态
        Debug.Log("SceneTransitionManager: player = " + (player != null ? "存在" : "不存在"));
        Debug.Log("SceneTransitionManager: playerTransform = " + (playerTransform != null ? "存在" : "不存在"));
        Debug.Log("SceneTransitionManager: characterMove = " + (characterMove != null ? "存在" : "不存在"));

        // 直接设置玩家位置（瞬移效果）
        if (playerTransform != null)
        {
            // 直接在目标位置生成/瞬移，不使用移动动画
            playerTransform.position = TARGET_POSITION;
            Debug.Log("SceneTransitionManager: 玩家已瞬移至目标位置: " + TARGET_POSITION);

            // 如果CharacterMove组件存在，更新其位置数据以保持一致性
            if (characterMove != null)
            {
                characterMove.UpdatePosition();
                Debug.Log("SceneTransitionManager: 已调用characterMove.UpdatePosition()更新位置数据");
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager: characterMove为空，无法更新位置数据");
                // 如果CharacterMove为空，尝试重新获取
                if (player != null)
                {
                    characterMove = player.GetComponent<CharacterMove>();
                    if (characterMove != null)
                    {
                        characterMove.UpdatePosition();
                        Debug.Log("SceneTransitionManager: 重新获取characterMove并更新位置数据");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager: 无法设置玩家位置，playerTransform为空！");
            // 如果playerTransform为空，尝试通过player获取
            if (player != null)
            {
                playerTransform = player.transform;
                if (playerTransform != null)
                {
                    playerTransform.position = TARGET_POSITION;
                    Debug.Log("SceneTransitionManager: 重新获取playerTransform并设置位置");
                }
            }
        }

        // 触发下一个场景的对话
        if (dialogueManager != null)
        {
            Debug.Log("SceneTransitionManager: 准备触发下一个场景对话: " + NEXT_SCENE_DIALOGUE_FILE);

            // 确保对话管理器不在活跃状态
            if (!dialogueManager.IsDialogueActive())
            {
                // 直接调用DialogueManager的公开方法来设置和启动对话，避免使用反射
                try
                {
                    // 根据对话文件名动态设置对话类型
                    // 对于包含选择的对话文件，设置为选择类型
                    bool isChoiceDialogue = NEXT_SCENE_DIALOGUE_FILE.Contains("choice") ||
                                           NEXT_SCENE_DIALOGUE_FILE.Contains("earthquake_father_smoking");
                    dialogueManager.SetDialogueType(isChoiceDialogue);
                    Debug.Log("SceneTransitionManager: 已设置对话类型为" + (isChoiceDialogue ? "选择类型" : "提示类型"));
                    dialogueManager.StartDialogue(NEXT_SCENE_DIALOGUE_FILE);
                    Debug.Log("SceneTransitionManager: 已启动对话文件: " + NEXT_SCENE_DIALOGUE_FILE);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("SceneTransitionManager: 启动对话时发生错误: " + ex.Message);

                    // 备用方案：创建临时的ItemDialogueTrigger对象来触发对话
                    Debug.Log("SceneTransitionManager: 尝试备用方案触发对话");
                    GameObject triggerObj = new GameObject("TempSceneTransitionTrigger");
                    ItemDialogueTrigger trigger = triggerObj.AddComponent<ItemDialogueTrigger>();

                    // 设置触发参数
                    trigger.dialogueCSVFileName = NEXT_SCENE_DIALOGUE_FILE;

                    // 根据对话文件名动态设置对话类型
                    // 对于包含选择的对话文件，设置为选择类型
                    trigger.isChoiceTypeDialogue = NEXT_SCENE_DIALOGUE_FILE.Contains("choice") ||
                                                  NEXT_SCENE_DIALOGUE_FILE.Contains("earthquake_father_smoking");

                    trigger.triggerRange = 100f; // 设置一个足够大的范围以确保立即触发
                    trigger.requireInteractionKey = false; // 不需要按键，自动触发
                    trigger.triggerOnce = true; // 只触发一次

                    // 使用反射调用私有方法TriggerDialogue直接触发对话
                    System.Reflection.MethodInfo methodInfo = typeof(ItemDialogueTrigger).GetMethod("TriggerDialogue",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(trigger, null);
                    }
                    else
                    {
                        Debug.LogError("SceneTransitionManager: 无法找到TriggerDialogue方法！");
                    }

                    // 等待一帧后删除临时对象
                    Destroy(triggerObj, 1f);
                }
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager: 对话管理器正在活跃状态，无法触发新对话");

                // 如果对话正在进行中，延迟触发
                Invoke("TriggerNextSceneDialogueDelayed", 1f);
            }
        }
    }

    /// <summary>
    /// 延迟触发下一个场景对话的方法
    /// </summary>
    private void TriggerNextSceneDialogueDelayed()
    {
        if (dialogueManager != null && !dialogueManager.IsDialogueActive())
        {
            Debug.Log("SceneTransitionManager: 延迟触发下一个场景对话: " + NEXT_SCENE_DIALOGUE_FILE);

            // 根据对话文件名动态设置对话类型
            // 对于包含选择的对话文件，设置为选择类型
            bool isChoiceDialogue = NEXT_SCENE_DIALOGUE_FILE.Contains("choice") ||
                                   NEXT_SCENE_DIALOGUE_FILE.Contains("earthquake_father_smoking");
            dialogueManager.SetDialogueType(isChoiceDialogue);
            Debug.Log("SceneTransitionManager: 已设置对话类型为" + (isChoiceDialogue ? "选择类型" : "提示类型"));

            dialogueManager.StartDialogue(NEXT_SCENE_DIALOGUE_FILE);
        }
        else
        {
            Debug.LogError("SceneTransitionManager: 延迟触发对话失败，对话管理器仍在活跃状态");
        }
    }

    private void OnDestroy()
    {
        // 移除事件监听，防止内存泄漏
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }
}