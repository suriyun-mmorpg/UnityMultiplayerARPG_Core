using LiteNetLibManager;
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
        // Sync variables
        public event System.Action<string> onIdChange;
        public event System.Action<string> onCharacterNameChange;
        public event System.Action<int> onLevelChange;
        public event System.Action<int> onExpChange;
        public event System.Action<bool> onIsImmuneChange;
        public event System.Action<int> onCurrentMpChange;
        public event System.Action<int> onCurrentFoodChange;
        public event System.Action<int> onCurrentWaterChange;
        public event System.Action<byte> onEquipWeaponSetChange;
        public event System.Action<bool> onIsWeaponsSheathedChange;
        public event System.Action<ushort> onPitchChange;
        public event System.Action<AimPosition> onAimPositionChange;
        public event System.Action<uint> onTargetEntityIdChange;
        // Sync lists
        public event System.Action<LiteNetLibSyncList.Operation, int> onSelectableWeaponSetsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onAttributesOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onSkillsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onSkillUsagesOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onBuffsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onEquipItemsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onNonEquipItemsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onSummonsOperation;
        // Action events
        public event AttackRoutineDelegate onAttackRoutine;
        public event UseSkillRoutineDelegate onUseSkillRoutine;
        public event LaunchDamageEntityDelegate onLaunchDamageEntity;
        // Buff events
        public event ApplyBuffDelegate onApplyBuff;
        public event AppliedRecoveryAmountDelegate onBuffHpRecovery;
        public event AppliedRecoveryAmountDelegate onBuffHpDecrease;
        public event AppliedRecoveryAmountDelegate onBuffMpRecovery;
        public event AppliedRecoveryAmountDelegate onBuffMpDecrease;
        public event AppliedRecoveryAmountDelegate onBuffStaminaRecovery;
        public event AppliedRecoveryAmountDelegate onBuffStaminaDecrease;
        public event AppliedRecoveryAmountDelegate onBuffFoodRecovery;
        public event AppliedRecoveryAmountDelegate onBuffFoodDecrease;
        public event AppliedRecoveryAmountDelegate onBuffWaterRecovery;
        public event AppliedRecoveryAmountDelegate onBuffWaterDecrease;
        // Enemy spotted events
        public event NotifyEnemySpottedDelegate onNotifyEnemySpotted;
        public event NotifyEnemySpottedByAllyDelegate onNotifyEnemySpottedByAlly;

        public void OnAttackRoutine(
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            AimPosition aimPosition)
        {
            if (onAttackRoutine != null)
                onAttackRoutine.Invoke(isLeftHand, weapon, simulateSeed, triggerIndex, damageInfo, damageAmounts, aimPosition);
        }

        public void OnUseSkillRoutine(
            BaseSkill skill,
            int level,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition)
        {
            if (onUseSkillRoutine != null)
                onUseSkillRoutine.Invoke(skill, level, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);
        }

        public void OnLaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition)
        {
            if (onLaunchDamageEntity != null)
                onLaunchDamageEntity.Invoke(isLeftHand, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, aimPosition);
        }
    }
}
