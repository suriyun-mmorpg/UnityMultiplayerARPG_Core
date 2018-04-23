using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Item))]
public class ItemEditor : BaseCustomEditor
{
    private static Item cacheItem;
    protected override void SetFieldCondition()
    {
        if (cacheItem == null)
            cacheItem = CreateInstance<Item>();
        // Armor
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.armorType));
        // Weapon
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.subEquipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.weaponType));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.damageAmount));
        // Shield
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        // Potion
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Potion.ToString(), cacheItem.GetMemberName(a => a.buff));
    }
}
