using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlaySFX("菜单界面0919-1.mp3");
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            AudioManager.Instance.StopBGM();
            GameManager.Instance.NewGame();
        }
            
    }
}
