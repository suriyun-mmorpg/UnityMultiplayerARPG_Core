using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterSkillAndBuffComponent : BaseCharacterComponent
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 0.5f;

        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData nonApplierRecoveryBuff;
        private readonly Dictionary<IGameEntity, CharacterRecoveryData> recoveryBuffs = new Dictionary<IGameEntity, CharacterRecoveryData>();

        protected void Update()
        {
            if (!CacheEntity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (CacheEntity.IsRecaching || CacheEntity.IsDead())
                return;

            updatingTime += deltaTime;
            if (updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                // Removing summons if it should
                int count = CacheEntity.Summons.Count;
                CharacterSummon summon;
                for (int i = count - 1; i >= 0; --i)
                {
                    summon = CacheEntity.Summons[i];
                    if (summon.ShouldRemove())
                    {
                        CacheEntity.Summons.RemoveAt(i);
                        summon.UnSummon(CacheEntity);
                    }
                    else
                    {
                        summon.Update(updatingTime);
                        CacheEntity.Summons[i] = summon;
                    }
                }
                // Removing skill usages if it should
                count = CacheEntity.SkillUsages.Count;
                CharacterSkillUsage skillUsage;
                for (int i = count - 1; i >= 0; --i)
                {
                    skillUsage = CacheEntity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                        CacheEntity.SkillUsages.RemoveAt(i);
                    else
                    {
                        skillUsage.Update(updatingTime);
                        CacheEntity.SkillUsages[i] = skillUsage;
                    }
                }
                // Removing non-equip items if it should
                count = CacheEntity.NonEquipItems.Count;
                bool hasRemovedItem = false;
                CharacterItem nonEquipItem;
                for (int i = count - 1; i >= 0; --i)
                {
                    nonEquipItem = CacheEntity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove())
                    {
                        CacheEntity.NonEquipItems.RemoveAt(i);
                        hasRemovedItem = true;
                    }
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(updatingTime);
                            CacheEntity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                if (hasRemovedItem)
                    CacheEntity.FillEmptySlots();
                // Removing buffs if it should
                count = CacheEntity.Buffs.Count;
                CharacterBuff buff;
                for (int i = count - 1; i >= 0; --i)
                {
                    buff = CacheEntity.Buffs[i];
                    float duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                        CacheEntity.Buffs.RemoveAt(i);
                    else
                    {
                        buff.Update(updatingTime);
                        CacheEntity.Buffs[i] = buff;
                    }
                    // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                    if (duration > 0f)
                    {
                        if (buff.BuffApplier != null && !recoveryBuffs.ContainsKey(buff.BuffApplier))
                            recoveryBuffs.Add(buff.BuffApplier, default(CharacterRecoveryData));

                        CharacterRecoveryData recoveryData = buff.BuffApplier != null ? recoveryBuffs[buff.BuffApplier] : nonApplierRecoveryBuff;
                        float tempAmount = 0f;
                        // Damage over time
                        DamageElement damageElement;
                        MinMaxFloat damageAmount;
                        float damage;
                        foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in buff.GetDamageOverTimes())
                        {
                            damageElement = damageOverTime.Key;
                            damageAmount = damageOverTime.Value;
                            damage = damageElement.GetDamageReducedByResistance(CacheEntity, damageAmount.Random());
                            if (damage > 0f)
                                tempAmount += damage / duration * updatingTime;
                        }
                        recoveryData.decreasingHp += tempAmount;
                        // Hp recovery
                        tempAmount = (float)buff.GetRecoveryHp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingHp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingHp += tempAmount;
                        // Mp recovery
                        tempAmount = (float)buff.GetRecoveryMp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingMp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingMp += tempAmount;
                        // Stamina recovery
                        tempAmount = (float)buff.GetRecoveryStamina() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingStamina += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingStamina += tempAmount;
                        // Food recovery
                        tempAmount = (float)buff.GetRecoveryFood() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingFood += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingFood += tempAmount;
                        // Water recovery
                        tempAmount = (float)buff.GetRecoveryWater() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingWater += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingWater += tempAmount;

                        recoveryData = recoveryData.Apply(CacheEntity, buff.BuffApplier);

                        if (buff.BuffApplier != null)
                            recoveryBuffs[buff.BuffApplier] = recoveryData;
                        else
                            nonApplierRecoveryBuff = recoveryData;

                        CacheEntity.ValidateRecovery(buff.BuffApplier);
                    }
                }
                updatingTime = 0;
            }
        }
    }
}
