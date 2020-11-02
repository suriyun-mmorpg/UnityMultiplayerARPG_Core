using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        // Note: You may use `Awake` dev extension to setup an events and `OnDestroy` to desetup an events
        // Generic events
        [Header("Character Entity Events")]
        public UnityEvent onDead = new UnityEvent();
        public UnityEvent onRespawn = new UnityEvent();
        public UnityEvent onLevelUp = new UnityEvent();
        // Attack functions
        public event AttackRoutineDelegate onAttackRoutine;
        // Use skill functions
        public event UseSkillRoutineDelegate onUseSkillRoutine;
        // Apply buff functions
        public event ApplyBuffDelegate onApplyBuff;
        // Sync variables
        public System.Action<string> onIdChange;
        public System.Action<string> onCharacterNameChange;
        public System.Action<short> onLevelChange;
        public System.Action<int> onExpChange;
        public System.Action<int> onCurrentHpChange;
        public System.Action<int> onCurrentMpChange;
        public System.Action<int> onCurrentFoodChange;
        public System.Action<int> onCurrentWaterChange;
        public System.Action<byte> onEquipWeaponSetChange;
        public System.Action<byte> onPitchChange;
        public System.Action<uint> onTargetEntityIdChange;
        // Sync lists
        public System.Action<LiteNetLibSyncList.Operation, int> onSelectableWeaponSetsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onAttributesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillUsagesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onBuffsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onNonEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSummonsOperation;
    }
}
