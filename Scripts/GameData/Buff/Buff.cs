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
        public CharacterStatsIncremental increaseStatsRate;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeIncremental[] increaseAttributes;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeIncremental[] increaseAttributesRate;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ResistanceIncremental[] increaseResistances;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ArmorIncremental[] increaseArmors;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public DamageIncremental[] increaseDamages;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public DamageIncremental[] damageOverTimes;
        public bool disallowMove;
        public bool disallowAttack;
        public bool disallowUseSkill;
        public bool disallowUseItem;
        public bool isHide;
        public bool muteFootstepSound;
        public GameEffect[] effects;
    }
}
