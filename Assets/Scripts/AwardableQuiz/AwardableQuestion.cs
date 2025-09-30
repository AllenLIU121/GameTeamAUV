using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AwardableQuestion : MonoBehaviour
{
    public Sprite rightCircle;
    public Sprite falseCircle;
    public Sprite defaultCircle;
    public Sprite rightBar;

    public bool answerRight = false;
    public bool answerFalse = false;

    public Transform[] eachOptionFather = new Transform[4];
    public QuestionsSQ questionsSO;

    public Text theQuestion;
    public Text[] optionsText = new Text[4];
    public Button[] optionsBtn = new Button[4];
    public Image[] optionsToggleImage = new Image[4];

    public GameObject nextObject;
    public Transform Panel ;
    public GameObject Revive;

    public int questionIndex = 0;
    public int clickIndex = -1;
    //public int rightIndex = -1;
    //public Coroutine rightCoro;

    public float timer = 0;
    public int rightCount = 0;
    
    // 标志变量，用于确保最后一个问题的逻辑只执行一次
    private bool isFinalQuestionProcessed = false;

    // Start is called before the first frame update
    void Start()
    {
        nextObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            optionsText[i] = eachOptionFather[i].GetChild(0).GetComponent<Text>();
            optionsToggleImage[i] = eachOptionFather[i].GetChild(1).GetComponent<Image>();
            optionsBtn[i] = eachOptionFather[i].GetComponent<Button>();
        }
        
        Panel = this.transform.GetChild(1).GetComponent<Transform>();
        //Revive = this.transform.GetChild(3).GetComponent<Transform>();
    }

    // Update is called once per frame 
    void Update()
    {

        if (answerRight == true || answerFalse == true)
        {

            for (int i = 0; i < 4; i++)
            {
                optionsBtn[i].interactable = false;
            }
        }

        QuestionCheckOut();
        WhenFalse();
        WhenRight();
        AfterFinalQuestion();

    }

    void QuestionCheckOut()
    {

        if (clickIndex != -1 && clickIndex == questionsSO.eachQuestionRightIndex[questionIndex])
        {
            print("QuestionCheckOut1");

            answerRight = true;
        }
        if (clickIndex != -1 && clickIndex != questionsSO.eachQuestionRightIndex[questionIndex])
        {
            answerFalse = true;
            print("QuestionCheckOut2");

        }

    }
    void WhenRight()
    {
        if (answerRight)
        {
            optionsToggleImage[questionsSO.eachQuestionRightIndex[questionIndex]].sprite = rightCircle;
            timer += Time.deltaTime;

        }
        if (timer > 2 && answerRight)
        {
            questionIndex = questionIndex < 13 ? questionIndex += 1 : 13;
            theQuestion.text = questionsSO.questions[questionIndex];

            nextObject.SetActive(false);
            answerRight = false;
            clickIndex = -1;
            rightCount++;
            for (int i = 0; i < 4; i++)
            {
                optionsBtn[i].interactable = true;
                optionsToggleImage[i].sprite = defaultCircle;
                optionsText[i].text = questionsSO.options[questionIndex * 4 + i];
            }

            print("RefeshAndUpdata");
            timer = 0;
        }


    }


    public void WhenFalse()
    {




        if (answerFalse)
        {
            optionsToggleImage[questionsSO.eachQuestionRightIndex[questionIndex]].sprite = rightCircle;
            optionsToggleImage[clickIndex].sprite = falseCircle;

            for (int i = 0; i < 4; i++)
            {

                optionsBtn[i].interactable = false;
            }
            nextObject.SetActive(true);
            answerFalse = false;
        }
    }

    public void onClickA()
    {
        clickIndex = 0;
    }
    public void onClickB()
    {
        clickIndex = 1;
    }
    public void onClickC()
    {
        clickIndex = 2;
    }
    public void onClickD()
    {
        clickIndex = 3;
    }

    public void NextBtn()
    {
        questionIndex = questionIndex < 13 ? questionIndex += 1 : 13;
        theQuestion.text = questionsSO.questions[questionIndex];

        nextObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            optionsToggleImage[i].sprite = defaultCircle;
            optionsBtn[i].interactable = true;
            optionsText[i].text = questionsSO.options[questionIndex * 4 + i];

        }
        answerFalse = false;
        clickIndex = -1;
    }

    public void AfterFinalQuestion()
    {
        // 只有当questionIndex达到13且尚未处理过最后一个问题时，才执行逻辑
        if (questionIndex == 13 && !isFinalQuestionProcessed)
        {
            // 标记为已处理，防止重复执行
            isFinalQuestionProcessed = true;
            
            if (rightCount > 8)
            {
                Panel.gameObject.SetActive(false);
                nextObject.SetActive(false);
                Revive.GetComponent<Text>().text = @"他(她)回来了\o / \o / \o / \o /";
                Revive.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("正确率: " + rightCount);
                
                Panel.gameObject.SetActive(false);
                nextObject.SetActive(false);
                Revive.GetComponent<Text>().text = @"他(她)没回来/o \ /o \ /o \ /o \";
                Revive.gameObject.SetActive(true);
            }
            
            // 延迟调用EmbassyDialogueFlow的CheckFamilyStatus方法
            Invoke("CallEmbassyDialogueFlowCheckFamilyStatus", 2f);
        }
    }
    
    private void CallEmbassyDialogueFlowCheckFamilyStatus()
    {
        // 查找EmbassyDialogueFlow组件并调用CheckFamilyStatus方法
        EmbassyDialogueFlow dialogueFlow = FindObjectOfType<EmbassyDialogueFlow>();
        if (dialogueFlow != null)
        {
            Debug.Log("AwardableQuestion: 问答结束，调用EmbassyDialogueFlow的CheckFamilyStatus方法");
            dialogueFlow.CheckFamilyStatus();
        }
        else
        {
            // 添加容错处理：尝试直接从场景中查找并激活回家视频播放逻辑
            Debug.LogError("AwardableQuestion: 找不到EmbassyDialogueFlow组件，尝试直接触发回家流程");
            
            // // 查找回家视频播放器
            // VideoPlayer goHomeVideoPlayer = FindObjectOfType<VideoPlayer>();
            // if (goHomeVideoPlayer != null)
            // {
            //     Debug.Log("AwardableQuestion: 找到VideoPlayer组件，尝试直接播放回家视频");
            //     goHomeVideoPlayer.gameObject.SetActive(true);
            //     goHomeVideoPlayer.Play();
            // }
            // else
            // {
            //     Debug.LogError("AwardableQuestion: 找不到VideoPlayer组件，无法触发回家流程");
            // }
        }
    }
}


