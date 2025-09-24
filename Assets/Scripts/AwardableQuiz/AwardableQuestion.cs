using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        //AfterFinalQuestion();

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
        if (questionIndex == 13  && rightCount  > 8)
        {

            Panel.gameObject.SetActive(false);
            nextObject.SetActive(false);
            Revive.GetComponent<Text>().text = @"他(她)回来了\o / \o / \o / \o /";
            Revive.gameObject.SetActive(true);
        }
        else if(questionIndex == 13)
        {
            print(rightCount);
   
            Panel.gameObject.SetActive(false);
            nextObject.SetActive(false);
            Revive.GetComponent<Text>().text = @"他(她)没回来/o \ /o \ /o \ /o \";
            Revive.gameObject.SetActive(true);
            
        }
    }
}


