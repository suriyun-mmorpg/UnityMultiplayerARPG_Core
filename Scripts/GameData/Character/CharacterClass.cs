using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClass", menuName = "Create GameData/CharacterClass")]
public class CharacterClass : BaseGameData
{
    public CharacterAttributeAmount[] baseAttributes;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;
    public Skill[] skills;
}
