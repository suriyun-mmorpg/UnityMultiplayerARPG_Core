using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public event ApplyBuffDelegate onApplyBuff;

        public void ApplyBuff(int dataId, BuffType type, short level, IGameEntity buffApplier)
        {
            if (IsDead() || !IsServer)
                return;

            int buffIndex = this.IndexOfBuff(dataId, type);
            if (buffIndex >= 0)
                buffs.RemoveAt(buffIndex);

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
                    tempReceivingDamage = (float)damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (tempReceivingDamage > 0f)
                        tempAmount += (int)tempReceivingDamage;
                }
                recoveryData.decreasingHp += tempAmount;
                // Hp recovery
                tempAmount = newBuff.GetRecoveryHp();
                if (tempAmount > 0)
                    recoveryData.recoveryingHp += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingHp += tempAmount;
                // Mp recovery
                tempAmount = newBuff.GetRecoveryMp();
                if (tempAmount > 0)
                    recoveryData.recoveryingMp += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingMp += tempAmount;
                // Stamina recovery
                tempAmount = newBuff.GetRecoveryStamina();
                if (tempAmount > 0)
                    recoveryData.recoveryingStamina += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingStamina += tempAmount;
                // Food recovery
                tempAmount = newBuff.GetRecoveryFood();
                if (tempAmount > 0)
                    recoveryData.recoveryingFood += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingFood += tempAmount;
                // Water recovery
                tempAmount = newBuff.GetRecoveryWater();
                if (tempAmount > 0)
                    recoveryData.recoveryingWater += tempAmount;
                else if (tempAmount < 0)
                    recoveryData.decreasingWater += tempAmount;

                recoveryData = recoveryData.Apply(this, buffApplier);
                ValidateRecovery(buffApplier);
            }

            if (onApplyBuff != null)
                onApplyBuff.Invoke(dataId, type, level);
        }
    }
}
