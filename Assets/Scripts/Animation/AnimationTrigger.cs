using UnityEngine;
using System.Collections;
using DialogueSystem;
/// <summary>
/// 动画触发器 - 控制窗口震动和相机地震动画每隔一段时间播放一次
/// </summary>
public class AnimationTrigger : MonoBehaviour
{
    [Header("动画设置")]
    public GameObject windowObject;  // 挂载了Animation组件的窗口物体
    public GameObject cameraObject;  // 挂载了Animation组件的相机物体

    [Header("触发设置")]
    public string windowAnimationName = "windowshake";  // Animation组件中的动画名称
    public string cameraAnimationName = "cameraEarthquake";  // Animation组件中的动画名称
    public float baseInterval = 10f;  // 基础间隔时间（秒）
    public float randomVariation = 2f;  // 随机变化范围（秒）

    [Header("倒塌机制设置")]
    public GameObject deadPanelPrefab;  // 死亡面板预制体引用
    public int collapseStartThreshold = 5;  // 开始计算倒塌概率的地震次数阈值
    public float baseCollapseProbability = 0.2f;  // 基础倒塌概率（20%）
    public float probabilityIncrement = 0.2f;  // 每次增加的概率（20%）

    private Coroutine animationCoroutine;  // 动画协程引用
    private int earthquakeCount = 0;  // 地震次数计数器
    private GameObject instantiatedDeadPanel;  // 实例化后的死亡面板引用

    private void Start()
    {
        // 启动动画循环协程
        StartAnimationCycle();
    }

    private void OnDestroy()
    {
        // 停止协程，避免内存泄漏
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // 清理实例化的面板
        if (instantiatedDeadPanel != null)
        {
            Destroy(instantiatedDeadPanel);
        }
    }

    /// <summary>
    /// 开始动画循环
    /// </summary>
    private void StartAnimationCycle()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationCycle());
    }

    /// <summary>
    /// 动画循环协程
    /// </summary>
    private IEnumerator AnimationCycle()
    {
        while (true)
        {
            // 计算随机间隔时间（基础时间 ± 随机变化）
            float randomInterval = baseInterval + Random.Range(-randomVariation, randomVariation);

            // 等待随机间隔时间
            yield return new WaitForSeconds(randomInterval);

            // 触发动画
            PlayAnimations();
        }
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    private void PlayAnimations()
    {
        // 增加地震计数
        earthquakeCount++;
        Debug.Log($"第 {earthquakeCount} 次地震发生");

        // 触发窗口震动动画
        if (windowObject != null)
        {
            Animation windowAnimation = windowObject.GetComponent<Animation>();
            if (windowAnimation != null && windowAnimation[windowAnimationName] != null)
            {
                windowAnimation.Play(windowAnimationName);
                Debug.Log($"触发窗口震动动画: {windowAnimationName}");
            }
            else
            {
                Debug.LogWarning("窗口物体上没有Animation组件或找不到指定名称的动画");
            }
        }

        // 触发相机地震动画
        if (cameraObject != null)
        {
            Animation cameraAnimation = cameraObject.GetComponent<Animation>();
            if (cameraAnimation != null && cameraAnimation[cameraAnimationName] != null)
            {
                cameraAnimation.Play(cameraAnimationName);
                Debug.Log($"触发相机地震动画: {cameraAnimationName}");
            }
            else
            {
                Debug.LogWarning("相机物体上没有Animation组件或找不到指定名称的动画");
            }
        }

        // 检查是否需要计算倒塌概率
        if (earthquakeCount >= collapseStartThreshold)
        {
            CheckBuildingCollapse();
        }
    }

    /// <summary>
    /// 检查楼房是否倒塌
    /// </summary>
    private void CheckBuildingCollapse()
    {
        // 计算当前倒塌概率
        float currentProbability = baseCollapseProbability + (earthquakeCount - collapseStartThreshold) * probabilityIncrement;
        // 确保概率不超过100%
        currentProbability = Mathf.Min(currentProbability, 1.0f);

        Debug.Log($"当前地震次数: {earthquakeCount}, 倒塌概率: {currentProbability:P0}");

        // 随机判断是否触发倒塌
        if (Random.value <= currentProbability)
        {
            TriggerBuildingCollapse();
        }
    }

    /// <summary>
    /// 触发楼房倒塌，显示死亡面板
    /// </summary>
    private void TriggerBuildingCollapse()
    {
        Debug.Log("楼房倒塌！触发死亡结局");
        
        // 暂停游戏
        Debug.Log("AnimationTrigger: 触发死亡结局，暂停游戏");
        GameManager.Instance?.ChangeGameState(GameState.Paused);
        
        // 如果有对话正在进行，结束对话
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null && dialogueManager.IsDialogueActive())
        {
            Debug.Log("AnimationTrigger: 结束当前正在进行的对话");
            // 反射调用EndDialogue方法，因为它是私有的
            System.Reflection.MethodInfo endDialogueMethod = typeof(DialogueManager).GetMethod("EndDialogue", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (endDialogueMethod != null)
            {
                endDialogueMethod.Invoke(dialogueManager, null);
            }
        }

        // 显示死亡面板
        if (deadPanelPrefab != null)
        {
            // 确保只实例化一个面板
            if (instantiatedDeadPanel == null)
            {
                // 查找Final Canvas作为父对象
                GameObject finalCanvas = GameObject.Find("Final Canvas");

                if (finalCanvas != null)
                {
                    // 在Final Canvas下实例化死亡面板
                    instantiatedDeadPanel = Instantiate(deadPanelPrefab, finalCanvas.transform);
                    Debug.Log("死亡面板已在Final Canvas下实例化并显示");
                }
                else
                {
                    // 如果找不到Final Canvas，使用默认实例化方式
                    instantiatedDeadPanel = Instantiate(deadPanelPrefab);
                    Debug.LogWarning("未找到Final Canvas，使用默认方式实例化死亡面板");
                }
            }
            else
            {
                instantiatedDeadPanel.SetActive(true);
                Debug.Log("死亡面板已显示（之前已实例化）");
            }
        }
        else
        {
            Debug.LogWarning("死亡面板预制体未设置，请在Inspector中赋值");
        }

        // 停止动画循环
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    /// <summary>
    /// 手动触发动画（可从外部调用）
    /// </summary>
    public void TriggerAnimations()
    {
        PlayAnimations();
    }

    /// <summary>
    /// 重置动画循环
    /// </summary>
    public void ResetAnimationCycle()
    {
        earthquakeCount = 0; // 重置地震计数

        // 清理实例化的面板
        if (instantiatedDeadPanel != null)
        {
            Destroy(instantiatedDeadPanel);
            instantiatedDeadPanel = null;
        }

        StartAnimationCycle();
    }

    /// <summary>
    /// 停止动画循环
    /// </summary>
    public void StopAnimationCycle()
    {
        // 停止并清除协程
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            Debug.Log("动画循环已完全停止");
        }
        
        // 停止正在播放的窗口动画
        if (windowObject != null)
        {
            Animation windowAnimation = windowObject.GetComponent<Animation>();
            if (windowAnimation != null)
            {
                windowAnimation.Stop();
                windowAnimation.Rewind();
                Debug.Log("窗口动画已停止并重置");
            }
        }
        
        // 停止正在播放的相机动画
        if (cameraObject != null)
        {
            Animation cameraAnimation = cameraObject.GetComponent<Animation>();
            if (cameraAnimation != null)
            {
                cameraAnimation.Stop();
                cameraAnimation.Rewind();
                Debug.Log("相机动画已停止并重置");
            }
        }
    }

    /// <summary>
    /// 获取当前地震次数（可从外部访问）
    /// </summary>
    public int GetEarthquakeCount()
    {
        return earthquakeCount;
    }

    /// <summary>
    /// 手动设置地震次数（用于测试或特殊情况）
    /// </summary>
    public void SetEarthquakeCount(int count)
    {
        earthquakeCount = Mathf.Max(0, count);
        Debug.Log($"手动设置地震次数为: {earthquakeCount}");
    }
}