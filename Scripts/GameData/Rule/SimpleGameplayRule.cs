using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "SimpleGameplayRule", menuName = "Create GameplayRule/SimpleGameplayRule")]
    public class SimpleGameplayRule : BaseGameplayRule
    {
        public short increaseStatPointEachLevel = 5;
        public short increaseSkillPointEachLevel = 1;
        public int hungryWhenFoodLowerThan = 40;
        public int thirstyWhenWaterLowerThan = 40;
        public float staminaRecoveryPerSeconds = 5;
        public float staminaDecreasePerSeconds = 5;
        public float foodDecreasePerSeconds = 4;
        public float waterDecreasePerSeconds = 2;
        public float moveSpeedRateWhileSprint = 1.5f;
        public float normalDecreaseWeaponDurability = 0.5f;
        public float normalDecreaseShieldDurability = 0.5f;
        public float normalDecreaseArmorDurability = 0.1f;
        public float blockedDecreaseWeaponDurability = 0.5f;
        public float blockedDecreaseShieldDurability = 0.75f;
        public float blockedDecreaseArmorDurability = 0.15f;
        public float criticalDecreaseWeaponDurability = 0.75f;
        public float criticalDecreaseShieldDurability = 0.5f;
        public float criticalDecreaseArmorDurability = 0.15f;
        public float missDecreaseWeaponDurability = 0f;
        public float missDecreaseShieldDurability = 0;
        public float missDecreaseArmorDurability = 0f;
        [Range(0f, 1f)]
        public float hpRecoveryRatePerSeconds = 0.05f;
        [Range(0f, 1f)]
        public float mpRecoveryRatePerSeconds = 0.05f;
        [Range(0f, 1f)]
        public float hpDecreaseRatePerSecondsWhenHungry = 0.05f;
        [Range(0f, 1f)]
        public float mpDecreaseRatePerSecondsWhenHungry = 0.05f;
        [Range(0f, 1f)]
        public float hpDecreaseRatePerSecondsWhenThirsty = 0.05f;
        [Range(0f, 1f)]
        public float mpDecreaseRatePerSecondsWhenThirsty = 0.05f;

        public override float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
        {
            // Attacker stats
            var attackerStats = attacker.CacheStats;
            // Damage receiver stats
            var dmgReceiverStats = damageReceiver.CacheStats;
            // Calculate chance to hit
            var attackerAcc = attackerStats.accuracy;
            var dmgReceiverEva = dmgReceiverStats.evasion;
            var attackerLvl = attacker.Level;
            var dmgReceiverLvl = damageReceiver.Level;
            var hitChance = 2f;

            if (attackerAcc != 0 && dmgReceiverEva != 0)
                hitChance *= (attackerAcc / (attackerAcc + dmgReceiverEva));

            if (attackerLvl != 0 && dmgReceiverLvl != 0)
                hitChance *= ((float)attackerLvl / (float)(attackerLvl + dmgReceiverLvl));

            // Minimum hit chance is 5%
            if (hitChance < 0.05f)
                hitChance = 0.05f;
            // Maximum hit chance is 95%
            if (hitChance > 0.95f)
                hitChance = 0.95f;
            return hitChance;
        }

        public override float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
        {
            var criRate = damageReceiver.CacheStats.criRate;
            // Minimum critical chance is 5%
            if (criRate < 0.05f)
                criRate = 0.05f;
            // Maximum critical chance is 95%
            if (criRate > 0.95f)
                criRate = 0.95f;
            return criRate;
        }

        public override float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage)
        {
            return damage * attacker.CacheStats.criDmgRate;
        }

        public override float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
        {
            var blockRate = damageReceiver.CacheStats.blockRate;
            // Minimum block chance is 5%
            if (blockRate < 0.05f)
                blockRate = 0.05f;
            // Maximum block chance is 95%
            if (blockRate > 0.95f)
                blockRate = 0.95f;
            return blockRate;
        }

        public override float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage)
        {
            var blockDmgRate = damageReceiver.CacheStats.blockDmgRate;
            // Minimum block damage is 5%
            if (blockDmgRate < 0.05f)
                blockDmgRate = 0.05f;
            // Maximum block damage is 95%
            if (blockDmgRate > 0.95f)
                blockDmgRate = 0.95f;
            return damage - (damage * blockDmgRate);
        }

        public override float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount, DamageElement damageElement)
        {
            if (damageElement == null)
                return damageAmount -= damageReceiver.CacheStats.armor; // If armor is minus damage will be increased
            var resistances = damageReceiver.CacheResistances;
            float resistanceAmount = 0f;
            resistances.TryGetValue(damageElement, out resistanceAmount);
            if (resistanceAmount > damageElement.maxResistanceAmount)
                resistanceAmount = damageElement.maxResistanceAmount;
            return damageAmount -= damageAmount * resistanceAmount; // If resistance is minus damage will be increased
        }

        public override float GetRecoveryHpPerSeconds(BaseCharacterEntity character)
        {
            if (IsHungry(character))
                return 0;
            return character.CacheMaxHp * hpRecoveryRatePerSeconds;
        }

        public override float GetRecoveryMpPerSeconds(BaseCharacterEntity character)
        {
            if (IsThirsty(character))
                return 0;
            return character.CacheMaxMp * mpRecoveryRatePerSeconds;
        }

        public override float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character)
        {
            return staminaRecoveryPerSeconds;
        }

        public override float GetDecreasingHpPerSeconds(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
                return 0f;
            var result = 0f;
            if (IsHungry(character))
                result += character.CacheMaxHp * hpDecreaseRatePerSecondsWhenHungry;
            if (IsThirsty(character))
                result += character.CacheMaxHp * hpDecreaseRatePerSecondsWhenThirsty;
            return result;
        }

        public override float GetDecreasingMpPerSeconds(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
                return 0f;
            var result = 0f;
            if (IsHungry(character))
                result += character.CacheMaxMp * mpDecreaseRatePerSecondsWhenHungry;
            if (IsThirsty(character))
                result += character.CacheMaxMp * mpDecreaseRatePerSecondsWhenThirsty;
            return result;
        }

        public override float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
                return 0f;
            return staminaDecreasePerSeconds;
        }

        public override float GetDecreasingFoodPerSeconds(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
                return 0f;
            return foodDecreasePerSeconds;
        }

        public override float GetDecreasingWaterPerSeconds(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
                return 0f;
            return waterDecreasePerSeconds;
        }

        public override float GetMoveSpeed(BaseCharacterEntity character)
        {
            if (character is MonsterCharacterEntity)
            {
                var monsterCharacter = character as MonsterCharacterEntity;
                return monsterCharacter.isWandering ? monsterCharacter.MonsterDatabase.wanderMoveSpeed : monsterCharacter.CacheMoveSpeed;
            }
            return character.CacheMoveSpeed * (character.isSprinting ? moveSpeedRateWhileSprint : 1f);
        }

        public override bool IsHungry(BaseCharacterEntity character)
        {
            return character.CurrentFood < hungryWhenFoodLowerThan;
        }

        public override bool IsThirsty(BaseCharacterEntity character)
        {
            return character.CurrentWater < thirstyWhenWaterLowerThan;
        }

        public override bool IncreaseExp(BaseCharacterEntity character, int exp)
        {
            var isLevelUp = false;
            var oldLevel = character.Level;
            character.Exp += exp;
            var playerCharacter = character as IPlayerCharacterData;
            var nextLevelExp = character.GetNextLevelExp();
            while (nextLevelExp > 0 && character.Exp >= nextLevelExp)
            {
                character.Exp = character.Exp - nextLevelExp;
                ++character.Level;
                nextLevelExp = character.GetNextLevelExp();
                if (playerCharacter != null)
                {
                    playerCharacter.StatPoint += increaseStatPointEachLevel;
                    playerCharacter.SkillPoint += increaseSkillPointEachLevel;
                }
                isLevelUp = true;
            }
            return isLevelUp;
        }

        public override float GetEquipmentBonusRate(CharacterItem characterItem)
        {
            if (characterItem.GetMaxDurability() <= 0)
                return 1;
            var durabilityRate = (float)characterItem.durability / (float)characterItem.GetMaxDurability();
            if (durabilityRate > 0.5f)
                return 1f;
            else if (durabilityRate > 0.3f)
                return 0.75f;
            else if (durabilityRate > 0.15f)
                return 0.5f;
            else if (durabilityRate > 0.05f)
                return 0.25f;
            else
                return 0f;
        }

        public override void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage)
        {
            var decreaseWeaponDurability = normalDecreaseWeaponDurability;
            var decreaseShieldDurability = normalDecreaseShieldDurability;
            var decreaseArmorDurability = normalDecreaseArmorDurability;
            switch (combatAmountType)
            {
                case CombatAmountType.BlockedDamage:
                    decreaseWeaponDurability = blockedDecreaseWeaponDurability;
                    decreaseShieldDurability = blockedDecreaseShieldDurability;
                    decreaseArmorDurability = blockedDecreaseArmorDurability;
                    break;
                case CombatAmountType.CriticalDamage:
                    decreaseWeaponDurability = criticalDecreaseWeaponDurability;
                    decreaseShieldDurability = criticalDecreaseShieldDurability;
                    decreaseArmorDurability = criticalDecreaseArmorDurability;
                    break;
                case CombatAmountType.Miss:
                    decreaseWeaponDurability = missDecreaseWeaponDurability;
                    decreaseShieldDurability = missDecreaseShieldDurability;
                    decreaseArmorDurability = missDecreaseArmorDurability;
                    break;
            }
            // Decrease Weapon Durability
            var tempDestroy = false;
            var equipWeapons = attacker.EquipWeapons;
            var rightHand = equipWeapons.rightHand;
            var leftHand = equipWeapons.leftHand;
            if (rightHand.GetWeaponItem() != null && rightHand.GetMaxDurability() > 0)
            {
                rightHand = DecreaseDurability(rightHand, decreaseWeaponDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.rightHand = CharacterItem.Empty;
                else
                    equipWeapons.rightHand = rightHand;
            }
            if (leftHand.GetWeaponItem() != null && leftHand.GetMaxDurability() > 0)
            {
                leftHand = DecreaseDurability(leftHand, decreaseWeaponDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.leftHand = CharacterItem.Empty;
                else
                    equipWeapons.leftHand = leftHand;
            }
            attacker.EquipWeapons = equipWeapons;
            // Decrease Shield Durability
            equipWeapons = damageReceiver.EquipWeapons;
            rightHand = equipWeapons.rightHand;
            leftHand = equipWeapons.leftHand;
            if (rightHand.GetShieldItem() != null && rightHand.GetMaxDurability() > 0)
            {
                rightHand = DecreaseDurability(rightHand, decreaseShieldDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.rightHand = CharacterItem.Empty;
                else
                    equipWeapons.rightHand = rightHand;
            }
            if (leftHand.GetShieldItem() != null && leftHand.GetMaxDurability() > 0)
            {
                leftHand = DecreaseDurability(leftHand, decreaseShieldDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.leftHand = CharacterItem.Empty;
                else
                    equipWeapons.leftHand = leftHand;
            }
            damageReceiver.EquipWeapons = equipWeapons;
            // Decrease Armor Durability
            var count = damageReceiver.EquipItems.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                var equipItem = damageReceiver.EquipItems[i];
                if (equipItem.GetMaxDurability() <= 0)
                    continue;
                equipItem = DecreaseDurability(equipItem, decreaseArmorDurability, out tempDestroy);
                if (tempDestroy)
                    damageReceiver.EquipItems.RemoveAt(i);
                else
                    damageReceiver.EquipItems[i] = equipItem;
            }
        }

        private CharacterItem DecreaseDurability(CharacterItem characterItem, float decreaseDurability, out bool destroy)
        {
            destroy = false;
            var item = characterItem.GetEquipmentItem();
            if (item != null)
            {
                if (characterItem.durability - decreaseDurability <= 0 && item.destroyIfBroken)
                    destroy = true;
                characterItem.durability -= decreaseDurability;
                if (characterItem.durability < 0)
                    characterItem.durability = 0;
            }
            return characterItem;
        }
    }
}
