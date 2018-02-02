using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSelectionManager))]
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

    public override void Show()
    {
        base.Show();
        buttonStart.onClick.RemoveListener(OnClickStart);
        buttonStart.onClick.AddListener(OnClickStart);
        buttonDelete.onClick.RemoveListener(OnClickDelete);
        buttonDelete.onClick.AddListener(OnClickDelete);
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
        LoadCharacters();
    }

    protected void LoadCharacters()
    {
        SelectionManager.Clear();
        // Unenabled buttons
        buttonStart.gameObject.SetActive(false);
        buttonDelete.gameObject.SetActive(false);
        // Remove all models
        characterModelContainer.RemoveChildren();
        // Show list of created characters
        var selectableCharacters = CharacterDataExtension.LoadAllPersistentCharacterData();
        TempList.Generate(selectableCharacters, (character, ui) =>
        {
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.data = character;
            SelectionManager.Add(uiCharacter);
            // TODO: Instantiate character model to show in screen
        });
        characterModelContainer.SetChildrenActive(false);
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
        characterModelContainer.SetChildrenActive(false);
        ShowCharacter(ui.data.Id);
    }

    protected void ShowCharacter(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;
        // TODO: Show select character model
    }

    public virtual void OnClickStart()
    {
        if (SelectionManager.SelectedUI == null)
            return;
    }

    public virtual void OnClickDelete()
    {
        if (SelectionManager.SelectedUI == null)
            return;

        SelectionManager.SelectedUI.data.DeletePersistentCharacterData();
        // Reload characters
        LoadCharacters();
    }
}
