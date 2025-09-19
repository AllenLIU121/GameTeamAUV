using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public void OnPauseClick()
    {
        GameManager.Instance.ChangeGameState(GameState.Paused);
        // Time.timeScale = 0f;
        // Debug.Log("Pause clicked, timeScale set to 0");
    }

    public void OnResumeClick()
    {
        GameManager.Instance.ChangeGameState(GameState.Playing);
        // Time.timeScale = 1f;
        // Debug.Log("Resume clicked, timeScale set to 1");
    }
}