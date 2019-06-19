using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            id.deliveryMethod = DeliveryMethod.ReliableOrdered;
            id.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            level.deliveryMethod = DeliveryMethod.ReliableOrdered;
            level.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            exp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            exp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentHp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentHp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentMp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentMp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentFood.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentFood.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentWater.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentWater.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            equipWeapons.deliveryMethod = DeliveryMethod.ReliableOrdered;
            equipWeapons.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isHidding.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isHidding.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            // Character movement
            movementState.deliveryMethod = DeliveryMethod.Sequenced;
            movementState.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            movementState.doNotSyncInitialDataImmediately = true;
            currentDirection.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirection.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentDirection.doNotSyncInitialDataImmediately = true;
            currentDirectionType.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirectionType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentDirectionType.doNotSyncInitialDataImmediately = true;

            attributes.forOwnerOnly = false;
            skills.forOwnerOnly = true;
            skillUsages.forOwnerOnly = true;
            buffs.forOwnerOnly = false;
            equipItems.forOwnerOnly = false;
            nonEquipItems.forOwnerOnly = true;
            summons.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnIdChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            currentHp.onChange += OnCurrentHpChange;
            currentMp.onChange += OnCurrentMpChange;
            currentFood.onChange += OnCurrentFoodChange;
            currentWater.onChange += OnCurrentWaterChange;
            equipWeapons.onChange += OnEquipWeaponsChange;
            isHidding.onChange += OnIsHiddingChange;
            // On list changes events
            attributes.onOperation += OnAttributesOperation;
            skills.onOperation += OnSkillsOperation;
            skillUsages.onOperation += OnSkillUsagesOperation;
            buffs.onOperation += OnBuffsOperation;
            equipItems.onOperation += OnEquipItemsOperation;
            nonEquipItems.onOperation += OnNonEquipItemsOperation;
            summons.onOperation += OnSummonsOperation;
            // Register Network functions
            RegisterNetFunction<bool>(NetFuncPlayWeaponLaunchEffect);
            RegisterNetFunction<Vector3>(NetFuncSetAimPosition);
            RegisterNetFunction(NetFuncUnsetAimPosition);
            RegisterNetFunction<bool>(NetFuncAttackWithoutAimPosition);
            RegisterNetFunction<bool, Vector3>(NetFuncAttackWithAimPosition);
            RegisterNetFunction<int, bool>(NetFuncUseSkillWithoutAimPosition);
            RegisterNetFunction<int, bool, Vector3>(NetFuncUseSkillWithAimPosition);
            RegisterNetFunction<short>(NetFuncUseItem);
            RegisterNetFunction<byte, int, byte>(NetFuncPlayActionAnimation);
            RegisterNetFunction<int, float>(NetFuncSkillCasting);
            RegisterNetFunction(NetFuncSkillCastingInterrupted);
            RegisterNetFunction<PackedUInt>(NetFuncPickupItem);
            RegisterNetFunction<short, short>(NetFuncDropItem);
            RegisterNetFunction<short, byte, short>(NetFuncEquipItem);
            RegisterNetFunction<byte, short>(NetFuncUnEquipItem);
            RegisterNetFunction(NetFuncOnDead);
            RegisterNetFunction(NetFuncOnRespawn);
            RegisterNetFunction(NetFuncOnLevelUp);
            RegisterNetFunction<PackedUInt>(NetFuncUnSummon);
            RegisterNetFunction<short, short>(NetFuncSwapOrMergeNonEquipItems);
            RegisterNetFunction<bool>(NetFuncReload);

            // Setup character movement here to make it able to register net elements / functions
            InitialRequiredComponents();
            // Get all character components (include character movement)
            GetComponents(characterComponents);
            foreach (BaseCharacterComponent characterComponent in characterComponents)
            {
                // Register net elements / functions here
                characterComponent.EntityOnSetup(this);
            }

            if (teleportingPosition.HasValue)
            {
                Teleport(teleportingPosition.Value);
                teleportingPosition = null;
            }
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            id.onChange -= OnIdChange;
            level.onChange -= OnLevelChange;
            exp.onChange -= OnExpChange;
            currentHp.onChange -= OnCurrentHpChange;
            currentMp.onChange -= OnCurrentMpChange;
            currentFood.onChange -= OnCurrentFoodChange;
            currentWater.onChange -= OnCurrentWaterChange;
            equipWeapons.onChange -= OnEquipWeaponsChange;
            isHidding.onChange -= OnIsHiddingChange;
            // On list changes events
            attributes.onOperation -= OnAttributesOperation;
            skills.onOperation -= OnSkillsOperation;
            skillUsages.onOperation -= OnSkillUsagesOperation;
            buffs.onOperation -= OnBuffsOperation;
            equipItems.onOperation -= OnEquipItemsOperation;
            nonEquipItems.onOperation -= OnNonEquipItemsOperation;
            summons.onOperation -= OnSummonsOperation;

            foreach (BaseCharacterComponent characterComponent in characterComponents)
            {
                characterComponent.EntityOnDestroy(this);
            }

            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
        }
    }
}
