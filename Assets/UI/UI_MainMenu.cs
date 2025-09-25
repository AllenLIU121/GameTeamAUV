using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlaySFX("Assets/Audio/BGM/1.菜单界面0919-1.mp3");
    }

    void Update()
    {
        if (Input.anyKeyDown)
            GameManager.Instance.NewGame();
    }
}
