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
    public DamageAmount[] damageAmounts;
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

    private Dictionary<string, DamageAmount> tempDamageAmounts;
    public Dictionary<string, DamageAmount> TempDamageAmounts
    {
        get
        {
            if (tempDamageAmounts == null)
            {
                tempDamageAmounts = new Dictionary<string, DamageAmount>();
                foreach (var damageAmount in damageAmounts)
                {
                    var id = damageAmount.damageElement == null ? GameDataConst.DEFAULT_DAMAGE_ID : damageAmount.damageElement.Id;
                    tempDamageAmounts[id] = damageAmount;
                }
            }
            return tempDamageAmounts;
        }
    }
}

[System.Serializable]
public class SkillLevel
{
    public Skill skill;
    public int level;
}
