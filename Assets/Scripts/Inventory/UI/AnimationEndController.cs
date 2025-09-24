using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AnimationComponentEndController : MonoBehaviour
{
    [Tooltip("挂载了Animation组件的目标物体")]
    public GameObject targetObject;

    [Tooltip("动画片段的名称")]
    public string animationClipName;

    [Tooltip("动画结束后要加载的场景名称")]
    public string nextSceneName;

    [Tooltip("黑屏过渡用的CanvasGroup组件")]
    public CanvasGroup blackScreen;

    [Tooltip("黑屏过渡的持续时间（秒）")]
    public float fadeDuration = 1f;

    private Animation targetAnimation;
    private bool isAnimationPlaying = false;
    private bool isAnimationCompleted = false;

    void Start()
    {
        // 初始化黑屏为透明
        if (blackScreen != null)
        {
            blackScreen.alpha = 0;
        }

        // 验证目标物体和Animation组件
        if (targetObject == null)
        {
            return;
        }

        // 获取目标物体上的Animation组件
        targetAnimation = targetObject.GetComponent<Animation>();

        // 开始异步加载下一场景
        StartCoroutine(LoadNextSceneAsync());
    }

    void Update()
    {
        if (targetAnimation == null || isAnimationCompleted)
            return;

        // 检测动画是否开始播放（首次进入播放状态）
        if (!isAnimationPlaying && targetAnimation.IsPlaying(animationClipName))
        {
            isAnimationPlaying = true;
        }

        // 检测动画是否从播放状态变为结束状态
        if (isAnimationPlaying && !targetAnimation.IsPlaying(animationClipName))
        {
            isAnimationCompleted = true;
            StartCoroutine(StartBlackScreenTransition());
        }
    }

    // 执行黑屏过渡效果
    IEnumerator StartBlackScreenTransition()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            if (blackScreen != null)
            {
                blackScreen.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (blackScreen != null)
        {
            blackScreen.alpha = 1f;
        }

        // 切换到下一场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator LoadNextSceneAsync()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }
        }
    }
}

