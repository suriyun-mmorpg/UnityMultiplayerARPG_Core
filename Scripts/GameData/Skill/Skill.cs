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
public struct SkillBuff
{
    [Header("Duration")]
    [Tooltip("If buff duration less than or equals to 0, buff stats won't applied")]
    public float baseDuration;
    public float durationIncreaseEachLevel;
    [Header("Hp recovery")]
    public int baseRecoveryHp;
    public float recoveryHpIncreaseEachLevel;
    [Header("Mp recovery")]
    public int baseRecoveryMp;
    public float recoveryMpIncreaseEachLevel;
    [Header("Add Stats")]
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    [Header("Add Attributes")]
    public AttributeIncremental[] increaseAttributes;
    [Header("Add Resistances")]
    public ResistanceIncremental[] increaseResistances;
}

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public ActionAnimation castAnimation;
    public int maxLevel;

    [Header("Consume Mp")]
    public int baseConsumeMp;
    public float consumeMpIncreaseEachLevel;

    [Header("Cool Down")]
    public float baseCoolDownDuration;
    public float coolDownDurationIncreaseEachLevel;

    [Header("Requirements")]
    public SkillRequirement requirement;

    [Header("Attack")]
    public SkillAttackType skillAttackType;

    [Header("Attack As Pure Skill Damage")]
    public DamageAttribute baseDamageAttribute;
    public DamageAttribute[] additionalDamageAttributes;
    public Damage damage;
    public DamageEffectivenessAttribute[] effectivenessAttributes;

    [Header("Attack As Weapon Damage Inflict")]
    public float baseInflictRate;
    public float inflictRateIncreaseEachLevel;
    public DamageAttribute[] inflictDamageAttributes;

    [Header("Attack Debuff")]
    public bool isDebuff;
    public SkillBuff debuff;

    [Header("Buffs")]
    public SkillBuffType skillBuffType;
    public SkillBuff buff;

    private Dictionary<Skill, int> tempRequireSkillLevels;
    public Dictionary<Skill, int> TempRequireSkillLevels
    {
        get
        {
            if (tempRequireSkillLevels == null)
                tempRequireSkillLevels = GameDataHelpers.MakeSkillLevelDictionary(requirement.skillLevels, new Dictionary<Skill, int>());
            return tempRequireSkillLevels;
        }
    }

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
                    if (effectivenessAttribute.attribute == null || effectivenessAttribute.effectiveness == 0f)
                        continue;
                    var id = effectivenessAttribute.attribute.Id;
                    if (!tempEffectivenessAttributes.ContainsKey(id))
                        tempEffectivenessAttributes[id] = effectivenessAttribute.effectiveness;
                    else
                        tempEffectivenessAttributes[id] += effectivenessAttribute.effectiveness;
                }
            }
            return tempEffectivenessAttributes;
        }
    }
}

[System.Serializable]
public struct SkillRequirement
{
    public int baseCharacterLevel;
    public float characterLevelIncreaseEachLevel;
    public SkillLevel[] skillLevels;
}

[System.Serializable]
public struct SkillLevel
{
    public Skill skill;
    public int level;
}
