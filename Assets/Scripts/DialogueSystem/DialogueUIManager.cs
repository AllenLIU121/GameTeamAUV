using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DialogueSystem
{
    /// <summary>
    /// 对话UI管理器（负责UI实例化、组件绑定、状态更新）
    /// </summary>
    public class DialogueUIManager
    {
        private readonly DialogueUIConfig _uiConfig;
        private bool _isUILoaded = false;
        private bool _isCurrentChoiceType = true; // 当前对话UI类型

        public bool IsUILoaded => _isUILoaded;
        public bool IsCurrentChoiceType => _isCurrentChoiceType;

        public DialogueUIManager(DialogueUIConfig uiConfig)
        {
            _uiConfig = uiConfig;
        }

        /// <summary>
        /// 实例化并绑定所有UI组件
        /// </summary>
        public void InstantiateAndBindUI(Transform parent)
        {
            Debug.Log("DialogueUIManager: 开始实例化UI");
            _isUILoaded = false;

            // 1. 实例化选择类型UI
            if (_uiConfig.choiceDialogueUIPrefab != null)
            {
                var choiceUI = Object.Instantiate(_uiConfig.choiceDialogueUIPrefab, parent);
                choiceUI.name = "ChoiceTypeDialogueUI";
                choiceUI.SetActive(false);
                _uiConfig.choiceTypeDialogueUI = choiceUI;

                // 绑定选择类型对话框与文本组件
                _uiConfig.choiceTypeDialogueBox = FindChildObject(choiceUI, "Dialogue Box");
                if (_uiConfig.choiceTypeDialogueBox != null)
                {
                    _uiConfig.choiceTypeDialogueText = FindTextComponent(_uiConfig.choiceTypeDialogueBox, "Dialogue");
                    // 绑定角色名称文本（路径：Dialogue Box -> profile -> Name bar -> Name）
                    var profile = FindChildObject(_uiConfig.choiceTypeDialogueBox, "profile");
                    if (profile != null)
                    {
                        var nameBar = FindChildObject(profile, "Name bar");
                        if (nameBar != null)
                        {
                            _uiConfig.choiceTypeCharacterNameText = FindTextComponent(nameBar, "Name");
                        }
                    }
                }

                // 绑定选择容器与按钮预制体
                _uiConfig.choiceContainer = FindTransform(choiceUI, "ChoiceContainer");
                if (_uiConfig.choiceContainer != null)
                {
                    // 从容器中获取默认按钮预制体（兼容原逻辑）
                    _uiConfig.choiceButtonPrefab = FindChildObject(_uiConfig.choiceContainer.gameObject, "Nope")
                        ?? FindChildObject(_uiConfig.choiceContainer.gameObject, "Right");
                    if (_uiConfig.choiceButtonPrefab != null)
                    {
                        // 创建独立预制体（避免影响原UI）
                        var tempPrefab = Object.Instantiate(_uiConfig.choiceButtonPrefab, parent);
                        tempPrefab.name = "ChoiceButtonPrefab";
                        tempPrefab.SetActive(false);
                        _uiConfig.choiceButtonPrefab = tempPrefab;
                    }
                }
            }

            // 2. 实例化提示类型UI
            if (_uiConfig.hintDialogueUIPrefab != null)
            {
                var hintUI = Object.Instantiate(_uiConfig.hintDialogueUIPrefab, parent);
                hintUI.name = "HintTypeDialogueUI";
                hintUI.SetActive(false);
                _uiConfig.hintTypeDialogueUI = hintUI;

                // 绑定提示类型对话框与文本组件
                _uiConfig.hintTypeDialogueBox = FindChildObject(hintUI, "Dialogue Box");
                if (_uiConfig.hintTypeDialogueBox != null)
                {
                    _uiConfig.hintTypeDialogueText = FindTextComponent(_uiConfig.hintTypeDialogueBox, "Dialogue");
                }
            }

            // 验证当前类型UI是否加载成功
            _isUILoaded = _isCurrentChoiceType
                ? _uiConfig.choiceTypeDialogueUI != null && _uiConfig.choiceTypeDialogueBox != null
                : _uiConfig.hintTypeDialogueUI != null && _uiConfig.hintTypeDialogueBox != null;

            Debug.Log($"DialogueUIManager: UI实例化完成，加载状态={_isUILoaded}");
        }

        /// <summary>
        /// 切换对话UI类型（选择/提示）
        /// </summary>
        public void SetDialogueType(bool isChoiceType)
        {
            if (_isCurrentChoiceType == isChoiceType) return;
            _isCurrentChoiceType = isChoiceType;
            UpdateUIState();
        }

        /// <summary>
        /// 更新UI显示状态（激活对应类型UI，隐藏其他）
        /// </summary>
        public void UpdateUIState(bool isActive = true)
        {
            if (!_isUILoaded) return;

            // 控制选择类型UI
            if (_uiConfig.choiceTypeDialogueUI != null)
            {
                _uiConfig.choiceTypeDialogueUI.SetActive(_isCurrentChoiceType && isActive);
                if (_uiConfig.choiceTypeDialogueBox != null)
                {
                    _uiConfig.choiceTypeDialogueBox.SetActive(_isCurrentChoiceType && isActive);
                }
                if (_uiConfig.choiceContainer != null)
                {
                    _uiConfig.choiceContainer.gameObject.SetActive(_isCurrentChoiceType && isActive);
                }
            }

            // 控制提示类型UI
            if (_uiConfig.hintTypeDialogueUI != null)
            {
                _uiConfig.hintTypeDialogueUI.SetActive(!_isCurrentChoiceType && isActive);
                if (_uiConfig.hintTypeDialogueBox != null)
                {
                    _uiConfig.hintTypeDialogueBox.SetActive(!_isCurrentChoiceType && isActive);
                }
            }

            // 确保UI在最上层显示
            if (_isCurrentChoiceType && _uiConfig.choiceTypeDialogueUI != null)
            {
                _uiConfig.choiceTypeDialogueUI.transform.SetAsLastSibling();
            }
            else if (!_isCurrentChoiceType && _uiConfig.hintTypeDialogueUI != null)
            {
                _uiConfig.hintTypeDialogueUI.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 显示单条对话内容（角色名+文本）
        /// </summary>
        public void ShowDialogueContent(DialogueEntry entry)
        {
            if (!_isUILoaded) return;

            // 显示角色名（仅选择类型UI支持）
            if (_isCurrentChoiceType && _uiConfig.choiceTypeCharacterNameText != null)
            {
                _uiConfig.choiceTypeCharacterNameText.text = entry.characterName;
            }

            // 显示对话文本（根据当前类型选择对应文本组件）
            var targetText = _isCurrentChoiceType ? _uiConfig.choiceTypeDialogueText : _uiConfig.hintTypeDialogueText;
            if (targetText != null)
            {
                targetText.text = entry.dialogueText;
            }
        }

        /// <summary>
        /// 显示对话选项按钮
        /// </summary>
        public void ShowChoiceButtons(List<Choice> choices, System.Action<int, Choice> onChoiceSelected)
        {
            if (!_isUILoaded || !_isCurrentChoiceType) return;
            if (_uiConfig.choiceButtonPrefab == null || _uiConfig.choiceContainer == null)
            {
                Debug.LogError("DialogueUIManager: 缺少选择按钮预制体或容器");
                return;
            }

            // 清除现有按钮
            ClearChoiceButtons();

            // 为容器添加布局组件（确保按钮排版正确）
            AddLayoutToChoiceContainer();

            // 实例化新按钮
            var prefabRect = _uiConfig.choiceButtonPrefab.GetComponent<RectTransform>();
            var originalSize = prefabRect != null ? prefabRect.sizeDelta : Vector2.zero;

            foreach (var choice in choices)
            {
                var buttonObj = Object.Instantiate(_uiConfig.choiceButtonPrefab, _uiConfig.choiceContainer);
                buttonObj.name = $"ChoiceButton_{choice.choiceText}";
                buttonObj.SetActive(true);
                buttonObj.transform.localScale = Vector3.one;

                // 还原预制体尺寸
                if (prefabRect != null)
                {
                    var buttonRect = buttonObj.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        buttonRect.sizeDelta = originalSize;
                        buttonRect.anchorMin = prefabRect.anchorMin;
                        buttonRect.anchorMax = prefabRect.anchorMax;
                        buttonRect.pivot = prefabRect.pivot;
                    }
                }

                // 设置按钮文本
                var buttonText = buttonObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = choice.choiceText;
                }

                // 绑定点击事件
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    var choiceCopy = choice; // 避免闭包捕获问题
                    button.onClick.AddListener(() => onChoiceSelected?.Invoke(choiceCopy.nextDialogueIndex, choiceCopy));
                }
            }
        }

        /// <summary>
        /// 清除所有选择按钮
        /// </summary>
        public void ClearChoiceButtons()
        {
            if (_uiConfig.choiceContainer == null) return;
            foreach (Transform child in _uiConfig.choiceContainer)
            {
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 隐藏所有对话UI
        /// </summary>
        public void HideAllUI()
        {
            UpdateUIState(false);
            ClearChoiceButtons();
        }

        /// <summary>
        /// 为选择容器添加布局组件（确保按钮垂直排列）
        /// </summary>
        private void AddLayoutToChoiceContainer()
        {
            if (_uiConfig.choiceContainer == null) return;

            // 添加垂直布局组件
            if (_uiConfig.choiceContainer.GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = _uiConfig.choiceContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = 30;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
            }

            // 添加内容自适应组件
            if (_uiConfig.choiceContainer.GetComponent<ContentSizeFitter>() == null)
            {
                var sizeFitter = _uiConfig.choiceContainer.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        // ------------------------------ 辅助方法 ------------------------------
        private GameObject FindChildObject(GameObject parent, string name)
        {
            if (parent == null) return null;
            var child = parent.transform.Find(name);
            return child != null ? child.gameObject : null;
        }

        private Text FindTextComponent(GameObject parent, string childName)
        {
            var child = FindChildObject(parent, childName);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private Transform FindTransform(GameObject parent, string childName)
        {
            return parent != null ? parent.transform.Find(childName) : null;
        }
    }
}