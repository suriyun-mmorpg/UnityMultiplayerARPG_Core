using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIList))]
public class UICharacterSelection : UIBase
{
    public Transform characterModelContainer;
    [Header("UI Elements")]
    public Button buttonStart;
    public Button buttonDelete;
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
        buttonStart.onClick.RemoveListener(OnClickStart);
        buttonStart.onClick.AddListener(OnClickStart);
        buttonDelete.onClick.RemoveListener(OnClickDelete);
        buttonDelete.onClick.AddListener(OnClickDelete);
        LoadCharacters();
    }

    protected void LoadCharacters()
    {
        selectedUI = null;
        // Unenabled buttons
        buttonStart.gameObject.SetActive(false);
        buttonDelete.gameObject.SetActive(false);
        // Remove all models
        characterModelContainer.RemoveChildren();
        // Show list of created characters
        var selectableCharacters = CharacterDataExtension.LoadAllPersistentCharacterData();
        UICharacter firstUI = null;
        TempList.Generate(selectableCharacters, (character, ui) =>
        {
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.characterData = character;
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
        buttonStart.gameObject.SetActive(true);
        buttonDelete.gameObject.SetActive(true);
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

    public virtual void OnClickStart()
    {
        if (selectedUI == null)
            return;
    }

    public virtual void OnClickDelete()
    {
        if (selectedUI == null)
            return;

        selectedUI.characterData.DeletePersistentCharacterData();
        // Reload characters
        LoadCharacters();
    }
}
