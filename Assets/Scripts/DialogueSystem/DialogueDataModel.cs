using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    /// <summary>
    /// 角色数据模型
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public GameObject characterSprite;
        public Vector3 characterPosition = Vector3.zero;
        public float fadeSpeed = 1f;
    }

    /// <summary>
    /// 对话选项模型
    /// </summary>
    [System.Serializable]
    public class Choice
    {
        public string choiceText;
        public int nextDialogueIndex;
        public bool isSelected; // 标记是否已选择
    }

    /// <summary>
    /// 单条对话数据模型
    /// </summary>
    [System.Serializable]
    public class DialogueEntry
    {
        public string characterName;
        public string dialogueText;
        public bool hasChoices;
        public List<Choice> choices = new List<Choice>();
        public int characterExpression = 0;
        public bool isReturnPoint; // 标记是否为返回点
        public bool isEndPoint = false; // 标记是否为结束点
        public string onDialogueCompleteMethod = ""; // 对话完成后要调用的方法名
        public bool isDeadly = false; // 标记是否会导致死亡
    }

    /// <summary>
    /// 对话UI配置（存储预制体和组件引用）
    /// </summary>
    [System.Serializable]
    public class DialogueUIConfig
    {
        // 预制体
        public GameObject choiceDialogueUIPrefab;
        public GameObject hintDialogueUIPrefab;
        public GameObject choiceButtonPrefab;

        // 选择类型UI组件
        public GameObject choiceTypeDialogueUI;
        public Text choiceTypeDialogueText;
        public Text choiceTypeCharacterNameText;
        public GameObject choiceTypeDialogueBox;
        public Transform choiceContainer;

        // 提示类型UI组件
        public GameObject hintTypeDialogueUI;
        public Text hintTypeDialogueText;
        public GameObject hintTypeDialogueBox;
    }

    /// <summary>
    /// 对话系统配置（聚合所有外部配置）
    /// </summary>
    [System.Serializable]
    public class DialogueSystemConfig
    {
        public DialogueUIConfig uiConfig;
        public List<CharacterData> characters;
        public float typingSpeed = 0.05f;
        public string defaultCSVFileName = "soldier_dialogue.csv";
        public float characterFadeTime = 0.5f;
        public bool loadFromCSV = true;
        public List<DialogueEntry> fallbackDialogues;
    }
}