using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CharacterSkillAndBuffSystem : ComponentSystem
{
    public const float SKILL_BUFF_UPDATE_DURATION = 0.5f;

    struct Components
    {
        public CharacterSkillAndBuffData skillAndBuffData;
        public CharacterRecoveryData recoveryData;
    }

    protected override void OnUpdate()
    {
        var deltaTime = Time.unscaledDeltaTime;
        foreach (var comp in GetEntities<Components>())
        {
            UpdateSkillAndBuff(deltaTime, comp.skillAndBuffData, comp.recoveryData, comp.skillAndBuffData.CacheCharacterEntity);
        }
    }

    protected static void UpdateSkillAndBuff(float deltaTime, CharacterSkillAndBuffData skillAndBuffData, CharacterRecoveryData recoveryData, BaseCharacterEntity characterEntity)
    {
        if (characterEntity.isRecaching || characterEntity.CurrentHp <= 0 || !characterEntity.IsServer)
            return;

        skillAndBuffData.skillBuffUpdateDeltaTime += deltaTime;
        if (skillAndBuffData.skillBuffUpdateDeltaTime >= SKILL_BUFF_UPDATE_DURATION)
        {
            var count = characterEntity.skills.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                var skill = characterEntity.skills[i];
                if (skill.ShouldUpdate())
                {
                    skill.Update(skillAndBuffData.skillBuffUpdateDeltaTime);
                    characterEntity.skills[i] = skill;
                }
            }
            count = characterEntity.buffs.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                var buff = characterEntity.buffs[i];
                var duration = buff.GetDuration();
                if (buff.ShouldRemove())
                    characterEntity.buffs.RemoveAt(i);
                else
                {
                    buff.Update(skillAndBuffData.skillBuffUpdateDeltaTime);
                    characterEntity.buffs[i] = buff;
                }
                recoveryData.recoveryingHp += duration > 0f ? (float)buff.GetBuffRecoveryHp() / duration * skillAndBuffData.skillBuffUpdateDeltaTime : 0f;
                recoveryData.recoveryingMp += duration > 0f ? (float)buff.GetBuffRecoveryMp() / duration * skillAndBuffData.skillBuffUpdateDeltaTime : 0f;
                recoveryData.recoveryingStamina += duration > 0f ? (float)buff.GetBuffRecoveryStamina() / duration * skillAndBuffData.skillBuffUpdateDeltaTime : 0f;
                recoveryData.recoveryingFood += duration > 0f ? (float)buff.GetBuffRecoveryFood() / duration * skillAndBuffData.skillBuffUpdateDeltaTime : 0f;
                recoveryData.recoveryingWater += duration > 0f ? (float)buff.GetBuffRecoveryWater() / duration * skillAndBuffData.skillBuffUpdateDeltaTime : 0f;
            }
            skillAndBuffData.skillBuffUpdateDeltaTime = 0;
        }
    }
}
