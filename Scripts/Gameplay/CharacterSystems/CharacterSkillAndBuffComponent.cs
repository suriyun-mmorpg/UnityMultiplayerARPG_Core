using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterSkillAndBuffComponent : BaseCharacterComponent
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 0.5f;

        #region Buff System Data
        [HideInInspector, System.NonSerialized]
        public float updatingTime;
        #endregion

        private CharacterRecoveryComponent cacheCharacterRecovery;
        public CharacterRecoveryComponent CacheCharacterRecovery
        {
            get
            {
                if (cacheCharacterRecovery == null)
                    cacheCharacterRecovery = GetComponent<CharacterRecoveryComponent>();
                return cacheCharacterRecovery;
            }
        }

        protected void Update()
        {
            UpdateSkillAndBuff(Time.unscaledDeltaTime, this, CacheCharacterRecovery, CacheEntity);
        }

        protected static void UpdateSkillAndBuff(float deltaTime, CharacterSkillAndBuffComponent component, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
        {
            if (characterEntity.IsRecaching || characterEntity.IsDead() || !characterEntity.IsServer)
                return;

            component.updatingTime += deltaTime;
            if (component.updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                // Removing summons if it should
                int count = characterEntity.Summons.Count;
                CharacterSummon summon;
                for (int i = count - 1; i >= 0; --i)
                {
                    summon = characterEntity.Summons[i];
                    if (summon.ShouldRemove())
                    {
                        characterEntity.Summons.RemoveAt(i);
                        summon.UnSummon(characterEntity);
                    }
                    else
                    {
                        summon.Update(component.updatingTime);
                        characterEntity.Summons[i] = summon;
                    }
                }
                // Removing skill usages if it should
                count = characterEntity.SkillUsages.Count;
                CharacterSkillUsage skillUsage;
                for (int i = count - 1; i >= 0; --i)
                {
                    skillUsage = characterEntity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                        characterEntity.SkillUsages.RemoveAt(i);
                    else
                    {
                        skillUsage.Update(component.updatingTime);
                        characterEntity.SkillUsages[i] = skillUsage;
                    }
                }
                // Removing non-equip items if it should
                count = characterEntity.NonEquipItems.Count;
                bool hasRemovedItem = false;
                CharacterItem nonEquipItem;
                for (int i = count - 1; i >= 0; --i)
                {
                    nonEquipItem = characterEntity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove())
                    {
                        characterEntity.NonEquipItems.RemoveAt(i);
                        hasRemovedItem = true;
                    }
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(component.updatingTime);
                            characterEntity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                if (hasRemovedItem)
                    characterEntity.FillEmptySlots();
                // Removing buffs if it should
                count = characterEntity.Buffs.Count;
                CharacterBuff buff;
                for (int i = count - 1; i >= 0; --i)
                {
                    buff = characterEntity.Buffs[i];
                    float duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                        characterEntity.Buffs.RemoveAt(i);
                    else
                    {
                        buff.Update(component.updatingTime);
                        characterEntity.Buffs[i] = buff;
                    }
                    // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                    if (duration > 0f)
                    {
                        float tempAmount = 0f;
                        // Damage over time
                        DamageElement damageElement;
                        MinMaxFloat damageAmount;
                        float damage;
                        foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in buff.GetDamageOverTimes())
                        {
                            damageElement = damageOverTime.Key;
                            damageAmount = damageOverTime.Value;
                            damage = damageElement.GetDamageReducedByResistance(characterEntity, damageAmount.Random());
                            if (damage > 0f)
                                tempAmount += damage / duration * component.updatingTime;
                        }
                        recoveryData.decreasingHp += tempAmount;
                        // Hp recovery
                        tempAmount = (float)buff.GetRecoveryHp() / duration * component.updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingHp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingHp += tempAmount;
                        // Mp recovery
                        tempAmount = (float)buff.GetRecoveryMp() / duration * component.updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingMp += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingMp += tempAmount;
                        // Stamina recovery
                        tempAmount = (float)buff.GetRecoveryStamina() / duration * component.updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingStamina += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingStamina += tempAmount;
                        // Food recovery
                        tempAmount = (float)buff.GetRecoveryFood() / duration * component.updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingFood += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingFood += tempAmount;
                        // Water recovery
                        tempAmount = (float)buff.GetRecoveryWater() / duration * component.updatingTime;
                        if (tempAmount > 0)
                            recoveryData.recoveryingWater += tempAmount;
                        else if (tempAmount < 0)
                            recoveryData.decreasingWater += tempAmount;
                    }
                }
                component.updatingTime = 0;
            }
        }
    }
}
