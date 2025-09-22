// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using System.IO;
// using UnityEngine.Events;



// [System.Serializable]
// public class CharacterData
// {
//     public string characterName;
//     public GameObject characterSprite;
//     public Vector3 characterPosition = Vector3.zero;
//     public float fadeSpeed = 1f;
// }

// [System.Serializable]
// public class DialogueEntry
// {
//     public string characterName;
//     public string dialogueText;
//     public bool hasChoices;
//     public List<Choice> choices = new List<Choice>();
//     public int characterExpression = 0;
//     public bool isReturnPoint; // 标记是否为返回点
//     public bool isEndPoint = false; // 标记是否为结束点
//     public string onDialogueCompleteMethod = ""; // 对话完成后要调用的方法名
// }

// [System.Serializable]
// public class Choice
// {
//     public string choiceText;
//     public int nextDialogueIndex;
//     public bool isSelected; // 标记是否已选择
// }

// public class DialogueManager : MonoBehaviour
// {
//     [Header("UI预制体")]
//     public GameObject choiceDialogueUIPrefab; // 拖入你的选择对话UI预制体
//     public GameObject hintDialogueUIPrefab; // 拖入你的提示对话UI预制体

//     [Header("通用设置")]
//     public float typingSpeed = 0.05f;
//     public string csvFileName = "soldier_dialogue.csv";

//     [Header("对话类型")]
//     // 当前对话类型
//     public bool isCurrentDialogueChoiceType = true;

//     [Header("选择类型UI")]
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public GameObject choiceTypeDialogueUI;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public Text choiceTypeDialogueText;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public Text choiceTypeCharacterNameText;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public GameObject choiceTypeDialogueBox;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public GameObject choiceButtonPrefab;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public Transform choiceContainer;

//     [Header("提示类型UI")]
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public GameObject hintTypeDialogueUI;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public Text hintTypeDialogueText;
//     [Tooltip("可手动设置或通过代码自动设置")]
//     public GameObject hintTypeDialogueBox;

//     [Header("角色系统")]
//     public List<CharacterData> characters = new List<CharacterData>();
//     public float characterFadeTime = 0.5f;

//     [Header("End of Dialogue")]
//     public UnityEvent onDialogueEnd; // 对话结束时的回调

//     [Header("Debug")]
//     public bool loadFromCSV = true;
//     public List<DialogueEntry> fallbackDialogues = new List<DialogueEntry>();

//     private List<DialogueEntry> dialogues = new List<DialogueEntry>();
//     private int currentDialogueIndex = 0;
//     private bool isTyping = false;
//     private bool isWaitingForChoice = false;
//     private bool isChangingCharacter = false;
//     private Coroutine typingCoroutine;
//     private string currentCharacterName = "";
//     private Dictionary<int, List<Choice>> availableChoices = new Dictionary<int, List<Choice>>(); // 存储每个选择点的原始选择
//     private bool dialogueActiveFlag = false; // 额外的标志来跟踪对话是否真正激活
//     private bool isUILoaded = false; // 标记UI是否已实例化

//     void Start()
//     {
//         InitializeCharacters();
//         LoadDialogues();
//         // 初始不实例化UI，等待触发时再实例化

//         // 初始化对话结束事件
//         if (onDialogueEnd == null)
//         {
//             onDialogueEnd = new UnityEvent();
//         }
//     }

//     /// <summary>
//     /// 动态实例化UI并绑定组件
//     /// </summary>
//     private void InstantiateAndBindUI()
//     {
//         Debug.Log("DialogueManager: 开始实例化所有对话UI预制体");

//         // 重置UI状态
//         choiceTypeDialogueUI = null;
//         hintTypeDialogueUI = null;

//         // 保存当前对话类型，因为我们需要实例化两种类型的UI
//         bool originalDialogueType = isCurrentDialogueChoiceType;

//         // 不再要求两种类型的UI都加载成功，只需要确保当前对话类型的UI可用
//         bool currentTypeUILoaded = false;

//         // 1. 实例化选择类型UI
//         if (choiceDialogueUIPrefab != null)
//         {
//             Debug.Log("DialogueManager: 开始实例化选择类型UI预制体");

//             // 临时设置为选择类型，以便正确绑定组件
//             isCurrentDialogueChoiceType = true;

//             // 实例化选择类型UI预制体
//             GameObject choiceUIInstance = Instantiate(choiceDialogueUIPrefab, transform);
//             choiceUIInstance.name = "ChoiceTypeDialogueUI";
//             choiceUIInstance.SetActive(false); // 初始隐藏
//             Debug.Log("DialogueManager: 选择类型UI实例已创建，名称: " + choiceUIInstance.name);

//             // 绑定选择类型UI组件
//             choiceTypeDialogueUI = choiceUIInstance;
//             Debug.Log("DialogueManager: 已绑定选择类型UI根对象");

//             choiceTypeDialogueBox = FindChildObject(choiceUIInstance, "Dialogue Box");
//             if (choiceTypeDialogueBox != null)
//             {
//                 Debug.Log("DialogueManager: 已找到Dialogue Box");
//                 choiceTypeDialogueText = FindTextComponent(choiceTypeDialogueBox, "Dialogue");
//                 if (choiceTypeDialogueText != null)
//                 {
//                     Debug.Log("DialogueManager: 已绑定Dialogue文本组件");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("DialogueManager: 未找到Dialogue文本组件");
//                 }

//                 // 角色名称文本路径：Dialogue Box -> profile -> Name bar -> Name
//                 GameObject profile = FindChildObject(choiceTypeDialogueBox, "profile");
//                 if (profile != null)
//                 {
//                     Debug.Log("DialogueManager: 已找到profile对象");
//                     GameObject nameBar = FindChildObject(profile, "Name bar");
//                     if (nameBar != null)
//                     {
//                         Debug.Log("DialogueManager: 已找到Name bar对象");
//                         choiceTypeCharacterNameText = FindTextComponent(nameBar, "Name");
//                         if (choiceTypeCharacterNameText != null)
//                         {
//                             Debug.Log("DialogueManager: 已绑定Name文本组件");
//                         }
//                         else
//                         {
//                             Debug.LogWarning("DialogueManager: 未找到Name文本组件");
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogWarning("DialogueManager: 未找到Name bar对象");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogWarning("DialogueManager: 未找到profile对象");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("DialogueManager: 未找到Dialogue Box");
//             }

//             // 选择容器路径：直接在根节点下
//             choiceContainer = FindTransform(choiceUIInstance, "ChoiceContainer");
//             if (choiceContainer != null)
//             {
//                 Debug.Log("DialogueManager: 已找到ChoiceContainer");

//                 // 查找选择按钮预制体（在ChoiceContainer下）
//                 choiceButtonPrefab = FindChildObject(choiceContainer.gameObject, "Nope");
//                 if (choiceButtonPrefab == null)
//                 {
//                     choiceButtonPrefab = FindChildObject(choiceContainer.gameObject, "Right");
//                 }

//                 if (choiceButtonPrefab != null)
//                 {
//                     Debug.Log("DialogueManager: 已找到选择按钮预制体: " + choiceButtonPrefab.name);
//                     // 创建一个新的预制体用于选择按钮（不影响原始UI）
//                     GameObject tempPrefab = Instantiate(choiceButtonPrefab);
//                     tempPrefab.name = "ChoiceButtonPrefab";
//                     choiceButtonPrefab = tempPrefab;
//                     choiceButtonPrefab.SetActive(false);
//                     // 将临时预制体设置为当前游戏对象的子对象，避免场景混乱
//                     choiceButtonPrefab.transform.SetParent(transform);
//                 }
//                 else
//                 {
//                     Debug.LogWarning("DialogueManager: 未找到选择按钮预制体");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("DialogueManager: 未找到ChoiceContainer");
//             }

//             // 初始隐藏选择类型UI
//             choiceTypeDialogueUI.SetActive(false);
//             if (choiceTypeDialogueBox != null)
//             {
//                 choiceTypeDialogueBox.SetActive(false);
//             }

//             // 如果当前对话类型是选择类型，则标记为已加载
//             if (originalDialogueType)
//             {
//                 currentTypeUILoaded = true;
//             }
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: 选择类型UI预制体未设置！");
//             // 只有当前对话类型是选择类型时才影响加载状态
//             if (originalDialogueType)
//             {
//                 currentTypeUILoaded = false;
//             }
//         }

//         // 2. 实例化提示类型UI
//         if (hintDialogueUIPrefab != null)
//         {
//             Debug.Log("DialogueManager: 开始实例化提示类型UI预制体");

//             // 临时设置为提示类型，以便正确绑定组件
//             isCurrentDialogueChoiceType = false;

//             // 实例化提示类型UI预制体
//             GameObject hintUIInstance = Instantiate(hintDialogueUIPrefab, transform);
//             hintUIInstance.name = "HintTypeDialogueUI";
//             hintUIInstance.SetActive(false); // 初始隐藏
//             Debug.Log("DialogueManager: 提示类型UI实例已创建，名称: " + hintUIInstance.name);

//             // 绑定提示类型UI组件
//             hintTypeDialogueUI = hintUIInstance;
//             Debug.Log("DialogueManager: 已绑定提示类型UI根对象");

//             // 强制设置为非null，防止后续检查失败
//             if (hintTypeDialogueUI == null)
//             {
//                 Debug.LogError("DialogueManager: 提示类型UI实例化失败，返回的对象为null！");
//                 // 只有当前对话类型是提示类型时才影响加载状态
//                 if (!originalDialogueType)
//                 {
//                     currentTypeUILoaded = false;
//                 }
//             }

//             hintTypeDialogueBox = FindChildObject(hintUIInstance, "Dialogue Box");
//             if (hintTypeDialogueBox != null)
//             {
//                 Debug.Log("DialogueManager: 已找到Dialogue Box");
//                 hintTypeDialogueText = FindTextComponent(hintTypeDialogueBox, "Dialogue");
//                 if (hintTypeDialogueText != null)
//                 {
//                     Debug.Log("DialogueManager: 已绑定Dialogue文本组件");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("DialogueManager: 未找到Dialogue文本组件");
//                 }

//                 // 提示类型UI可能也需要角色名称文本
//                 // 尝试按照相同的路径查找角色名称文本组件
//                 GameObject profile = FindChildObject(hintTypeDialogueBox, "profile");
//                 if (profile != null)
//                 {
//                     Debug.Log("DialogueManager: 已找到profile对象");
//                     GameObject nameBar = FindChildObject(profile, "Name bar");
//                     if (nameBar != null)
//                     {
//                         Debug.Log("DialogueManager: 已找到Name bar对象");
//                         // 可以为提示类型UI添加一个专门的角色名称文本字段
//                         // 但目前代码中没有hintTypeCharacterNameText字段，所以这里不设置
//                     }
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("DialogueManager: 未找到Dialogue Box");
//             }

//             // 初始隐藏提示类型UI
//             hintTypeDialogueUI.SetActive(false);
//             if (hintTypeDialogueBox != null)
//             {
//                 hintTypeDialogueBox.SetActive(false);
//             }

//             // 如果当前对话类型是提示类型，则标记为已加载
//             if (!originalDialogueType)
//             {
//                 currentTypeUILoaded = true;
//             }
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: 提示类型UI预制体未设置！");
//             // 只有当前对话类型是提示类型时才影响加载状态
//             if (!originalDialogueType)
//             {
//                 currentTypeUILoaded = false;
//             }
//         }

//         // 恢复原始对话类型
//         isCurrentDialogueChoiceType = originalDialogueType;

//         // 设置UI加载状态，现在只需要当前对话类型的UI加载成功即可
//         isUILoaded = currentTypeUILoaded;

//         Debug.Log("DialogueManager: UI实例化和绑定完成，isUILoaded = " + isUILoaded);
//         if (!isUILoaded)
//         {
//             Debug.LogWarning("DialogueManager: 当前对话类型的UI预制体未成功加载，请检查Unity Inspector中的设置。");
//         }
//     }

//     /// <summary>
//     /// 辅助方法：在子对象中查找游戏对象
//     /// </summary>
//     private GameObject FindChildObject(GameObject parent, string name)
//     {
//         if (parent == null)
//             return null;

//         Transform child = parent.transform.Find(name);
//         return child != null ? child.gameObject : null;
//     }

//     /// <summary>
//     /// 辅助方法：查找文本组件
//     /// </summary>
//     private Text FindTextComponent(GameObject parent, string childName)
//     {
//         GameObject child = FindChildObject(parent, childName);
//         if (child != null)
//         {
//             return child.GetComponent<Text>();
//         }
//         return null;
//     }

//     /// <summary>
//     /// 辅助方法：查找变换组件
//     /// </summary>
//     private Transform FindTransform(GameObject parent, string childName)
//     {
//         if (parent == null)
//             return null;

//         return parent.transform.Find(childName);
//     }

//     private void HideAllDialogueUI()
//     {
//         Debug.Log("DialogueManager: 隐藏所有对话UI");
//         if (choiceTypeDialogueUI != null)
//         {
//             choiceTypeDialogueUI.SetActive(false);
//         }

//         if (hintTypeDialogueUI != null)
//         {
//             hintTypeDialogueUI.SetActive(false);
//         }

//         dialogueActiveFlag = false;
//     }

//     void Update()
//     {
//         // 先检查dialogueActiveFlag
//         if (!dialogueActiveFlag)
//         {
//             // 检查UI状态
//             bool uiActive = false;
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueBox != null)
//             {
//                 uiActive = choiceTypeDialogueBox.activeSelf;
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueBox != null)
//             {
//                 uiActive = hintTypeDialogueBox.activeSelf;
//             }

//             if (uiActive)
//             {
//                 // UI已经显示但dialogueActiveFlag为false，立即设置为true
//                 Debug.Log("DialogueManager: UI显示但dialogueActiveFlag为false，立即设置为true");
//                 dialogueActiveFlag = true;
//             }
//             else
//             {
//                 // 对话确实未激活，直接返回
//                 return;
//             }
//         }
//         else
//         {
//             // 对话被标记为活动，但UI可能未正确显示，进行额外检查
//             bool uiActuallyActive = false;
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueBox != null)
//             {
//                 uiActuallyActive = choiceTypeDialogueBox.activeSelf;
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueBox != null)
//             {
//                 uiActuallyActive = hintTypeDialogueBox.activeSelf;
//             }

//             // 如果对话应该活动但UI未显示，尝试重新激活UI
//             if (!uiActuallyActive)
//             {
//                 Debug.LogWarning("DialogueManager: dialogueActiveFlag为true但UI未显示，尝试重新激活UI...");
//                 if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
//                 {
//                     choiceTypeDialogueUI.SetActive(true);
//                     choiceTypeDialogueUI.transform.SetAsLastSibling(); // 确保UI在最上层显示
//                     if (choiceTypeDialogueBox != null)
//                     {
//                         choiceTypeDialogueBox.SetActive(true);
//                         choiceTypeDialogueBox.transform.SetAsLastSibling(); // 确保对话框在最上层显示
//                     }
//                 }
//                 else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
//                 {
//                     hintTypeDialogueUI.SetActive(true);
//                     hintTypeDialogueUI.transform.SetAsLastSibling(); // 确保UI在最上层显示
//                     if (hintTypeDialogueBox != null)
//                     {
//                         hintTypeDialogueBox.SetActive(true);
//                         hintTypeDialogueBox.transform.SetAsLastSibling(); // 确保对话框在最上层显示
//                     }
//                 }
//             }
//         }

//         // 处理用户输入
//         if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
//             && !isWaitingForChoice && !isChangingCharacter)
//         {
//             Debug.Log("DialogueManager: 检测到鼠标点击或空格键，执行对话操作");
//             if (isTyping)
//             {
//                 CompleteTyping();
//             }
//             else
//             {
//                 NextDialogue();
//             }
//         }

//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             ReloadCSV();
//         }
//     }

//     // 检查对话是否正在进行
//     public bool IsDialogueActive()
//     {
//         if (!isUILoaded)
//             return false;

//         bool choiceTypeActive = isCurrentDialogueChoiceType &&
//                                 choiceTypeDialogueBox != null &&
//                                 choiceTypeDialogueBox.activeSelf;

//         bool hintTypeActive = !isCurrentDialogueChoiceType &&
//                               hintTypeDialogueBox != null &&
//                               hintTypeDialogueBox.activeSelf;

//         bool result = choiceTypeActive || hintTypeActive;

//         // 仅输出调试信息，不再直接修改dialogueActiveFlag
//         // 对话状态的管理现在由Update方法和其他专门的激活逻辑处理
//         if (result && !dialogueActiveFlag)
//         {
//             Debug.Log("DialogueManager: UI显示但dialogueActiveFlag为false");
//         }
//         else if (!result && dialogueActiveFlag)
//         {
//             Debug.Log("DialogueManager: UI隐藏但dialogueActiveFlag为true");
//         }

//         return result;
//     }

//     /// <summary>
//     /// 重写StartDialogue，确保UI已实例化
//     /// </summary>
//     public void StartDialogue()
//     {
//         if (!isUILoaded)
//         {
//             InstantiateAndBindUI(); // 首次触发时实例化UI
//         }
//         StartDialogueAt(0);
//     }

//     /// <summary>
//     /// 加载指定的CSV文件并开始对话
//     /// </summary>
//     /// <param name="csvFileName">CSV文件名</param>
//     public void StartDialogue(string csvFileName)
//     {
//         SetDialogueCSVFile(csvFileName);
//         if (!isUILoaded)
//         {
//             InstantiateAndBindUI(); // 首次触发时实例化UI
//         }
//         StartDialogueAt(0);
//     }

//     /// <summary>
//     /// 从指定索引开始对话
//     /// </summary>
//     /// <param name="index">对话索引</param>
//     public void StartDialogueAt(int index)
//     {
//         // 记录当前对话类型
//         string dialogueType = isCurrentDialogueChoiceType ? "选择类型" : "提示类型";
//         Debug.Log("DialogueManager: 开始准备对话 - 类型: " + dialogueType + ", 索引: " + index);

//         // 保存当前对话类型状态
//         bool currentType = isCurrentDialogueChoiceType;
//         Debug.Log("DialogueManager: 保存当前对话类型状态: " + (currentType ? "选择类型" : "提示类型"));

//         // 确保UI已实例化
//         Debug.Log("DialogueManager: 检查UI是否已实例化，当前isUILoaded: " + isUILoaded);
//         if (!isUILoaded)
//         {
//             Debug.Log("DialogueManager: UI未实例化，准备调用InstantiateAndBindUI");

//             // 确保在实例化UI前已经正确设置了对话类型
//             if (currentType)
//             {
//                 Debug.Log("DialogueManager: 将使用选择类型UI预制体");
//             }
//             else
//             {
//                 Debug.Log("DialogueManager: 将使用提示类型UI预制体");
//             }

//             InstantiateAndBindUI();

//             // 检查实例化是否成功
//             if (!isUILoaded)
//             {
//                 Debug.LogError("DialogueManager: UI实例化失败，无法启动对话！");
//                 return;
//             }
//         }
//         else
//         {
//             Debug.Log("DialogueManager: UI已实例化，跳过InstantiateAndBindUI");
//         }

//         // 验证对话数据是否存在
//         if (dialogues == null || dialogues.Count == 0)
//         {
//             Debug.LogError("DialogueManager: 对话数据为空，请先加载对话文件！当前CSV文件: " + csvFileName);
//             return;
//         }

//         // 验证索引是否有效
//         if (index < 0 || index >= dialogues.Count)
//         {
//             Debug.LogError(string.Format("DialogueManager: 对话索引 {0} 超出范围 [0, {1}]", index, dialogues.Count - 1));
//             return;
//         }

//         // 设置当前对话索引
//         currentDialogueIndex = Mathf.Clamp(index, 0, dialogues.Count - 1);
//         Debug.Log("DialogueManager: 设置当前对话索引为: " + currentDialogueIndex);

//         // 更新UI并显示对话
//         Debug.Log("DialogueManager: 调用UpdateDialogueUI更新UI配置");
//         UpdateDialogueUI();

//         // 确保对话类型仍然是正确的（防止在UpdateDialogueUI中被意外修改）
//         isCurrentDialogueChoiceType = currentType;
//         Debug.Log("DialogueManager: 确认对话类型为: " + (isCurrentDialogueChoiceType ? "选择类型" : "提示类型"));

//         // 强制激活正确的UI容器，确保在调用ShowDialogue前UI已经准备好
//         if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
//         {
//             choiceTypeDialogueUI.SetActive(true);
//             if (choiceTypeDialogueBox != null)
//             {
//                 choiceTypeDialogueBox.SetActive(true);
//             }
//             Debug.Log("DialogueManager: 已预激活选择类型UI");
//         }
//         else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
//         {
//             hintTypeDialogueUI.SetActive(true);
//             if (hintTypeDialogueBox != null)
//             {
//                 hintTypeDialogueBox.SetActive(true);
//             }
//             Debug.Log("DialogueManager: 已预激活提示类型UI");
//         }

//         Debug.Log("DialogueManager: 调用ShowDialogue显示对话内容");
//         ShowDialogue();

//         // 添加额外的重试机制，确保UI正确显示
//         if (dialogueActiveFlag == false)
//         {
//             Debug.LogWarning("DialogueManager: 第一次ShowDialogue调用后对话仍未激活，尝试重试...");
//             // 直接重试ShowDialogue
//             ShowDialogue();
//         }

//         // 移除立即检查dialogueActiveFlag的代码，因为ShowDialogue中的设置需要时间完成
//         // 使用协程延迟一帧后再检查对话激活状态
//         StartCoroutine(CheckDialogueActiveAfterFrame());
//     }

//     // 增强版对话激活检查协程，多次尝试确保UI显示
//     private IEnumerator CheckDialogueActiveAfterFrame()
//     {
//         const int maxRetries = 5; // 最大重试次数
//         const float retryInterval = 0.1f; // 重试间隔（秒）
//         int retryCount = 0;

//         // 在短时间内多次检查对话激活状态
//         while (!dialogueActiveFlag && retryCount < maxRetries)
//         {
//             retryCount++;

//             // 检查实际的UI状态
//             bool uiActuallyActive = false;
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueBox != null)
//             {
//                 uiActuallyActive = choiceTypeDialogueBox.activeSelf;
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueBox != null)
//             {
//                 uiActuallyActive = hintTypeDialogueBox.activeSelf;
//             }

//             if (uiActuallyActive)
//             {
//                 // UI已经显示但dialogueActiveFlag为false，立即设置为true
//                 Debug.Log("DialogueManager: 对话启动后第" + retryCount + "次检查，UI实际已显示，立即设置dialogueActiveFlag为true");
//                 dialogueActiveFlag = true;
//                 break;
//             }

//             Debug.LogWarning("DialogueManager: 对话启动后第" + retryCount + "次检查，dialogueActiveFlag仍为false且UI未显示，尝试重新激活...");

//             // 尝试重新激活对话
//             if (isUILoaded && currentDialogueIndex < dialogues.Count)
//             {
//                 Debug.Log("DialogueManager: 第" + retryCount + "次尝试重新激活对话...");
//                 ShowDialogue();
//             }

//             // 等待一段时间后再检查
//             yield return new WaitForSeconds(retryInterval);
//         }

//         // 最后一次检查
//         if (!dialogueActiveFlag)
//         {
//             Debug.LogError("DialogueManager: 在" + maxRetries + "次尝试后，对话仍未成功激活！请检查UI预制体设置和Canvas层级。");

//             // 最后尝试：强制设置dialogueActiveFlag为true并手动显示UI
//             dialogueActiveFlag = true;
//             Debug.LogWarning("DialogueManager: 强制设置dialogueActiveFlag为true，并最后一次尝试显示UI...");

//             // 强制激活UI
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
//             {
//                 choiceTypeDialogueUI.SetActive(true);
//                 choiceTypeDialogueUI.transform.SetAsLastSibling();
//                 if (choiceTypeDialogueBox != null)
//                 {
//                     choiceTypeDialogueBox.SetActive(true);
//                     choiceTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
//             {
//                 hintTypeDialogueUI.SetActive(true);
//                 hintTypeDialogueUI.transform.SetAsLastSibling();
//                 if (hintTypeDialogueBox != null)
//                 {
//                     hintTypeDialogueBox.SetActive(true);
//                     hintTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("DialogueManager: 对话成功激活！");
//         }
//     }

//     // 显示对话
//     void ShowDialogue()
//     {
//         // 记录当前对话类型
//         string dialogueType = isCurrentDialogueChoiceType ? "选择类型" : "提示类型";
//         Debug.Log("DialogueManager: 开始显示" + dialogueType + "对话，索引: " + currentDialogueIndex);

//         // 确保UI已经加载
//         if (!isUILoaded)
//         {
//             Debug.LogWarning("DialogueManager: UI尚未加载完成！尝试重新实例化UI...");
//             InstantiateAndBindUI();
//             // 如果重新实例化后仍未加载成功，则返回
//             if (!isUILoaded)
//             {
//                 Debug.LogError("DialogueManager: 重新实例化UI失败！");
//                 return;
//             }
//         }

//         if (currentDialogueIndex >= dialogues.Count)
//         {
//             Debug.Log("DialogueManager: 对话索引超出范围，结束对话");
//             EndDialogue();
//             return;
//         }

//         // 确保UI可见
//         bool uiActivationSuccessful = false;

//         // 增强版UI激活逻辑
//         if (isCurrentDialogueChoiceType)
//         {
//             if (choiceTypeDialogueUI == null)
//             {
//                 Debug.LogError("DialogueManager: 选择类型UI未初始化！");
//                 // 尝试重新实例化UI
//                 Debug.Log("DialogueManager: 尝试重新实例化选择类型UI...");
//                 InstantiateAndBindUI();
//                 if (choiceTypeDialogueUI == null)
//                 {
//                     Debug.LogError("DialogueManager: 重新实例化选择类型UI失败！");
//                     return;
//                 }
//             }
//             else
//             {
//                 Debug.Log("DialogueManager: 选择类型UI对象检查 - 名称: " + choiceTypeDialogueUI.name + ", 活跃状态: " + choiceTypeDialogueUI.activeSelf);
//             }

//             Debug.Log("DialogueManager: 激活选择类型UI");
//             choiceTypeDialogueUI.SetActive(true);
//             choiceTypeDialogueUI.transform.SetAsLastSibling(); // 确保UI在最上层显示
//             Debug.Log("DialogueManager: 选择类型UI激活后状态: " + choiceTypeDialogueUI.activeSelf);

//             if (choiceTypeDialogueBox != null)
//             {
//                 Debug.Log("DialogueManager: 选择类型对话框检查 - 名称: " + choiceTypeDialogueBox.name + ", 活跃状态: " + choiceTypeDialogueBox.activeSelf);
//                 Debug.Log("DialogueManager: 显示对话框");
//                 choiceTypeDialogueBox.SetActive(true);
//                 choiceTypeDialogueBox.transform.SetAsLastSibling(); // 确保对话框在最上层显示
//                 Debug.Log("DialogueManager: 选择类型对话框激活后状态: " + choiceTypeDialogueBox.activeSelf);
//                 uiActivationSuccessful = true;
//             }
//             else
//             {
//                 Debug.LogError("DialogueManager: choiceTypeDialogueBox为null！");
//             }
//         }
//         else
//         {
//             if (hintTypeDialogueUI == null)
//             {
//                 Debug.LogError("DialogueManager: 提示类型UI未初始化！");
//                 // 尝试重新实例化UI
//                 Debug.Log("DialogueManager: 尝试重新实例化提示类型UI...");
//                 InstantiateAndBindUI();
//                 if (hintTypeDialogueUI == null)
//                 {
//                     Debug.LogError("DialogueManager: 重新实例化提示类型UI失败！");
//                     return;
//                 }
//             }
//             else
//             {
//                 Debug.Log("DialogueManager: 提示类型UI对象检查 - 名称: " + hintTypeDialogueUI.name + ", 活跃状态: " + hintTypeDialogueUI.activeSelf);
//             }

//             Debug.Log("DialogueManager: 激活提示类型UI");
//             hintTypeDialogueUI.SetActive(true);
//             hintTypeDialogueUI.transform.SetAsLastSibling(); // 确保UI在最上层显示
//             Debug.Log("DialogueManager: 提示类型UI激活后状态: " + hintTypeDialogueUI.activeSelf);

//             if (hintTypeDialogueBox != null)
//             {
//                 Debug.Log("DialogueManager: 提示类型对话框检查 - 名称: " + hintTypeDialogueBox.name + ", 活跃状态: " + hintTypeDialogueBox.activeSelf);
//                 Debug.Log("DialogueManager: 显示对话框");
//                 hintTypeDialogueBox.SetActive(true);
//                 hintTypeDialogueBox.transform.SetAsLastSibling(); // 确保对话框在最上层显示
//                 Debug.Log("DialogueManager: 提示类型对话框激活后状态: " + hintTypeDialogueBox.activeSelf);
//                 uiActivationSuccessful = true;
//             }
//             else
//             {
//                 Debug.LogError("DialogueManager: hintTypeDialogueBox为null！");
//             }
//         }

//         // 添加额外的UI可见性检查
//         if (!uiActivationSuccessful)
//         {
//             Debug.LogWarning("DialogueManager: UI激活标志为false，但尝试强制显示UI...");
//             // 强制再次激活UI元素
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
//             {
//                 choiceTypeDialogueUI.SetActive(true);
//                 choiceTypeDialogueUI.transform.SetAsLastSibling();
//                 if (choiceTypeDialogueBox != null)
//                 {
//                     choiceTypeDialogueBox.SetActive(true);
//                     choiceTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
//             {
//                 hintTypeDialogueUI.SetActive(true);
//                 hintTypeDialogueUI.transform.SetAsLastSibling();
//                 if (hintTypeDialogueBox != null)
//                 {
//                     hintTypeDialogueBox.SetActive(true);
//                     hintTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//         }

//         // 立即设置对话状态为活动，无论UI激活是否成功
//         dialogueActiveFlag = true;
//         Debug.Log("DialogueManager: 对话状态已设为活动 (dialogueActiveFlag = true)");

//         // 添加额外的UI可见性检查
//         if (!uiActivationSuccessful)
//         {
//             Debug.LogWarning("DialogueManager: UI激活标志为false，但尝试强制显示UI...");
//             // 强制再次激活UI元素
//             if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
//             {
//                 choiceTypeDialogueUI.SetActive(true);
//                 choiceTypeDialogueUI.transform.SetAsLastSibling();
//                 if (choiceTypeDialogueBox != null)
//                 {
//                     choiceTypeDialogueBox.SetActive(true);
//                     choiceTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//             else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
//             {
//                 hintTypeDialogueUI.SetActive(true);
//                 hintTypeDialogueUI.transform.SetAsLastSibling();
//                 if (hintTypeDialogueBox != null)
//                 {
//                     hintTypeDialogueBox.SetActive(true);
//                     hintTypeDialogueBox.transform.SetAsLastSibling();
//                 }
//             }
//         }

//         DialogueEntry currentEntry = dialogues[currentDialogueIndex];

//         // 输出当前对话信息
//         Debug.Log("DialogueManager: 显示对话 - 角色: " + currentEntry.characterName + ", 文本: " + currentEntry.dialogueText);

//         StartCoroutine(ChangeCharacter(currentEntry.characterName, () =>
//         {
//             Text nameText = GetCurrentCharacterNameText();
//             if (nameText != null)
//             {
//                 nameText.text = currentEntry.characterName;
//             }

//             ClearChoiceButtons();

//             if (typingCoroutine != null)
//             {
//                 StopCoroutine(typingCoroutine);
//             }
//             typingCoroutine = StartCoroutine(TypeText(currentEntry.dialogueText));

//             // 只有选择类型UI才显示选择按钮
//             if (isCurrentDialogueChoiceType && currentEntry.hasChoices && currentEntry.choices.Count > 0)
//             {
//                 Debug.Log("DialogueManager: 显示" + currentEntry.choices.Count + "个选择项");
//                 isWaitingForChoice = true;
//                 StartCoroutine(ShowChoicesAfterTyping(currentEntry.choices));
//             }
//             else
//             {
//                 isWaitingForChoice = false;
//             }
//         }));
//     }

//     // 结束对话
//     void EndDialogue()
//     {
//         Debug.Log("DialogueManager: 执行EndDialogue方法");
//         // 清除所有选项按钮
//         ClearChoiceButtons();

//         // 禁用整个对话UI（包括对话框和遮罩）- 无论当前对话类型是什么，都隐藏所有可能的UI元素
//         if (choiceTypeDialogueUI != null)
//         {
//             choiceTypeDialogueUI.SetActive(false);
//         }
//         if (choiceTypeDialogueBox != null)
//         {
//             choiceTypeDialogueBox.SetActive(false);
//         }
//         if (hintTypeDialogueUI != null)
//         {
//             hintTypeDialogueUI.SetActive(false);
//         }
//         if (hintTypeDialogueBox != null)
//         {
//             hintTypeDialogueBox.SetActive(false);
//         }

//         if (!string.IsNullOrEmpty(currentCharacterName))
//         {
//             CharacterData currentChar = characters.Find(c => c.characterName == currentCharacterName);
//             if (currentChar != null && currentChar.characterSprite != null)
//             {
//                 StartCoroutine(FadeCharacter(currentChar.characterSprite, false));
//             }
//         }

//         // 重置对话索引，防止对话被反复触发
//         currentDialogueIndex = 0;

//         Debug.Log("DialogueManager: 对话结束，触发结束事件");
//         dialogueActiveFlag = false;
//         // 触发对话结束事件
//         onDialogueEnd.Invoke();

//         // 确保所有UI都被隐藏
//         HideAllDialogueUI();
//     }

//     void LoadDialogues()
//     {
//         if (loadFromCSV)
//         {
//             LoadDialoguesFromCSV();
//         }
//         else
//         {
//             dialogues = fallbackDialogues;
//         }
//     }

//     /// <summary>
//     /// 设置对话CSV文件
//     /// </summary>
//     /// <param name="newCSVFileName">新的CSV文件名</param>
//     public void SetDialogueCSVFile(string newCSVFileName)
//     {
//         if (csvFileName != newCSVFileName)
//         {
//             csvFileName = newCSVFileName;
//             LoadDialogues();
//             Debug.Log("对话文件已更改为: " + newCSVFileName);
//         }
//     }

//     // 设置对话类型（选择类型或提示类型）
//     public void SetDialogueType(bool isChoiceType)
//     {
//         isCurrentDialogueChoiceType = isChoiceType;
//         UpdateDialogueUI();
//     }

//     // 更新对话UI显示
//     private void UpdateDialogueUI()
//     {
//         Debug.Log("DialogueManager: 开始更新对话UI，当前对话类型: " + (isCurrentDialogueChoiceType ? "选择类型" : "提示类型"));

//         // 检查UI是否已经被实例化
//         if (!isUILoaded)
//         {
//             Debug.LogWarning("DialogueManager: UI尚未加载完成，尝试重新实例化UI...");
//             InstantiateAndBindUI();
//             // 如果重新实例化后仍未加载成功，则返回
//             if (!isUILoaded)
//             {
//                 Debug.LogError("DialogueManager: 重新实例化UI失败，无法更新UI显示！");
//                 return;
//             }
//         }

//         // 激活对应的UI容器
//         if (choiceTypeDialogueUI != null)
//         {
//             Debug.Log("DialogueManager: 设置选择类型UI容器激活状态: " + isCurrentDialogueChoiceType);
//             choiceTypeDialogueUI.SetActive(isCurrentDialogueChoiceType);
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: choiceTypeDialogueUI为null");
//         }

//         if (hintTypeDialogueUI != null)
//         {
//             Debug.Log("DialogueManager: 设置提示类型UI容器激活状态: " + (!isCurrentDialogueChoiceType));
//             hintTypeDialogueUI.SetActive(!isCurrentDialogueChoiceType);
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: hintTypeDialogueUI为null");
//         }

//         // 更新对话框显示状态
//         if (choiceTypeDialogueBox != null)
//         {
//             Debug.Log("DialogueManager: 设置选择类型对话框激活状态: " + isCurrentDialogueChoiceType);
//             choiceTypeDialogueBox.SetActive(isCurrentDialogueChoiceType);
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: choiceTypeDialogueBox为null");
//         }

//         if (hintTypeDialogueBox != null)
//         {
//             Debug.Log("DialogueManager: 设置提示类型对话框激活状态: " + (!isCurrentDialogueChoiceType));
//             hintTypeDialogueBox.SetActive(!isCurrentDialogueChoiceType);
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager: hintTypeDialogueBox为null");
//         }

//         // 额外检查：确保当前应该显示的UI组件不为null
//         if (isCurrentDialogueChoiceType && choiceTypeDialogueUI == null)
//         {
//             Debug.LogError("DialogueManager: 选择类型对话需要显示，但choiceTypeDialogueUI为null！");
//         }
//         else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI == null)
//         {
//             Debug.LogError("DialogueManager: 提示类型对话需要显示，但hintTypeDialogueUI为null！");
//         }

//         Debug.Log("DialogueManager: 对话UI更新完成");
//     }

//     // 获取当前使用的对话文本组件
//     private Text GetCurrentDialogueText()
//     {
//         return isCurrentDialogueChoiceType ? choiceTypeDialogueText : hintTypeDialogueText;
//     }

//     // 获取当前使用的角色名称文本组件
//     private Text GetCurrentCharacterNameText()
//     {
//         return isCurrentDialogueChoiceType ? choiceTypeCharacterNameText : null;
//     }

//     void LoadDialoguesFromCSV()
//     {
//         string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);

//         if (File.Exists(filePath))
//         {
//             try
//             {
//                 string[] lines = File.ReadAllLines(filePath);
//                 dialogues.Clear();
//                 availableChoices.Clear();

//                 for (int i = 1; i < lines.Length; i++)
//                 {
//                     string[] values = SplitCSVLine(lines[i]);

//                     if (values.Length >= 7)
//                     {
//                         DialogueEntry entry = new DialogueEntry();
//                         entry.characterName = values[0];
//                         entry.dialogueText = values[1];

//                         if (bool.TryParse(values[2], out bool hasChoices))
//                         {
//                             entry.hasChoices = hasChoices;
//                         }

//                         if (entry.hasChoices)
//                         {
//                             if (!string.IsNullOrEmpty(values[3]) && int.TryParse(values[4], out int choice1Index))
//                             {
//                                 Choice choice1 = new Choice();
//                                 choice1.choiceText = values[3];
//                                 choice1.nextDialogueIndex = choice1Index;
//                                 choice1.isSelected = false;
//                                 entry.choices.Add(choice1);
//                             }

//                             if (!string.IsNullOrEmpty(values[5]) && int.TryParse(values[6], out int choice2Index))
//                             {
//                                 Choice choice2 = new Choice();
//                                 choice2.choiceText = values[5];
//                                 choice2.nextDialogueIndex = choice2Index;
//                                 choice2.isSelected = false;
//                                 entry.choices.Add(choice2);
//                             }

//                             // 存储每个选择点的原始选择
//                             availableChoices[i] = new List<Choice>(entry.choices);
//                         }

//                         if (values.Length >= 8 && int.TryParse(values[7], out int expression))
//                         {
//                             entry.characterExpression = expression;
//                         }

//                         // 标记是否为返回点(值为-1时)
//                         if (values.Length >= 9 && int.TryParse(values[8], out int isReturn) && isReturn == -1)
//                         {
//                             entry.isReturnPoint = true;
//                         }

//                         // 标记是否为结束点(值为1时) - 现在在第10个字段
//                         if (values.Length >= 10 && int.TryParse(values[9], out int isEnd) && isEnd == 1)
//                         {
//                             entry.isEndPoint = true;
//                         }

//                         // 读取对话完成后要调用的方法名（第11个字段）
//                         if (values.Length >= 11)
//                         {
//                             entry.onDialogueCompleteMethod = values[10];
//                         }

//                         dialogues.Add(entry);
//                     }
//                 }

//                 Debug.Log($"成功加载 {dialogues.Count} 条对话");
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"加载CSV文件失败: {e.Message}");
//                 dialogues = fallbackDialogues;
//             }
//         }
//         else
//         {
//             Debug.LogWarning($"CSV文件不存在: {filePath.Replace("\\", "\\\\")}");
//             dialogues = fallbackDialogues;
//         }
//     }

//     string[] SplitCSVLine(string line)
//     {
//         List<string> result = new List<string>();
//         bool inQuotes = false;
//         string currentField = "";

//         for (int i = 0; i < line.Length; i++)
//         {
//             char c = line[i];

//             if (c == '"' && (i == 0 || line[i - 1] != '\\'))
//             {
//                 inQuotes = !inQuotes;
//             }
//             else if (c == ',' && !inQuotes)
//             {
//                 result.Add(currentField);
//                 currentField = "";
//             }
//             else
//             {
//                 currentField += c;
//             }
//         }

//         result.Add(currentField);
//         return result.ToArray();
//     }

//     [ContextMenu("重新加载CSV文件")]
//     public void ReloadCSV()
//     {
//         LoadDialogues();
//         Debug.Log("CSV文件已重新加载");
//     }

//     void InitializeCharacters()
//     {
//         foreach (CharacterData character in characters)
//         {
//             if (character.characterSprite != null)
//             {
//                 character.characterSprite.SetActive(false);
//             }
//         }
//     }



//     void CompleteTyping()
//     {
//         if (typingCoroutine != null)
//         {
//             StopCoroutine(typingCoroutine);
//         }

//         Text currentText = GetCurrentDialogueText();
//         if (currentText != null)
//         {
//             currentText.text = dialogues[currentDialogueIndex].dialogueText;
//         }
//         isTyping = false;
//     }

//     public void NextDialogue()
//     {
//         // 确保UI已实例化
//         if (!isUILoaded)
//         {
//             InstantiateAndBindUI();
//         }

//         // 检查是否是结束点
//         if (currentDialogueIndex < dialogues.Count && dialogues[currentDialogueIndex].isEndPoint)
//         {
//             // 执行结束对话逻辑
//             EndDialogue();
//             return;
//         }

//         // 检查是否是返回点
//         if (currentDialogueIndex < dialogues.Count && dialogues[currentDialogueIndex].isReturnPoint)
//         {
//             // 返回到选择点并显示未选择的选项
//             ReturnToChoicePoint();
//             return;
//         }

//         currentDialogueIndex++;
//         ShowDialogue();
//     }

//     void ReturnToChoicePoint()
//     {
//         // 查找最近的选择点
//         int choicePointIndex = -1;
//         for (int i = currentDialogueIndex; i >= 0; i--)
//         {
//             if (dialogues[i].hasChoices)
//             {
//                 choicePointIndex = i;
//                 break;
//             }
//         }

//         if (choicePointIndex != -1 && availableChoices.ContainsKey(choicePointIndex))
//         {
//             // 获取未选择的选项
//             List<Choice> remainingChoices = new List<Choice>();
//             foreach (Choice choice in availableChoices[choicePointIndex])
//             {
//                 if (!choice.isSelected)
//                 {
//                     remainingChoices.Add(choice);
//                 }
//             }

//             if (remainingChoices.Count > 0)
//             {
//                 // 返回到选择点并显示剩余选项
//                 currentDialogueIndex = choicePointIndex;
//                 dialogues[currentDialogueIndex].choices = remainingChoices;
//                 ShowDialogue();
//             }
//             else
//             {
//                 // 所有选项都已选择，继续下一段对话
//                 currentDialogueIndex++;
//                 ShowDialogue();
//             }
//         }
//         else
//         {
//             // 没有找到选择点，继续下一段对话
//             currentDialogueIndex++;
//             ShowDialogue();
//         }
//     }

//     IEnumerator ChangeCharacter(string newCharacterName, System.Action onComplete = null)
//     {
//         if (currentCharacterName == newCharacterName)
//         {
//             onComplete?.Invoke();
//             yield break;
//         }

//         isChangingCharacter = true;

//         if (!string.IsNullOrEmpty(currentCharacterName))
//         {
//             CharacterData currentChar = characters.Find(c => c.characterName == currentCharacterName);
//             if (currentChar != null && currentChar.characterSprite != null)
//             {
//                 yield return StartCoroutine(FadeCharacter(currentChar.characterSprite, false));
//                 currentChar.characterSprite.SetActive(false);
//             }
//         }

//         CharacterData newChar = characters.Find(c => c.characterName == newCharacterName);
//         if (newChar != null && newChar.characterSprite != null)
//         {
//             newChar.characterSprite.SetActive(true);
//             newChar.characterSprite.transform.localPosition = newChar.characterPosition;
//             yield return StartCoroutine(FadeCharacter(newChar.characterSprite, true));
//         }

//         currentCharacterName = newCharacterName;
//         isChangingCharacter = false;

//         onComplete?.Invoke();
//     }

//     IEnumerator FadeCharacter(GameObject character, bool fadeIn)
//     {
//         Image characterImage = character.GetComponent<Image>();
//         if (characterImage == null)
//         {
//             SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
//             if (spriteRenderer != null)
//             {
//                 yield return StartCoroutine(FadeSpriteRenderer(spriteRenderer, fadeIn));
//             }
//             yield break;
//         }

//         float startAlpha = fadeIn ? 0f : 1f;
//         float targetAlpha = fadeIn ? 1f : 0f;
//         float elapsedTime = 0f;

//         Color color = characterImage.color;
//         color.a = startAlpha;
//         characterImage.color = color;

//         while (elapsedTime < characterFadeTime)
//         {
//             elapsedTime += Time.deltaTime;
//             float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / characterFadeTime);
//             color.a = alpha;
//             characterImage.color = color;
//             yield return null;
//         }

//         color.a = targetAlpha;
//         characterImage.color = color;
//     }

//     IEnumerator FadeSpriteRenderer(SpriteRenderer spriteRenderer, bool fadeIn)
//     {
//         float startAlpha = fadeIn ? 0f : 1f;
//         float targetAlpha = fadeIn ? 1f : 0f;
//         float elapsedTime = 0f;

//         Color color = spriteRenderer.color;
//         color.a = startAlpha;
//         spriteRenderer.color = color;

//         while (elapsedTime < characterFadeTime)
//         {
//             elapsedTime += Time.deltaTime;
//             float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / characterFadeTime);
//             color.a = alpha;
//             spriteRenderer.color = color;
//             yield return null;
//         }

//         color.a = targetAlpha;
//         spriteRenderer.color = color;
//     }

//     IEnumerator TypeText(string text)
//     {
//         isTyping = true;

//         Text currentText = GetCurrentDialogueText();
//         if (currentText != null)
//         {
//             currentText.text = "";

//             foreach (char letter in text.ToCharArray())
//             {
//                 currentText.text += letter;
//                 yield return new WaitForSeconds(typingSpeed);
//             }
//         }

//         isTyping = false;
//     }

//     IEnumerator ShowChoicesAfterTyping(List<Choice> choices)
//     {
//         while (isTyping)
//         {
//             yield return null;
//         }

//         if (choiceButtonPrefab == null || choiceContainer == null)
//         {
//             Debug.LogError("DialogueManager: 缺少选择按钮预制体或选择容器组件");
//             yield break;
//         }

//         // 检查choiceContainer是否有VerticalLayoutGroup组件，如果没有则添加
//         if (choiceContainer.GetComponent<VerticalLayoutGroup>() == null)
//         {
//             VerticalLayoutGroup layoutGroup = choiceContainer.gameObject.AddComponent<VerticalLayoutGroup>();
//             layoutGroup.childAlignment = TextAnchor.UpperCenter;
//             layoutGroup.spacing = 30; // 设置按钮间距
//             layoutGroup.childForceExpandWidth = false; // 不强制拉伸按钮宽度
//             layoutGroup.childForceExpandHeight = false; // 不强制拉伸按钮高度
//             layoutGroup.childControlWidth = false; // 不控制子物体宽度
//             layoutGroup.childControlHeight = false; // 不控制子物体高度
//         }

//         // 检查choiceContainer是否有ContentSizeFitter组件，如果没有则添加
//         if (choiceContainer.GetComponent<ContentSizeFitter>() == null)
//         {
//             ContentSizeFitter sizeFitter = choiceContainer.gameObject.AddComponent<ContentSizeFitter>();
//             sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//             sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
//         }

//         // 获取原始按钮预制体的尺寸
//         RectTransform prefabRect = choiceButtonPrefab.GetComponent<RectTransform>();
//         Vector2 originalSize = Vector2.zero;
//         if (prefabRect != null)
//         {
//             originalSize = prefabRect.sizeDelta;
//         }

//         foreach (Choice choice in choices)
//         {
//             GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
//             choiceButton.SetActive(true);
//             choiceButton.transform.localScale = Vector3.one; // 确保按钮缩放正确

//             // 应用原始预制体的尺寸
//             RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
//             if (buttonRect != null && prefabRect != null)
//             {
//                 buttonRect.sizeDelta = originalSize;
//                 buttonRect.anchorMin = prefabRect.anchorMin;
//                 buttonRect.anchorMax = prefabRect.anchorMax;
//                 buttonRect.pivot = prefabRect.pivot;
//             }

//             Text buttonText = choiceButton.GetComponentInChildren<Text>();
//             if (buttonText != null)
//             {
//                 buttonText.text = choice.choiceText;
//             }

//             Button button = choiceButton.GetComponent<Button>();
//             if (button != null)
//             {
//                 int nextIndex = choice.nextDialogueIndex;
//                 button.onClick.AddListener(() => OnChoiceSelected(nextIndex, choice));
//             }
//         }
//     }

//     public void OnChoiceSelected(int nextDialogueIndex, Choice selectedChoice)
//     {
//         // 标记选择为已选择
//         selectedChoice.isSelected = true;

//         ClearChoiceButtons();
//         isWaitingForChoice = false;

//         // 检查是否有DisasterManualPickup组件需要处理这个选择
//         DisasterManualPickup[] manualPickupComponents = FindObjectsOfType<DisasterManualPickup>();
//         foreach (DisasterManualPickup manualPickup in manualPickupComponents)
//         {
//             // 尝试找出当前选择在choices列表中的索引
//             int choiceIndex = -1;
//             if (currentDialogueIndex < dialogues.Count)
//             {
//                 for (int i = 0; i < dialogues[currentDialogueIndex].choices.Count; i++)
//                 {
//                     if (dialogues[currentDialogueIndex].choices[i] == selectedChoice)
//                     {
//                         choiceIndex = i;
//                         break;
//                     }
//                 }
//             }

//             // 只有在disaster_manual_pickup.csv对话中，且选择索引匹配pickupChoiceIndex时，才调用OnManualPickedUp方法
//             if (manualPickup != null && csvFileName == "disaster_manual_pickup.csv" && choiceIndex == manualPickup.pickupChoiceIndex)
//             {
//                 Debug.Log("DialogueManager: 检测到选择了获取防灾手册的选项，调用OnManualPickedUp方法");
//                 manualPickup.OnManualPickedUp();
//             }
//         }

//         if (nextDialogueIndex == -1)
//         {
//             // 检查当前对话是否有完成后要调用的方法
//             if (currentDialogueIndex < dialogues.Count && !string.IsNullOrEmpty(dialogues[currentDialogueIndex].onDialogueCompleteMethod))
//             {
//                 // 使用反射查找所有包含指定方法的组件
//                 MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
//                 foreach (MonoBehaviour component in allComponents)
//                 {
//                     // 检查组件是否有指定的方法
//                     var method = component.GetType().GetMethod(
//                         dialogues[currentDialogueIndex].onDialogueCompleteMethod,
//                         System.Reflection.BindingFlags.Public |
//                         System.Reflection.BindingFlags.Instance |
//                         System.Reflection.BindingFlags.FlattenHierarchy
//                     );

//                     if (method != null)
//                     {
//                         Debug.Log("DialogueManager: 调用方法: " + dialogues[currentDialogueIndex].onDialogueCompleteMethod +
//                                   " 在组件: " + component.name);
//                         method.Invoke(component, null);
//                     }
//                 }
//             }

//             EndDialogue();
//         }
//         else
//         {
//             currentDialogueIndex = nextDialogueIndex;
//             ShowDialogue();
//         }
//     }

//     void ClearChoiceButtons()
//     {
//         if (choiceContainer == null)
//             return;

//         foreach (Transform child in choiceContainer)
//         {
//             Destroy(child.gameObject);
//         }
//     }
// }