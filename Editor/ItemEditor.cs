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
            // Junk
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Junk.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            // Armor
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.maxSocket));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.requirement));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseStatsRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseAttributesRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseArmors));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.increaseSkillLevels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.equipmentSet));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.armorType));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.armorAmount));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.maxDurability));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Armor.ToString(), cacheItem.GetMemberName(a => a.destroyIfBroken));
            // Weapon
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.maxSocket));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.subEquipmentModels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.requirement));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseStatsRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseAttributesRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseArmors));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.increaseSkillLevels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.equipmentSet));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.weaponType));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.damageAmount));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.harvestDamageAmount));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.moveSpeedRateWhileAttacking));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.ammoCapacity));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.weaponAbility));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.crosshairSetting));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.fireType));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.fireStagger));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.fireSpread));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.maxDurability));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Weapon.ToString(), cacheItem.GetMemberName(a => a.destroyIfBroken));
            // Shield
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.maxSocket));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.equipmentModels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.requirement));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseStats));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseStatsRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseAttributes));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseAttributesRate));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseResistances));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseArmors));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseDamages));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.increaseSkillLevels));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.equipmentSet));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Shield.ToString(), cacheItem.GetMemberName(a => a.armorAmount));
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
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Building.ToString(), cacheItem.GetMemberName(a => a.buildingEntity));
            // Pet
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Pet.ToString(), cacheItem.GetMemberName(a => a.petEntity));
            // Socket Enhancer
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.SocketEnhancer.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.SocketEnhancer.ToString(), cacheItem.GetMemberName(a => a.socketEnhanceEffect));
            // Mount
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Mount.ToString(), cacheItem.GetMemberName(a => a.mountEntity));
            // Attribute Increase
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.AttributeIncrease.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.AttributeIncrease.ToString(), cacheItem.GetMemberName(a => a.attributeAmount));
            // Attribute Reset
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.AttributeReset.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            // Skill Use
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Skill.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.Skill.ToString(), cacheItem.GetMemberName(a => a.skillLevel));
            // Skill Learn
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.SkillLearn.ToString(), cacheItem.GetMemberName(a => a.maxStack));
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.SkillLearn.ToString(), cacheItem.GetMemberName(a => a.skillLevel));
            // Skill Reset
            ShowOnEnum(cacheItem.GetMemberName(a => a.itemType), ItemType.SkillReset.ToString(), cacheItem.GetMemberName(a => a.maxStack));
        }
    }
}
