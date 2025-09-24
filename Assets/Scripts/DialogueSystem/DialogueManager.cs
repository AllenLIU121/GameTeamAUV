using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace DialogueSystem
{
    /// <summary>
    /// 对话核心管理器（控制对话流程，聚合所有拆分模块）
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [Header("对话系统配置")]
        public DialogueSystemConfig systemConfig;

        [Header("对话结束回调")]
        public UnityEvent onDialogueEnd;

        [Header("UI 配置")]
        [Tooltip("可以直接在Inspector中拖入死亡面板预制体")]
        [SerializeField] private GameObject _diedPanelPrefab;

        // 模块引用
        private DialogueDataLoader _dataLoader;
        private CharacterController _characterController;
        private DialogueUIManager _uiManager;

        // 对话状态变量
        private List<DialogueEntry> _dialogues = new List<DialogueEntry>();
        private int _currentDialogueIndex = 0;
        private bool _isTyping = false;
        private bool _isWaitingForChoice = false;
        private bool _dialogueActiveFlag = false;
        private Coroutine _typingCoroutine;
        private string _currentCSVPath;

        void Start()
        {
            // 初始化模块
            InitializeModules();

            // 初始化结束事件
            if (onDialogueEnd == null)
                onDialogueEnd = new UnityEvent();

            // 加载默认对话数据
            _currentCSVPath = Path.Combine(Application.streamingAssetsPath, systemConfig.defaultCSVFileName);
            _dialogues = _dataLoader.LoadDialogues(systemConfig.loadFromCSV, _currentCSVPath);
        }

        void Update()
        {
            // 对话未激活时跳过输入处理
            if (!_dialogueActiveFlag || !_uiManager.IsUILoaded)
                return;

            // 处理用户输入（点击/空格继续，R键重新加载CSV）
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                && !_isWaitingForChoice)
            {
                if (_isTyping)
                    CompleteTyping();
                else
                    NextDialogue();
            }

            if (Input.GetKeyDown(KeyCode.R))
                ReloadCSV();
        }

        // ------------------------------ 模块初始化 ------------------------------
        private void InitializeModules()
        {
            // 初始化数据加载器
            _dataLoader = new DialogueDataLoader(systemConfig);

            // 初始化角色控制器
            _characterController = new CharacterController(
                systemConfig.characters,
                systemConfig.characterFadeTime
            );

            // 初始化UI管理器
            _uiManager = new DialogueUIManager(systemConfig.uiConfig);
        }

        // ------------------------------ 外部调用接口 ------------------------------
        /// <summary>
        /// 启动对话（从索引0开始）
        /// </summary>
        public void StartDialogue()
        {
            StartDialogueAt(0);
        }

        /// <summary>
        /// 加载指定CSV并启动对话
        /// </summary>
        public void StartDialogue(string csvFileName)
        {
            var newCsvPath = Path.Combine(Application.streamingAssetsPath, csvFileName);
            if (_currentCSVPath != newCsvPath)
            {
                _currentCSVPath = newCsvPath;
                _dialogues = _dataLoader.LoadDialogues(systemConfig.loadFromCSV, _currentCSVPath);
            }
            StartDialogueAt(0);
        }

        /// <summary>
        /// 从指定索引启动对话
        /// </summary>
        public void StartDialogueAt(int index)
        {
            // 验证前置条件
            if (!_uiManager.IsUILoaded)
            {
                _uiManager.InstantiateAndBindUI(transform);
                if (!_uiManager.IsUILoaded)
                {
                    Debug.LogError("DialogueManager: UI加载失败，无法启动对话");
                    return;
                }
            }

            if (_dialogues.Count == 0)
            {
                Debug.LogError("DialogueManager: 对话数据为空，无法启动对话");
                return;
            }

            // 修正索引范围
            _currentDialogueIndex = Mathf.Clamp(index, 0, _dialogues.Count - 1);

            // 激活UI并显示对话
            _uiManager.UpdateUIState(true);
            _dialogueActiveFlag = true;
            ShowCurrentDialogue();

            // 启动重试机制（确保UI正确显示）
            StartCoroutine(CheckDialogueActiveRetry());
        }

        /// <summary>
        /// 切换对话类型（选择/提示）
        /// </summary>
        public void SetDialogueType(bool isChoiceType)
        {
            _uiManager.SetDialogueType(isChoiceType);
        }

        /// <summary>
        /// 检查对话是否正在进行
        /// </summary>
        public bool IsDialogueActive()
        {
            if (!_uiManager.IsUILoaded) return false;
            return _dialogueActiveFlag;
        }

        /// <summary>
        /// 重新加载当前CSV
        /// </summary>
        [ContextMenu("重新加载CSV")]
        public void ReloadCSV()
        {
            _dialogues = _dataLoader.ReloadCSV(_currentCSVPath);
            Debug.Log("DialogueManager: CSV重新加载完成");
        }

        // ------------------------------ 对话流程控制 ------------------------------
        /// <summary>
        /// 显示当前索引的对话
        /// </summary>
        private void ShowCurrentDialogue()
        {
            // 索引超出范围时结束对话
            if (_currentDialogueIndex >= _dialogues.Count)
            {
                EndDialogue();
                return;
            }

            var currentEntry = _dialogues[_currentDialogueIndex];
            Debug.Log($"DialogueManager: 显示对话 - 角色：{currentEntry.characterName}，索引：{_currentDialogueIndex}");

            // 切换角色并显示对话内容
            StartCoroutine(_characterController.ChangeCharacter(currentEntry.characterName, () =>
            {
                // 清除打字协程
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);

                // 立即设置对话内容（包括角色名称和对话文本）
                _uiManager.ShowDialogueContent(currentEntry);
                
                // 开始打字动画
                _typingCoroutine = StartCoroutine(TypeDialogueText(currentEntry));

                // 处理选择项
                if (currentEntry.hasChoices && currentEntry.choices.Count > 0 && _uiManager.IsCurrentChoiceType)
                {
                    _isWaitingForChoice = true;
                    StartCoroutine(ShowChoicesAfterTyping(currentEntry.choices));
                }
                else
                {
                    _isWaitingForChoice = false;
                }
            }));
        }

        /// <summary>
        /// 打字动画协程
        /// </summary>
        private IEnumerator TypeDialogueText(DialogueEntry entry)
        {
            _isTyping = true;
            var targetText = _uiManager.IsCurrentChoiceType
                ? systemConfig.uiConfig.choiceTypeDialogueText
                : systemConfig.uiConfig.hintTypeDialogueText;

            if (targetText != null)
            {
                targetText.text = "";
                foreach (char c in entry.dialogueText.ToCharArray())
                {
                    targetText.text += c;
                    yield return new WaitForSeconds(systemConfig.typingSpeed);
                }
            }

            _isTyping = false;
        }

        /// <summary>
        /// 打字完成后显示选项
        /// </summary>
        private IEnumerator ShowChoicesAfterTyping(List<Choice> choices)
        {
            while (_isTyping)
                yield return null;

            _uiManager.ShowChoiceButtons(choices, OnChoiceSelected);
        }

        /// <summary>
        /// 立即完成打字动画
        /// </summary>
        private void CompleteTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _isTyping = false;

                // 直接显示完整文本
                var currentEntry = _dialogues[_currentDialogueIndex];
                var targetText = _uiManager.IsCurrentChoiceType
                    ? systemConfig.uiConfig.choiceTypeDialogueText
                    : systemConfig.uiConfig.hintTypeDialogueText;

                if (targetText != null)
                    targetText.text = currentEntry.dialogueText;
            }
        }

        /// <summary>
        /// 处理选项选择
        /// </summary>
        private void OnChoiceSelected(int nextIndex, Choice selectedChoice)
        {
            // 标记选择状态
            selectedChoice.isSelected = true;
            _isWaitingForChoice = false;
            _uiManager.ClearChoiceButtons();

            // 特殊处理防灾手册选择（保持原有业务逻辑）
            HandleDisasterManualPickup(selectedChoice);

            // 处理跳转逻辑
            if (nextIndex == -1)
            {
                // 执行对话完成回调方法
                ExecuteDialogueCompleteMethod();
                EndDialogue();
            }
            else
            {
                _currentDialogueIndex = nextIndex;
                ShowCurrentDialogue();
            }
        }

        /// <summary>
        /// 进入下一条对话
        /// </summary>
        public void NextDialogue()
        {
            // 检查是否是结束点
            if (_currentDialogueIndex < _dialogues.Count && _dialogues[_currentDialogueIndex].isEndPoint)
            {
                ExecuteDialogueCompleteMethod();
                EndDialogue();
                return;
            }

            // 检查是否是返回点
            if (_currentDialogueIndex < _dialogues.Count && _dialogues[_currentDialogueIndex].isReturnPoint)
            {
                ReturnToChoicePoint();
                return;
            }

            // 正常进入下一条
            _currentDialogueIndex++;
            ShowCurrentDialogue();
        }

        /// <summary>
        /// 返回最近的选择点
        /// </summary>
        private void ReturnToChoicePoint()
        {
            // 查找最近的选择点
            int choicePointIndex = -1;
            for (int i = _currentDialogueIndex; i >= 0; i--)
            {
                if (_dialogues[i].hasChoices)
                {
                    choicePointIndex = i;
                    break;
                }
            }

            if (choicePointIndex != -1 && _dataLoader.AvailableChoices.ContainsKey(choicePointIndex))
            {
                // 筛选未选择的选项
                var remainingChoices = new List<Choice>();
                foreach (var choice in _dataLoader.AvailableChoices[choicePointIndex])
                {
                    if (!choice.isSelected)
                        remainingChoices.Add(choice);
                }

                if (remainingChoices.Count > 0)
                {
                    // 返回到选择点并显示剩余选项
                    _currentDialogueIndex = choicePointIndex;
                    _dialogues[_currentDialogueIndex].choices = remainingChoices;
                    ShowCurrentDialogue();
                }
                else
                {
                    // 所有选项都已选择，继续下一段
                    _currentDialogueIndex++;
                    ShowCurrentDialogue();
                }
            }
            else
            {
                // 未找到选择点，继续下一段
                _currentDialogueIndex++;
                ShowCurrentDialogue();
            }
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        private void EndDialogue()
        {
            Debug.Log("DialogueManager: 对话结束");

            // 隐藏UI和角色
            _uiManager.HideAllUI();
            StartCoroutine(_characterController.HideCurrentCharacter());

            // 检查当前对话是否是致命的（在重置索引前检查）
            bool isDeadlyDialogue = _dialogues.Count > 0 && _currentDialogueIndex >= 0 && _currentDialogueIndex < _dialogues.Count && _dialogues[_currentDialogueIndex].isDeadly;

            // 重置状态
            _dialogueActiveFlag = false;
            _currentDialogueIndex = 0;
            _isWaitingForChoice = false;

            // 根据之前保存的isDeadly状态决定后续操作
            if (isDeadlyDialogue)
            {
                Debug.Log("DialogueManager: 检测到致命对话，显示死亡界面");
                ShowDeathScreen();
            }
            else
            {
                // 触发结束事件
                onDialogueEnd.Invoke();
            }
        }

        /// <summary>
        /// 显示死亡界面
        /// </summary>
        private void ShowDeathScreen()
        {
            Debug.Log("DialogueManager: 显示死亡界面");
            GameObject diedPanel = null;

            // 优先使用Inspector中配置的预制体
            if (_diedPanelPrefab != null)
            {
                Debug.Log("DialogueManager: 使用Inspector中配置的死亡面板预制体");
                // 查找或创建Canvas作为父对象
                Canvas canvas = FindSuitableCanvas();
                if (canvas != null)
                {
                    diedPanel = Instantiate(_diedPanelPrefab, canvas.transform);
                    diedPanel.name = "DiedPanel";
                }
                else
                {
                    Debug.LogWarning("DialogueManager: 未找到合适的Canvas来实例化死亡面板");
                }
            }

            // 如果没有配置预制体，或者实例化失败，尝试在场景中查找
            if (diedPanel == null)
            {
                diedPanel = GameObject.Find("UI/Final Canvas/DiedPanel");
                if (diedPanel != null)
                {
                    Debug.Log("DialogueManager: 在场景中找到死亡面板");
                    diedPanel.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("DialogueManager: 未找到场景中的死亡UI面板，尝试从Resources加载预制体");
                    // 尝试从Resources加载DiedPanel预制体
                    GameObject diedPanelPrefab = Resources.Load<GameObject>("DiedPanel");

                    if (diedPanelPrefab != null)
                    {
                        Debug.Log("DialogueManager: 从Resources找到DiedPanel预制体，正在实例化");
                        // 查找或创建Canvas作为父对象
                        Canvas canvas = FindSuitableCanvas();
                        if (canvas != null)
                        {
                            diedPanel = Instantiate(diedPanelPrefab, canvas.transform);
                            diedPanel.name = "DiedPanel";
                        }
                    }
                    else
                    {
                        Debug.LogWarning("DialogueManager: 未找到DiedPanel预制体");
                        // 如果没有找到死亡面板，仍然触发结束事件
                        onDialogueEnd.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 查找合适的Canvas作为UI父对象
        /// </summary>
        private Canvas FindSuitableCanvas()
        {
            // 优先级：Final Canvas > UI/Canvas > 其他Canvas > 创建新Canvas
            Canvas canvas = GameObject.Find("UI/Final Canvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GameObject.Find("UI/Canvas")?.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            return canvas;
        }

        // ------------------------------ 辅助逻辑 ------------------------------
        /// <summary>
        /// 防灾手册选择特殊处理（保持原有业务逻辑）
        /// </summary>
        private void HandleDisasterManualPickup(Choice selectedChoice)
        {
            if (_currentCSVPath.Contains("disaster_manual_pickup.csv"))
            {
                var manualPickups = FindObjectsOfType<DisasterManualPickup>();
                foreach (var pickup in manualPickups)
                {
                    // 查找选择索引
                    int choiceIndex = -1;
                    if (_currentDialogueIndex < _dialogues.Count)
                    {
                        for (int i = 0; i < _dialogues[_currentDialogueIndex].choices.Count; i++)
                        {
                            if (_dialogues[_currentDialogueIndex].choices[i] == selectedChoice)
                            {
                                choiceIndex = i;
                                break;
                            }
                        }
                    }

                    if (choiceIndex == pickup.pickupChoiceIndex)
                    {
                        pickup.OnManualPickedUp();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 执行对话完成回调方法
        /// </summary>
        private void ExecuteDialogueCompleteMethod()
        {
            if (_currentDialogueIndex >= _dialogues.Count) return;

            var methodName = _dialogues[_currentDialogueIndex].onDialogueCompleteMethod;
            if (string.IsNullOrEmpty(methodName)) return;

            // 反射查找并执行方法
            var allComponents = FindObjectsOfType<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                var method = component.GetType().GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy
                );

                if (method != null)
                {
                    Debug.Log($"DialogueManager: 执行回调方法 {methodName}（组件：{component.name}）");
                    method.Invoke(component, null);
                    break;
                }
            }
        }

        /// <summary>
        /// 对话激活重试机制（确保UI正确显示）
        /// </summary>
        private IEnumerator CheckDialogueActiveRetry()
        {
            const int maxRetries = 3;
            const float retryInterval = 0.2f;
            int retryCount = 0;

            while (retryCount < maxRetries && !_dialogueActiveFlag)
            {
                retryCount++;
                Debug.LogWarning($"DialogueManager: 对话激活重试 {retryCount}/{maxRetries}");

                _uiManager.UpdateUIState(true);
                _dialogueActiveFlag = true;

                yield return new WaitForSeconds(retryInterval);
            }
        }
    }

    // 兼容原有代码的防灾手册接口（实际实现需在其他脚本）
    public class DisasterManualPickup : MonoBehaviour
    {
        public int pickupChoiceIndex;
        public virtual void OnManualPickedUp() { }
    }
}
