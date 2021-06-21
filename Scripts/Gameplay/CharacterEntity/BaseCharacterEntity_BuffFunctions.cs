using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public void ApplyBuff(int dataId, BuffType type, short level, EntityInfo buffApplier)
        {
            if (!IsServer || this.IsDead())
                return;

            Buff tempBuff;
            bool isExtendDuration = false;
            int maxStack = 0;
            switch (type)
            {
                case BuffType.SkillBuff:
                    if (!GameInstance.Skills.ContainsKey(dataId) || !GameInstance.Skills[dataId].IsBuff)
                        return;
                    tempBuff = GameInstance.Skills[dataId].Buff;
                    isExtendDuration = tempBuff.isExtendDuration;
                    maxStack = tempBuff.GetMaxStack(level);
                    break;
                case BuffType.SkillDebuff:
                    if (!GameInstance.Skills.ContainsKey(dataId) || !GameInstance.Skills[dataId].IsDebuff)
                        return;
                    tempBuff = GameInstance.Skills[dataId].Debuff;
                    isExtendDuration = tempBuff.isExtendDuration;
                    maxStack = tempBuff.GetMaxStack(level);
                    break;
                case BuffType.PotionBuff:
                    if (!GameInstance.Items.ContainsKey(dataId) || !GameInstance.Items[dataId].IsPotion())
                        return;
                    tempBuff = (GameInstance.Items[dataId] as IPotionItem).Buff;
                    isExtendDuration = tempBuff.isExtendDuration;
                    maxStack = tempBuff.GetMaxStack(level);
                    break;
                case BuffType.GuildSkillBuff:
                    if (!GameInstance.GuildSkills.ContainsKey(dataId))
                        return;
                    tempBuff = GameInstance.GuildSkills[dataId].Buff;
                    isExtendDuration = tempBuff.isExtendDuration;
                    maxStack = tempBuff.GetMaxStack(level);
                    break;
                case BuffType.StatusEffect:
                    if (!GameInstance.StatusEffects.ContainsKey(dataId))
                        return;
                    tempBuff = GameInstance.StatusEffects[dataId].Buff;
                    isExtendDuration = tempBuff.isExtendDuration;
                    maxStack = tempBuff.GetMaxStack(level);
                    break;
            }

            if (isExtendDuration)
            {
                int buffIndex = this.IndexOfBuff(dataId, type);
                if (buffIndex >= 0)
                {
                    CharacterBuff characterBuff = buffs[buffIndex];
                    characterBuff.level = level;
                    characterBuff.buffRemainsDuration += buffs[buffIndex].GetDuration();
                    buffs[buffIndex] = characterBuff;
                    return;
                }
            }
            else
            {
                if (maxStack > 1)
                {
                    List<int> indexesOfBuff = this.IndexesOfBuff(dataId, type);
                    while (indexesOfBuff.Count > maxStack)
                    {
                        int buffIndex = indexesOfBuff[0];
                        if (buffIndex >= 0)
                            buffs.RemoveAt(buffIndex);
                        indexesOfBuff.RemoveAt(0);
                    }
                }
                else
                {
                    // `maxStack` <= 0, assume that it's = `1`
                    int buffIndex = this.IndexOfBuff(dataId, type);
                    if (buffIndex >= 0)
                        buffs.RemoveAt(buffIndex);
                }
            }

            CharacterBuff newBuff = CharacterBuff.Create(type, dataId, level);
            newBuff.Apply(buffApplier);
            buffs.Add(newBuff);

            if (newBuff.GetDuration() <= 0f)
            {
                CharacterRecoveryData recoveryData = default(CharacterRecoveryData);
                int tempAmount = 0;
                // Damage over time
                DamageElement damageElement;
                MinMaxFloat damageAmount;
                float tempReceivingDamage;
                foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in newBuff.GetDamageOverTimes())
                {
                    damageElement = damageOverTime.Key;
                    damageAmount = damageOverTime.Value;
                    tempReceivingDamage = (float)damageElement.GetDamageReducedByResistance(this.GetCaches().Resistances, this.GetCaches().Armors, damageAmount.Random(Random.Range(0, 255)));
                    if (tempReceivingDamage > 0f)
                        tempAmount += (int)tempReceivingDamage;
                }
                recoveryData.decreasingHp += tempAmount;
                // Hp recovery
                tempAmount = newBuff.GetRecoveryHp();
                if (tempAmount > 0)
                    recoveryData.recoveryingHp += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingHp += -tempAmount;
                // Mp recovery
                tempAmount = newBuff.GetRecoveryMp();
                if (tempAmount > 0)
                    recoveryData.recoveryingMp += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingMp += -tempAmount;
                // Stamina recovery
                tempAmount = newBuff.GetRecoveryStamina();
                if (tempAmount > 0)
                    recoveryData.recoveryingStamina += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingStamina += -tempAmount;
                // Food recovery
                tempAmount = newBuff.GetRecoveryFood();
                if (tempAmount > 0)
                    recoveryData.recoveryingFood += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingFood += -tempAmount;
                // Water recovery
                tempAmount = newBuff.GetRecoveryWater();
                if (tempAmount > 0)
                    recoveryData.recoveryingWater += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingWater += -tempAmount;

                recoveryData = recoveryData.Apply(this, buffApplier);
                // Causer is the entity whom applied buffs to this entity
                ValidateRecovery(buffApplier);
            }

            OnApplyBuff(dataId, type, level);
        }
    }
}
