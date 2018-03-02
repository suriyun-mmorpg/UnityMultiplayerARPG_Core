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
    public DamageAmount[] damages;
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
}

[System.Serializable]
public class SkillLevel
{
    public SkillLevel skill;
    public int level;
}
