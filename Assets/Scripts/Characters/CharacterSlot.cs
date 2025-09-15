using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    [Header("角色UI组件")]
    public Image avatar;     
    public Slider hungerBar; 
    public Slider staminaBar;  

    private string _characterID; 

    public void InitSlot(CharacterSO characterSO, float initHunger, float initStamina)
    {
        _characterID = characterSO.characterID;
        avatar.sprite = characterSO.characterPortrait; 

        hungerBar.maxValue = characterSO.maxHunger;
        staminaBar.maxValue = characterSO.maxStamina;

        hungerBar.value = initHunger;
        staminaBar.value = initStamina;
    }

    public void UpdateSlot(float newHunger, float newStamina)
    {
        Debug.Log($"[CharacterSlot] 角色 {_characterID} 更新UI - 饥饿：{newHunger}，体力：{newStamina}");
        hungerBar.value = newHunger;
        staminaBar.value = newStamina;
        // 额外检查Slider组件是否有效
        if (hungerBar == null) Debug.LogError($"[CharacterSlot] 角色 {_characterID} 的hungerBar未赋值！");
        if (staminaBar == null) Debug.LogError($"[CharacterSlot] 角色 {_characterID} 的staminaBar未赋值！");
    }

    public string GetCharacterID() => _characterID;
}