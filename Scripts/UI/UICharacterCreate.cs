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
    private UIList tempList;
    public UIList TempList
    {
        get
        {
            if (tempList == null)
                tempList = GetComponent<UIList>();
            return tempList;
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
        var selectableCharacters = GameInstance.CharacterPrototypes.Values.Where(a => a.canCreateByPlayer).ToList();
        TempList.Generate(selectableCharacters, (characterPrototype, ui) =>
        {
            var character = new CharacterData();
            character.Id = characterPrototype.Id;
            character.SetNewCharacterData(characterPrototype.title, characterPrototype.Id);
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.data = character;
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
        ShowCharacter(ui.data.Id);
    }

    protected void ShowCharacter(string id)
    {
        if (string.IsNullOrEmpty(id) || !CharacterModels.ContainsKey(id))
            return;
        CharacterModels[id].gameObject.SetActive(true);
    }

    public virtual void OnClickCreate()
    {
        var selectedUI = SelectionManager.SelectedUI;
        if (selectedUI == null)
        {
            // TODO: Error dialog
            Debug.LogWarning("Cannot create character, did not selected character class");
            return;
        }
        var prototypeId = selectedUI.data.PrototypeId;
        // TODO: May validate name
        var characterName = inputCharacterName.text;
        if (string.IsNullOrEmpty(characterName.Trim()))
        {
            // TODO: Error dialog
            Debug.LogWarning("Cannot create character, character name is empty");
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
