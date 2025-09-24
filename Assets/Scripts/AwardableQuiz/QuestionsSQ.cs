using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName ="Qusetions") ]
public class QuestionsSQ : ScriptableObject
{
    public string[] questions = new string[14];
    public string[] options = new string[56];
    public int[] eachQuestionRightIndex = new int[14];
    //public Options[] eachOption = new Options[4];
}

//[Serializable]
//public class Options
//{
//    public GameObject eachOptionFather;
//    public GameObject eachOptionText;
//    public GameObject eachOptoionToggle;
//}

