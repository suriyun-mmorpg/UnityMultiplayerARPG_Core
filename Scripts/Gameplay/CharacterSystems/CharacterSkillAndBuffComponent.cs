using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterSkillAndBuffComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 0.5f;

        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData nonApplierRecoveryBuff;
        private readonly Dictionary<string, CharacterRecoveryData> recoveryBuffs = new Dictionary<string, CharacterRecoveryData>();

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

            updatingTime += deltaTime;
            if (updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                // Removing summons if it should
                int count = Entity.Summons.Count;
                CharacterSummon summon;
                for (int i = count - 1; i >= 0; --i)
                {
                    summon = Entity.Summons[i];
                    if (summon.ShouldRemove())
                    {
                        Entity.Summons.RemoveAt(i);
                        summon.UnSummon(Entity);
                    }
                    else
                    {
                        summon.Update(updatingTime);
                        Entity.Summons[i] = summon;
                    }
                }
                // Removing skill usages if it should
                count = Entity.SkillUsages.Count;
                CharacterSkillUsage skillUsage;
                for (int i = count - 1; i >= 0; --i)
                {
                    skillUsage = Entity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                        Entity.SkillUsages.RemoveAt(i);
                    else
                    {
                        skillUsage.Update(updatingTime);
                        Entity.SkillUsages[i] = skillUsage;
                    }
                }
                // Removing non-equip items if it should
                count = Entity.NonEquipItems.Count;
                bool hasRemovedItem = false;
                CharacterItem nonEquipItem;
                for (int i = count - 1; i >= 0; --i)
                {
                    nonEquipItem = Entity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove())
                    {
                        if (CurrentGameInstance.IsLimitInventorySlot)
                            Entity.NonEquipItems[i] = CharacterItem.Empty;
                        else
                            Entity.NonEquipItems.RemoveAt(i);
                        hasRemovedItem = true;
                    }
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(updatingTime);
                            Entity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                if (hasRemovedItem)
                    Entity.FillEmptySlots();
                // Removing buffs if it should
                count = Entity.Buffs.Count;
                CharacterBuff buff;
                float duration;
                for (int i = count - 1; i >= 0; --i)
                {
                    buff = Entity.Buffs[i];
                    duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                        Entity.Buffs.RemoveAt(i);
                    else
                    {
                        buff.Update(updatingTime);
                        Entity.Buffs[i] = buff;
                    }
                    // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                    if (duration > 0f)
                    {
                        if (buff.BuffApplier != null && !recoveryBuffs.ContainsKey(buff.BuffApplier.id))
                            recoveryBuffs.Add(buff.BuffApplier.id, default(CharacterRecoveryData));

                        CharacterRecoveryData recoveryData = buff.BuffApplier != null ? recoveryBuffs[buff.BuffApplier.id] : nonApplierRecoveryBuff;
                        float tempAmount = 0f;
                        // Damage over time
                        DamageElement damageElement;
                        MinMaxFloat damageAmount;
                        float damage;
                        foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in buff.GetDamageOverTimes())
                        {
                            damageElement = damageOverTime.Key;
                            damageAmount = damageOverTime.Value;
                            damage = damageElement.GetDamageReducedByResistance(Entity.GetCaches().Resistances, Entity.GetCaches().Armors, damageAmount.Random(Random.Range(0, 255)));
                            if (damage > 0f)
                                tempAmount += damage / duration * updatingTime;
                        }
                        recoveryData.decreasingHp += tempAmount;
                        // Hp recovery
                        tempAmount = (float)buff.GetRecoveryHp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingHp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingHp += -tempAmount;
                        // Mp recovery
                        tempAmount = (float)buff.GetRecoveryMp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingMp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingMp += -tempAmount;
                        // Stamina recovery
                        tempAmount = (float)buff.GetRecoveryStamina() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingStamina += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingStamina += -tempAmount;
                        // Food recovery
                        tempAmount = (float)buff.GetRecoveryFood() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingFood += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingFood += -tempAmount;
                        // Water recovery
                        tempAmount = (float)buff.GetRecoveryWater() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingWater += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingWater += -tempAmount;

                        recoveryData = recoveryData.Apply(Entity, buff.BuffApplier);

                        if (buff.BuffApplier != null)
                            recoveryBuffs[buff.BuffApplier.id] = recoveryData;
                        else
                            nonApplierRecoveryBuff = recoveryData;

                        // Causer is the entity whom applied buffs to this entity
                        Entity.ValidateRecovery(buff.BuffApplier);
                    }
                    // Don't update next buffs if character dead
                    if (Entity.IsDead())
                        break;
                }
                updatingTime = 0;
            }
        }
    }
}
