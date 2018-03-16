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
                    rightHandSlot.indexOfData = -1;
                    rightHandSlot.equipPosition = equipPosition;
                    cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                    SelectionManager.Add(rightHandSlot);
                }
                if (leftHandSlot != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                    leftHandSlot.indexOfData = -1;
                    leftHandSlot.equipPosition = equipPosition;
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
                        otherEquipSlot.ui.indexOfData = -1;
                        otherEquipSlot.ui.equipPosition = equipPosition;
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
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
        base.Show();
    }

    public override void Hide()
    {
        SelectionManager.DeselectSelectedUI();
        base.Hide();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null && uiGameplay.uiNonEquipItems != null)
            uiGameplay.uiNonEquipItems.SelectionManager.DeselectSelectedUI();

        if (uiItemDialog != null)
        {
            uiItemDialog.Show();
            uiItemDialog.Data = ui.Data;
            uiItemDialog.indexOfData = ui.indexOfData;
            uiItemDialog.equipPosition = ui.equipPosition;
            uiItemDialog.selectionManager = selectionManager;
        }
    }

    protected void OnDeselectCharacterItem(UICharacterItem ui)
    {
        if (uiItemDialog != null)
            uiItemDialog.Hide();
    }

    public void UpdateData(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return;

        var slots = CacheEquipItemSlots.Values;
        // Clear slots data
        foreach (var slot in slots)
        {
            slot.Data = CharacterItem.Empty;
            slot.indexOfData = -1;
            slot.equipPosition = string.Empty;
        }

        string tempPosition;
        UICharacterItem tempSlot;
        var equipItems = characterEntity.equipItems;
        for (var i = 0; i < equipItems.Count; ++i)
        {
            var equipItem = equipItems[i];
            var armorItem = equipItem.GetArmorItem();
            if (armorItem == null)
                continue;

            tempPosition = armorItem.EquipPosition;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                tempSlot.Data = equipItem;
                tempSlot.indexOfData = -1;
                tempSlot.equipPosition = tempPosition;
            }
        }

        var equipWeapons = characterEntity.EquipWeapons;
        var rightHandEquipment = equipWeapons.rightHand.GetEquipmentItem();
        var leftHandEquipment = equipWeapons.leftHand.GetEquipmentItem();
        tempPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
        if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
        {
            if (rightHandEquipment != null)
            {
                tempSlot.Data = equipWeapons.rightHand;
                tempSlot.indexOfData = -1;
                tempSlot.equipPosition = tempPosition;
            }
        }
        tempPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
        {
            if (leftHandEquipment != null)
            {
                tempSlot.Data = equipWeapons.leftHand;
                tempSlot.indexOfData = -1;
                tempSlot.equipPosition = tempPosition;
            }
        }
    }
}
