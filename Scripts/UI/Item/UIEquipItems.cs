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
                    rightHandSlot.equipPosition = equipPosition;
                    cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                    SelectionManager.Add(rightHandSlot);
                }
                if (leftHandSlot != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                    leftHandSlot.equipPosition = equipPosition;
                    cacheEquipItemSlots.Add(equipPosition, leftHandSlot);
                    SelectionManager.Add(leftHandSlot);
                }
                foreach (var otherEquipSlot in otherEquipSlots)
                {
                    if (!string.IsNullOrEmpty(otherEquipSlot.armorType.equipPosition) &&
                        otherEquipSlot.ui != null && 
                        !cacheEquipItemSlots.ContainsKey(otherEquipSlot.armorType.equipPosition))
                    {
                        var equipPosition = otherEquipSlot.armorType.equipPosition;
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
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
    }

    public override void Hide()
    {
        base.Hide();
        SelectionManager.DeselectSelectedUI();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null && uiGameplay.uiEquipItems != null)
            uiGameplay.uiEquipItems.SelectionManager.DeselectSelectedUI();

        if (uiItemDialog != null)
        {
            uiItemDialog.Data = ui.Data;
            uiItemDialog.Show();
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
                tempSlot.indexOfData = i;
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
            }
        }
        tempPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
        {
            if (leftHandEquipment != null)
            {
                tempSlot.Data = equipWeapons.leftHand;
                tempSlot.indexOfData = -1;
            }
        }
    }
}
