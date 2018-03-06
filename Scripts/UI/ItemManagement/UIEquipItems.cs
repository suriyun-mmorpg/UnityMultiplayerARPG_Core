using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UICharacterItemSelectionManager))]
public class UIEquipItems : UIBase
{
    public System.Action<UICharacterItem> onSelectCharacterItem;
    public UIEquipItemSlot rightHandSlot;
    public UIEquipItemSlot leftHandSlot;
    public UIEquipItemSlot[] otherEquipSlots;

    private Dictionary<string, UIEquipItemSlot> tempEquipItemSlots = null;
    public Dictionary<string, UIEquipItemSlot> TempEquipItemSlots
    {
        get
        {
            if (tempEquipItemSlots == null)
            {
                tempEquipItemSlots = new Dictionary<string, UIEquipItemSlot>();
                SelectionManager.Clear();
                if (rightHandSlot != null)
                {
                    rightHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                    tempEquipItemSlots.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandSlot);
                    SelectionManager.Add(rightHandSlot);
                }
                if (leftHandSlot != null)
                {
                    leftHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                    tempEquipItemSlots.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandSlot);
                    SelectionManager.Add(leftHandSlot);
                }
                foreach (var otherEquipSlot in otherEquipSlots)
                {
                    if (otherEquipSlot != null && !tempEquipItemSlots.ContainsKey(otherEquipSlot.equipPosition))
                    {
                        tempEquipItemSlots.Add(otherEquipSlot.equipPosition, otherEquipSlot);
                        SelectionManager.Add(otherEquipSlot);
                    }
                }
            }
            return tempEquipItemSlots;
        }
    }

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

    private UICharacterItemSelectionManager selectionManager;
    public UICharacterItemSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterItemSelectionManager>();
            return selectionManager;
        }
    }
    
    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        if (ui == null)
            return;

        if (onSelectCharacterItem != null)
            onSelectCharacterItem(ui);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rightHandSlot != null)
        {
            rightHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
            EditorUtility.SetDirty(rightHandSlot);
        }
        if (leftHandSlot != null)
        {
            leftHandSlot.equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
            EditorUtility.SetDirty(leftHandSlot);
        }
    }
#endif

    public void SetItems(IList<CharacterItem> equipItems)
    {
        var slots = TempEquipItemSlots.Values;
        // Clear slots data
        foreach (var slot in slots)
        {
            slot.data.Empty();
        }

        foreach (var equipItem in equipItems)
        {
            var weaponItem = equipItem.GetWeaponItem();
            var shieldItem = equipItem.GetShieldItem();
            var equipmentItem = equipItem.GetEquipmentItem();
            if (equipmentItem == null)
                continue;

            var position = equipmentItem.equipPosition;
            if (weaponItem != null || shieldItem != null)
                position = equipItem.isSubWeapon ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND;

            if (TempEquipItemSlots.ContainsKey(position))
                TempEquipItemSlots[position].data = equipItem;
        }
    }
}
