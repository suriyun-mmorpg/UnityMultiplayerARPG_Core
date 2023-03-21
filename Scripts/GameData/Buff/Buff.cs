using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Buff
    {
        public static readonly Buff Empty = new Buff();

        [Header("Generic Settings")]
        public string tag;
        public string[] restrictTags;
        [Tooltip("If it is not enable, it will have 100% chance to apply the buff")]
        public bool enableApplyChance;
        [Tooltip("1 = 100% chance to apply the buff")]
        public IncrementalFloat applyChance;

        [Header("Settings for Passive and Active Skills")]
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
        [Header("Settings for Active Skills only")]
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
        [Tooltip("Applies damage within duration to character. This won't be applied to monster's summoner.")]
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] damageOverTimes;
        [Tooltip("`disallowMove`, `disallowAttack`, `disallowUseSkill`, `disallowUseItem` and `freezeAnimation` will be used if this is `None`")]
        public AilmentPresets ailment;
        [Tooltip("Disallow character to move while applied. This won't be applied to monster's summoner.")]
        public bool disallowMove;
        public bool disallowSprint;
        public bool disallowWalk;
        public bool disallowJump;
        public bool disallowCrouch;
        public bool disallowCrawl;
        [Tooltip("Disallow character to attack while applied. This won't be applied to monster's summoner.")]
        public bool disallowAttack;
        [Tooltip("Disallow character to use skill while applied. This won't be applied to monster's summoner.")]
        public bool disallowUseSkill;
        [Tooltip("Disallow character to use item while applied. This won't be applied to monster's summoner.")]
        public bool disallowUseItem;
        [Tooltip("Freeze animation while the buff is applied")]
        public bool freezeAnimation;
        [Tooltip("1 = 100% chance to remove the buff when attacking")]
        public IncrementalFloat removeBuffWhenAttackChance;
        [Tooltip("1 = 100% chance to remove the buff when attacked")]
        public IncrementalFloat removeBuffWhenAttackedChance;
        [Tooltip("1 = 100% chance to remove the buff when using skill")]
        public IncrementalFloat removeBuffWhenUseSkillChance;
        [Tooltip("1 = 100% chance to remove the buff when using item")]
        public IncrementalFloat removeBuffWhenUseItemChance;
        [Tooltip("1 = 100% chance to remove the buff when picking item up")]
        public IncrementalFloat removeBuffWhenPickupItemChance;
        [Tooltip("Hide character. This won't be applied to monster's summoner.")]
        public bool isHide;
        [Tooltip("Mute character movement sound while applied. This won't be applied to monster's summoner.")]
        public bool muteFootstepSound;
        [Tooltip("Status effects that can be applied to the attacker when attacking.")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacking;
        [Tooltip("Status effects that can be applied to the enemy when attacking.")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacking;
        [Tooltip("Status effects that can be applied to the attacker when attacked.")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacked;
        [Tooltip("Status effects that can be applied to the enemy when attacked.")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacked;
        [Tooltip("If this is `TRUE` it will not be removed when the character dies")]
        public bool doNotRemoveOnDead;
        [Tooltip("If this is `TRUE` it will extend duration when applying buff, not remove and re-apply")]
        public bool isExtendDuration;
        [Tooltip("Max stack to applies buff, it won't be used while `isExtendDuration` is `TRUE`")]
        public IncrementalInt maxStack;
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
