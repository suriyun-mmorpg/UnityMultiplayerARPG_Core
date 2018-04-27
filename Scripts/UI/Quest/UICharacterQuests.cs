using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UICharacterQuestSelectionManager))]
public class UICharacterQuests : UIBase
{
    public ICharacterData character { get; protected set; }
    public UICharacterQuest uiQuestDialog;
    public UICharacterQuest uiCharacterQuestPrefab;
    public Transform uiCharacterQuestContainer;

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiCharacterQuestPrefab.gameObject;
                cacheList.uiContainer = uiCharacterQuestContainer;
            }
            return cacheList;
        }
    }

    private UICharacterQuestSelectionManager selectionManager;
    public UICharacterQuestSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterQuestSelectionManager>();
            selectionManager.selectionMode = UISelectionMode.SelectSingle;
            return selectionManager;
        }
    }

    public override void Show()
    {
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterQuest);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterQuest);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterQuest);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterQuest);
        base.Show();
    }

    public override void Hide()
    {
        SelectionManager.DeselectSelectedUI();
        base.Hide();
    }

    protected void OnSelectCharacterQuest(UICharacterQuest ui)
    {
        if (uiQuestDialog != null)
        {
            uiQuestDialog.selectionManager = SelectionManager;
            uiQuestDialog.Setup(ui.Data, character, ui.indexOfData);
            uiQuestDialog.Show();
        }
    }

    protected void OnDeselectCharacterQuest(UICharacterQuest ui)
    {
        if (uiQuestDialog != null)
            uiQuestDialog.Hide();
    }

    public void UpdateData(IPlayerCharacterData character)
    {
        this.character = character;
        var selectedQuestId = SelectionManager.SelectedUI != null ? SelectionManager.SelectedUI.Data.questId : "";
        SelectionManager.DeselectSelectedUI();
        SelectionManager.Clear();

        if (character == null)
        {
            CacheList.HideAll();
            return;
        }

        var characterQuests = character.Quests;
        CacheList.Generate(characterQuests, (index, characterQuest, ui) =>
        {
            var uiCharacterQuest = ui.GetComponent<UICharacterQuest>();
            uiCharacterQuest.Setup(characterQuest, character, index);
            uiCharacterQuest.Show();
            SelectionManager.Add(uiCharacterQuest);
            if (selectedQuestId.Equals(characterQuest.questId))
                uiCharacterQuest.OnClickSelect();
        });
    }
}
