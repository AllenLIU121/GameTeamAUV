using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public bool isReturnPoint; // ����Ƿ�Ϊ���ص�
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextDialogueIndex;
    public bool isSelected; // ����Ƿ���ѡ��
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;
    public GameObject dialogueBox;
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    public Button continueButton;

    [Header("Character System")]
    public List<CharacterData> characters = new List<CharacterData>();
    public float characterFadeTime = 0.5f;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;
    public string csvFileName = "soldier_dialogue.csv";

    [Header("End of Dialogue")]
    public UnityEvent onDialogueEnd; // �Ի�����ʱ�Ļص�

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

        StartCoroutine(ChangeCharacter(currentEntry.characterName, () => {
            if (characterNameText != null)
            {
                characterNameText.text = currentEntry.characterName;
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

        dialogueText.text = dialogues[currentDialogueIndex].dialogueText;
        isTyping = false;
    }

    public void NextDialogue()
    {
        // ����Ƿ��Ƿ��ص�
        if (currentDialogueIndex < dialogues.Count && dialogues[currentDialogueIndex].isReturnPoint)
        {
            // ���ص�ѡ��㲢��ʾδѡ���ѡ��
            ReturnToChoicePoint();
            return;
        }

        currentDialogueIndex++;
        ShowDialogue();
    }

    void ReturnToChoicePoint()
    {
        // ���������ѡ���
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
            // ��ȡδѡ���ѡ��
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
                // �ص�ѡ��㲢��ʾʣ��ѡ��
                currentDialogueIndex = choicePointIndex;
                dialogues[currentDialogueIndex].choices = remainingChoices;
                ShowDialogue();
            }
            else
            {
                // ����ѡ���ѡ�񣬼�����һ���Ի�
                currentDialogueIndex++;
                ShowDialogue();
            }
        }
        else
        {
            // û���ҵ�ѡ��㣬������һ���Ի�
            currentDialogueIndex++;
            ShowDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);

        if (!string.IsNullOrEmpty(currentCharacterName))
        {
            CharacterData currentChar = characters.Find(c => c.characterName == currentCharacterName);
            if (currentChar != null && currentChar.characterSprite != null)
            {
                StartCoroutine(FadeCharacter(currentChar.characterSprite, false));
            }
        }

        Debug.Log("�Ի�����");
        // �����Ի������¼�
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
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
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
            TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
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
        // ���ѡ��Ϊ��ѡ��
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