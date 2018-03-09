using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillAttackType
{
    None,
    PureSkillDamage,
    WeaponDamageInflict,
}

public enum SkillBuffType
{
    None,
    BuffToUser,
}

[System.Serializable]
public class SkillBuff
{
    [Header("Duration")]
    [Tooltip("If buff duration less than or equals to 0, buff stats won't applied")]
    public float baseDuration;
    public float durationIncreaseEachLevel;
    [Header("Hp recovery")]
    public float baseRecoveryHp;
    public float recoveryHpIncreaseEachLevel;
    [Header("Mp recovery")]
    public float baseRecoveryMp;
    public float recoveryMpIncreaseEachLevel;
    [Header("Add Attributes")]
    public CharacterAttributeIncremental[] increaseAttributes;
    [Header("Add Resistances")]
    public CharacterResistanceIncremental[] increaseResistances;
    [Header("Add Stats")]
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
}

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public ActionAnimation castAnimation;
    public int maxLevel;

    [Header("Consume Mp")]
    public float baseConsumeMp;
    public float consumeMpIncreaseEachLevel;

    [Header("Cool Down")]
    public float baseCoolDownDuration;
    public float coolDownDurationIncreaseEachLevel;

    [Header("Requirements")]
    public int requireCharacterLevel;
    public SkillLevel[] requireSkillLevels;

    [Header("Attack")]
    public SkillAttackType skillAttackType;

    [Header("Attack As Pure Skill Damage")]
    public DamageAttribute baseDamageAttribute;
    public DamageAttribute[] additionalDamageAttributes;
    public Damage damage;
    public DamageEffectivenessAttribute[] effectivenessAttributes;

    [Header("Attack As Weapon Damage Inflict")]
    public float baseInflictPercentage;
    public float inflictPercentageIncreaseEachLevel;
    public DamageAttribute[] inflictDamageAttributes;

    [Header("Attack Debuff")]
    public bool isDebuff;
    public SkillBuff debuff;

    [Header("Buffs")]
    public SkillBuffType skillBuffType;
    public SkillBuff buff;

    private Dictionary<string, float> tempEffectivenessAttributes;
    public Dictionary<string, float> TempEffectivenessAttributes
    {
        get
        {
            if (tempEffectivenessAttributes == null)
            {
                tempEffectivenessAttributes = new Dictionary<string, float>();
                foreach (var effectivenessAttribute in effectivenessAttributes)
                {
                    if (effectivenessAttribute.attribute == null)
                        continue;
                    tempEffectivenessAttributes[effectivenessAttribute.attribute.Id] = effectivenessAttribute.effectiveness;
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
