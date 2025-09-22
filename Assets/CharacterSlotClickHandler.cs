using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Unity.VisualScripting;

public class CharacterSlotClickHandler : MonoBehaviour
{
    [Header("角色SO")]
    [SerializeField]private CharacterSO characterSO;

    private SkillManager skillManager;
    public float currentCooldown;
    
    void Start()
    {
        skillManager= GameStateManager.Instance.Skill;
        skillManager.ActivateSkill(characterSO.characterID, characterSO.skill.skillID,0);
    }

    private float lastCooldown = -1f;

    private void Update()
    { 
        if (characterSO == null || characterSO.skill == null) return;
        currentCooldown = skillManager.GetRemainingCooldown(characterSO.characterID, characterSO.skill.skillID);
        Debug.Log(currentCooldown);
        
    }
    
}

