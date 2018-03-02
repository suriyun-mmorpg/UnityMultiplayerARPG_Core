using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
public class Skill : BaseGameData
{
    public SkillLevel[] requireSkillLevels;
    public int consumeMp;
    public float coolDown;
    [Header("Attack")]
    public bool isAttack;
    public DamageEntity damageEntityPrefab;
    public DamageAmount[] damageAmounts;
    [Header("Buffs")]
    public bool isBuff;
    [Tooltip("`buffDistance` = 0, will buff only caster")]
    public float buffDistance;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;

    public DamageEntity DamageEntityPrefab
    {
        get
        {
            if (damageEntityPrefab == null)
                return GameInstance.Singleton.damageEntityPrefab;
            return damageEntityPrefab;
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
                    var id = damageAmount.damage == null ? GameDataConst.DEFAULT_DAMAGE_ID : damageAmount.damage.Id;
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
    public SkillLevel skill;
    public int level;
}
