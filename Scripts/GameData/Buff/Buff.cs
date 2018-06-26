using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class Buff
    {
        [Tooltip("If buff duration less than or equals to 0, buff stats won't applied")]
        public IncrementalFloat duration;
        public IncrementalInt recoveryHp;
        public IncrementalInt recoveryMp;
        public IncrementalInt recoveryStamina;
        public IncrementalInt recoveryFood;
        public IncrementalInt recoveryWater;
        public CharacterStatsIncremental increaseStats;
        public AttributeIncremental[] increaseAttributes;
        public ResistanceIncremental[] increaseResistances;
        public DamageIncremental[] increaseDamages;
        public GameEffect[] effects;
    }
}
