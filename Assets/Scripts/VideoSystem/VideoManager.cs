using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class VideoConfig
{
    public string videoID;
    public VideoClip videoClip; 
}

public class VideoManager : Singleton<VideoManager>
{
    private VideoPlayer videoPlayer;
    public List<VideoConfig> videoList;
    private Dictionary<string, VideoClip> videoDict = new Dictionary<string, VideoClip>();
    private float fadeDuration = 1f;
    private CanvasGroup canvasGroup;
    private string videoId;

    protected override void Awake()
    {
        base.Awake();

        // 确保视频播放器已赋值
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError($"[VideoManager] VideoPlayer is null");
        }

        if (videoList.Count <= 0)
        {
            Debug.LogError($"[VideoManager] VideoConfigs is null or empty");
        }
        else
        {
            InitializeVideoDict();
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError($"[VideoManager] CanvasGroup not found");
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        videoPlayer.loopPointReached += OnVideoEnd;
        EventManager.Instance.Subscribe<OnSceneLoaded>(HandleSceneLoaded);
    }

    // void Start()
    // {
    //     // 开始播放视频
    //     videoPlayer.Play();
    // }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoEnd;
        EventManager.Instance.Unsubscribe<OnSceneLoaded>(HandleSceneLoaded);
    }

    private void InitializeVideoDict()
    {
        foreach (VideoConfig videoEntry in videoList)
        {
            if (!videoDict.ContainsKey(videoEntry.videoID))
                videoDict[videoEntry.videoID] = videoEntry.videoClip;
        }
    }

    private void HandleSceneLoaded(OnSceneLoaded _)
    {
        videoPlayer.targetCamera = Camera.main;
    }

    public void PlayVideoClip(string videoID, float fadeDuration = 1f)
    {
        if (videoDict.TryGetValue(videoID, out VideoClip videoClip))
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.StopAllSFX();

            videoId = videoID;
            StartCoroutine(FadeInCanvas(videoClip, fadeDuration));
        }
    }

    // 视频播放结束时调用
    public void OnVideoEnd(VideoPlayer vp)
    {
        vp.clip = null;

        EventManager.Instance.Publish(new OnVideoEnd { videoId = videoId });

        videoId = "";
        StartCoroutine(FadeOutCanvas());

        // if (!string.IsNullOrEmpty(nextSceneName))
        // {
        //     if (SceneController.Instance != null)
        //         SceneController.Instance.LoadSceneAsync(nextSceneName);
        //     else
        //         SceneManager.LoadScene(nextSceneName);
        // }
    }

    // 可选：添加跳过视频的功能
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // // 停止视频播放
            // videoPlayer.Stop();
            // 加载下一个场景
            OnVideoEnd(videoPlayer);
        }
    }

    private IEnumerator FadeInCanvas(VideoClip videoClip, float fadeDuration)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
   
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }

    private IEnumerator FadeOutCanvas()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

