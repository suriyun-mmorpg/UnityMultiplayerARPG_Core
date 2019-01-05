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
            UpdateSkillAndBuff(Time.unscaledDeltaTime, this, CacheCharacterRecovery, CacheCharacterEntity);
        }

        protected static void UpdateSkillAndBuff(float deltaTime, CharacterSkillAndBuffComponent component, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
        {
            if (characterEntity.isRecaching || characterEntity.IsDead() || !characterEntity.IsServer)
                return;

            component.updatingTime += deltaTime;
            if (component.updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                int count = characterEntity.Summons.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    CharacterSummon summon = characterEntity.Summons[i];
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
                count = characterEntity.SkillUsages.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    CharacterSkillUsage skillUsage = characterEntity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                        characterEntity.SkillUsages.RemoveAt(i);
                    else
                    {
                        skillUsage.Update(component.updatingTime);
                        characterEntity.SkillUsages[i] = skillUsage;
                    }
                }
                count = characterEntity.NonEquipItems.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    CharacterItem nonEquipItem = characterEntity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove())
                        characterEntity.NonEquipItems.RemoveAt(i);
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(component.updatingTime);
                            characterEntity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                count = characterEntity.Buffs.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    CharacterBuff buff = characterEntity.Buffs[i];
                    float duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                        characterEntity.Buffs.RemoveAt(i);
                    else
                    {
                        buff.Update(component.updatingTime);
                        characterEntity.Buffs[i] = buff;
                    }
                    recoveryData.recoveryingHp += duration > 0f ? (float)buff.GetBuffRecoveryHp() / duration * component.updatingTime : 0f;
                    recoveryData.recoveryingMp += duration > 0f ? (float)buff.GetBuffRecoveryMp() / duration * component.updatingTime : 0f;
                    recoveryData.recoveryingStamina += duration > 0f ? (float)buff.GetBuffRecoveryStamina() / duration * component.updatingTime : 0f;
                    recoveryData.recoveryingFood += duration > 0f ? (float)buff.GetBuffRecoveryFood() / duration * component.updatingTime : 0f;
                    recoveryData.recoveryingWater += duration > 0f ? (float)buff.GetBuffRecoveryWater() / duration * component.updatingTime : 0f;
                }
                component.updatingTime = 0;
            }
        }
    }
}
