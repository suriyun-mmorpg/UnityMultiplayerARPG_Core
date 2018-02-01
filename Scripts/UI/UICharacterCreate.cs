using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIList))]
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

    protected UICharacter selectedUI;

    public override void Show()
    {
        base.Show();
        LoadCharacters();
    }

    protected void LoadCharacters()
    {
        selectedUI = null;
        // Show list of characters that can be create
        var creatableClasses = GameInstance.CharacterClasses.Values.Where(a => a.canCreateByPlayer).ToList();
        UICharacter firstUI = null;
        TempList.Generate(creatableClasses, (creatableClass, ui) =>
        {
            var characterData = new CharacterData();
            characterData.SetNewCharacterData("", creatableClass.Id);
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.characterData = characterData;
            uiCharacter.eventOnSelect.RemoveAllListeners();
            uiCharacter.eventOnSelect.AddListener(OnSelectCharacter);
            if (firstUI == null)
                firstUI = uiCharacter;
            // TODO: Instantiate character model to show in screen
        });
        characterModelContainer.SetChildrenActive(false);
        // Show first character
        OnSelectCharacter(firstUI);
    }

    public override void Hide()
    {
        base.Hide();
        characterModelContainer.RemoveChildren();
    }

    protected void OnSelectCharacter(UICharacter ui)
    {
        if (ui == null)
            return;
        selectedUI = ui;
        characterModelContainer.SetChildrenActive(false);
        ShowCharacter(ui.characterData.Id);
    }

    protected void ShowCharacter(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;
        // TODO: Show select character model
    }

    public virtual void OnClickCreate()
    {
        if (selectedUI == null)
        {
            // TODO: Error dialog
            Debug.LogWarning("Cannot create character, did not selected character class");
            return;
        }
        var classId = selectedUI.characterData.ClassId;
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
        characterData.SetNewCharacterData(characterName, classId);
        characterData.Id = characterId;
        characterData.SavePersistentCharacterData();

        if (eventOnCreateCharacter != null)
            eventOnCreateCharacter.Invoke();
    }
}
