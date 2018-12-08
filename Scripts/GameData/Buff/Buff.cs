using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Buff
    {
        [Tooltip("If duration less than or equals to 0, buff stats won't applied only recovery will be applied")]
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
