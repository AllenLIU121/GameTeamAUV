// using UnityEngine;
// using UnityEngine.UI;
// using DialogueSystem;
// using System.Reflection;

// /// <summary>
// /// 死亡面板控制器 - 处理死亡面板的交互逻辑
// /// </summary>
// public class DiedPanelController : MonoBehaviour
// {
//     [Header("按钮引用")]
//     [SerializeField] private Button rollbackButton;

//     private void Awake()
//     {
//         if (rollbackButton == null)
//         {
//             rollbackButton = transform.Find("RollbackButton")?.GetComponent<Button>();
//         }
//     }

//     private void Start()
//     {
//         // 绑定按钮点击事件
//         if (rollbackButton != null)
//         {
//             rollbackButton.onClick.AddListener(OnRollbackButtonClick);
//         }
//     }

//     /// <summary>
//     /// 回档按钮点击处理 - 返回上一次选择的地方
//     /// </summary>
//     private void OnRollbackButtonClick()
//     {

//         GameStateManager.Instance?.SnapshotRollback(new OnGameRollback());
//         Debug.Log("DiedPanelController: 成功调用GameStateManager的SnapshotRollback方法");

//         // 获取DialogueManager实例
//         DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();

//         if (dialogueManager != null)
//         {
//             // 重置对话选项状态，确保所有选项都可以重新选择
//             ResetDialogueChoices(dialogueManager);

//             // 设置对话类型为选择类型
//             MethodInfo setDialogueTypeMethod = typeof(DialogueManager).GetMethod("SetDialogueType",
//                 BindingFlags.Public | BindingFlags.Instance);
//             if (setDialogueTypeMethod != null)
//             {
//                 setDialogueTypeMethod.Invoke(dialogueManager, new object[] { true });
//             }

//             // 获取当前对话索引
//             FieldInfo currentIndexField = typeof(DialogueManager).GetField("_currentDialogueIndex",
//                 BindingFlags.NonPublic | BindingFlags.Instance);
//             int currentIndex = 0;
//             if (currentIndexField != null)
//             {
//                 currentIndex = (int)currentIndexField.GetValue(dialogueManager);
//                 Debug.Log("DiedPanelController: 当前对话索引: " + currentIndex);

//                 if (currentIndex > 0)
//                 {
//                     currentIndex = Mathf.Max(0, currentIndex - 1);
//                     Debug.Log("DiedPanelController: 调整索引回到选择点位置: " + currentIndex);
//                     currentIndexField.SetValue(dialogueManager, currentIndex);
//                 }
//             }

//             // 确保对话活动标志已设置
//             SetDialogueActiveFlag(dialogueManager, true);

//             // 先暂停游戏
//             GameManager.Instance?.ChangeGameState(GameState.Paused);

//             // 直接调用ShowCurrentDialogue方法来重新显示对话，这会正确处理选择UI的显示
//             ShowCurrentDialogue(dialogueManager);

//             // 额外确保UI状态和选择按钮正确显示
//             EnsureDialogueUIAndChoices(dialogueManager);
//         }
//         else
//         {
//             Debug.LogError("DiedPanelController: 找不到DialogueManager实例");
//         }

//         // 隐藏死亡面板
//         HideDeathPanel();
//     }

//     /// <summary>
//     /// 确保对话UI和选择按钮正确显示
//     /// </summary>
//     private void EnsureDialogueUIAndChoices(DialogueManager dialogueManager)
//     {
//         try
//         {
//             // 通过反射获取DialogueManager中的_uiManager字段
//             FieldInfo uiManagerField = typeof(DialogueManager).GetField("_uiManager",
//                 BindingFlags.NonPublic | BindingFlags.Instance);
//             if (uiManagerField != null)
//             {
//                 object uiManager = uiManagerField.GetValue(dialogueManager);
//                 if (uiManager != null)
//                 {
//                     // 调用DialogueUIManager的UpdateUIState方法
//                     MethodInfo updateUIStateMethod = uiManager.GetType().GetMethod("UpdateUIState",
//                         BindingFlags.Public | BindingFlags.Instance);
//                     if (updateUIStateMethod != null)
//                     {
//                         updateUIStateMethod.Invoke(uiManager, new object[] { true });
//                     }

//                     // 检查当前是否应该显示选择按钮
//                     FieldInfo currentIndexField = typeof(DialogueManager).GetField("_currentDialogueIndex",
//                         BindingFlags.NonPublic | BindingFlags.Instance);
//                     FieldInfo dialoguesField = typeof(DialogueManager).GetField("_dialogues",
//                         BindingFlags.NonPublic | BindingFlags.Instance);

//                     if (currentIndexField != null && dialoguesField != null)
//                     {
//                         int currentIndex = (int)currentIndexField.GetValue(dialogueManager);
//                         System.Collections.IList dialogues = (System.Collections.IList)dialoguesField.GetValue(dialogueManager);

//                         if (dialogues != null && currentIndex >= 0 && currentIndex < dialogues.Count)
//                         {
//                             object currentEntry = dialogues[currentIndex];

//                             // 获取当前对话的hasChoices属性
//                             PropertyInfo hasChoicesProperty = currentEntry.GetType().GetProperty("hasChoices");
//                             bool hasChoices = false;

//                             if (hasChoicesProperty != null)
//                             {
//                                 hasChoices = (bool)hasChoicesProperty.GetValue(currentEntry);
//                             }

//                             // 如果有选择项，确保选择容器可见
//                             if (hasChoices)
//                             {
//                                 MethodInfo setChoiceContainerActiveMethod = uiManager.GetType().GetMethod("UpdateUIState",
//                                     BindingFlags.Public | BindingFlags.Instance);
//                                 if (setChoiceContainerActiveMethod != null)
//                                 {
//                                     setChoiceContainerActiveMethod.Invoke(uiManager, new object[] { true });
//                                 }

//                                 Debug.Log("DiedPanelController: 确保选择容器可见");
//                             }
//                         }
//                     }
//                 }
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError("DiedPanelController: 确保对话UI和选择按钮时出错: " + e.Message);
//         }
//     }

//     /// <summary>
//     /// 设置对话活动标志
//     /// </summary>
//     private void SetDialogueActiveFlag(DialogueManager dialogueManager, bool isActive)
//     {
//         try
//         {
//             FieldInfo activeFlagField = typeof(DialogueManager).GetField("_dialogueActiveFlag",
//                 BindingFlags.NonPublic | BindingFlags.Instance);

//             if (activeFlagField != null)
//             {
//                 activeFlagField.SetValue(dialogueManager, isActive);
//                 Debug.Log("DiedPanelController: 成功设置对话活动标志为: " + isActive);
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError("DiedPanelController: 设置对话活动标志时出错: " + e.Message);
//         }
//     }

//     /// <summary>
//     /// 显示当前对话
//     /// </summary>
//     private void ShowCurrentDialogue(DialogueManager dialogueManager)
//     {
//         try
//         {
//             MethodInfo showCurrentDialogueMethod = typeof(DialogueManager).GetMethod("ShowCurrentDialogue",
//                 BindingFlags.NonPublic | BindingFlags.Instance);

//             if (showCurrentDialogueMethod != null)
//             {
//                 showCurrentDialogueMethod.Invoke(dialogueManager, null);
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError("DiedPanelController: 显示当前对话时出错: " + e.Message);
//         }
//     }

//     /// <summary>
//     /// 重置对话选择状态，确保所有选项都可以重新选择
//     /// </summary>
//     private void ResetDialogueChoices(DialogueManager dialogueManager)
//     {
//         try
//         {
//             // 使用反射获取当前对话列表
//             FieldInfo dialoguesField = typeof(DialogueManager).GetField("_dialogues",
//                 BindingFlags.NonPublic | BindingFlags.Instance);

//             if (dialoguesField != null)
//             {
//                 System.Collections.IList dialogues = (System.Collections.IList)dialoguesField.GetValue(dialogueManager);

//                 if (dialogues != null)
//                 {
//                     // 遍历所有对话条目
//                     int resetCount = 0;
//                     foreach (object dialogueEntry in dialogues)
//                     {
//                         // 检查是否有选择项
//                         PropertyInfo hasChoicesProperty = dialogueEntry.GetType().GetProperty("hasChoices");
//                         bool hasChoices = false;

//                         if (hasChoicesProperty != null)
//                         {
//                             hasChoices = (bool)hasChoicesProperty.GetValue(dialogueEntry);
//                         }

//                         if (hasChoices)
//                         {
//                             // 获取选择项列表
//                             PropertyInfo choicesProperty = dialogueEntry.GetType().GetProperty("choices");
//                             if (choicesProperty != null)
//                             {
//                                 System.Collections.IList choices = (System.Collections.IList)choicesProperty.GetValue(dialogueEntry);
//                                 if (choices != null)
//                                 {
//                                     // 重置每个选择项的isSelected标志
//                                     foreach (object choice in choices)
//                                     {
//                                         FieldInfo isSelectedField = choice.GetType().GetField("isSelected",
//                                             BindingFlags.Public | BindingFlags.Instance);

//                                         if (isSelectedField != null)
//                                         {
//                                             bool currentValue = (bool)isSelectedField.GetValue(choice);
//                                             if (currentValue)
//                                             {
//                                                 isSelectedField.SetValue(choice, false);
//                                                 resetCount++;
//                                             }
//                                         }
//                                     }
//                                 }
//                             }
//                         }
//                     }

//                 }
//             }

//             // 重置等待选择状态
//             FieldInfo isWaitingForChoiceField = typeof(DialogueManager).GetField("_isWaitingForChoice",
//                 BindingFlags.NonPublic | BindingFlags.Instance);

//             if (isWaitingForChoiceField != null)
//             {
//                 isWaitingForChoiceField.SetValue(dialogueManager, false);
//                 Debug.Log("DiedPanelController: 重置等待选择状态为false");
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError("DiedPanelController: 重置对话选项状态时出错: " + e.Message);
//         }
//     }

//     /// <summary>
//     /// 隐藏死亡面板
//     /// </summary>
//     private void HideDeathPanel()
//     {
//         gameObject.SetActive(false);
//     }

//     /// <summary>
//     /// 显示死亡面板
//     /// </summary>
//     public void ShowDeathPanel()
//     {
//         gameObject.SetActive(true);
//     }
// }