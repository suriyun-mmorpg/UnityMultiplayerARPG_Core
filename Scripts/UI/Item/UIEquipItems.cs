using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UICharacterItemSelectionManager))]
public class UIEquipItems : UIBase
{
    public UICharacterItem uiItemDialog;
    public UICharacterItem rightHandSlot;
    public UICharacterItem leftHandSlot;
    public UICharacterItemPair[] otherEquipSlots;

    private Dictionary<string, UICharacterItem> cacheEquipItemSlots = null;
    public Dictionary<string, UICharacterItem> CacheEquipItemSlots
    {
        get
        {
            if (cacheEquipItemSlots == null)
            {
                cacheEquipItemSlots = new Dictionary<string, UICharacterItem>();
                SelectionManager.Clear();
                if (rightHandSlot != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                    rightHandSlot.Setup(GetEmptyUIData(), -1, equipPosition);
                    cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                    SelectionManager.Add(rightHandSlot);
                }
                if (leftHandSlot != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                    leftHandSlot.Setup(GetEmptyUIData(), -1, equipPosition);
                    cacheEquipItemSlots.Add(equipPosition, leftHandSlot);
                    SelectionManager.Add(leftHandSlot);
                }
                foreach (var otherEquipSlot in otherEquipSlots)
                {
                    if (!string.IsNullOrEmpty(otherEquipSlot.armorType.Id) &&
                        otherEquipSlot.ui != null && 
                        !cacheEquipItemSlots.ContainsKey(otherEquipSlot.armorType.Id))
                    {
                        var equipPosition = otherEquipSlot.armorType.Id;
                        otherEquipSlot.ui.Setup(GetEmptyUIData(), -1, equipPosition);
                        cacheEquipItemSlots.Add(equipPosition, otherEquipSlot.ui);
                        SelectionManager.Add(otherEquipSlot.ui);
                    }
                }
            }
            return cacheEquipItemSlots;
        }
    }

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

        if (uiItemDialog != null && ui.Data.Key.IsValid())
        {
            uiItemDialog.Show();
            uiItemDialog.selectionManager = selectionManager;
            uiItemDialog.Setup(ui.Data, ui.indexOfData, ui.equipPosition);
        }
        else if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();
    }

    protected void OnSelectedCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;

        if (uiGameplay != null && !ui.Data.Key.IsValid())
            uiGameplay.DeselectSelectedItem();
    }

    protected void OnDeselectCharacterItem(UICharacterItem ui)
    {
        if (uiItemDialog != null)
            uiItemDialog.Hide();
    }

    public void UpdateData(ICharacterData characterData)
    {
        var slots = CacheEquipItemSlots.Values;
        // Clear slots data
        foreach (var slot in slots)
        {
            slot.Setup(GetEmptyUIData(), -1, string.Empty);
        }

        if (characterData == null)
            return;

        string tempPosition;
        UICharacterItem tempSlot;
        var equipItems = characterData.EquipItems;
        for (var i = 0; i < equipItems.Count; ++i)
        {
            var equipItem = equipItems[i];
            var armorItem = equipItem.GetArmorItem();
            if (armorItem == null)
                continue;

            tempPosition = armorItem.EquipPosition;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
                tempSlot.Setup(new KeyValuePair<CharacterItem, int>(equipItem, equipItem.level), -1, tempPosition);
        }

        var equipWeapons = characterData.EquipWeapons;
        var rightHandEquipment = equipWeapons.rightHand.GetEquipmentItem();
        var leftHandEquipment = equipWeapons.leftHand.GetEquipmentItem();
        tempPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
        if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
        {
            if (rightHandEquipment != null)
                tempSlot.Setup(GetEmptyUIData(), -1, tempPosition);
        }
        tempPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
        {
            if (leftHandEquipment != null)
                tempSlot.Setup(GetEmptyUIData(), -1, tempPosition);
        }
    }

    private KeyValuePair<CharacterItem, int> GetEmptyUIData()
    {
        return new KeyValuePair<CharacterItem, int>(CharacterItem.Empty, 1);
    }
}
