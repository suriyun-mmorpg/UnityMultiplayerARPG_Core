using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSelectionManager))]
public class UICharacterCreate : UIBase
{
    public Transform characterModelContainer;
    [Header("Input")]
    public InputField inputCharacterName;
    [Header("Event")]
    public UnityEvent eventOnCreateCharacter;
    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
                cacheList = GetComponent<UIList>();
            return cacheList;
        }
    }

    private UICharacterSelectionManager selectionManager;
    public UICharacterSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterSelectionManager>();
            return selectionManager;
        }
    }

    protected readonly Dictionary<string, CharacterModel> CharacterModels = new Dictionary<string, CharacterModel>();

    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
        LoadCharacters();
    }

    protected void LoadCharacters()
    {
        SelectionManager.Clear();
        // Show list of characters that can be create
        var selectableCharacters = GameInstance.CharacterPrototypes.Values.ToList();
        CacheList.Generate(selectableCharacters, (index, characterPrototype, ui) =>
        {
            var character = new CharacterData();
            character.Id = characterPrototype.Id;
            character.SetNewCharacterData(characterPrototype.title, characterPrototype.Id);
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.Data = character;
            // Select trigger when add first entry so deactive all models is okay beacause first model will active
            var characterModel = character.InstantiateModel(characterModelContainer);
            CharacterModels[character.Id] = characterModel;
            characterModel.gameObject.SetActive(false);
            SelectionManager.Add(uiCharacter);
        });
    }

    public override void Hide()
    {
        base.Hide();
        characterModelContainer.RemoveChildren();
        inputCharacterName.text = "";
    }

    protected void OnSelectCharacter(UICharacter ui)
    {
        if (ui == null)
            return;

        characterModelContainer.SetChildrenActive(false);
        ShowCharacter(ui.Data.Id);
    }

    protected void ShowCharacter(string id)
    {
        if (string.IsNullOrEmpty(id) || !CharacterModels.ContainsKey(id))
            return;
        CharacterModels[id].gameObject.SetActive(true);
    }

    public virtual void OnClickCreate()
    {
        var gameInstance = GameInstance.Singleton;
        var selectedUI = SelectionManager.SelectedUI;
        if (selectedUI == null)
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
            Debug.LogWarning("Cannot create character, did not selected character class");
            return;
        }
        var prototypeId = selectedUI.Data.PrototypeId;
        var characterName = inputCharacterName.text.Trim();
        var minCharacterNameLength = gameInstance.minCharacterNameLength;
        var maxCharacterNameLength = gameInstance.maxCharacterNameLength;
        if (characterName.Length < minCharacterNameLength)
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Character name is too short");
            Debug.LogWarning("Cannot create character, character name is too short");
            return;
        }
        if (characterName.Length > maxCharacterNameLength)
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Character name is too long");
            Debug.LogWarning("Cannot create character, character name is too long");
            return;
        }

        var characterId = System.Guid.NewGuid().ToString();
        var characterData = new CharacterData();
        characterData.SetNewCharacterData(characterName, prototypeId);
        characterData.Id = characterId;
        characterData.SavePersistentCharacterData();

        if (eventOnCreateCharacter != null)
            eventOnCreateCharacter.Invoke();
    }
}
