using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterSkillAndBuffComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 1f;

        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData nonApplierRecoveryBuff;
        private Dictionary<string, CharacterRecoveryData> recoveryBuffs;

        public override void EntityStart()
        {
            nonApplierRecoveryBuff = new CharacterRecoveryData(Entity, null);
            recoveryBuffs = new Dictionary<string, CharacterRecoveryData>();
        }

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;
            updatingTime += deltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

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
                    {
                        Entity.SkillUsages.RemoveAt(i);
                    }
                    else
                    {
                        skillUsage.Update(updatingTime);
                        Entity.SkillUsages[i] = skillUsage;
                    }
                }
                // Removing buffs if it should
                count = Entity.Buffs.Count;
                CharacterBuff buff;
                float duration;
                for (int i = count - 1; i >= 0; --i)
                {
                    buff = Entity.Buffs[i];
                    duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                    {
                        Entity.Buffs.RemoveAt(i);
                    }
                    else
                    {
                        buff.Update(updatingTime);
                        Entity.Buffs[i] = buff;
                    }
                    // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                    if (duration > 0f)
                    {
                        if (buff.BuffApplier != null && !recoveryBuffs.ContainsKey(buff.BuffApplier.id))
                            recoveryBuffs.Add(buff.BuffApplier.id, new CharacterRecoveryData(Entity, buff.BuffApplier));

                        CharacterRecoveryData recoveryData = buff.BuffApplier != null ? recoveryBuffs[buff.BuffApplier.id] : nonApplierRecoveryBuff;
                        // Damage over time
                        foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in buff.GetDamageOverTimes())
                        {
                            recoveryData.IncreaseDamageOverTimes(damageOverTime.Key, damageOverTime.Value / duration * updatingTime);
                        }
                        float tempAmount;
                        // Hp recovery
                        tempAmount = (float)buff.GetRecoveryHp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.RecoveryingHp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.DecreasingHp += -tempAmount;
                        // Mp recovery
                        tempAmount = (float)buff.GetRecoveryMp() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.RecoveryingMp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.DecreasingMp += -tempAmount;
                        // Stamina recovery
                        tempAmount = (float)buff.GetRecoveryStamina() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.RecoveryingStamina += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.DecreasingStamina += -tempAmount;
                        // Food recovery
                        tempAmount = (float)buff.GetRecoveryFood() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.RecoveryingFood += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.DecreasingFood += -tempAmount;
                        // Water recovery
                        tempAmount = (float)buff.GetRecoveryWater() / duration * updatingTime;
                        if (tempAmount > 0)
                            recoveryData.RecoveryingWater += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.DecreasingWater += -tempAmount;
                        // Apply
                        recoveryData.Apply();
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
