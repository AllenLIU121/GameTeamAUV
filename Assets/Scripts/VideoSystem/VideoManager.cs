using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName; // 视频播放完成后要加载的场景名称

    void Start()
    {
        // 确保视频播放器已赋值
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // 注册视频播放完成事件
        videoPlayer.loopPointReached += OnVideoEnd;

        // 开始播放视频
        videoPlayer.Play();
    }

    // 视频播放结束时调用
    public void OnVideoEnd(VideoPlayer vp)
    {
        string sceneToLoad = nextSceneName;
        if(!string.IsNullOrEmpty(sceneToLoad)){
            if (SceneController.Instance != null)
                SceneController.Instance.LoadSceneAsync(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }

        else
        {
            gameObject.SetActive(false);
        }
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
}

