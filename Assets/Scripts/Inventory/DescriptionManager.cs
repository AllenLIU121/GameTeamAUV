using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

public class DescriptionManager : MonoBehaviour,IPointerDownHandler
{
    public GameObject DescriptionPanel;
    public Text DescriptionText;
    public CharacterSO Character;
    private CharacterSlot characterSlot;

    private CharacterPanel characterpanel;
    private Canvas parentCanvas;
    private RectTransform tooltipRect;

    void Start()
    {
        //开局隐藏
        DescriptionPanel.SetActive(false);
        DescriptionText.fontSize = 9;
        parentCanvas = DescriptionPanel.GetComponentInParent<Canvas>();
        tooltipRect = DescriptionPanel.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        characterSlot =  GetComponent<CharacterSlot>();
        string characterName = characterSlot.ToString();
        characterName = Regex.Replace(characterName, @"\s*\(.*\)", "").Trim();
        DisplayDescription(Character);
    }
    
    //物品描述
    public void DisplayDescription(ItemSO descriptionItem)
    {
        switch (descriptionItem.itemID)
        {
            case "biscuit":
                DescriptionText.text = @"【压轴硬货】 - 压缩饼干
效果： 饥饿+35，体力+20
评语： 重量轻，抗饿强，完美平衡了便携与效能。";
                break;
            case "canned_meat":
                DescriptionText.text = @"【开罐即食的快乐】 - 肉罐头
效果： 饥饿+30，体力+20
评语： 无需烹饪，安全感十足。";
                break;
            case "cooked_instant_noodle":
                DescriptionText.text = @"【灵魂慰藉】 - 泡面
效果： 饥饿+20，体力+10
评语： 收益不高，但提供的是“现代文明”的精神满足感。";
                break;
            case "cooked_meat":
                DescriptionText.text = @"【硬菜核心】 - 熟肉
效果： 饥饿+40，体力+25";
                break;
            case "cooked_rice":
                DescriptionText.text = @"【五公斤的希望】 - 大米
效果： 饥饿+20，体力+30
评语： 团队的战略储备粮！一旦被奶奶的锅铲点化，就是全家的盛宴。";
                break;
            case "freeze_dried_food":
                DescriptionText.text = @"【轻量级选手】 - 冻干食品
效果： 饥饿+30，体力+15
评语： 压缩饼干的温和版，虽然收益略低，但味道还不错。";
                break;
            case "milk_powder":
                DescriptionText.text = @"【宝宝专属口粮】 - 奶粉 / 婴儿辅食
效果： 奶粉（饥饿+30，体力+15）；辅食（饥饿+35，体力+20）
评语： 弟弟专属！";
                break;
            case "raw_meat":
                DescriptionText.text = @"【硬菜核心】 - 生肉 (需烹饪)
效果： 烹饪后饥饿+40，体力+25";
                break;
            case "raw_rice":
                DescriptionText.text = @"【五公斤的希望】 - 大米 (需烹饪)
效果： 烹饪后=5份米饭，总收益：饥饿+250，体力+150
评语： 团队的战略储备粮！一旦被奶奶的锅铲点化，就是全家的盛宴。";
                break;
            case "uncooked_instant_noodle":
                DescriptionText.text = @"【灵魂慰藉】 - 泡面 (需烹饪)
效果： 烹饪后饥饿+20，体力+10
评语： 收益不高，但提供的是“现代文明”的精神满足感。";
                break;
            case "water":
                DescriptionText.text = @"【生命之水】 - 饮用水
效果： 饥饿+10，体力+30
评语：人体必需品！";
                break;
            case "antibiotics":
                DescriptionText.text = @"抗生素
效果：直接移咳嗽负面状态，并恢复10点体力。
评语：孩子咳嗽老不好？试试抗生素？";
                break;
            case "antidiarrheal":
                DescriptionText.text = @"止泻药
效果：直接移除腹泻负面状态，并恢复10点体力。
评语：肠胃不好很需要！";
                break;
            case "cold_medicine":
                DescriptionText.text = @"感冒药
效果： 直接移除感冒负面状态，并恢复10点体力。
评语：家中常备！";
                break;
            case "baby_food":
                DescriptionText.text = @"【婴儿辅食】-婴儿专属
效果：饥饿值+35，体力值+20
评语： 这个还是弟弟吃比较好一点。";
                break;
            case "canned_yellow_peach":
                DescriptionText.text = @"【妈妈的圣物·万物皆可治】 - 黄桃罐头
效果： 饥饿+10，体力+15，并 【疾病导致的体力流失速度减半，持续60秒！】
评语： 神器！不仅是食物，更是战略医疗物资。";
                break;
            case "desserts":
                DescriptionText.text = @"【甜蜜充电宝】 - 甜点
效果： 饥饿+5，并 【持续5秒恢复共10点体力】
评语： 维生素的甜蜜版，瞬间回体量稍低但持久。";
                break;
            case "vegetable_drink":
                DescriptionText.text = @"【轻体小清新】 - 蔬菜果汁
效果： 饥饿+3，并 【持续2秒恢复共2点体力】
评语： 效果比较微妙，大概相当于喝了一口功能饮料。";
                break;
            case "vitamins":
                DescriptionText.text = @"【续命小药丸】 - 维生素
效果： 饥饿+1，并 【持续5秒恢复共12.5点体力】
评语： 别看它加饿少，细水长流才是真！";
                break;
            default:
                break;
        }
        SetPanelPosition();
        DescriptionPanel.SetActive(true);
    }
    
    //人物描述
    public void DisplayDescription(CharacterSO descriptionCharacter)
    {
        switch (descriptionCharacter.skill.skillID)
        {
            case "brother_skill" :
                DescriptionText.text = @"弟弟：有弟弟在感觉体力都变多了！，我不敢想失去弟弟会有什么后果…";
                break;
            case "cat_skill" :
                DescriptionText.text = @"猫：幸运猫猫！有它在找到特殊资源的概率谜之上升！也是哄弟弟的好帮手，不知道为什么弟弟总是很喜欢猫猫，摸到猫猫的弟弟会非常开心！";
                break;
            case "father_skill" :
                DescriptionText.text = @"爸爸：不靠谱的爸爸但父爱如山，能扛！爸爸在不需要担心有东西提不动！背包负重+5kg，名副其实的家庭劳动力";
                break;
            case "grandmother_father_skill" :
                DescriptionText.text = @"奶奶：奶奶掌勺，化腐朽为神奇，什么食物经过奶奶的手都能变得无比美味！这就是奶奶的味道！";
                break;
            case "grandmother_mother_skill" :
                DescriptionText.text = @"姥姥：在姥姥眼里食物没有”变质”的概念，经过姥姥的手可以将那些在变质边缘疯狂试探的食物重获新生！效果堪比冰箱！";
                break;
            case "mom_skill" :
                DescriptionText.text = @"妈妈：妈妈牌家庭医生总会备着一些需要的药品。更恐怖的是，她掌握着宇宙的终极治愈法则——黄桃罐头。无论你是感冒发烧还是心情低落，一罐下去，满血复活，药到病除！";
                break;
            case "sister_skill" :
                DescriptionText.text = @"妹妹：看到活泼可爱的妹妹，无论有什么困难都可以撑住，在她的可爱光环照耀下，大家的体力条都掉得慢了一些。";
                break;
            default:
                break;
        }

        SetPanelPosition();
        DescriptionPanel.SetActive(true);
        
    }
    //  设置ui位置
    private void SetPanelPosition()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localPoint);
        //加一个偏移量，要不然挡住了ui
        tooltipRect.anchoredPosition = localPoint + new Vector2(20, 20);
        //有个bug，就是坐标改变了，但是视图没变，再次打印坐标发现坐标又回去了
        Debug.Log(tooltipRect.anchoredPosition);
    }
    
}
