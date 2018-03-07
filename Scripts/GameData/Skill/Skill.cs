using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public SkillLevel[] requireSkillLevels;
    public int maxLevel;
    public float baseConsumeMp;
    public float consumeMpIncreaseEachLevel;
    public float baseCoolDownDuration;
    public float coolDownDurationIncreaseEachLevel;
    public ActionAnimation castAnimation;
    [Header("Attack")]
    public bool isAttack;
    public DamageAttribute[] damageAttributes;
    public Damage damage;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    [Header("Buffs")]
    public bool isBuff;
    [Tooltip("`buffDistance` = 0, will buff only caster")]
    [Range(0f, 100f)]
    public float buffDistance;
    [Tooltip("If buff duration less than or equals to 0, buff stats won't applied")]
    public float baseBuffDuration;
    public float buffDurationIncreaseEachLevel;
    public float baseRecoveryHp;
    public float recoveryHpIncreaseEachLevel;
    public float baseRecoveryMp;
    public float recoveryMpIncreaseEachLevel;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;

    private Dictionary<string, DamageEffectivenessAttribute> tempEffectivenessAttributes;
    public Dictionary<string, DamageEffectivenessAttribute> TempEffectivenessAttributes
    {
        get
        {
            if (tempEffectivenessAttributes == null)
            {
                tempEffectivenessAttributes = new Dictionary<string, DamageEffectivenessAttribute>();
                foreach (var effectivenessAttribute in effectivenessAttributes)
                {
                    if (effectivenessAttribute.attribute == null)
                        continue;
                    tempEffectivenessAttributes[effectivenessAttribute.attribute.Id] = effectivenessAttribute;
                }
            }
            return tempEffectivenessAttributes;
        }
    }
}

[System.Serializable]
public class SkillLevel
{
    public Skill skill;
    public int level;
}
