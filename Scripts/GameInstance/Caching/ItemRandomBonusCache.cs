using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class ItemRandomBonusCache
    {
        private CharacterStats _characterStats;
        public CharacterStats CharacterStats { get { return _characterStats; } }
        private CharacterStats _characterStatsRate;
        public CharacterStats CharacterStatsRate { get { return _characterStatsRate; } }
        public Dictionary<Attribute, float> AttributeAmounts { get; private set; }
        public Dictionary<Attribute, float> AttributeAmountRates { get; private set; }
        public Dictionary<DamageElement, float> ResistanceAmounts { get; private set; }
        public Dictionary<DamageElement, float> ArmorAmounts { get; private set; }
        public Dictionary<DamageElement, MinMaxFloat> DamageAmounts { get; private set; }
        public Dictionary<BaseSkill, int> SkillLevels { get; private set; }
        public int DataId { get; private set; }
        public int RandomSeed { get; private set; }

        public ItemRandomBonusCache(IEquipmentItem equipmentItem, int randomSeed)
        {
            DataId = equipmentItem.DataId;
            RandomSeed = randomSeed;
            _characterStats = new CharacterStats();
            _characterStatsRate = new CharacterStats();
            AttributeAmounts = new Dictionary<Attribute, float>();
            AttributeAmountRates = new Dictionary<Attribute, float>();
            ResistanceAmounts = new Dictionary<DamageElement, float>();
            ArmorAmounts = new Dictionary<DamageElement, float>();
            DamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            SkillLevels = new Dictionary<BaseSkill, int>();
            System.Random random = new System.Random(randomSeed);
            int appliedAmount = 0;
            ItemRandomBonus randomBonus = equipmentItem.RandomBonus;
            System.Action[] actions = new System.Action[8];
            actions[0] = () => RandomAttributeAmounts(random, randomBonus, ref appliedAmount);
            actions[1] = () => RandomAttributeAmountRates(random, randomBonus, ref appliedAmount);
            actions[2] = () => RandomResistanceAmounts(random, randomBonus, ref appliedAmount);
            actions[3] = () => RandomArmorAmounts(random, randomBonus, ref appliedAmount);
            actions[4] = () => RandomDamageAmounts(random, randomBonus, ref appliedAmount);
            actions[5] = () => RandomSkillLevels(random, randomBonus, ref appliedAmount);
            actions[6] = () => RandomCharacterStats(random, randomBonus, randomBonus.randomCharacterStats, ref _characterStats, ref appliedAmount);
            actions[7] = () => RandomCharacterStats(random, randomBonus, randomBonus.randomCharacterStatsRate, ref _characterStatsRate, ref appliedAmount);
            actions.Shuffle(random);
            actions[0].Invoke();
            actions[1].Invoke();
            actions[2].Invoke();
            actions[3].Invoke();
            actions[4].Invoke();
            actions[5].Invoke();
            actions[6].Invoke();
            actions[7].Invoke();
        }

        public void RandomAttributeAmounts(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomAttributeAmounts != null && randomBonus.randomAttributeAmounts.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomAttributeAmounts.Length; ++i)
                {
                    if (!randomBonus.randomAttributeAmounts[i].Apply(random)) continue;
                    AttributeAmounts = GameDataHelpers.CombineAttributes(AttributeAmounts, randomBonus.randomAttributeAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomAttributeAmountRates(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomAttributeAmountRates != null && randomBonus.randomAttributeAmountRates.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomAttributeAmountRates.Length; ++i)
                {
                    if (!randomBonus.randomAttributeAmountRates[i].Apply(random)) continue;
                    AttributeAmountRates = GameDataHelpers.CombineAttributes(AttributeAmountRates, randomBonus.randomAttributeAmountRates[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomResistanceAmounts(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomResistanceAmounts != null && randomBonus.randomResistanceAmounts.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomResistanceAmounts.Length; ++i)
                {
                    if (!randomBonus.randomResistanceAmounts[i].Apply(random)) continue;
                    ResistanceAmounts = GameDataHelpers.CombineResistances(ResistanceAmounts, randomBonus.randomResistanceAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomArmorAmounts(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomArmorAmounts != null && randomBonus.randomArmorAmounts.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomArmorAmounts.Length; ++i)
                {
                    if (!randomBonus.randomArmorAmounts[i].Apply(random)) continue;
                    ArmorAmounts = GameDataHelpers.CombineArmors(ArmorAmounts, randomBonus.randomArmorAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomDamageAmounts(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomDamageAmounts != null && randomBonus.randomDamageAmounts.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomDamageAmounts.Length; ++i)
                {
                    if (!randomBonus.randomDamageAmounts[i].Apply(random)) continue;
                    DamageAmounts = GameDataHelpers.CombineDamages(DamageAmounts, randomBonus.randomDamageAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f, 1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomSkillLevels(System.Random random, ItemRandomBonus randomBonus, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;
            if (randomBonus.randomSkillLevels != null && randomBonus.randomSkillLevels.Length > 0)
            {
                for (int i = 0; i < randomBonus.randomSkillLevels.Length; ++i)
                {
                    if (!randomBonus.randomSkillLevels[i].Apply(random)) continue;
                    SkillLevels = GameDataHelpers.CombineSkills(SkillLevels, randomBonus.randomSkillLevels[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    appliedAmount++;
                    if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                        return;
                }
            }
        }

        public void RandomCharacterStats(System.Random random, ItemRandomBonus randomBonus, RandomCharacterStats randomStats, ref CharacterStats stats, ref int appliedAmount)
        {
            if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                return;

            if (randomStats.ApplyHp(random))
            {
                stats.hp = randomStats.GetRandomedHp(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyHpRecovery(random))
            {
                stats.hpRecovery = randomStats.GetRandomedHpRecovery(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyHpLeechRate(random))
            {
                stats.hpLeechRate = randomStats.GetRandomedHpLeechRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyMp(random))
            {
                stats.mp = randomStats.GetRandomedMp(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyMpRecovery(random))
            {
                stats.mpRecovery = randomStats.GetRandomedMpRecovery(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyMpLeechRate(random))
            {
                stats.mpLeechRate = randomStats.GetRandomedMpLeechRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyStamina(random))
            {
                stats.stamina = randomStats.GetRandomedStamina(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyStaminaRecovery(random))
            {
                stats.staminaRecovery = randomStats.GetRandomedStaminaRecovery(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyStaminaLeechRate(random))
            {
                stats.staminaLeechRate = randomStats.GetRandomedStaminaLeechRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyFood(random))
            {
                stats.food = randomStats.GetRandomedFood(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyWater(random))
            {
                stats.water = randomStats.GetRandomedWater(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyAccuracy(random))
            {
                stats.accuracy = randomStats.GetRandomedAccuracy(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyEvasion(random))
            {
                stats.evasion = randomStats.GetRandomedEvasion(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyCriRate(random))
            {
                stats.criRate = randomStats.GetRandomedCriRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyCriDmgRate(random))
            {
                stats.criDmgRate = randomStats.GetRandomedCriDmgRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyBlockRate(random))
            {
                stats.blockRate = randomStats.GetRandomedBlockRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyBlockDmgRate(random))
            {
                stats.blockDmgRate = randomStats.GetRandomedBlockDmgRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyMoveSpeed(random))
            {
                stats.moveSpeed = randomStats.GetRandomedMoveSpeed(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyAtkSpeed(random))
            {
                stats.atkSpeed = randomStats.GetRandomedAtkSpeed(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyWeightLimit(random))
            {
                stats.weightLimit = randomStats.GetRandomedWeightLimit(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplySlotLimit(random))
            {
                stats.slotLimit = randomStats.GetRandomedSlotLimit(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyGoldRate(random))
            {
                stats.goldRate = randomStats.GetRandomedGoldRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyExpRate(random))
            {
                stats.expRate = randomStats.GetRandomedExpRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (randomStats.ApplyItemDropRate(random))
            {
                stats.itemDropRate = randomStats.GetRandomedItemDropRate(random);
                appliedAmount++;
                if (randomBonus.maxRandomStatsAmount > 0 && appliedAmount >= randomBonus.maxRandomStatsAmount)
                    return;
            }

            if (GameExtensionInstance.onRandomCharacterStats != null)
                GameExtensionInstance.onRandomCharacterStats(random, randomBonus, randomStats, ref stats, ref appliedAmount);
        }
    }
}
