using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Buff
{
    [Tooltip("If buff duration less than or equals to 0, buff stats won't applied")]
    public IncrementalFloat duration;
    public IncrementalInt recoveryHp;
    public IncrementalInt recoveryMp;
    public CharacterStatsIncremental increaseStats;
    public AttributeIncremental[] increaseAttributes;
    public ResistanceIncremental[] increaseResistances;
    public GameEffect[] effects;
}
