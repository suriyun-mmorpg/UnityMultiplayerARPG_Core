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

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public ActionAnimation castAnimation;
    [Range(1, 30)]
    public int maxLevel = 1;

    [Header("Consume Mp")]
    public IncrementalInt consumeMp;

    [Header("Cool Down")]
    public IncrementalFloat coolDownDuration;

    [Header("Requirements")]
    public SkillRequirement requirement;

    [Header("Attack")]
    public SkillAttackType skillAttackType;
    public GameEffectCollection hitEffects;

    [Header("Attack As Pure Skill Damage")]
    public DamageInfo damageInfo;
    public DamageIncremental damageAttribute;
    public DamageEffectivenessAttribute[] effectivenessAttributes;

    [Header("Attack As Weapon Damage Inflict")]
    public IncrementalFloat inflictRate;

    [Header("Additional Damage Attributes")]
    public DamageIncremental[] additionalDamageAttributes;

    [Header("Attack Debuff")]
    public bool isDebuff;
    public Buff debuff;

    [Header("Buffs")]
    public SkillBuffType skillBuffType;
    public Buff buff;

    private Dictionary<Skill, int> cacheRequireSkillLevels;
    public Dictionary<Skill, int> CacheRequireSkillLevels
    {
        get
        {
            if (cacheRequireSkillLevels == null)
                cacheRequireSkillLevels = GameDataHelpers.MakeSkillLevelsDictionary(requirement.skillLevels, new Dictionary<Skill, int>());
            return cacheRequireSkillLevels;
        }
    }

    private Dictionary<Attribute, float> cacheEffectivenessAttributes;
    public Dictionary<Attribute, float> CacheEffectivenessAttributes
    {
        get
        {
            if (cacheEffectivenessAttributes == null)
                cacheEffectivenessAttributes = GameDataHelpers.MakeDamageEffectivenessAttributesDictionary(effectivenessAttributes, new Dictionary<Attribute, float>());
            return cacheEffectivenessAttributes;
        }
    }
}

[System.Serializable]
public struct SkillRequirement
{
    public IncrementalInt characterLevel;
    public SkillLevel[] skillLevels;
}

[System.Serializable]
public struct SkillLevel
{
    public Skill skill;
    public int level;
}
