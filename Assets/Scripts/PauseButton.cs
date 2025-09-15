using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public void OnPauseClick()
    {
        Time.timeScale = 0f;
    }

    public void OnResumeClick()
    {
        Time.timeScale = 1f;
    }
}