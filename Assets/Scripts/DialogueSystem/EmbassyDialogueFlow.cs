using UnityEngine;
using DialogueSystem;
using System.Collections.Generic;
using System.Reflection;

public class EmbassyDialogueFlow : MonoBehaviour
{
    [Header("对话文件配置")]
    public string firstDialogueFile = "embassy_arrival.csv";    // 视频结束后的第一个对话
    public string familyDeceasedDialogueFile = "embassy_loss.csv";  // 家人有去世时的对话
    public string allAliveDialogueFile = "embassy_success.csv";     // 家人都存活时的对话

    [Header("组件引用")]
    public DialogueManager dialogueManager;
    public CharacterManager characterManager;
    public VideoManager videoManager;
    public AwardableQuestion awardableQuestion; // 使用AwardableQuestion中的正确率逻辑

    private string currentDialogueType = "";

    // 当前问题索引（用于处理无参数的方法调用）
    private int currentQuestionIndex = 0;
    private bool currentAnswerIsCorrect = false;

    private void Awake()
    {
        // 自动获取组件引用
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (videoManager == null)
            videoManager = FindObjectOfType<VideoManager>();

        if (awardableQuestion == null)
            awardableQuestion = FindObjectOfType<AwardableQuestion>();
    }

    private void Start()
    {
        // 确保视频播放结束后调用我们的方法而不是默认的场景加载
        if (videoManager != null)
        {
            // 移除现有的视频结束事件监听器
            videoManager.videoPlayer.loopPointReached -= videoManager.OnVideoEnd;
            // 添加我们自己的事件监听器
            videoManager.videoPlayer.loopPointReached += OnVideoEnd;
        }

        // 监听对话结束事件
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.AddListener(OnDialogueEnd);
        }
    }

    private void OnDestroy()
    {
        // 清理事件监听器
        if (videoManager != null)
        {
            videoManager.videoPlayer.loopPointReached -= OnVideoEnd;
        }

        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }

    // 视频播放结束时调用
    private void OnVideoEnd(UnityEngine.Video.VideoPlayer vp)
    {
        Debug.Log("EmbassyDialogueFlow: 视频播放结束，开始对话");

        // 直接检查家人存活状态并跳转到相应对话
        if (dialogueManager != null && characterManager != null)
        {
            CheckFamilyStatusForArrival();
        }
    }

    private void CheckFamilyStatusForArrival()
    {
        // 获取所有角色数量和存活角色数量
        int totalCharacters = characterManager.GetAllCharacterStatus().Count;
        int aliveCharacters = characterManager.GetAllAliveCharacterStatus().Count;

        Debug.Log($"EmbassyDialogueFlow: 总角色数: {totalCharacters}, 存活角色数: {aliveCharacters}");

        // 判断是否有家人去世
        bool hasFamilyDeceased = aliveCharacters < totalCharacters;

        // 设置当前对话类型
        currentDialogueType = hasFamilyDeceased ? "family_deceased" : "all_alive";

        // 先加载CSV文件
        dialogueManager.StartDialogue(firstDialogueFile);

        // 根据家人是否死亡跳转到不同的对话索引
        int targetDialogueIndex = hasFamilyDeceased ? 2 : 0;

        // 延迟调用StartDialogueAt，确保CSV文件已加载完成
        StartCoroutine(StartDialogueAtIndexAfterDelay(targetDialogueIndex));
    }

    // 延迟调用StartDialogueAt，确保CSV文件已加载完成
    private System.Collections.IEnumerator StartDialogueAtIndexAfterDelay(int index)
    {
        // 等待一帧，确保CSV文件已加载
        yield return null;

        // 跳转到指定索引的对话
        dialogueManager.StartDialogueAt(index);
    }

    // 对话结束时调用
    private void OnDialogueEnd()
    {
        Debug.Log("EmbassyDialogueFlow: 对话结束，处理后续逻辑");

        // 检查当前对话是否是embassy_arrival.csv
        if (dialogueManager != null && dialogueManager.GetType().GetField("_currentCSVPath", BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            string currentCSVPath = dialogueManager.GetType().GetField("_currentCSVPath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dialogueManager) as string;
            if (!string.IsNullOrEmpty(currentCSVPath) && currentCSVPath.Contains("embassy_arrival.csv"))
            {
                // 显示AwardableQuestion问答预制体
                if (awardableQuestion != null)
                {
                    Debug.Log("EmbassyDialogueFlow: 显示AwardableQuestion问答预制体");
                    awardableQuestion.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("EmbassyDialogueFlow: 找不到AwardableQuestion组件");
                }
                return;
            }
        }

        if (currentDialogueType == "first")
        {
            // 第一个对话结束后，检查家人存活状态
            CheckFamilyStatus();
        }
        else if (currentDialogueType == "family_deceased" || currentDialogueType == "all_alive")
        {
            // 答题对话结束后，触发回家动画
            TriggerGoHomeAnimation();
        }

    }

    // 检查家人存活状态
    public void CheckFamilyStatus()
    {
        if (characterManager == null)
        {
            Debug.LogError("EmbassyDialogueFlow: 找不到CharacterManager组件");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("EmbassyDialogueFlow: 找不到DialogueManager组件");
            return;
        }

        // 获取所有角色数量和存活角色数量
        int totalCharacters = characterManager.GetAllCharacterStatus().Count;
        int aliveCharacters = characterManager.GetAllAliveCharacterStatus().Count;

        Debug.Log($"EmbassyDialogueFlow: 总角色数: {totalCharacters}, 存活角色数: {aliveCharacters}");

        // 判断是否有家人去世
        bool hasFamilyDeceased = aliveCharacters < totalCharacters;

        // 获取答题正确率状态
        bool isSuccessRateHigh = GetIsSuccessRate();
        Debug.Log($"EmbassyDialogueFlow: 答题正确率是否高于60%: {isSuccessRateHigh}");

        if (hasFamilyDeceased)
        {
            // 有家人去世
            currentDialogueType = "family_deceased";
            // 重置答题系统
            ResetAnswerSystem();
            // 加载对应的对话文件
            dialogueManager.StartDialogue(familyDeceasedDialogueFile);

            int targetDialogueIndex = isSuccessRateHigh ? 1 : 2;

            // 如果正确率高，则复活家人
            if (isSuccessRateHigh)
            {
                Debug.Log("EmbassyDialogueFlow: 答题正确率高于60%，复活家人");
                ReviveFamilyMembers();
            }

            StartCoroutine(StartDialogueAtIndexAfterDelay(targetDialogueIndex));
        }
        else
        {
            // 家人都存活
            currentDialogueType = "all_alive";
            // 重置答题系统
            ResetAnswerSystem();
            // 加载对应的对话文件
            dialogueManager.StartDialogue(allAliveDialogueFile);

            // 根据正确率跳转到不同的对话索引
            int targetDialogueIndex = isSuccessRateHigh ? 0 : 1;
            StartCoroutine(StartDialogueAtIndexAfterDelay(targetDialogueIndex));
        }
    }

    // 触发回家动画
    private void TriggerGoHomeAnimation()
    {
        Debug.Log("EmbassyDialogueFlow: 触发回家动画");

        Debug.Log("正在播放回家动画...");
    }

    // 重置答题系统
    private void ResetAnswerSystem()
    {
        currentQuestionIndex = 0;
    }


    // 获取AwardableQuestion中的正确率
    private bool GetIsSuccessRate()
    {
        if (awardableQuestion == null)
        {
            Debug.LogError("EmbassyDialogueFlow: 找不到AwardableQuestion组件");
            return false;
        }

        return awardableQuestion.rightCount > 8;
    }

    // 复活家人
    public void ReviveFamilyMembers()
    {
        if (characterManager == null)
        {
            Debug.LogError("EmbassyDialogueFlow: 找不到CharacterManager组件，无法复活家人");
            return;
        }

        // 获取所有角色
        var allCharacters = characterManager.GetAllCharacterStatus();

        foreach (var characterStatus in allCharacters)
        {
            if (!characterStatus.IsAlive)
            {
                // 这里需要通过修改体力值来复活角色
                Debug.Log($"EmbassyDialogueFlow: 复活角色: {characterStatus.CharacterID}");

                // 恢复体力值到最大值的50%来复活角色
                characterStatus.ModifyStamina(characterStatus.MaxStamina * 0.5f);
            }
        }
    }
}