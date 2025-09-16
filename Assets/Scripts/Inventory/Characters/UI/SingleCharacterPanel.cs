using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleCharacterPanel : MonoBehaviour
{
    // [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider skillCooldownSlider;

    public void InitializeCharacterData(Sprite portrait, float stamina, float hunger)
    {
        // characterName.text = name;
        characterPortrait.sprite = portrait;
        staminaSlider.maxValue = stamina;
        staminaSlider.value = stamina;
        hungerSlider.maxValue = hunger;
        hungerSlider.value = hunger;
        // skillCooldownSlider.value = 0f;
    }

    // ---------------- 角色数值更新接口 ----------------
    public void UpdateCharacterStamina(float stamina) => staminaSlider.value = stamina;
    public void UpdateCharacterHunger(float hunger) => hungerSlider.value = hunger;
    public void UpdateCharacterSkillCooldown(float skillCooldown) => skillCooldownSlider.value = skillCooldown;
}
 