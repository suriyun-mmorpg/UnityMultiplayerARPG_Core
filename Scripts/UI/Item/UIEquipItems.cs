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
    public UICharacterItemPair rightHandSlot;
    public UICharacterItemPair leftHandSlot;
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
                if (rightHandSlot.ui != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                    rightHandSlot.ui.equipPosition = equipPosition;
                    cacheEquipItemSlots.Add(equipPosition, rightHandSlot.ui);
                    SelectionManager.Add(rightHandSlot.ui);
                }
                if (leftHandSlot.ui != null)
                {
                    var equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                    leftHandSlot.ui.equipPosition = equipPosition;
                    cacheEquipItemSlots.Add(equipPosition, leftHandSlot.ui);
                    SelectionManager.Add(leftHandSlot.ui);
                }
                foreach (var otherEquipSlot in otherEquipSlots)
                {
                    if (!string.IsNullOrEmpty(otherEquipSlot.equipPosition) &&
                        otherEquipSlot.ui != null && 
                        !cacheEquipItemSlots.ContainsKey(otherEquipSlot.equipPosition))
                    {
                        var equipPosition = otherEquipSlot.equipPosition;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        rightHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
        leftHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        EditorUtility.SetDirty(this);
    }
#endif

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

        var equipItems = characterEntity.equipItems;
        for (var i = 0; i < equipItems.Count; ++i)
        {
            var equipItem = equipItems[i];
            var weaponItem = equipItem.GetWeaponItem();
            var shieldItem = equipItem.GetShieldItem();
            var equipmentItem = equipItem.GetEquipmentItem();
            if (equipmentItem == null)
                continue;

            var position = equipmentItem.equipPosition;
            if (weaponItem != null || shieldItem != null)
                position = equipItem.isSubWeapon ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND;

            if (CacheEquipItemSlots.ContainsKey(position))
            {
                var slot = CacheEquipItemSlots[position];
                slot.Data = equipItem;
                slot.indexOfData = i;
            }
        }
    }
}
