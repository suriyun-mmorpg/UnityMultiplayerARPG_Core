using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(Item))]
    [CanEditMultipleObjects]
    public class ItemEditor : BaseCustomEditor
    {
        private static Item cacheItem;
        protected override void SetFieldCondition()
        {
            if (cacheItem == null)
                cacheItem = CreateInstance<Item>();

            // Armor
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.maxSocket));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.equipmentModels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.requirement));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseStats));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseStatsRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseAttributes));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseAttributesRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseResistances));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseArmors));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseDamages));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.increaseSkillLevels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.equipmentSet));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.armorType));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.armorAmount));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.maxDurability));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Armor), nameof(cacheItem.destroyIfBroken));
            // Weapon
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.maxSocket));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.equipmentModels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.subEquipmentModels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.requirement));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.launchClip));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.reloadClip));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.emptyClip));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseStats));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseStatsRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseAttributes));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseAttributesRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseResistances));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseArmors));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseDamages));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.increaseSkillLevels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.equipmentSet));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.weaponType));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.damageAmount));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.harvestDamageAmount));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.moveSpeedRateWhileAttacking));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.moveSpeedRateWhileCharging));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.moveSpeedRateWhileReloading));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.ammoCapacity));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.weaponAbility));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.crosshairSetting));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.fireType));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.fireStagger));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.fireSpread));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.maxDurability));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Weapon), nameof(cacheItem.destroyIfBroken));
            // Shield
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.maxSocket));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.equipmentModels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.requirement));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseStats));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseStatsRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseAttributes));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseAttributesRate));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseResistances));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseArmors));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseDamages));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.increaseSkillLevels));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.equipmentSet));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.armorAmount));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.maxDurability));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Shield), nameof(cacheItem.destroyIfBroken));
            // Potion
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Potion), nameof(cacheItem.buff));
            // Ammo
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Ammo), nameof(cacheItem.increaseDamages));
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Ammo), nameof(cacheItem.ammoType));
            // Building
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Building), nameof(cacheItem.buildingEntity));
            // Pet
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Pet), nameof(cacheItem.petEntity));
            // Socket Enhancer
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.SocketEnhancer), nameof(cacheItem.socketEnhanceEffect));
            // Mount
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Mount), nameof(cacheItem.mountEntity));
            // Attribute Increase
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.AttributeIncrease), nameof(cacheItem.attributeAmount));
            // Skill Use
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.Skill), nameof(cacheItem.skillLevel));
            // Skill Learn
            ShowOnEnum(nameof(cacheItem.itemType), nameof(Item.LegacyItemType.SkillLearn), nameof(cacheItem.skillLevel));
        }
    }
}
