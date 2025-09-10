using System.Collections.Generic;

public class CharacterManager : Singleton<CharacterManager>
{
    public List<CharacterSO> charactersSO;
    private Dictionary<string, CharacterSO> characterSODict = new Dictionary<string, CharacterSO>();

    protected override void Awake()
    {
        base.Awake();

        foreach (var characterSO in charactersSO)
        {
            if (characterSO != null && !string.IsNullOrEmpty(characterSO.characterID))
            {
                 characterSODict[characterSO.characterID] = characterSO;
            }
        }
    }

    private void Start()
    {
        InitializeCharacters();
    }

    private void InitializeCharacters()
    {
        if (GameSaveManager.Instance.GetCharacterData(charactersSO[0].characterID) == null)
        {
            foreach (var characterSO in charactersSO)
            {
                var newCharacterData = new CharacterRuntimeData
                {
                    characterID = characterSO.characterID,
                    maxStamina = characterSO.maxStamina,
                    maxHunger = characterSO.maxHunger,
                    currentStamina = characterSO.maxStamina,
                    currentHunger = characterSO.maxHunger
                };

                GameSaveManager.Instance.RegisterNewCharacter(newCharacterData);
            }
            
            GameSaveManager.Instance.PublishCharacterDataForSync();
        }
    }

    public CharacterSO GetCharacterSO(string characterID)
    {
        characterSODict.TryGetValue(characterID, out CharacterSO characterSO);
        return characterSO;
    }
}
