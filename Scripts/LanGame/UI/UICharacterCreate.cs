using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UICharacterSelectionManager))]
public class UICharacterCreate : UIBase
{
    public UICharacter uiCharacterPrefab;
    public Transform uiCharacterContainer;
    public Transform characterModelContainer;
    [Header("UI Elements")]
    public InputField inputCharacterName;
    public Button buttonCreate;
    [Header("Event")]
    public UnityEvent eventOnCreateCharacter;

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiCharacterPrefab.gameObject;
                cacheList.uiContainer = uiCharacterContainer;
            }
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
            selectionManager.selectionMode = UISelectionMode.Toggle;
            return selectionManager;
        }
    }

    private readonly Dictionary<int, CharacterModel> CharacterModels = new Dictionary<int, CharacterModel>();
    
    public override void Show()
    {
        buttonCreate.onClick.RemoveListener(OnClickCreate);
        buttonCreate.onClick.AddListener(OnClickCreate);
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
        LoadCharacters();
        base.Show();
    }

    private void LoadCharacters()
    {
        SelectionManager.Clear();
        // Show list of characters that can be create
        var selectableCharacters = GameInstance.PlayerCharacters.Values.ToList();
        CacheList.Generate(selectableCharacters, (index, character, ui) =>
        {
            var dataId = character.HashId;
            var characterData = new PlayerCharacterData();
            characterData.DataId = dataId;
            characterData.SetNewCharacterData(character.title, character.HashId);
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.Setup(characterData, dataId);
            // Select trigger when add first entry so deactivate all models is okay beacause first model will active
            var characterModel = characterData.InstantiateModel(characterModelContainer);
            CharacterModels[characterData.dataId] = characterModel;
            characterModel.gameObject.SetActive(false);
            SelectionManager.Add(uiCharacter);
        });
    }

    public override void Hide()
    {
        characterModelContainer.RemoveChildren();
        inputCharacterName.text = "";
        base.Hide();
    }

    private void OnSelectCharacter(UICharacter ui)
    {
        characterModelContainer.SetChildrenActive(false);
        ShowCharacter(ui.dataId);
    }

    private void ShowCharacter(int id)
    {
        CharacterModel characterModel;
        if (!CharacterModels.TryGetValue(id, out characterModel))
            return;
        characterModel.gameObject.SetActive(true);
    }

    private void OnClickCreate()
    {
        var gameInstance = GameInstance.Singleton;
        var selectedUI = SelectionManager.SelectedUI;
        if (selectedUI == null)
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
            Debug.LogWarning("Cannot create character, did not selected character class");
            return;
        }
        var dataId = selectedUI.dataId;
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

        var characterId = GenericUtils.GetUniqueId();
        var characterData = new PlayerCharacterData();
        characterData.Id = characterId;
        characterData.SetNewCharacterData(characterName, dataId);
        characterData.SavePersistentCharacterData();

        if (eventOnCreateCharacter != null)
            eventOnCreateCharacter.Invoke();
    }
}
