using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Inventory;

// 处理UI元素的拖拽逻辑，包括物品栏中的物品拖拽到角色栏或场景中
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("拖拽设置")]
    [Tooltip("要在场景中生成的物品预制体")]
    public GameObject itemPrefab;
    [Tooltip("是否使用表面作为拖拽平面")]
    public bool dragOnSurfaces = true;
    [Tooltip("拖拽图标的透明度")]
    [Range(0f, 1f)] public float dragIconAlpha = 0.8f;
    [Tooltip("是否允许拖放到场景中")]
    public bool allowDropToScene = true;
    [Tooltip("是否在拖拽时阻止物品栏滚动")]
    public bool blockScrollWhenDragging = true;

    // 拖拽状态
    private GameObject m_DraggingIcon;
    private RectTransform m_DraggingPlane;
    private CanvasGroup canvasGroup;
    private Image originalImage; // 原始物品图像组件
    private int slotIndex = -1; // 当前物品槽索引

    // 缓存组件引用
    private ScrollRect parentScrollRect;
    private Canvas parentCanvas;
    private InventoryManager cachedInventoryManager;

    private void Awake()
    {
        // 缓存InventoryManager引用，避免多次查找
        cachedInventoryManager = FindObjectOfType<InventoryManager>();
    }

    /// <summary>
    /// 鼠标按下时调用，准备拖拽所需数据
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
#if DEBUG
        Debug.Log("[DragHandler] 鼠标按下: " + gameObject.name);
#endif

        // 尝试获取父级的SingleSlotPanel来获取槽位索引
        SingleSlotPanel slotPanel = GetComponentInParent<SingleSlotPanel>();
        if (slotPanel != null)
        {
            // 尝试通过反射获取槽位索引（临时解决方案，建议在SingleSlotPanel中提供公开方法）
            try
            {
                var fieldInfo = slotPanel.GetType().GetField("slotIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    slotIndex = (int)fieldInfo.GetValue(slotPanel);
#if DEBUG
                    Debug.Log("[DragHandler] 获取到槽位索引: " + slotIndex);
#endif
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[DragHandler] 无法获取槽位索引: " + e.Message);
            }
        }

        // 查找父级的ScrollRect组件
        parentScrollRect = FindInParents<ScrollRect>(gameObject);
#if DEBUG
        if (parentScrollRect != null)
        {
            Debug.Log("[DragHandler] 找到父级ScrollRect: " + parentScrollRect.name);
        }
        else
        {
            Debug.LogWarning("[DragHandler] 未找到父级ScrollRect组件");
        }
#endif
    }

    /// <summary>
    /// 开始拖拽时调用，创建拖拽图标和设置拖拽状态
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
#if DEBUG
        Debug.Log("[DragHandler] 开始拖拽: " + gameObject.name + " (槽位索引: " + slotIndex + ")");
#endif

        // 阻止ScrollRect滚动（如果启用了此功能）
        if (blockScrollWhenDragging && parentScrollRect != null)
        {
            parentScrollRect.enabled = false;
#if DEBUG
            Debug.Log("[DragHandler] 已禁用ScrollRect滚动");
#endif
        }

        // 获取画布
        parentCanvas = FindInParents<Canvas>(gameObject);
        if (parentCanvas == null)
        {
            Debug.LogError("[DragHandler] 找不到父级Canvas组件，无法开始拖拽");
            return;
        }

        // 存储原始图像引用
        originalImage = GetComponent<Image>();
        if (originalImage == null)
        {
            Debug.LogError("[DragHandler] 缺少Image组件，无法开始拖拽");
            return;
        }

        // 创建临时拖拽图标
        CreateDraggingIcon();

        // 设置初始位置
        SetDraggedPosition(eventData);

        // 拖拽已开始
    }

    /// <summary>
    /// 创建拖拽图标并设置其属性
    /// </summary>
    private void CreateDraggingIcon()
    {
        // 安全检查
        if (originalImage == null || parentCanvas == null)
        {
            Debug.LogError("[DragHandler] 无法创建拖拽图标，必要组件为空");
            return;
        }

        // 创建临时拖拽图标
        m_DraggingIcon = new GameObject("DraggingIcon");
        m_DraggingIcon.transform.SetParent(parentCanvas.transform, false);
        m_DraggingIcon.transform.SetAsLastSibling();

        // 添加图像组件并复制原物品图像
        var image = m_DraggingIcon.AddComponent<Image>();
        image.sprite = originalImage.sprite;
        image.SetNativeSize();

        // 设置合适的锚点和对齐方式
        var rt = m_DraggingIcon.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;

            // 复制原图像的RectTransform大小
            RectTransform originalRt = originalImage.GetComponent<RectTransform>();
            if (originalRt != null)
            {
                rt.sizeDelta = originalRt.sizeDelta;
            }
        }

        // 添加CanvasGroup使图标忽略射线检测并设置透明度
        canvasGroup = m_DraggingIcon.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = dragIconAlpha;

        // 设置拖拽平面
        if (dragOnSurfaces)
            m_DraggingPlane = transform as RectTransform;
        else
            m_DraggingPlane = parentCanvas.transform as RectTransform;

        // 调整原始图像的CanvasGroup（如果存在）
        var originalCanvasGroup = GetComponent<CanvasGroup>();
        if (originalCanvasGroup == null)
        {
            originalCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            originalCanvasGroup.blocksRaycasts = true;
        }
        originalCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 拖拽过程中调用，更新拖拽图标的位置
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (m_DraggingIcon != null)
        {
            SetDraggedPosition(eventData);
        }
        else
        {
            Debug.LogWarning("[DragHandler] 拖拽图标为空，无法更新位置");
        }
    }

    /// <summary>
    /// 结束拖拽时调用，处理拖拽完成后的逻辑
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 清理拖拽图标
        CleanupDraggingIcon();

        // 恢复原始图像的透明度
        if (originalImage != null)
        {
            var originalCanvasGroup = GetComponent<CanvasGroup>();
            if (originalCanvasGroup != null)
            {
                originalCanvasGroup.alpha = 1.0f;
            }
        }

        // 恢复ScrollRect滚动（如果之前禁用了它）
        if (blockScrollWhenDragging && parentScrollRect != null)
        {
            parentScrollRect.enabled = true;
        }

        // 处理拖放逻辑
        ProcessDrop(eventData);

        // 拖拽已结束
        slotIndex = -1;
    }

    /// <summary>
    /// 清理拖拽图标和相关资源
    /// </summary>
    private void CleanupDraggingIcon()
    {
        if (m_DraggingIcon != null)
        {
            // 使用DestroyImmediate以确保立即清理
            DestroyImmediate(m_DraggingIcon);
            m_DraggingIcon = null;
            canvasGroup = null;
            m_DraggingPlane = null;
        }
    }

    /// <summary>
    /// 处理拖放操作，包括拖放到场景或UI元素上
    /// </summary>
    private void ProcessDrop(PointerEventData eventData)
    {
        if (!allowDropToScene || itemPrefab == null)
            return;

        // 获取所有在鼠标位置的Raycast结果
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        // 检查是否有UI元素被击中
        bool isOverUI = raycastResults.Count > 0;

        if (!isOverUI)
        {
            // 拖放到场景中
            DropToScene(eventData);
        }
        else
        {
            // 拖放到UI元素上
            DropToUIElement(eventData);
        }
    }

    /// <summary>
    /// 处理拖放到场景中的逻辑
    /// </summary>
    private void DropToScene(PointerEventData eventData)
    {
        // 在鼠标位置生成物品
        if (Camera.main == null || parentCanvas == null || slotIndex < 0 || cachedInventoryManager == null)
            return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localPoint))
        {
            // 将物品实例化在Canvas下，确保UI系统能正确渲染
            GameObject instantiatedItem = Instantiate(itemPrefab, parentCanvas.transform);

            // 设置物品在Canvas中的位置和缩放
            RectTransform itemRt = instantiatedItem.GetComponent<RectTransform>();
            if (itemRt != null)
            {
                itemRt.anchoredPosition = localPoint;
                itemRt.localScale = Vector3.one;
#if DEBUG
                Debug.Log("[DragHandler] 物品成功放置到Canvas: " + instantiatedItem.name + " 在本地位置: " + localPoint);
#endif
            }

            // 获取物品信息并设置到WorldItemClickHandler组件
            // 使用完全限定名确保正确获取组件
            Inventory.WorldItemClickHandler clickHandler = instantiatedItem.GetComponent<Inventory.WorldItemClickHandler>();
            if (clickHandler != null)
            {
                // 使用GameStateManager获取物品ID和数量
                var gameData = GameStateManager.Instance?.currentData;
                if (gameData != null && slotIndex < gameData.inventorySlots.Count)
                {
                    var slot = gameData.inventorySlots[slotIndex];
                    clickHandler.itemID = slot.itemID;
                    clickHandler.quantity = 1; // 每次拖放一个物品
                }
            }
        }
    }

    /// <summary>
    /// 处理拖放到UI元素上的逻辑
    /// </summary>
    private void DropToUIElement(PointerEventData eventData)
    {
        GameObject dropTarget = eventData.pointerEnter;
        if (dropTarget == null)
            return;

        // 检查是否是角色槽位
        CharacterSlot characterSlot = dropTarget.GetComponent<CharacterSlot>();
        if (characterSlot != null)
        {
            HandleDropOnCharacterSlot(characterSlot);
        }
        // 可以在这里添加其他类型的UI元素处理逻辑
    }

    /// <summary>
    /// 处理物品拖拽到角色槽位上的逻辑
    /// </summary>
    private void HandleDropOnCharacterSlot(CharacterSlot characterSlot)
    {
#if DEBUG
        Debug.Log("[DragHandler] 物品放置到角色栏: " + characterSlot.gameObject.name);
#endif

        // 使用缓存的InventoryManager引用
        if (cachedInventoryManager != null && slotIndex != -1)
        {
            // 调用InventoryManager处理物品拖拽到角色的逻辑
            cachedInventoryManager.HandleDropOnCharacter(slotIndex, characterSlot.GetCharacterID());
#if DEBUG
            Debug.Log("[DragHandler] 物品拖拽到角色 " + characterSlot.GetCharacterID() + " 上");
#endif
        }
        else
        {
            // 缓存失效时，尝试重新获取
            cachedInventoryManager = FindObjectOfType<InventoryManager>();
            if (cachedInventoryManager != null && slotIndex != -1)
            {
                cachedInventoryManager.HandleDropOnCharacter(slotIndex, characterSlot.GetCharacterID());
            }
            else
            {
                Debug.LogWarning("[DragHandler] 无法处理物品拖拽到角色，InventoryManager为空或槽位索引无效");
            }
        }
    }

    /// <summary>
    /// 设置拖拽图标的位置，考虑Canvas的渲染模式
    /// </summary>
    private void SetDraggedPosition(PointerEventData eventData)
    {
        if (m_DraggingIcon == null || m_DraggingPlane == null)
        {
            Debug.LogWarning("[DragHandler] 无法设置拖拽位置：拖拽图标或拖拽平面为空");
            return;
        }

        var rt = m_DraggingIcon.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("[DragHandler] 拖拽图标缺少RectTransform组件");
            return;
        }

        // 根据Canvas的渲染模式调整位置计算方式
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 直接使用屏幕坐标
            rt.position = eventData.position;
        }
        else
        {
            // 计算世界空间中的位置
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                m_DraggingPlane,
                eventData.position,
                eventData.pressEventCamera,
                out globalMousePos))
            {
                rt.position = globalMousePos;
                rt.rotation = m_DraggingPlane.rotation;
            }
            else
            {
                // 回退方案
                rt.position = eventData.position;
            }
        }

        rt.localScale = Vector3.one;
    }

    /// <summary>
    /// 在游戏对象及其父对象中查找指定类型的组件
    /// </summary>
    /// <typeparam name="T">要查找的组件类型</typeparam>
    /// <param name="go">起始游戏对象</param>
    /// <returns>找到的组件，如果没有找到则返回null</returns>
    private static T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null)
            return null;

        var component = go.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        Transform t = go.transform.parent;
        while (t != null)
        {
            component = t.gameObject.GetComponent<T>();
            if (component != null)
                return component;

            t = t.parent;
        }

        return null;
    }

    /// <summary>
    /// 防止拖拽图标泄漏，在对象销毁时确保清理
    /// </summary>
    private void OnDestroy()
    {
        CleanupDraggingIcon();
    }
}