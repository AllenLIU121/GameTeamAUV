using System.Collections;
using UnityEngine;
using DialogueSystem;
public class EarthquakeFlowManager : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public float delayBetweenDialogues = 5f;
    private bool isSecondDialogueShown = false;
    private bool isThirdDialogueReady = false;
    private bool hasDisasterManual = false; // 标记玩家是否获得防灾手册

    void Start()
    {
        // 确保DialogueManager已经被初始化
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        // 开局就打开第一个文件对应的UI
        StartCoroutine(StartFirstDialogue());
    }

    IEnumerator StartFirstDialogue()
    {
        yield return new WaitForSeconds(1f); // 短暂延迟确保场景加载完成

        // 检查是否已有对话在进行，如果有则等待
        while (dialogueManager.IsDialogueActive())
        {
            yield return new WaitForSeconds(1f);
        }

        dialogueManager.SetDialogueType(true); // 设置为选择类型但没有选项，这样可以显示头像
        dialogueManager.StartDialogue("earthquake_first_encounter.csv");
        StartCoroutine(WaitForSecondDialogue());
    }

    IEnumerator WaitForSecondDialogue()
    {
        // 等待第一个对话结束
        while (dialogueManager.IsDialogueActive())
        {
            yield return null;
        }

        // 等待10秒后开启第二个文件对应的UI
        Debug.Log("第一个对话结束，10秒后开始自言自语对话");
        yield return new WaitForSeconds(5f);

        // 检查是否已有对话在进行，如果有则等待
        while (dialogueManager.IsDialogueActive())
        {
            yield return new WaitForSeconds(1f);
        }

        // 显示自言自语对话
        dialogueManager.SetDialogueType(true); // 设置为选择类型但没有选项，这样可以显示头像
        dialogueManager.StartDialogue("earthquake_warning.csv");

        // 等待自言自语对话结束
        while (dialogueManager.IsDialogueActive())
        {
            yield return null;
        }

        // 等待3秒后显示广播对话
        Debug.Log("自言自语对话结束，3秒后开始广播对话");
        yield return new WaitForSeconds(3f);

        // 检查是否已有对话在进行，如果有则等待
        while (dialogueManager.IsDialogueActive())
        {
            yield return new WaitForSeconds(1f);
        }

        dialogueManager.SetDialogueType(false);
        dialogueManager.StartDialogue("earthquake_broadcast.csv");
        isSecondDialogueShown = true;
    }

    void Update()
    {
        // 当玩家获得防灾手册且第三个对话未触发时，触发第三个对话
        if (hasDisasterManual && !isThirdDialogueReady && isSecondDialogueShown && !dialogueManager.IsDialogueActive())
        {
            isThirdDialogueReady = true;
            Debug.Log("玩家已获得防灾手册，触发第三个对话");
            StartCoroutine(TriggerThirdDialogueAfterDelay(2f)); // 短暂延迟后触发
        }
    }

    /// <summary>
    /// 供其他脚本调用，标记玩家获得了防灾手册
    /// </summary>
    public void SetPlayerHasDisasterManual()
    {
        hasDisasterManual = true;
        Debug.Log("玩家获得了防灾手册");
    }

    IEnumerator TriggerThirdDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 检查是否已有对话在进行，如果有则等待
        while (dialogueManager.IsDialogueActive())
        {
            yield return new WaitForSeconds(1f);
        }

        dialogueManager.SetDialogueType(true);
        dialogueManager.StartDialogue("earthquake_father_smoking.csv");
    }
}