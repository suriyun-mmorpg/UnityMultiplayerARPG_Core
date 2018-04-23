using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq.Expressions;
using System;

[CustomEditor(typeof(Item))]
public class ItemEditor : BaseCustomEditor
{
    private static Item cacheItem;
    protected override void SetFieldCondition()
    {
        if (cacheItem == null)
            cacheItem = CreateInstance<Item>();
        // Armor
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.equipmentModels));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.requirement));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.increaseAttributes));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.increaseResistances));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.increaseDamages));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.increaseStats));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Armor.ToString(), GetMemberName(() => cacheItem.armorType));
        // Weapon
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.equipmentModels));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.subEquipmentModels));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.requirement));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.increaseAttributes));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.increaseResistances));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.increaseDamages));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.increaseStats));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.weaponType));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Weapon.ToString(), GetMemberName(() => cacheItem.damageAmount));
        // Shield
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.equipmentModels));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.requirement));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.increaseAttributes));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.increaseResistances));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.increaseDamages));
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Shield.ToString(), GetMemberName(() => cacheItem.increaseStats));
        // Potion
        ShowOnEnum(GetMemberName(() => cacheItem.itemType), ItemType.Potion.ToString(), GetMemberName(() => cacheItem.buff));
    }

    public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
    {
        MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
        return expressionBody.Member.Name;
    }
}
