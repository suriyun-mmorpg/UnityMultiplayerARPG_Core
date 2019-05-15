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
            id.deliveryMethod = DeliveryMethod.ReliableSequenced;
            id.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            level.deliveryMethod = DeliveryMethod.ReliableSequenced;
            level.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            exp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            exp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentHp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentHp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentMp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentMp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentFood.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentFood.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentWater.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentWater.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            equipWeapons.deliveryMethod = DeliveryMethod.ReliableSequenced;
            equipWeapons.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isHidding.deliveryMethod = DeliveryMethod.ReliableSequenced;
            isHidding.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            movementState.deliveryMethod = DeliveryMethod.Sequenced;
            movementState.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

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
            movementState.onChange += OnMovementStateChange;
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

            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
        }
    }
}
