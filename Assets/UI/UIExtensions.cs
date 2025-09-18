using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIExtensions
{
    private static Dictionary<int, float> lastClickTimes = new Dictionary<int, float>();

    /// <summary>
    /// 添加按钮监听器
    /// 1. 防重监听
    /// 2. 防抖
    /// 3. 点击时播放音效
    /// </summary>
    public static void AddButtonListener(this Button button, UnityAction action, float debounceTime = 0.1f, string clickSound = null)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // 按钮防抖
            int instanceId = button.GetInstanceID();
            lastClickTimes.TryGetValue(instanceId, out float lastClickTime);
            if (Time.unscaledTime - lastClickTime < debounceTime) return;
            lastClickTimes[instanceId] = Time.unscaledTime;

            // 播放点击音效
            if (!string.IsNullOrEmpty(clickSound))
            {
                // AudioManager.Instance.PlaySFX("ui click sfx");
            }

            action?.Invoke();
        });
    }
}
