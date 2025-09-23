using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName; // 视频播放完成后要加载的场景名称

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
    void OnVideoEnd(VideoPlayer vp)
    {
        // 加载下一个场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // 可选：添加跳过视频的功能
    void Update()
    {
        if (Input.anyKeyDown)
        {
            // 停止视频播放
            videoPlayer.Stop();
            // 加载下一个场景
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}

