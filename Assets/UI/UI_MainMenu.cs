using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    private bool isGameStart = false;

    void Awake()
    {
        EventManager.Instance.Subscribe<OnVideoEnd>(HandleVideoEnd);
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM("菜单界面0919-1.mp3");
    }

    void OnDestroy()
    {
        EventManager.Instance.Unsubscribe<OnVideoEnd>(HandleVideoEnd);
    }

    void Update()
    {
        if (!isGameStart && Input.anyKeyDown)
        {
            isGameStart = true;
            AudioManager.Instance.StopBGM();
            VideoManager.Instance.PlayVideoClip("game start");
            GetComponentInChildren<Canvas>().gameObject.SetActive(false);
        }
    }

    void HandleVideoEnd(OnVideoEnd videoData)
    {
        if (videoData.videoId == "game start")
        {
            GameManager.Instance.NewGame();
        }
    }
}
