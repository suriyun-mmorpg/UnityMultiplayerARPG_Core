using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Buff
    {
        public static readonly Buff Empty = new Buff();
        [Tooltip("If duration less than or equals to 0, buff stats won't applied only recovery will be applied. This won't be applied to monster's summoner.")]
        public IncrementalFloat duration;
        [Tooltip("Recover character's current HP. This won't be applied to monster's summoner.")]
        public IncrementalInt recoveryHp;
        [Tooltip("Recover character's current MP. This won't be applied to monster's summoner.")]
        public IncrementalInt recoveryMp;
        [Tooltip("Recover character's current stamina. This won't be applied to monster's summoner.")]
        public IncrementalInt recoveryStamina;
        [Tooltip("Recover character's current food. This won't be applied to monster's summoner.")]
        public IncrementalInt recoveryFood;
        [Tooltip("Recover character's current water. This won't be applied to monster's summoner.")]
        public IncrementalInt recoveryWater;
        [Tooltip("Increase character's stats.")]
        public CharacterStatsIncremental increaseStats;
        [Tooltip("Increase character's stats rate.")]
        public CharacterStatsIncremental increaseStatsRate;
        [Tooltip("Increase character's attributes.")]
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributes;
        [Tooltip("Increase character's attributes rate.")]
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributesRate;
        [Tooltip("Increase character's resistances.")]
        [ArrayElementTitle("damageElement")]
        public ResistanceIncremental[] increaseResistances;
        [Tooltip("Increase character's armors.")]
        [ArrayElementTitle("damageElement")]
        public ArmorIncremental[] increaseArmors;
        [Tooltip("Increase character's damages.")]
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] increaseDamages;
        [Tooltip("Applies damage within duration to character. This won't be applied to monster's summoner.")]
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] damageOverTimes;
        [Tooltip("Disallow character to move while applied. This won't be applied to monster's summoner.")]
        public bool disallowMove;
        [Tooltip("Disallow character to attack while applied. This won't be applied to monster's summoner.")]
        public bool disallowAttack;
        [Tooltip("Disallow character to use skill while applied. This won't be applied to monster's summoner.")]
        public bool disallowUseSkill;
        [Tooltip("Disallow character to use item while applied. This won't be applied to monster's summoner.")]
        public bool disallowUseItem;
        [Tooltip("Hide character. This won't be applied to monster's summoner.")]
        public bool isHide;
        [Tooltip("Mute character movement sound while applied. This won't be applied to monster's summoner.")]
        public bool muteFootstepSound;
        [Tooltip("Status effects that can be applied to the attacker when attacking")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacking;
        [Tooltip("Status effects that can be applied to the enemy when attacking")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacking;
        [Tooltip("Status effects that can be applied to the attacker when attacked")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacked;
        [Tooltip("Status effects that can be applied to the enemy when attacked")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacked;
        [Tooltip("Game effects which appearing on character while applied. This won't be applied to monster's summoner.")]
        public GameEffect[] effects;

        public void PrepareRelatesData()
        {
            GameInstance.AddAttributes(increaseAttributes);
            GameInstance.AddAttributes(increaseAttributesRate);
            GameInstance.AddDamageElements(increaseResistances);
            GameInstance.AddDamageElements(increaseArmors);
            GameInstance.AddDamageElements(increaseDamages);
            GameInstance.AddStatusEffects(selfStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(enemyStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(selfStatusEffectsWhenAttacked);
            GameInstance.AddStatusEffects(enemyStatusEffectsWhenAttacked);
        }
    }
}
