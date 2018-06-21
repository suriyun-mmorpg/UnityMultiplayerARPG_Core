using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Item))]
[CanEditMultipleObjects]
public class ItemEditor : BaseCustomEditor
{
    private static Item cacheItem;
    protected override void SetFieldCondition()
    {
        if (cacheItem == null)
            cacheItem = CreateInstance<Item>();
        // Junk
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Junk.ToString(), cacheItem.GetMemberName(a => a.maxStack));
        // Armor
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.maxLevel));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.armorType));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.maxDurability));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.destroyIfBroken));
        // Weapon
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.maxLevel));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.subEquipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.weaponType));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.damageAmount));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.maxDurability));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.destroyIfBroken));
        // Shield
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.maxLevel));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.requirement));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.maxDurability));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.destroyIfBroken));
        // Potion
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Potion.ToString(), cacheItem.GetMemberName(a => a.maxStack));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Potion.ToString(), cacheItem.GetMemberName(a => a.buff));
        // Ammo
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Ammo.ToString(), cacheItem.GetMemberName(a => a.maxStack));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Ammo.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Ammo.ToString(), cacheItem.GetMemberName(a => a.ammoType));
        // Building
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Building.ToString(), cacheItem.GetMemberName(a => a.maxStack));
        ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Building.ToString(), cacheItem.GetMemberName(a => a.buildingObject));
    }
}
