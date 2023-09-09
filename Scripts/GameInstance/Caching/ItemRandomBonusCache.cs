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
        public byte Version { get; private set; }

        private ItemRandomBonus _randomBonus;
        private int _appliedAmount = 0;

        public ItemRandomBonusCache(IEquipmentItem equipmentItem, int randomSeed, byte version)
        {
            DataId = equipmentItem.DataId;
            RandomSeed = randomSeed;
            Version = version;
            _randomBonus = equipmentItem.RandomBonus;
            _characterStats = new CharacterStats();
            _characterStatsRate = new CharacterStats();
            AttributeAmounts = new Dictionary<Attribute, float>();
            AttributeAmountRates = new Dictionary<Attribute, float>();
            ResistanceAmounts = new Dictionary<DamageElement, float>();
            ArmorAmounts = new Dictionary<DamageElement, float>();
            DamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            SkillLevels = new Dictionary<BaseSkill, int>();
            System.Random random = new System.Random(randomSeed);
            System.Action[] actions = new System.Action[8];
            actions[0] = () => RandomAttributeAmounts(random);
            actions[1] = () => RandomAttributeAmountRates(random);
            actions[2] = () => RandomResistanceAmounts(random);
            actions[3] = () => RandomArmorAmounts(random);
            actions[4] = () => RandomDamageAmounts(random);
            actions[5] = () => RandomSkillLevels(random);
            actions[6] = () => RandomCharacterStats(random, false);
            actions[7] = () => RandomCharacterStats(random, true);
            actions.Shuffle(random);
            for (int i = 0; i < actions.Length; ++i)
            {
                if (IsReachedMaxRandomStatsAmount())
                    break;
                actions[i].Invoke();
            }
        }

        public bool IsReachedMaxRandomStatsAmount()
        {
            return _randomBonus.maxRandomStatsAmount > 0 && _appliedAmount >= _randomBonus.maxRandomStatsAmount;
        }

        public void RandomAttributeAmounts(System.Random random)
        {
            if (_randomBonus.randomAttributeAmounts != null && _randomBonus.randomAttributeAmounts.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomAttributeAmounts.Length; ++i)
                {
                    if (!_randomBonus.randomAttributeAmounts[i].Apply(random)) continue;
                    AttributeAmounts = GameDataHelpers.CombineAttributes(AttributeAmounts, _randomBonus.randomAttributeAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomAttributeAmountRates(System.Random random)
        {
            if (_randomBonus.randomAttributeAmountRates != null && _randomBonus.randomAttributeAmountRates.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomAttributeAmountRates.Length; ++i)
                {
                    if (!_randomBonus.randomAttributeAmountRates[i].Apply(random)) continue;
                    AttributeAmountRates = GameDataHelpers.CombineAttributes(AttributeAmountRates, _randomBonus.randomAttributeAmountRates[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomResistanceAmounts(System.Random random)
        {
            if (_randomBonus.randomResistanceAmounts != null && _randomBonus.randomResistanceAmounts.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomResistanceAmounts.Length; ++i)
                {
                    if (!_randomBonus.randomResistanceAmounts[i].Apply(random)) continue;
                    ResistanceAmounts = GameDataHelpers.CombineResistances(ResistanceAmounts, _randomBonus.randomResistanceAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomArmorAmounts(System.Random random)
        {
            if (_randomBonus.randomArmorAmounts != null && _randomBonus.randomArmorAmounts.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomArmorAmounts.Length; ++i)
                {
                    if (!_randomBonus.randomArmorAmounts[i].Apply(random)) continue;
                    ArmorAmounts = GameDataHelpers.CombineArmors(ArmorAmounts, _randomBonus.randomArmorAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomDamageAmounts(System.Random random)
        {
            if (_randomBonus.randomDamageAmounts != null && _randomBonus.randomDamageAmounts.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomDamageAmounts.Length; ++i)
                {
                    if (!_randomBonus.randomDamageAmounts[i].Apply(random)) continue;
                    DamageAmounts = GameDataHelpers.CombineDamages(DamageAmounts, _randomBonus.randomDamageAmounts[i].GetRandomedAmount(random).ToKeyValuePair(1f, 1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomSkillLevels(System.Random random)
        {
            if (_randomBonus.randomSkillLevels != null && _randomBonus.randomSkillLevels.Length > 0)
            {
                for (int i = 0; i < _randomBonus.randomSkillLevels.Length; ++i)
                {
                    if (!_randomBonus.randomSkillLevels[i].Apply(random)) continue;
                    SkillLevels = GameDataHelpers.CombineSkills(SkillLevels, _randomBonus.randomSkillLevels[i].GetRandomedAmount(random).ToKeyValuePair(1f));
                    _appliedAmount++;
                    if (IsReachedMaxRandomStatsAmount())
                        return;
                }
            }
        }

        public void RandomCharacterStatsHp(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyHp(random))
            {
                stats.hp = randomStats.GetRandomedHp(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsHpRecovery(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyHpRecovery(random))
            {
                stats.hpRecovery = randomStats.GetRandomedHpRecovery(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsHpLeechRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyHpLeechRate(random))
            {
                stats.hpLeechRate = randomStats.GetRandomedHpLeechRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsMp(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyMp(random))
            {
                stats.mp = randomStats.GetRandomedMp(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsMpRecovery(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyMpRecovery(random))
            {
                stats.mpRecovery = randomStats.GetRandomedMpRecovery(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsMpLeechRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyMpLeechRate(random))
            {
                stats.mpLeechRate = randomStats.GetRandomedMpLeechRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsStamina(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyStamina(random))
            {
                stats.stamina = randomStats.GetRandomedStamina(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsStaminaRecovery(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyStaminaRecovery(random))
            {
                stats.staminaRecovery = randomStats.GetRandomedStaminaRecovery(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsStaminaLeechRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyStaminaLeechRate(random))
            {
                stats.staminaLeechRate = randomStats.GetRandomedStaminaLeechRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsFood(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyFood(random))
            {
                stats.food = randomStats.GetRandomedFood(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsWater(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyWater(random))
            {
                stats.water = randomStats.GetRandomedWater(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsAccuracy(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyAccuracy(random))
            {
                stats.accuracy = randomStats.GetRandomedAccuracy(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsEvasion(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyEvasion(random))
            {
                stats.evasion = randomStats.GetRandomedEvasion(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsCriRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyCriRate(random))
            {
                stats.criRate = randomStats.GetRandomedCriRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsCriDmgRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyCriDmgRate(random))
            {
                stats.criDmgRate = randomStats.GetRandomedCriDmgRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsBlockRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyBlockRate(random))
            {
                stats.blockRate = randomStats.GetRandomedBlockRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsBlockDmgRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyBlockDmgRate(random))
            {
                stats.blockDmgRate = randomStats.GetRandomedBlockDmgRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsMoveSpeed(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyMoveSpeed(random))
            {
                stats.moveSpeed = randomStats.GetRandomedMoveSpeed(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsAtkSpeed(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyAtkSpeed(random))
            {
                stats.atkSpeed = randomStats.GetRandomedAtkSpeed(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsWeightLimit(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyWeightLimit(random))
            {
                stats.weightLimit = randomStats.GetRandomedWeightLimit(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsSlotLimit(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplySlotLimit(random))
            {
                stats.slotLimit = randomStats.GetRandomedSlotLimit(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsGoldRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyGoldRate(random))
            {
                stats.goldRate = randomStats.GetRandomedGoldRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsExpRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyExpRate(random))
            {
                stats.expRate = randomStats.GetRandomedExpRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsItemDropRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyItemDropRate(random))
            {
                stats.itemDropRate = randomStats.GetRandomedItemDropRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsJumpHeight(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyJumpHeight(random))
            {
                stats.jumpHeight = randomStats.GetRandomedJumpHeight(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsHeadDamageAbsorbs(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyHeadDamageAbsorbs(random))
            {
                stats.headDamageAbsorbs = randomStats.GetRandomedHeadDamageAbsorbs(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsBodyDamageAbsorbs(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyBodyDamageAbsorbs(random))
            {
                stats.bodyDamageAbsorbs = randomStats.GetRandomedBodyDamageAbsorbs(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsFallDamageAbsorbs(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyFallDamageAbsorbs(random))
            {
                stats.fallDamageAbsorbs = randomStats.GetRandomedFallDamageAbsorbs(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStatsGravityRate(System.Random random, RandomCharacterStats randomStats, ref CharacterStats stats)
        {
            if (randomStats.ApplyGravityRate(random))
            {
                stats.gravityRate = randomStats.GetRandomedGravityRate(random);
                _appliedAmount++;
            }
        }

        public void RandomCharacterStats(System.Random random, bool isRate)
        {
            CharacterStats tempStats = isRate ? _characterStatsRate : _characterStats;
            RandomCharacterStats randomStats = isRate ? _randomBonus.randomCharacterStatsRate : _randomBonus.randomCharacterStats;
            System.Action[] actions = new System.Action[29];
            actions[0] = () => RandomCharacterStatsHp(random, randomStats, ref tempStats);
            actions[1] = () => RandomCharacterStatsHpRecovery(random, randomStats, ref tempStats);
            actions[2] = () => RandomCharacterStatsHpLeechRate(random, randomStats, ref tempStats);
            actions[3] = () => RandomCharacterStatsMp(random, randomStats, ref tempStats);
            actions[4] = () => RandomCharacterStatsMpRecovery(random, randomStats, ref tempStats);
            actions[5] = () => RandomCharacterStatsMpLeechRate(random, randomStats, ref tempStats);
            actions[6] = () => RandomCharacterStatsStamina(random, randomStats, ref tempStats);
            actions[7] = () => RandomCharacterStatsStaminaRecovery(random, randomStats, ref tempStats);
            actions[8] = () => RandomCharacterStatsStaminaLeechRate(random, randomStats, ref tempStats);
            actions[9] = () => RandomCharacterStatsFood(random, randomStats, ref tempStats);
            actions[10] = () => RandomCharacterStatsWater(random, randomStats, ref tempStats);
            actions[11] = () => RandomCharacterStatsAccuracy(random, randomStats, ref tempStats);
            actions[12] = () => RandomCharacterStatsEvasion(random, randomStats, ref tempStats);
            actions[13] = () => RandomCharacterStatsCriRate(random, randomStats, ref tempStats);
            actions[14] = () => RandomCharacterStatsCriDmgRate(random, randomStats, ref tempStats);
            actions[15] = () => RandomCharacterStatsBlockRate(random, randomStats, ref tempStats);
            actions[16] = () => RandomCharacterStatsBlockDmgRate(random, randomStats, ref tempStats);
            actions[17] = () => RandomCharacterStatsMoveSpeed(random, randomStats, ref tempStats);
            actions[18] = () => RandomCharacterStatsAtkSpeed(random, randomStats, ref tempStats);
            actions[19] = () => RandomCharacterStatsWeightLimit(random, randomStats, ref tempStats);
            actions[20] = () => RandomCharacterStatsSlotLimit(random, randomStats, ref tempStats);
            actions[21] = () => RandomCharacterStatsGoldRate(random, randomStats, ref tempStats);
            actions[22] = () => RandomCharacterStatsExpRate(random, randomStats, ref tempStats);
            actions[23] = () => RandomCharacterStatsItemDropRate(random, randomStats, ref tempStats);
            actions[24] = () => RandomCharacterStatsJumpHeight(random, randomStats, ref tempStats);
            actions[25] = () => RandomCharacterStatsHeadDamageAbsorbs(random, randomStats, ref tempStats);
            actions[26] = () => RandomCharacterStatsBodyDamageAbsorbs(random, randomStats, ref tempStats);
            actions[27] = () => RandomCharacterStatsFallDamageAbsorbs(random, randomStats, ref tempStats);
            actions[28] = () => RandomCharacterStatsGravityRate(random, randomStats, ref tempStats);
            if (Version > 0)
                actions.Shuffle(random);
            for (int i = 0; i < actions.Length; ++i)
            {
                if (IsReachedMaxRandomStatsAmount())
                    break;
                actions[i].Invoke();
            }
            if (GameExtensionInstance.onRandomCharacterStats != null)
                GameExtensionInstance.onRandomCharacterStats(random, _randomBonus, isRate, ref tempStats, ref _appliedAmount);
            if (isRate)
                _characterStatsRate = tempStats;
            else
                _characterStats = tempStats;
        }
    }
}
