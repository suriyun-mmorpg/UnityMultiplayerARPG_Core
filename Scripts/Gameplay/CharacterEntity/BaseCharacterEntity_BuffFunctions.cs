using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public event ApplyBuffDelegate onApplyBuff;

        public void ApplyBuff(int dataId, BuffType type, short level)
        {
            if (IsDead() || !IsServer)
                return;

            int buffIndex = this.IndexOfBuff(dataId, type);
            if (buffIndex >= 0)
                buffs.RemoveAt(buffIndex);

            CharacterBuff newBuff = CharacterBuff.Create(type, dataId, level);
            newBuff.Apply();
            buffs.Add(newBuff);

            if (newBuff.GetDuration() <= 0f)
            {
                int tempAmount = 0;
                // Damage over time
                DamageElement damageElement;
                MinMaxFloat damageAmount;
                float tempReceivingDamage;
                foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in newBuff.GetDamageOverTimes())
                {
                    damageElement = damageOverTime.Key;
                    damageAmount = damageOverTime.Value;
                    tempReceivingDamage = (float)damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (tempReceivingDamage > 0f)
                        tempAmount += (int)tempReceivingDamage;
                }
                CurrentHp -= tempAmount;
                // Hp recovery
                tempAmount = newBuff.GetRecoveryHp();
                if (tempAmount != 0)
                {
                    CurrentHp += tempAmount;
                    RequestCombatAmount(CombatAmountType.HpRecovery, tempAmount);
                }
                // Mp recovery
                tempAmount = newBuff.GetRecoveryMp();
                if (tempAmount != 0)
                {
                    CurrentMp += tempAmount;
                    RequestCombatAmount(CombatAmountType.MpRecovery, tempAmount);
                }
                // Stamina recovery
                tempAmount = newBuff.GetRecoveryStamina();
                if (tempAmount != 0)
                {
                    CurrentStamina += tempAmount;
                    RequestCombatAmount(CombatAmountType.StaminaRecovery, tempAmount);
                }
                // Food recovery
                tempAmount = newBuff.GetRecoveryFood();
                if (tempAmount != 0)
                {
                    CurrentFood += tempAmount;
                    RequestCombatAmount(CombatAmountType.FoodRecovery, tempAmount);
                }
                // Water recovery
                tempAmount = newBuff.GetRecoveryWater();
                if (tempAmount != 0)
                {
                    CurrentWater += tempAmount;
                    RequestCombatAmount(CombatAmountType.WaterRecovery, tempAmount);
                }
            }
            ValidateRecovery();
            if (onApplyBuff != null)
                onApplyBuff.Invoke(dataId, type, level);
        }

        protected virtual void ApplyGuildSkillBuff(GuildSkill guildSkill, short level)
        {
            if (IsDead() || !IsServer || guildSkill == null || level <= 0)
                return;
            ApplyBuff(guildSkill.DataId, BuffType.GuildSkillBuff, level);
        }
    }
}
