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
        public float skillBuffUpdateDeltaTime;
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

            component.skillBuffUpdateDeltaTime += deltaTime;
            if (component.skillBuffUpdateDeltaTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                var count = characterEntity.Summons.Count;
                for (var i = count - 1; i >= 0; --i)
                {
                    var summon = characterEntity.Summons[i];
                    if (summon.ShouldRemove())
                    {
                        summon.UnSummon(characterEntity);
                        characterEntity.Summons.RemoveAt(i);
                    }
                    else
                    {
                        if (summon.type == SummonType.Skill)
                        {
                            summon.Update(component.skillBuffUpdateDeltaTime);
                            characterEntity.Summons[i] = summon;
                        }
                    }
                }
                count = characterEntity.SkillUsages.Count;
                for (var i = count - 1; i >= 0; --i)
                {
                    var skillUsage = characterEntity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                        characterEntity.SkillUsages.RemoveAt(i);
                    else
                    {
                        skillUsage.Update(component.skillBuffUpdateDeltaTime);
                        characterEntity.SkillUsages[i] = skillUsage;
                    }
                }
                count = characterEntity.NonEquipItems.Count;
                for (var i = count - 1; i >= 0; --i)
                {
                    var nonEquipItem = characterEntity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove())
                        characterEntity.NonEquipItems.RemoveAt(i);
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(component.skillBuffUpdateDeltaTime);
                            characterEntity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                count = characterEntity.Buffs.Count;
                for (var i = count - 1; i >= 0; --i)
                {
                    var buff = characterEntity.Buffs[i];
                    var duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                        characterEntity.Buffs.RemoveAt(i);
                    else
                    {
                        buff.Update(component.skillBuffUpdateDeltaTime);
                        characterEntity.Buffs[i] = buff;
                    }
                    recoveryData.recoveryingHp += duration > 0f ? (float)buff.GetBuffRecoveryHp() / duration * component.skillBuffUpdateDeltaTime : 0f;
                    recoveryData.recoveryingMp += duration > 0f ? (float)buff.GetBuffRecoveryMp() / duration * component.skillBuffUpdateDeltaTime : 0f;
                    recoveryData.recoveryingStamina += duration > 0f ? (float)buff.GetBuffRecoveryStamina() / duration * component.skillBuffUpdateDeltaTime : 0f;
                    recoveryData.recoveryingFood += duration > 0f ? (float)buff.GetBuffRecoveryFood() / duration * component.skillBuffUpdateDeltaTime : 0f;
                    recoveryData.recoveryingWater += duration > 0f ? (float)buff.GetBuffRecoveryWater() / duration * component.skillBuffUpdateDeltaTime : 0f;
                }
                component.skillBuffUpdateDeltaTime = 0;
            }
        }
    }
}
