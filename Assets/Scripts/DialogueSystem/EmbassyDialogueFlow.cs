using UnityEngine;
using DialogueSystem;
using System.Reflection;
using System.Collections;

public class EmbassyDialogueFlow : MonoBehaviour
{
    [Header("对话文件配置")]
    public string firstDialogueFile = "embassy_arrival.csv";    // 视频结束后的第一个对话
    public string familyDeceasedDialogueFile = "embassy_loss.csv";  // 家人有去世时的对话
    public string allAliveDialogueFile = "embassy_success.csv";     // 家人都存活时的对话

    [Header("组件引用")]
    private DialogueManager dialogueManager;
    private CharacterManager characterManager;
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

        if (awardableQuestion == null)
        {
            awardableQuestion = FindObjectOfType<AwardableQuestion>();
            if (awardableQuestion == null)
            {
                Debug.LogWarning("EmbassyDialogueFlow: 初始查找未找到AwardableQuestion组件，将在需要时再次尝试查找");
            }
        }

        EventManager.Instance.Subscribe<OnVideoEnd>(HandleVideoEnd);
    }

    private void Start()
    {
        if (VideoManager.Instance == null)
        {
            Debug.LogError($"[EmbassyDialogueFlow] VideoManager is null");
            return;
        }

        VideoManager.Instance.PlayVideoClip("game complete");
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnVideoEnd>(HandleVideoEnd);
        }
    }

    // 视频播放结束时触发
    private void HandleVideoEnd(OnVideoEnd videoData)
    {
        switch (videoData.videoId)
        {
            case "game complete":
                AudioManager.Instance.PlayBGM("结束钢琴0919-1.mp3");
                // 直接检查家人存活状态并跳转到相应对话
                if (dialogueManager != null && characterManager != null)
                {
                    CheckFamilyStatusForArrival();
                }
                break;

            case "questionnaire complete":
                OnGoHomeVideoEnd();
                break;
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

        StartCoroutine(WaitForDialogueEnd());
        Debug.Log($"dialogueActive: {dialogueManager.dialogueActive}");
        // OnQuestionsEnd();
    }

    private IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitUntil(() => !dialogueManager.dialogueActive);
        OnQuestionsEnd();
    }

    // 对话结束时调用
    private void OnQuestionsEnd()
    {
        Debug.Log("EmbassyDialogueFlow: 对话结束，处理后续逻辑");

        // 检查当前对话是否是embassy_arrival.csv
        if (dialogueManager != null && dialogueManager.GetType().GetField("_currentCSVPath", BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            string currentCSVPath = dialogueManager.GetType().GetField("_currentCSVPath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dialogueManager) as string;
            if (!string.IsNullOrEmpty(currentCSVPath) && currentCSVPath.Contains("embassy_arrival.csv"))
            {
                // 显示AwardableQuestion问答预制体
                if (awardableQuestion == null)
                {
                    // 尝试重新查找AwardableQuestion组件
                    awardableQuestion = FindObjectOfType<AwardableQuestion>();
                }

                if (awardableQuestion != null)
                {
                    Debug.Log("EmbassyDialogueFlow: 显示AwardableQuestion问答预制体");
                    awardableQuestion.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("EmbassyDialogueFlow: 找不到AwardableQuestion组件，将直接触发回家流程");
                    // 直接触发回家流程，避免游戏卡住
                    TriggerGoHomeAnimation();
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
            awardableQuestion.gameObject.SetActive(false);
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

        // 开始播放回家视频
        VideoManager.Instance.PlayVideoClip("questionnaire complete");

        Debug.Log("EmbassyDialogueFlow: 开始播放回家视频");
    }

    // 回家视频播放结束时调用
    private void OnGoHomeVideoEnd()
    {
        Debug.Log("EmbassyDialogueFlow: 回家视频播放结束，退出游戏");

        EventManager.Instance.Publish(new OnGameFinished());

        // 重新开始游戏
        SceneController.Instance.LoadSceneAsync(GameConstants.SceneName.MenuScene);
        AddressableManager.Instance.ReleaseAssetsByLabel(GameConstants.AddressablesAssetLabel.GameAudio);

        // 在编辑器模式下停止播放
#if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPlaying = false;
#endif
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
            // 尝试重新查找AwardableQuestion组件
            awardableQuestion = FindObjectOfType<AwardableQuestion>();
            if (awardableQuestion == null)
            {
                Debug.LogWarning("EmbassyDialogueFlow: 找不到AwardableQuestion组件，使用默认正确率结果");
                return true; // 使用默认值true，确保游戏流程能继续
            }
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
            characterStatus.Resurrect();
        }
    }
}