using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        // Note: You may use `Awake` dev extension to setup an events and `OnDestroy` to desetup an events
        // Generic events
        [Category("Events")]
        public UnityEvent onDead = new UnityEvent();
        public UnityEvent onRespawn = new UnityEvent();
        public UnityEvent onLevelUp = new UnityEvent();
        // Action events
        public event AttackRoutineDelegate onAttackRoutine;
        public event UseSkillRoutineDelegate onUseSkillRoutine;
        public event LaunchDamageEntityDelegate onLaunchDamageEntity;

        public void OnAttackRoutine(
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            AimPosition aimPosition)
        {
            if (onAttackRoutine != null)
                onAttackRoutine.Invoke(isLeftHand, weapon, hitIndex, damageInfo, damageAmounts, aimPosition);
        }

        public void OnUseSkillRoutine(
            BaseSkill skill,
            short level,
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition)
        {
            if (onUseSkillRoutine != null)
                onUseSkillRoutine.Invoke(skill, level, isLeftHand, weapon, hitIndex, damageAmounts, targetObjectId, aimPosition);
        }

        public void OnLaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            Dictionary<uint, int> hitBoxes)
        {
            if (onLaunchDamageEntity != null)
                onLaunchDamageEntity.Invoke(isLeftHand, weapon, damageAmounts, skill, skillLevel, randomSeed, aimPosition, stagger, hitBoxes);
        }
    }
}
