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
    public DamageAmount damage;
    [Header("Buffs")]
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;
}

[System.Serializable]
public class SkillLevel
{
    public SkillLevel skill;
    public int level;
}
