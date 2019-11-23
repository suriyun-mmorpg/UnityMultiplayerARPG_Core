using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Simple Gameplay Rule", menuName = "Create GameplayRule/Simple Gameplay Rule", order = -2999)]
    public class SimpleGameplayRule : BaseGameplayRule
    {
        [Header("Levelling/Stat/Skill")]
        public short increaseStatPointEachLevel = 5;
        public short increaseSkillPointEachLevel = 1;
        [Range(0f, 100f)]
        public float expLostPercentageWhenDeath = 2f;
        [Header("Stamina/Sprint")]
        public float staminaRecoveryPerSeconds = 5;
        public float staminaDecreasePerSeconds = 5;
        public float moveSpeedRateWhileSprint = 1.5f;
        [Header("Hp/Mp/Food/Water")]
        public int hungryWhenFoodLowerThan = 40;
        public int thirstyWhenWaterLowerThan = 40;
        public float foodDecreasePerSeconds = 4;
        public float waterDecreasePerSeconds = 2;
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
        [Header("Durability")]
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

        public override float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
        {
            // Attacker stats
            CharacterStats attackerStats = attacker.GetCaches().Stats;
            // Damage receiver stats
            CharacterStats dmgReceiverStats = damageReceiver.GetCaches().Stats;
            // Calculate chance to hit
            float attackerAcc = attackerStats.accuracy;
            float dmgReceiverEva = dmgReceiverStats.evasion;
            short attackerLvl = attacker.Level;
            short dmgReceiverLvl = damageReceiver.Level;
            float hitChance = 2f;

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
            float criRate = attacker.GetCaches().Stats.criRate;
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
            return damage * attacker.GetCaches().Stats.criDmgRate;
        }

        public override float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
        {
            float blockRate = damageReceiver.GetCaches().Stats.blockRate;
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
            float blockDmgRate = damageReceiver.GetCaches().Stats.blockDmgRate;
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
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            // Reduce damage by resistance
            float resistanceAmount = 0f;
            damageReceiver.GetCaches().Resistances.TryGetValue(damageElement, out resistanceAmount);
            if (resistanceAmount > damageElement.maxResistanceAmount)
                resistanceAmount = damageElement.maxResistanceAmount;
            damageAmount -= damageAmount * resistanceAmount; // If resistance is minus damage will be increased
            // Reduce damage by armor
            float armorAmount = 0f;
            damageReceiver.GetCaches().Armors.TryGetValue(damageElement, out armorAmount);
            // Formula: Attack * 100 / (100 + Defend)
            damageAmount *= 100f / (100f + armorAmount);
            return damageAmount;
        }

        public override float GetRecoveryHpPerSeconds(BaseCharacterEntity character)
        {
            if (IsHungry(character))
                return 0;
            return character.GetCaches().MaxHp * hpRecoveryRatePerSeconds;
        }

        public override float GetRecoveryMpPerSeconds(BaseCharacterEntity character)
        {
            if (IsThirsty(character))
                return 0;
            return character.GetCaches().MaxMp * mpRecoveryRatePerSeconds;
        }

        public override float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character)
        {
            return staminaRecoveryPerSeconds;
        }

        public override float GetDecreasingHpPerSeconds(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity)
                return 0f;
            float result = 0f;
            if (IsHungry(character))
                result += character.GetCaches().MaxHp * hpDecreaseRatePerSecondsWhenHungry;
            if (IsThirsty(character))
                result += character.GetCaches().MaxHp * hpDecreaseRatePerSecondsWhenThirsty;
            return result;
        }

        public override float GetDecreasingMpPerSeconds(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity)
                return 0f;
            float result = 0f;
            if (IsHungry(character))
                result += character.GetCaches().MaxMp * mpDecreaseRatePerSecondsWhenHungry;
            if (IsThirsty(character))
                result += character.GetCaches().MaxMp * mpDecreaseRatePerSecondsWhenThirsty;
            return result;
        }

        public override float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity || !character.MovementState.HasFlag(MovementState.IsSprinting))
                return 0f;
            return staminaDecreasePerSeconds;
        }

        public override float GetDecreasingFoodPerSeconds(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity)
                return 0f;
            return foodDecreasePerSeconds;
        }

        public override float GetDecreasingWaterPerSeconds(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity)
                return 0f;
            return waterDecreasePerSeconds;
        }

        public override float GetExpLostPercentageWhenDeath(BaseCharacterEntity character)
        {
            if (character is BaseMonsterCharacterEntity)
                return 0f;
            return expLostPercentageWhenDeath;
        }

        public override float GetMoveSpeed(BaseCharacterEntity character)
        {
            float moveSpeed = character.GetCaches().MoveSpeed;
            if (character is BaseMonsterCharacterEntity &&
                (character as BaseMonsterCharacterEntity).isWandering)
                moveSpeed = (character as BaseMonsterCharacterEntity).MonsterDatabase.wanderMoveSpeed;
            if (character.MovementState.HasFlag(MovementState.Forward) &&
                character.MovementState.HasFlag(MovementState.IsSprinting) &&
                character.CurrentStamina > 0f)
                moveSpeed *= moveSpeedRateWhileSprint;
            if (character.isAttackingOrUsingSkill)
                moveSpeed *= character.moveSpeedRateWhileAttackOrUseSkill;
            return moveSpeed;
        }

        public override float GetAttackSpeed(BaseCharacterEntity character)
        {
            float atkSpeed = character.GetCaches().AtkSpeed;
            // Minimum attack speed is 0.1
            if (atkSpeed <= 0.1f)
                atkSpeed = 0.1f;
            return atkSpeed;
        }

        public override float GetTotalWeight(ICharacterData character)
        {
            float result = character.EquipItems.GetTotalItemWeight() + character.NonEquipItems.GetTotalItemWeight();
            // Weight from right hand equipment
            if (character.EquipWeapons.rightHand.NotEmptySlot())
                result += character.EquipWeapons.rightHand.GetItem().weight;
            // Weight from left hand equipment
            if (character.EquipWeapons.leftHand.NotEmptySlot())
                result += character.EquipWeapons.leftHand.GetItem().weight;
            return result;
        }

        public override short GetTotalSlot(ICharacterData character)
        {
            return (short)(character.GetCaches().Stats.slotLimit + GameInstance.Singleton.baseSlotLimit);
        }

        public override bool IsHungry(BaseCharacterEntity character)
        {
            return foodDecreasePerSeconds > 0 && character.CurrentFood < hungryWhenFoodLowerThan;
        }

        public override bool IsThirsty(BaseCharacterEntity character)
        {
            return waterDecreasePerSeconds > 0 && character.CurrentWater < thirstyWhenWaterLowerThan;
        }

        public override bool RewardExp(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if ((character is BaseMonsterCharacterEntity) &&
                (character as BaseMonsterCharacterEntity).SummonType != SummonType.Pet)
            {
                // If it's monster and not pet, do not increase exp
                return false;
            }

            bool isLevelUp = false;
            int exp = Mathf.CeilToInt(reward.exp * multiplier);
            BasePlayerCharacterEntity playerCharacter = character as BasePlayerCharacterEntity;
            if (playerCharacter != null)
            {
                // Increase exp by guild's skills
                GuildData guildData;
                switch (rewardGivenType)
                {
                    case RewardGivenType.KillMonster:
                        if (playerCharacter.gameManager.TryGetGuild(playerCharacter.GuildId, out guildData))
                            exp += (int)(exp * guildData.IncreaseExpGainPercentage * 0.01f);
                        break;
                    case RewardGivenType.PartyShare:
                        if (playerCharacter.gameManager.TryGetGuild(playerCharacter.GuildId, out guildData))
                            exp += (int)(exp * guildData.IncreaseShareExpGainPercentage * 0.01f);
                        break;
                }
            }

            try
            {
                checked
                {
                    character.Exp += exp;
                }
            }
            catch (System.OverflowException)
            {
                character.Exp = int.MaxValue;
            }

            int nextLevelExp = character.GetNextLevelExp();
            while (nextLevelExp > 0 && character.Exp >= nextLevelExp)
            {
                character.Exp = character.Exp - nextLevelExp;
                ++character.Level;
                nextLevelExp = character.GetNextLevelExp();
                if (playerCharacter != null)
                {
                    try
                    {
                        checked
                        {
                            playerCharacter.StatPoint += increaseStatPointEachLevel;
                        }
                    }
                    catch (System.OverflowException)
                    {
                        playerCharacter.StatPoint = short.MaxValue;
                    }

                    try
                    {
                        checked
                        {
                            playerCharacter.SkillPoint += increaseSkillPointEachLevel;
                        }
                    }
                    catch (System.OverflowException)
                    {
                        playerCharacter.SkillPoint = short.MaxValue;
                    }
                }
                isLevelUp = true;
            }
            return isLevelUp;
        }

        public override void RewardCurrencies(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (character is BaseMonsterCharacterEntity)
            {
                // Don't give reward currencies to monsters
                return;
            }

            int gold = Mathf.CeilToInt(reward.gold * multiplier);
            BasePlayerCharacterEntity playerCharacter = character as BasePlayerCharacterEntity;
            if (playerCharacter != null)
            {
                // Increase exp by guild's skills
                GuildData guildData;
                switch (rewardGivenType)
                {
                    case RewardGivenType.KillMonster:
                        if (playerCharacter.gameManager.TryGetGuild(playerCharacter.GuildId, out guildData))
                            gold += (int)(gold * guildData.IncreaseGoldGainPercentage * 0.01f);
                        break;
                    case RewardGivenType.PartyShare:
                        if (playerCharacter.gameManager.TryGetGuild(playerCharacter.GuildId, out guildData))
                            gold += (int)(gold * guildData.IncreaseShareGoldGainPercentage * 0.01f);
                        break;
                }

                try
                {
                    checked
                    {
                        playerCharacter.Gold += gold;
                    }
                }
                catch (System.OverflowException)
                {
                    playerCharacter.Gold += int.MaxValue;
                }
            }
        }

        public override float GetEquipmentStatsRate(CharacterItem characterItem)
        {
            if (characterItem.GetMaxDurability() <= 0)
                return 1;
            float durabilityRate = (float)characterItem.durability / (float)characterItem.GetMaxDurability();
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
            float decreaseWeaponDurability = normalDecreaseWeaponDurability;
            float decreaseShieldDurability = normalDecreaseShieldDurability;
            float decreaseArmorDurability = normalDecreaseArmorDurability;
            GetDecreaseDurabilityAmount(combatAmountType, out decreaseWeaponDurability, out decreaseShieldDurability, out decreaseArmorDurability);
            // Decrease Weapon Durability
            DecreaseEquipWeaponsDurability(attacker, decreaseWeaponDurability);
            // Decrease Shield Durability
            DecreaseEquipShieldsDurability(damageReceiver, decreaseShieldDurability);
            // Decrease Armor Durability
            DecreaseEquipItemsDurability(damageReceiver, decreaseArmorDurability);
        }

        public override void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage)
        {
            float decreaseWeaponDurability = normalDecreaseWeaponDurability;
            float decreaseShieldDurability = normalDecreaseShieldDurability;
            float decreaseArmorDurability = normalDecreaseArmorDurability;
            GetDecreaseDurabilityAmount(combatAmountType, out decreaseWeaponDurability, out decreaseShieldDurability, out decreaseArmorDurability);
            // Decrease Weapon Durability
            DecreaseEquipWeaponsDurability(attacker, decreaseWeaponDurability);
        }

        private void GetDecreaseDurabilityAmount(CombatAmountType combatAmountType, out float decreaseWeaponDurability, out float decreaseShieldDurability, out float decreaseArmorDurability)
        {
            decreaseWeaponDurability = normalDecreaseWeaponDurability;
            decreaseShieldDurability = normalDecreaseShieldDurability;
            decreaseArmorDurability = normalDecreaseArmorDurability;
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
        }

        private void DecreaseEquipWeaponsDurability(BaseCharacterEntity entity, float decreaseDurability)
        {
            bool tempDestroy = false;
            EquipWeapons equipWeapons = entity.EquipWeapons;
            CharacterItem rightHand = equipWeapons.rightHand;
            CharacterItem leftHand = equipWeapons.leftHand;
            if (rightHand.GetWeaponItem() != null && rightHand.GetMaxDurability() > 0)
            {
                rightHand = DecreaseDurability(rightHand, decreaseDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.rightHand = CharacterItem.Empty;
                else
                    equipWeapons.rightHand = rightHand;
            }
            if (leftHand.GetWeaponItem() != null && leftHand.GetMaxDurability() > 0)
            {
                leftHand = DecreaseDurability(leftHand, decreaseDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.leftHand = CharacterItem.Empty;
                else
                    equipWeapons.leftHand = leftHand;
            }
            entity.EquipWeapons = equipWeapons;
        }

        private void DecreaseEquipShieldsDurability(BaseCharacterEntity entity, float decreaseDurability)
        {
            bool tempDestroy = false;
            EquipWeapons equipWeapons = entity.EquipWeapons;
            CharacterItem rightHand = equipWeapons.rightHand;
            CharacterItem leftHand = equipWeapons.leftHand;
            if (rightHand.GetShieldItem() != null && rightHand.GetMaxDurability() > 0)
            {
                rightHand = DecreaseDurability(rightHand, decreaseDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.rightHand = CharacterItem.Empty;
                else
                    equipWeapons.rightHand = rightHand;
            }
            if (leftHand.GetShieldItem() != null && leftHand.GetMaxDurability() > 0)
            {
                leftHand = DecreaseDurability(leftHand, decreaseDurability, out tempDestroy);
                if (tempDestroy)
                    equipWeapons.leftHand = CharacterItem.Empty;
                else
                    equipWeapons.leftHand = leftHand;
            }
            entity.EquipWeapons = equipWeapons;
        }

        private void DecreaseEquipItemsDurability(BaseCharacterEntity entity, float decreaseDurability)
        {
            bool tempDestroy = false;
            int count = entity.EquipItems.Count;
            for (int i = count - 1; i >= 0; --i)
            {
                CharacterItem equipItem = entity.EquipItems[i];
                if (equipItem.GetMaxDurability() <= 0)
                    continue;
                equipItem = DecreaseDurability(equipItem, decreaseDurability, out tempDestroy);
                if (tempDestroy)
                    entity.EquipItems.RemoveAt(i);
                else
                    entity.EquipItems[i] = equipItem;
            }
        }

        private CharacterItem DecreaseDurability(CharacterItem characterItem, float decreaseDurability, out bool destroy)
        {
            destroy = false;
            Item item = characterItem.GetEquipmentItem();
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

        public override bool CurrenciesEnoughToBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount)
        {
            return character.Gold >= sellItem.sellPrice * amount;
        }

        public override void DecreaseCurrenciesWhenBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount)
        {
            character.Gold -= sellItem.sellPrice * amount;
        }

        public override void IncreaseCurrenciesWhenSellItem(IPlayerCharacterData character, Item item, short amount)
        {
            character.Gold += item.sellPrice * amount;
        }

        public override bool CurrenciesEnoughToRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel)
        {
            return character.Gold >= refineLevel.RequireGold;
        }

        public override void DecreaseCurrenciesWhenRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel)
        {
            character.Gold -= refineLevel.RequireGold;
        }

        public override bool CurrenciesEnoughToRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice)
        {
            return character.Gold >= repairPrice.RequireGold;
        }

        public override void DecreaseCurrenciesWhenRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice)
        {
            character.Gold -= repairPrice.RequireGold;
        }

        public override bool CurrenciesEnoughToCraftItem(IPlayerCharacterData character, ItemCraft itemCraft)
        {
            return character.Gold >= itemCraft.RequireGold;
        }

        public override void DecreaseCurrenciesWhenCraftItem(IPlayerCharacterData character, ItemCraft itemCraft)
        {
            character.Gold -= itemCraft.RequireGold;
        }

        public override bool CurrenciesEnoughToCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting)
        {
            return character.Gold >= setting.CreateGuildRequiredGold;
        }

        public override void DecreaseCurrenciesWhenCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting)
        {
            character.Gold -= setting.CreateGuildRequiredGold;
        }

        public override Reward MakeMonsterReward(MonsterCharacter monster)
        {
            Reward result = new Reward();
            result.exp = monster.RandomExp();
            result.gold = monster.RandomGold();
            return result;
        }

        public override Reward MakeQuestReward(Quest quest)
        {
            Reward result = new Reward();
            result.exp = quest.rewardExp;
            result.gold = quest.rewardGold;
            return result;
        }

        public override float GetRecoveryUpdateDuration()
        {
            return 1f;
        }
    }
}
