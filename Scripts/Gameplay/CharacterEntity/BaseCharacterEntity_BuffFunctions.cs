using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        #region Events
        public event ApplyBuffDelegate onApplyBuff;
        public event AppliedRecoveryAmountDelegate onBuffHpRecovery;
        public event AppliedRecoveryAmountDelegate onBuffHpDecrease;
        public event AppliedRecoveryAmountDelegate onBuffMpRecovery;
        public event AppliedRecoveryAmountDelegate onBuffMpDecrease;
        public event AppliedRecoveryAmountDelegate onBuffStaminaRecovery;
        public event AppliedRecoveryAmountDelegate onBuffStaminaDecrease;
        public event AppliedRecoveryAmountDelegate onBuffFoodRecovery;
        public event AppliedRecoveryAmountDelegate onBuffFoodDecrease;
        public event AppliedRecoveryAmountDelegate onBuffWaterRecovery;
        public event AppliedRecoveryAmountDelegate onBuffWaterDecrease;
        #endregion

        public virtual void ApplyBuff(int dataId, BuffType type, short level, EntityInfo buffApplier)
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
                    while (indexesOfBuff.Count + 1 > maxStack)
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
            if (newBuff.GetBuff().disallowMove)
                StopMove();

            if (newBuff.GetDuration() <= 0f)
            {
                CharacterRecoveryData recoveryData = new CharacterRecoveryData(this, buffApplier);
                recoveryData.Setup(newBuff);
                recoveryData.Apply(1f);
            }

            if (onApplyBuff != null)
                onApplyBuff.Invoke(dataId, type, level, buffApplier);
        }

        public virtual void OnBuffHpRecovery(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentHp += amount;
            CallAllAppendCombatText(CombatAmountType.HpRecovery, DamageSource.None, 0, amount);
            if (onBuffHpRecovery != null)
                onBuffHpRecovery.Invoke(causer, amount);
        }

        public virtual void OnBuffHpDecrease(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentHp -= amount;
            CallAllAppendCombatText(CombatAmountType.HpDecrease, DamageSource.None, 0, amount);
            if (onBuffHpDecrease != null)
                onBuffHpDecrease.Invoke(causer, amount);
        }

        public virtual void OnBuffMpRecovery(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentMp += amount;
            CallAllAppendCombatText(CombatAmountType.MpRecovery, DamageSource.None, 0, amount);
            if (onBuffMpRecovery != null)
                onBuffMpRecovery.Invoke(causer, amount);
        }

        public virtual void OnBuffMpDecrease(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentMp -= amount;
            CallAllAppendCombatText(CombatAmountType.MpDecrease, DamageSource.None, 0, amount);
            if (onBuffMpDecrease != null)
                onBuffMpDecrease.Invoke(causer, amount);
        }

        public virtual void OnBuffStaminaRecovery(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentStamina += amount;
            CallAllAppendCombatText(CombatAmountType.StaminaRecovery, DamageSource.None, 0, amount);
            if (onBuffStaminaRecovery != null)
                onBuffStaminaRecovery.Invoke(causer, amount);
        }

        public virtual void OnBuffStaminaDecrease(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentStamina -= amount;
            CallAllAppendCombatText(CombatAmountType.StaminaDecrease, DamageSource.None, 0, amount);
            if (onBuffStaminaDecrease != null)
                onBuffStaminaDecrease.Invoke(causer, amount);
        }

        public virtual void OnBuffFoodRecovery(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentFood += amount;
            CallAllAppendCombatText(CombatAmountType.FoodRecovery, DamageSource.None, 0, amount);
            if (onBuffFoodRecovery != null)
                onBuffFoodRecovery.Invoke(causer, amount);
        }

        public virtual void OnBuffFoodDecrease(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentFood -= amount;
            CallAllAppendCombatText(CombatAmountType.FoodDecrease, DamageSource.None, 0, amount);
            if (onBuffFoodDecrease != null)
                onBuffFoodDecrease.Invoke(causer, amount);
        }

        public virtual void OnBuffWaterRecovery(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentWater += amount;
            CallAllAppendCombatText(CombatAmountType.WaterRecovery, DamageSource.None, 0, amount);
            if (onBuffWaterRecovery != null)
                onBuffWaterRecovery.Invoke(causer, amount);
        }

        public virtual void OnBuffWaterDecrease(EntityInfo causer, int amount)
        {
            if (amount < 0)
                amount = 0;
            CurrentWater -= amount;
            CallAllAppendCombatText(CombatAmountType.WaterDecrease, DamageSource.None, 0, amount);
            if (onBuffWaterDecrease != null)
                onBuffWaterDecrease.Invoke(causer, amount);
        }
    }
}
