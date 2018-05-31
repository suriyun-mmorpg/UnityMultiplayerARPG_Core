using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Active,
    Passive,
    CraftItem,
}

public enum SkillAttackType
{
    None,
    Normal,
    BasedOnWeapon,
}

public enum SkillBuffType
{
    None,
    BuffToUser,
}

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public SkillType skillType;
    [Range(1, 30)]
    public int maxLevel = 1;
    [Tooltip("Randoming cast animations")]
    public ActionAnimation[] castAnimations;
    [Tooltip("An available weapons, if it not set every weapons is available")]
    public WeaponType[] availableWeapons;

    [Header("Consume Mp")]
    public IncrementalInt consumeMp;

    [Header("Cool Down")]
    public IncrementalFloat coolDownDuration;

    [Header("Requirements")]
    public SkillRequirement requirement;

    [Header("Attack")]
    public SkillAttackType skillAttackType;
    public GameEffectCollection hitEffects;
    public DamageInfo damageInfo;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public DamageIncremental damageAmount;
    public DamageInflictionIncremental[] weaponDamageInflictions;
    public DamageIncremental[] additionalDamageAmounts;
    public bool isDebuff;
    public Buff debuff;

    [Header("Buffs")]
    public SkillBuffType skillBuffType;
    public Buff buff;

    [Header("Craft")]
    public Item craftingItem;
    public ItemAmount[] craftRequirements;

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
