using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterSkillAndBuffComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 1f;
        public const string KEY_VEHICLE_BUFF = "<VEHICLE_BUFF>";

        private float _updatingTime;
        private float _deltaTime;
        private Dictionary<string, CharacterRecoveryData> _recoveryBuffs;

        public override void EntityStart()
        {
            _recoveryBuffs = new Dictionary<string, CharacterRecoveryData>();
        }

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            _deltaTime = Time.unscaledDeltaTime;
            _updatingTime += _deltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

            if (_updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                float tempDuration;
                int tempCount;
                if (Entity.PassengingVehicleEntity != null)
                {
                    tempDuration = Entity.PassengingVehicleEntity.GetBuff().GetDuration();
                    if (tempDuration > 0f)
                    {
                        CharacterRecoveryData recoveryData;
                        if (!_recoveryBuffs.TryGetValue(KEY_VEHICLE_BUFF, out recoveryData))
                        {
                            recoveryData = new CharacterRecoveryData(Entity);
                            recoveryData.SetupByBuff(null, Entity.PassengingVehicleEntity.GetBuff());
                            _recoveryBuffs.Add(KEY_VEHICLE_BUFF, recoveryData);
                        }
                        recoveryData.Apply(1 / tempDuration * _updatingTime);
                    }
                }
                // Removing summons if it should
                if (!Entity.IsDead())
                {
                    tempCount = Entity.Summons.Count;
                    CharacterSummon summon;
                    for (int i = tempCount - 1; i >= 0; --i)
                    {
                        summon = Entity.Summons[i];
                        tempDuration = summon.GetBuff().GetDuration();
                        if (summon.ShouldRemove())
                        {
                            _recoveryBuffs.Remove(summon.id);
                            Entity.Summons.RemoveAt(i);
                            summon.UnSummon(Entity);
                        }
                        else
                        {
                            summon.Update(_updatingTime);
                            Entity.Summons[i] = summon;
                        }
                        // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                        if (tempDuration > 0f)
                        {
                            CharacterRecoveryData recoveryData;
                            if (!_recoveryBuffs.TryGetValue(summon.id, out recoveryData))
                            {
                                recoveryData = new CharacterRecoveryData(Entity);
                                recoveryData.SetupByBuff(null, summon.GetBuff());
                                _recoveryBuffs.Add(summon.id, recoveryData);
                            }
                            recoveryData.Apply(1 / tempDuration * _updatingTime);
                        }
                        // Don't update next buffs if character dead
                        if (Entity.IsDead())
                            break;
                    }
                }
                // Removing buffs if it should
                if (!Entity.IsDead())
                {
                    tempCount = Entity.Buffs.Count;
                    CharacterBuff buff;
                    for (int i = tempCount - 1; i >= 0; --i)
                    {
                        buff = Entity.Buffs[i];
                        tempDuration = buff.GetBuff().GetDuration();
                        if (buff.ShouldRemove())
                        {
                            _recoveryBuffs.Remove(buff.id);
                            Entity.Buffs.RemoveAt(i);
                        }
                        else
                        {
                            buff.Update(_updatingTime);
                            Entity.Buffs[i] = buff;
                        }
                        // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                        if (tempDuration > 0f)
                        {
                            CharacterRecoveryData recoveryData;
                            if (!_recoveryBuffs.TryGetValue(buff.id, out recoveryData))
                            {
                                recoveryData = new CharacterRecoveryData(Entity);
                                recoveryData.SetupByBuff(buff, buff.GetBuff());
                                _recoveryBuffs.Add(buff.id, recoveryData);
                            }
                            recoveryData.Apply(1 / tempDuration * _updatingTime);
                        }
                        // Don't update next buffs if character dead
                        if (Entity.IsDead())
                            break;
                    }
                }
                // Removing skill usages if it should
                if (!Entity.IsDead())
                {
                    tempCount = Entity.SkillUsages.Count;
                    CharacterSkillUsage skillUsage;
                    for (int i = tempCount - 1; i >= 0; --i)
                    {
                        skillUsage = Entity.SkillUsages[i];
                        if (skillUsage.ShouldRemove())
                        {
                            Entity.SkillUsages.RemoveAt(i);
                        }
                        else
                        {
                            skillUsage.Update(_updatingTime);
                            Entity.SkillUsages[i] = skillUsage;
                        }
                    }
                }
                _updatingTime = 0;
            }
        }

        public void OnAttack()
        {
            CharacterDataCache cache = Entity.CachedData;
            if (!cache.HavingChanceToRemoveBuffWhenAttack)
                return;
            bool stillHavingChance = false;
            CharacterBuff buff;
            float chance;
            for (int i = Entity.Buffs.Count - 1; i >= 0; --i)
            {
                buff = Entity.Buffs[i];
                chance = buff.GetBuff().GetRemoveBuffWhenAttackChance();
                if (chance > 0)
                {
                    if (Random.value > chance)
                    {
                        stillHavingChance = true;
                        continue;
                    }
                    Entity.Buffs.RemoveAt(i);
                }
            }
            if (!stillHavingChance)
                cache.ClearChanceToRemoveBuffWhenAttack();
        }

        public void OnAttacked()
        {
            CharacterDataCache cache = Entity.CachedData;
            if (!cache.HavingChanceToRemoveBuffWhenAttacked)
                return;
            bool stillHavingChance = false;
            CharacterBuff buff;
            float chance;
            for (int i = Entity.Buffs.Count - 1; i >= 0; --i)
            {
                buff = Entity.Buffs[i];
                chance = buff.GetBuff().GetRemoveBuffWhenAttackedChance();
                if (chance > 0)
                {
                    if (Random.value > chance)
                    {
                        stillHavingChance = true;
                        continue;
                    }
                    Entity.Buffs.RemoveAt(i);
                }
            }
            if (!stillHavingChance)
                cache.ClearChanceToRemoveBuffWhenAttacked();
        }

        public void OnUseSkill()
        {
            CharacterDataCache cache = Entity.CachedData;
            if (!cache.HavingChanceToRemoveBuffWhenUseSkill)
                return;
            bool stillHavingChance = false;
            CharacterBuff buff;
            float chance;
            for (int i = Entity.Buffs.Count - 1; i >= 0; --i)
            {
                buff = Entity.Buffs[i];
                chance = buff.GetBuff().GetRemoveBuffWhenUseSkillChance();
                if (chance > 0)
                {
                    if (Random.value > chance)
                    {
                        stillHavingChance = true;
                        continue;
                    }
                    Entity.Buffs.RemoveAt(i);
                }
            }
            if (!stillHavingChance)
                cache.ClearChanceToRemoveBuffWhenUseSkill();
        }

        public void OnUseItem()
        {
            CharacterDataCache cache = Entity.CachedData;
            if (!cache.HavingChanceToRemoveBuffWhenUseItem)
                return;
            bool stillHavingChance = false;
            CharacterBuff buff;
            float chance;
            for (int i = Entity.Buffs.Count - 1; i >= 0; --i)
            {
                buff = Entity.Buffs[i];
                chance = buff.GetBuff().GetRemoveBuffWhenUseItemChance();
                if (chance > 0)
                {
                    if (Random.value > chance)
                    {
                        stillHavingChance = true;
                        continue;
                    }
                    Entity.Buffs.RemoveAt(i);
                }
            }
            if (!stillHavingChance)
                cache.ClearChanceToRemoveBuffWhenUseItem();
        }

        public void OnPickupItem()
        {
            CharacterDataCache cache = Entity.CachedData;
            if (!cache.HavingChanceToRemoveBuffWhenPickupItem)
                return;
            bool stillHavingChance = false;
            CharacterBuff buff;
            float chance;
            for (int i = Entity.Buffs.Count - 1; i >= 0; --i)
            {
                buff = Entity.Buffs[i];
                chance = buff.GetBuff().GetRemoveBuffWhenPickupItemChance();
                if (chance > 0)
                {
                    if (Random.value > chance)
                    {
                        stillHavingChance = true;
                        continue;
                    }
                    Entity.Buffs.RemoveAt(i);
                }
            }
            if (!stillHavingChance)
                cache.ClearChanceToRemoveBuffWhenPickupItem();
        }
    }
}
