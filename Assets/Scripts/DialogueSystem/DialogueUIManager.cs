using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection; // 添加反射命名空间引用

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
        private Dictionary<string, UnityEngine.Sprite> _characterAvatars = new Dictionary<string, UnityEngine.Sprite>();
        private bool _hasLoadedCharacterData = false;
        private float _originalMaskAlpha = 1f; // 保存遮罩的原始透明度

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
                            // 绑定角色头像Image组件（假设在profile下有Image组件）
                            _uiConfig.choiceTypeCharacterAvatarImage = FindImageComponent(profile, "Image");
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

                // 保存遮罩的原始透明度
                SaveOriginalMaskAlpha();
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
        /// 保存遮罩的原始透明度
        /// </summary>
        private void SaveOriginalMaskAlpha()
        {
            if (_uiConfig.choiceTypeDialogueUI == null)
                return;

            // 查找遮罩（mask）组件
            Image maskImage = null;
            // 尝试查找常见的遮罩名称
            Transform maskTransform = _uiConfig.choiceTypeDialogueUI.transform.Find("Mask");

            if (maskTransform != null)
            {
                maskImage = maskTransform.GetComponent<Image>();
            }

            // 如果找到了遮罩组件，保存其原始透明度
            if (maskImage != null)
            {
                _originalMaskAlpha = maskImage.color.a;
                Debug.Log($"DialogueUIManager: 保存遮罩原始透明度={_originalMaskAlpha}");
            }
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
                    // 默认情况下激活选择容器，后续在ShowChoiceButtons中会根据是否有选项来决定
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
        /// 显示单条对话内容（角色名+文本+头像）
        /// </summary>
        public void ShowDialogueContent(DialogueEntry entry)
        {
            if (!_isUILoaded) return;

            // 显示角色名（仅选择类型UI支持）
            if (_isCurrentChoiceType && _uiConfig.choiceTypeCharacterNameText != null)
            {
                _uiConfig.choiceTypeCharacterNameText.text = entry.characterName;
            }

            // 显示角色头像（仅选择类型UI支持）
            if (_isCurrentChoiceType && _uiConfig.choiceTypeCharacterAvatarImage != null)
            {
                if (!string.IsNullOrEmpty(entry.characterName))
                {
                    // 确保角色数据已加载
                    LoadCharacterData();
                    
                    // 查找对应角色的头像（不区分大小写）
                    string characterNameLower = entry.characterName.ToLower();
                    if (_characterAvatars.TryGetValue(characterNameLower, out UnityEngine.Sprite avatarSprite))
                    {
                        // 使用找到的头像
                        _uiConfig.choiceTypeCharacterAvatarImage.sprite = avatarSprite;
                        _uiConfig.choiceTypeCharacterAvatarImage.gameObject.SetActive(true);
                        Debug.Log($"DialogueUIManager: 显示角色 '{entry.characterName}' 的头像");
                    }
                    else
                    {
                        // 如果找不到头像，尝试通过角色名直接查找（可能是Addressables中的资源）
                        // 这作为备用方案
                        Debug.LogWarning($"DialogueUIManager: 未找到角色 '{entry.characterName}' 的头像");
                        _uiConfig.choiceTypeCharacterAvatarImage.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // 没有角色名时隐藏头像
                    _uiConfig.choiceTypeCharacterAvatarImage.gameObject.SetActive(false);
                }
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

            // 如果没有选项，则隐藏选择容器并处理遮罩
            if (choices == null || choices.Count == 0)
            {
                _uiConfig.choiceContainer.gameObject.SetActive(false);
                HandleMaskVisibility(false);
                return;
            }

            // 有选项时显示选择容器并处理遮罩
            _uiConfig.choiceContainer.gameObject.SetActive(true);
            HandleMaskVisibility(true);

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
        /// 处理遮罩的可见性
        /// </summary>
        private void HandleMaskVisibility(bool hasChoices)
        {
            if (!_isUILoaded || !_isCurrentChoiceType || _uiConfig.choiceTypeDialogueUI == null)
                return;

            // 查找遮罩（mask）组件
            Image maskImage = null;
            // 尝试查找常见的遮罩名称
            Transform maskTransform = _uiConfig.choiceTypeDialogueUI.transform.Find("Mask") ??
                                     _uiConfig.choiceTypeDialogueUI.transform.Find("BackgroundMask") ??
                                     _uiConfig.choiceTypeDialogueUI.transform.Find("DialogueMask");

            if (maskTransform != null)
            {
                maskImage = maskTransform.GetComponent<Image>();
            }

            // 如果找到了遮罩组件
            if (maskImage != null)
            {
                if (hasChoices)
                {
                    // 有选项时恢复原始透明度
                    Color color = maskImage.color;
                    color.a = _originalMaskAlpha; // 恢复保存的原始透明度
                    maskImage.color = color;
                }
                else
                {
                    // 没有选项时设置透明度为0
                    Color color = maskImage.color;
                    color.a = 0f; // 设置透明度为0
                    maskImage.color = color;
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
        /// 隐藏选择容器和遮罩
        /// </summary>
        public void HideChoiceContainerAndMask()
        {
            if (!_isUILoaded || !_isCurrentChoiceType)
                return;

            // 隐藏选择容器
            if (_uiConfig.choiceContainer != null)
            {
                _uiConfig.choiceContainer.gameObject.SetActive(false);
            }

            // 设置遮罩透明度为0
            HandleMaskVisibility(false);
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

        private Image FindImageComponent(GameObject parent, string childName)
        {
            var child = FindChildObject(parent, childName);
            return child != null ? child.GetComponent<Image>() : null;
        }

        /// <summary>
        /// 加载所有角色数据
        /// </summary>
        private void LoadCharacterData()
        {   
            // 检查是否已经加载过角色数据
            if (_hasLoadedCharacterData && _characterAvatars.Count > 0) return;
            
            // 清空现有数据，准备重新加载
            _characterAvatars.Clear();
            
            try
            {   
                // 直接查找项目中所有CharacterSO资源
                var characterSOs = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.ScriptableObject>();
                
                foreach (var characterSO in characterSOs)
                {   
                    // 检查是否为CharacterSO类型
                    if (characterSO.GetType().Name == "CharacterSO")
                    {   
                        // 使用反射获取字段值
                        var characterNameField = characterSO.GetType().GetField("characterName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var characterPortraitField = characterSO.GetType().GetField("characterPortrait", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (characterNameField != null && characterPortraitField != null)
                        {   
                            string characterName = characterNameField.GetValue(characterSO) as string;
                            UnityEngine.Sprite portrait = characterPortraitField.GetValue(characterSO) as UnityEngine.Sprite;
                            
                            if (!string.IsNullOrEmpty(characterName) && portrait != null)
                            {   
                                // 存储角色头像（不区分大小写）
                                string nameKey = characterName.ToLower();
                                if (!_characterAvatars.ContainsKey(nameKey))
                                {   
                                    _characterAvatars[nameKey] = portrait;
                                    Debug.Log($"DialogueUIManager: 加载角色 '{characterName}' 的头像");
                                }
                                else
                                {   
                                    Debug.LogWarning($"DialogueUIManager: 角色名 '{characterName}' 重复，使用第一个找到的头像");
                                }
                            }
                        }
                    }
                }
                
                _hasLoadedCharacterData = true;
                Debug.Log($"DialogueUIManager: 总共加载 {_characterAvatars.Count} 个角色头像");
            }
            catch (System.Exception e)
            {   
                Debug.LogError($"DialogueUIManager: 加载角色数据时出错: {e.Message}\n{e.StackTrace}");
                // 如果出错，允许下次尝试重新加载
                _hasLoadedCharacterData = false;
            }
        }

        private Transform FindTransform(GameObject parent, string childName)
        {
            return parent != null ? parent.transform.Find(childName) : null;
        }
    }
}