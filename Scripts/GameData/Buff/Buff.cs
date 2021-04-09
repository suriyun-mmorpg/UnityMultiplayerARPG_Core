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
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributes;
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributesRate;
        [ArrayElementTitle("damageElement")]
        public ResistanceIncremental[] increaseResistances;
        [ArrayElementTitle("damageElement")]
        public ArmorIncremental[] increaseArmors;
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] increaseDamages;
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] damageOverTimes;
        public bool disallowMove;
        public bool disallowAttack;
        public bool disallowUseSkill;
        public bool disallowUseItem;
        public bool isHide;
        public bool muteFootstepSound;
        public GameEffect[] effects;

        public void PrepareRelatesData()
        {
            GameInstance.AddAttributes(increaseAttributes);
            GameInstance.AddAttributes(increaseAttributesRate);
            GameInstance.AddDamageElements(increaseResistances);
            GameInstance.AddDamageElements(increaseArmors);
            GameInstance.AddDamageElements(increaseDamages);
        }
    }
}
