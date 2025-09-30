using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 

public class HandbookManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject handbookPanel;  // 手册弹窗
    public Button handbookIcon;  // 手册图标按钮
    public Button closeButton;  // 关闭按钮
    public Text handbookText;  // 手册内容文本
    
    void Start()
    {
        // 绑定按钮事件
        handbookIcon.onClick.AddListener(OpenHandbook); 
        closeButton.onClick.AddListener(CloseHandbook); 
        
        // 初始隐藏手册
        handbookPanel.SetActive(false); 
        
        // 设置手册内容
        SetupHandbookContent(); 
    }
    
    void SetupHandbookContent()
    {
        if (handbookText != null)
        {
            handbookText.fontSize = 17; 
            handbookText.lineSpacing = 1f; 
            // handbookText.verticalAlignment = VerticalAlignmentOptions.Top; 
            // handbookText.horizontalAlignment = HorizontalAlignmentOptions.Left; 
            // handbookText.enableWordWrapping = true;  // 重要：启用自动换行
        }
        string content = @"防灾手册

地震：
遇震时尽快跑向空旷区域; 远离窗户、外墙或吊挂物; 不要使用电梯; 如有火灾，先关电／燃气，用湿布捂口鼻，从安全楼梯撤离。

火灾：
发现火情马上报警; 穿过浓烟时弯腰／匍匐前进，用湿布捂口鼻; 不要使用电梯; 如果无法通过主要出口，去阳台或屋顶呼救。

台风：
风雨来袭前加固门窗，清理可被风吹动的物品; 台风中如果在户外，应尽快进入坚固建筑物避险; 不要靠近广告牌、电线杆、临时搭建物; 减少外出。

洪水：
避免涉水或驾车穿越积水路段; 如果感到家中或所处地点可能被淹，及时撤离至高地或安全建筑; 洪水中保持头部露出水面，抓漂浮物; 谨防污染物与电线。

海啸：
若在海边感知强震或海水异常退却，应立即朝高地／内陆撤离; 不要在海岸边观浪或等待第一波过去; 听从官方警报和撤离指令; 海啸可能多波发生。

滑坡／塌方：
大雨或地震后若地面出现裂缝、水流急增、树木倾斜等异常，应远离山坡脚或沟谷底; 在户外注意避开山墙、悬崖边; 若被困在碎石中，寻找稳固掩护，发出声响求救，不要乱动。"; 
        
        handbookText.text = content; 
    }
    
    void OpenHandbook()
    {
        handbookPanel.SetActive(true); 
        // 暂停游戏或禁用其他交互
        Time.timeScale = 0f; 
    }
    
    void CloseHandbook()
    {
        handbookPanel.SetActive(false); 
        // 恢复游戏
        Time.timeScale = 1f; 
    }
    
    // 快捷键支持（比如按H键打开手册）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!handbookPanel.activeInHierarchy)
                OpenHandbook(); 
            else
                CloseHandbook(); 
        }
        
        // 按ESC键也可以关闭手册
        if (Input.GetKeyDown(KeyCode.Escape) && handbookPanel.activeInHierarchy)
        {
            CloseHandbook(); 
        }
    }
}