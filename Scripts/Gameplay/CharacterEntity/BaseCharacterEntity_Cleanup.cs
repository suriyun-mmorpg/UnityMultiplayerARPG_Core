using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                meleeDamageTransform = null;
                missileDamageTransform = null;
                characterUiTransform = null;
                miniMapUiTransform = null;
                chatBubbleTransform = null;
                race = null;
                if (UICharacterEntity != null)
                    Destroy(UICharacterEntity.gameObject);
                UICharacterEntity = null;
                if (UIChatBubble != null)
                    Destroy(UIChatBubble.gameObject);
                UIChatBubble = null;
                AttackComponent = null;
                UseSkillComponent = null;
                ReloadComponent = null;
                ChargeComponent = null;
                RecoveryComponent = null;
                SkillAndBuffComponent = null;
                AttackPhysicFunctions = null;
                FindPhysicFunctions = null;
                ModelManager = null;
                // Events
                onDead?.RemoveAllListeners();
                onDead = null;
                onRespawn?.RemoveAllListeners();
                onRespawn = null;
                onLevelUp?.RemoveAllListeners();
                onLevelUp = null;
                onRecached = null;
                onIdChange = null;
                onCharacterNameChange = null;
                onLevelChange = null;
                onExpChange = null;
                onIsInvincibleChange = null;
                onCurrentMpChange = null;
                onCurrentStaminaChange = null;
                onCurrentFoodChange = null;
                onCurrentWaterChange = null;
                onEquipWeaponSetChange = null;
                onIsWeaponsSheathedChange = null;
                onPitchChange = null;
                onAimPositionChange = null;
                onTargetEntityIdChange = null;
                onSelectableWeaponSetsOperation = null;
                onAttributesOperation = null;
                onSkillsOperation = null;
                onSkillUsagesOperation = null;
                onBuffsOperation = null;
                onEquipItemsOperation = null;
                onNonEquipItemsOperation = null;
                onSummonsOperation = null;
                onAttackRoutine = null;
                onUseSkillRoutine = null;
                onLaunchDamageEntity = null;
                onApplyBuff = null;
                onRemoveBuff = null;
                onBuffHpRecovery = null;
                onBuffHpDecrease = null;
                onBuffMpRecovery = null;
                onBuffMpDecrease = null;
                onBuffStaminaRecovery = null;
                onBuffStaminaDecrease = null;
                onBuffFoodRecovery = null;
                onBuffFoodDecrease = null;
                onBuffWaterRecovery = null;
                onBuffWaterDecrease = null;
                onNotifyEnemySpotted = null;
                onNotifyEnemySpottedByAlly = null;
            }
            // Actions
            RespawnGroundedCheckCountDown = 0f;
            RespawnInvincibleCountDown = 0f;
            LastUseItemTime = 0f;
            LastActionTime = 0f;
            FallDamageDisableState.Clear();
            _countDownToUpdateAppearances = FRAMES_BEFORE_UPDATE_APPEARANCES;
            // Caches
            this.RemoveCaches();
            CachedData = null;
            IsRecaching = false;
            // Buff Functions
            _restrictBuffTags?.Clear();
            // Client Ammo Simulation
            _rightWeaponAmmoSim = 0;
            _leftWeaponAmmoSim = 0;
            _countDownToUpdateAmmoSim = FRAMES_BEFORE_UPDATE_AMMO_SIM;
            // Damage Functions
            _isKilled = false;
            _beforeDamageReceivedHp = 0;
            _receivedDamageRecords?.Clear();
            // Move Functions
            _lastGrounded = false;
            _lastGroundedPosition = Vector3.zero;
        }
    }
}
