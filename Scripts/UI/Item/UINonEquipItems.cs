using UnityEngine;

[RequireComponent(typeof(UICharacterItemSelectionManager))]
public class UINonEquipItems : UIBase
{
    public ICharacterData character { get; protected set; }
    public UICharacterItem uiItemDialog;
    public UICharacterItem uiCharacterItemPrefab;
    public Transform uiCharacterItemContainer;

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiCharacterItemPrefab.gameObject;
                cacheList.uiContainer = uiCharacterItemContainer;
            }
            return cacheList;
        }
    }

    private UICharacterItemSelectionManager selectionManager;
    public UICharacterItemSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterItemSelectionManager>();
            selectionManager.selectionMode = UISelectionMode.SelectSingle;
            return selectionManager;
        }
    }
    
    public override void Show()
    {
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelected.RemoveListener(OnSelectedCharacterItem);
        SelectionManager.eventOnSelected.AddListener(OnSelectedCharacterItem);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
        base.Show();
    }

    public override void Hide()
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();
        base.Hide();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;

        if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();

        if (uiItemDialog != null && ui.Data.characterItem.IsValid())
        {
            uiItemDialog.selectionManager = SelectionManager;
            uiItemDialog.Setup(ui.Data, character, ui.indexOfData, ui.equipPosition);
            uiItemDialog.Show();
        }
        else if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();
    }

    protected void OnSelectedCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;

        if (uiGameplay != null && !ui.Data.characterItem.IsValid())
            uiGameplay.DeselectSelectedItem();
    }

    protected void OnDeselectCharacterItem(UICharacterItem ui)
    {
        if (uiItemDialog != null)
            uiItemDialog.Hide();
    }

    public void UpdateData(ICharacterData character)
    {
        this.character = character;
        var selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
        SelectionManager.DeselectSelectedUI();
        SelectionManager.Clear();

        if (character == null)
        {
            CacheList.HideAll();
            return;
        }

        var nonEquipItems = character.NonEquipItems;
        CacheList.Generate(nonEquipItems, (index, characterItem, ui) =>
        {
            var uiCharacterItem = ui.GetComponent<UICharacterItem>();
            uiCharacterItem.Setup(new CharacterItemLevelTuple(characterItem, characterItem.level), this.character, index, string.Empty);
            uiCharacterItem.Show();
            SelectionManager.Add(uiCharacterItem);
            if (selectedIdx == index)
                uiCharacterItem.OnClickSelect();
        });
    }
}
