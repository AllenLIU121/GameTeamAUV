using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public GameObject characterSprite;
    public Vector3 characterPosition = Vector3.zero;
    public float fadeSpeed = 1f;
}

[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public string dialogueText;
    public bool hasChoices;
    public List<Choice> choices = new List<Choice>();
    public int characterExpression = 0;
    public bool isReturnPoint; // 标记是否为返回点
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextDialogueIndex;
    public bool isSelected; // 标记是否已选择
}

public class DialogueManager : MonoBehaviour
{
    [Header("通用设置")]
    public float typingSpeed = 0.05f;
    public string csvFileName = "soldier_dialogue.csv";

    [Header("对话类型")]
    // 当前对话类型
    public bool isCurrentDialogueChoiceType = true;

    [Header("选择类型UI")]
    public GameObject choiceTypeDialogueUI;
    public Text choiceTypeDialogueText;
    public Text choiceTypeCharacterNameText;
    public GameObject choiceTypeDialogueBox;
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;

    [Header("提示类型UI")]
    public GameObject hintTypeDialogueUI;
    public Text hintTypeDialogueText;
    public Text hintTypeCharacterNameText;
    public GameObject hintTypeDialogueBox;

    [Header("角色系统")]
    public List<CharacterData> characters = new List<CharacterData>();
    public float characterFadeTime = 0.5f;

    [Header("End of Dialogue")]
    public UnityEvent onDialogueEnd; // 对话结束时的回调

    [Header("Debug")]
    public bool loadFromCSV = true;
    public List<DialogueEntry> fallbackDialogues = new List<DialogueEntry>();

    private List<DialogueEntry> dialogues = new List<DialogueEntry>();
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isChangingCharacter = false;
    private Coroutine typingCoroutine;
    private string currentCharacterName = "";
    private Dictionary<int, List<Choice>> availableChoices = new Dictionary<int, List<Choice>>(); // 存储每个选择点的原始选择

    void Start()
    {
        InitializeCharacters();
        LoadDialogues();
        HideAllDialogueUI();
    }

    private void HideAllDialogueUI()
    {
        if (choiceTypeDialogueUI != null)
        {
            choiceTypeDialogueUI.SetActive(false);
        }

        if (hintTypeDialogueUI != null)
        {
            hintTypeDialogueUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsDialogueActive()) return;

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

    /// <summary>
    /// 设置对话CSV文件
    /// </summary>
    /// <param name="newCSVFileName">新的CSV文件名</param>
    public void SetDialogueCSVFile(string newCSVFileName)
    {
        if (csvFileName != newCSVFileName)
        {
            csvFileName = newCSVFileName;
            LoadDialogues();
            Debug.Log("对话文件已更改为: " + newCSVFileName);
        }
    }

    // 设置对话类型（选择类型或提示类型）
    public void SetDialogueType(bool isChoiceType)
    {
        isCurrentDialogueChoiceType = isChoiceType;
        UpdateDialogueUI();
    }

    // 更新对话UI显示
    private void UpdateDialogueUI()
    {
        // 激活对应的UI容器
        if (choiceTypeDialogueUI != null)
        {
            choiceTypeDialogueUI.SetActive(isCurrentDialogueChoiceType);
        }

        if (hintTypeDialogueUI != null)
        {
            hintTypeDialogueUI.SetActive(!isCurrentDialogueChoiceType);
        }

        // 更新对话框显示状态
        if (choiceTypeDialogueBox != null)
        {
            choiceTypeDialogueBox.SetActive(isCurrentDialogueChoiceType);
        }

        if (hintTypeDialogueBox != null)
        {
            hintTypeDialogueBox.SetActive(!isCurrentDialogueChoiceType);
        }
    }

    // 获取当前使用的对话文本组件
    private Text GetCurrentDialogueText()
    {
        return isCurrentDialogueChoiceType ? choiceTypeDialogueText : hintTypeDialogueText;
    }

    // 获取当前使用的角色名称文本组件
    private Text GetCurrentCharacterNameText()
    {
        return isCurrentDialogueChoiceType ? choiceTypeCharacterNameText : hintTypeCharacterNameText;
    }

    // 检查对话是否正在进行
    public bool IsDialogueActive()
    {
        bool choiceTypeActive = isCurrentDialogueChoiceType &&
                                choiceTypeDialogueBox != null &&
                                choiceTypeDialogueBox.activeSelf;

        bool hintTypeActive = !isCurrentDialogueChoiceType &&
                              hintTypeDialogueBox != null &&
                              hintTypeDialogueBox.activeSelf;

        return choiceTypeActive || hintTypeActive;
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

                            // 存储每个选择点的原始选择
                            availableChoices[i] = new List<Choice>(entry.choices);
                        }

                        if (values.Length >= 8 && int.TryParse(values[7], out int expression))
                        {
                            entry.characterExpression = expression;
                        }

                        // 标记是否为返回点(值为-1时)
                        if (values.Length >= 9 && int.TryParse(values[8], out int isReturn) && isReturn == -1)
                        {
                            entry.isReturnPoint = true;
                        }

                        dialogues.Add(entry);
                    }
                }

                Debug.Log($"成功加载 {dialogues.Count} 条对话");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载CSV文件失败: {e.Message}");
                dialogues = fallbackDialogues;
            }
        }
        else
        {
            Debug.LogWarning($"CSV文件不存在: {filePath}");
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

    [ContextMenu("重新加载CSV文件")]
    public void ReloadCSV()
    {
        LoadDialogues();
        Debug.Log("CSV文件已重新加载");
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

    // 从指定索引开始对话
    public void StartDialogueAt(int index)
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] 对话数据为空，无法开始对话！");
            return;
        }

        if (index >= 0 && index < dialogues.Count)
        {
            currentDialogueIndex = index;
            UpdateDialogueUI();
            ShowDialogue();
        }
        else
        {
            Debug.LogWarning("[DialogueManager] 尝试从无效索引开始对话: " + index);
            // 索引无效时使用默认起始点
            currentDialogueIndex = 0;
            UpdateDialogueUI();
            ShowDialogue();

            Debug.LogWarning("[DialogueManager] 使用默认对话起点: 0");
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

        StartCoroutine(ChangeCharacter(currentEntry.characterName, () =>
        {
            Text nameText = GetCurrentCharacterNameText();
            if (nameText != null)
            {
                nameText.text = currentEntry.characterName;
            }

            ClearChoiceButtons();

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(currentEntry.dialogueText));

            if (currentEntry.hasChoices && currentEntry.choices.Count > 0)
            {
                isWaitingForChoice = true;
                StartCoroutine(ShowChoicesAfterTyping(currentEntry.choices));
            }
            else
            {
                isWaitingForChoice = false;
            }
        }));
    }

    void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        Text currentText = GetCurrentDialogueText();
        if (currentText != null)
        {
            currentText.text = dialogues[currentDialogueIndex].dialogueText;
        }
        isTyping = false;
    }

    public void NextDialogue()
    {
        // 检查是否是返回点
        if (currentDialogueIndex < dialogues.Count && dialogues[currentDialogueIndex].isReturnPoint)
        {
            // 返回到选择点并显示未选择的选项
            ReturnToChoicePoint();
            return;
        }

        currentDialogueIndex++;
        ShowDialogue();
    }

    void ReturnToChoicePoint()
    {
        // 查找最近的选择点
        int choicePointIndex = -1;
        for (int i = currentDialogueIndex; i >= 0; i--)
        {
            if (dialogues[i].hasChoices)
            {
                choicePointIndex = i;
                break;
            }
        }

        if (choicePointIndex != -1 && availableChoices.ContainsKey(choicePointIndex))
        {
            // 获取未选择的选项
            List<Choice> remainingChoices = new List<Choice>();
            foreach (Choice choice in availableChoices[choicePointIndex])
            {
                if (!choice.isSelected)
                {
                    remainingChoices.Add(choice);
                }
            }

            if (remainingChoices.Count > 0)
            {
                // 返回到选择点并显示剩余选项
                currentDialogueIndex = choicePointIndex;
                dialogues[currentDialogueIndex].choices = remainingChoices;
                ShowDialogue();
            }
            else
            {
                // 所有选项都已选择，继续下一段对话
                currentDialogueIndex++;
                ShowDialogue();
            }
        }
        else
        {
            // 没有找到选择点，继续下一段对话
            currentDialogueIndex++;
            ShowDialogue();
        }
    }

    void EndDialogue()
    {
        // 清除所有选项按钮
        ClearChoiceButtons();

        // 禁用整个对话UI（包括对话框和遮罩）
        if (isCurrentDialogueChoiceType && choiceTypeDialogueUI != null)
        {
            choiceTypeDialogueUI.SetActive(false);
        }
        else if (!isCurrentDialogueChoiceType && hintTypeDialogueUI != null)
        {
            hintTypeDialogueUI.SetActive(false);
        }

        if (!string.IsNullOrEmpty(currentCharacterName))
        {
            CharacterData currentChar = characters.Find(c => c.characterName == currentCharacterName);
            if (currentChar != null && currentChar.characterSprite != null)
            {
                StartCoroutine(FadeCharacter(currentChar.characterSprite, false));
            }
        }

        Debug.Log("对话结束");
        // 触发对话结束事件
        onDialogueEnd.Invoke();
    }

    IEnumerator ChangeCharacter(string newCharacterName, System.Action onComplete = null)
    {
        if (currentCharacterName == newCharacterName)
        {
            onComplete?.Invoke();
            yield break;
        }

        isChangingCharacter = true;

        if (!string.IsNullOrEmpty(currentCharacterName))
        {
            CharacterData currentChar = characters.Find(c => c.characterName == currentCharacterName);
            if (currentChar != null && currentChar.characterSprite != null)
            {
                yield return StartCoroutine(FadeCharacter(currentChar.characterSprite, false));
                currentChar.characterSprite.SetActive(false);
            }
        }

        CharacterData newChar = characters.Find(c => c.characterName == newCharacterName);
        if (newChar != null && newChar.characterSprite != null)
        {
            newChar.characterSprite.SetActive(true);
            newChar.characterSprite.transform.localPosition = newChar.characterPosition;
            yield return StartCoroutine(FadeCharacter(newChar.characterSprite, true));
        }

        currentCharacterName = newCharacterName;
        isChangingCharacter = false;

        onComplete?.Invoke();
    }

    IEnumerator FadeCharacter(GameObject character, bool fadeIn)
    {
        Image characterImage = character.GetComponent<Image>();
        if (characterImage == null)
        {
            SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                yield return StartCoroutine(FadeSpriteRenderer(spriteRenderer, fadeIn));
            }
            yield break;
        }

        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        Color color = characterImage.color;
        color.a = startAlpha;
        characterImage.color = color;

        while (elapsedTime < characterFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / characterFadeTime);
            color.a = alpha;
            characterImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        characterImage.color = color;
    }

    IEnumerator FadeSpriteRenderer(SpriteRenderer spriteRenderer, bool fadeIn)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        Color color = spriteRenderer.color;
        color.a = startAlpha;
        spriteRenderer.color = color;

        while (elapsedTime < characterFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / characterFadeTime);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        spriteRenderer.color = color;
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        Text currentText = GetCurrentDialogueText();
        if (currentText != null)
        {
            currentText.text = "";

            foreach (char letter in text.ToCharArray())
            {
                currentText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;
    }

    IEnumerator ShowChoicesAfterTyping(List<Choice> choices)
    {
        while (isTyping)
        {
            yield return null;
        }

        foreach (Choice choice in choices)
        {
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            Text buttonText = choiceButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            Button button = choiceButton.GetComponent<Button>();
            int nextIndex = choice.nextDialogueIndex;
            button.onClick.AddListener(() => OnChoiceSelected(nextIndex, choice));
        }
    }

    public void OnChoiceSelected(int nextDialogueIndex, Choice selectedChoice)
    {
        // 标记选择为已选择
        selectedChoice.isSelected = true;

        ClearChoiceButtons();
        isWaitingForChoice = false;

        if (nextDialogueIndex == -1)
        {
            EndDialogue();
        }
        else
        {
            currentDialogueIndex = nextDialogueIndex;
            ShowDialogue();
        }
    }

    void ClearChoiceButtons()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }


}