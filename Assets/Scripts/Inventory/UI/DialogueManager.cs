using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;



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

    private List<DialogueEntry> dialogues = new List<DialogueEntry>();
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isChangingCharacter = false;
    private Coroutine typingCoroutine;
    private string currentCharacterName = "";
    private Dictionary<int, List<Choice>> availableChoices = new Dictionary<int, List<Choice>>(); // �洢ÿ��ѡ���Ŀ���ѡ��

    void Start()
    {
        InitializeCharacters();
        LoadDialogues();
        StartDialogue();
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            && !isWaitingForChoice && !isChangingCharacter)
        {
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                NextDialogue();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadCSV();
        }
    }

    void LoadDialogues()
    {
        if (loadFromCSV)
        {
            LoadDialoguesFromCSV();
        }
        else
        {
            dialogues = fallbackDialogues;
        }
    }

    void LoadDialoguesFromCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                dialogues.Clear();
                availableChoices.Clear();

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = SplitCSVLine(lines[i]);

                    if (values.Length >= 7)
                    {
                        DialogueEntry entry = new DialogueEntry();
                        entry.characterName = values[0];
                        entry.dialogueText = values[1];

                        if (bool.TryParse(values[2], out bool hasChoices))
                        {
                            entry.hasChoices = hasChoices;
                        }

                        if (entry.hasChoices)
                        {
                            if (!string.IsNullOrEmpty(values[3]) && int.TryParse(values[4], out int choice1Index))
                            {
                                Choice choice1 = new Choice();
                                choice1.choiceText = values[3];
                                choice1.nextDialogueIndex = choice1Index;
                                choice1.isSelected = false;
                                entry.choices.Add(choice1);
                            }

                            if (!string.IsNullOrEmpty(values[5]) && int.TryParse(values[6], out int choice2Index))
                            {
                                Choice choice2 = new Choice();
                                choice2.choiceText = values[5];
                                choice2.nextDialogueIndex = choice2Index;
                                choice2.isSelected = false;
                                entry.choices.Add(choice2);
                            }

                            // �洢���ѡ��������ѡ��
                            availableChoices[i] = new List<Choice>(entry.choices);
                        }

                        if (values.Length >= 8 && int.TryParse(values[7], out int expression))
                        {
                            entry.characterExpression = expression;
                        }

                        // ����Ƿ�Ϊ���ص㣨���Ϊ-1��
                        if (values.Length >= 9 && int.TryParse(values[8], out int isReturn) && isReturn == -1)
                        {
                            entry.isReturnPoint = true;
                        }

                        dialogues.Add(entry);
                    }
                }

                Debug.Log($"�ɹ����� {dialogues.Count} ���Ի�");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"����CSV�ļ�ʧ��: {e.Message}");
                dialogues = fallbackDialogues;
            }
        }
        else
        {
            Debug.LogWarning($"CSV�ļ�������: {filePath}");
            dialogues = fallbackDialogues;
        }
    }

    string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    [ContextMenu("���¼���CSV�ļ�")]
    public void ReloadCSV()
    {
        LoadDialogues();
        Debug.Log("CSV�ļ������¼���");
    }

    void InitializeCharacters()
    {
        foreach (CharacterData character in characters)
        {
            if (character.characterSprite != null)
            {
                character.characterSprite.SetActive(false);
            }
        }
    }

    public void StartDialogue()
    {
        StartDialogueAt(0);
    }
    
    /// <summary>
    /// 从指定索引开始对话
    /// </summary>
    /// <param name="index">对话的起始索引</param>
    public void StartDialogueAt(int index)
    {
        if (index >= 0 && index < dialogues.Count)
        {
            currentDialogueIndex = index;
            dialogueBox.SetActive(true);
            ShowDialogue();
        }
        else
        {
            Debug.LogWarning("[DialogueManager] 尝试从无效索引开始对话: " + index);
            // 索引无效时使用默认起始点
            currentDialogueIndex = 0;
            dialogueBox.SetActive(true);
            ShowDialogue();
        }
    }

    void ShowDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogue();
            return;
        }

        DialogueEntry currentEntry = dialogues[currentDialogueIndex];

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