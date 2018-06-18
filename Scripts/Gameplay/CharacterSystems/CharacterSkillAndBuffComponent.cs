using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var deltaTime = Time.unscaledDeltaTime;
        UpdateSkillAndBuff(deltaTime, this, CacheCharacterRecovery, CacheCharacterEntity);
    }

    protected static void UpdateSkillAndBuff(float deltaTime, CharacterSkillAndBuffComponent skillAndBuffData, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
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
