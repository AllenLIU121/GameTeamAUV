using System.Collections.Generic;
using UnityEngine;

public class UI_CharactersPanel : MonoBehaviour
{
    [SerializeField] private GameObject singleCharacterPanel;
    private Dictionary<string, SingleCharacterPanel> characterPanels = new Dictionary<string, SingleCharacterPanel>();

    private void Start()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.Character != null)
        {
            List<CharacterSO> charactersSO = GameStateManager.Instance.Character.charactersSO;
            InitializeCharacterPanels(charactersSO);
        }
        else
        {
            Debug.LogError("GameStateManager or CharacterManager is null.");
            return;
        }

        EventManager.Instance.Subscribe<OnCharacterStatChanged>(HandleCharacterStatChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.Unsubscribe<OnCharacterStatChanged>(HandleCharacterStatChanged);
    }

    private void InitializeCharacterPanels(List<CharacterSO> charactersSO)
    {
        foreach (CharacterSO data in charactersSO)
        {
            GameObject characterInstance = Instantiate(singleCharacterPanel, transform);
            characterInstance.name = $"Character_{data.characterID}";

            SingleCharacterPanel characterPanel = characterInstance.GetComponent<SingleCharacterPanel>();
            if (characterPanel != null)
            {
                characterPanel.InitializeCharacterData(
                    // data.characterName,
                    data.characterPortrait,
                    data.maxStamina,
                    data.maxHunger
                );
                characterPanels.Add(data.characterID, characterPanel);
            }
        }
    }

    private void HandleCharacterStatChanged(OnCharacterStatChanged eventData)
    {
        if (characterPanels.TryGetValue(eventData.characterID, out SingleCharacterPanel characterPanel))
        {
            switch (eventData.statType)
            {
                case StatType.Stamina:
                    characterPanel.UpdateCharacterStamina(eventData.newValue);
                    break;
                    
                case StatType.Hunger:
                    characterPanel.UpdateCharacterHunger(eventData.newValue);
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Character panel not found for character ID: {eventData.characterID}");
        }
    }
}
