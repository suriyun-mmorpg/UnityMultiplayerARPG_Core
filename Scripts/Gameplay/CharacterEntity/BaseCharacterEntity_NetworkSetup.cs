using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            id.deliveryMethod = DeliveryMethod.ReliableSequenced;
            id.forOwnerOnly = false;
            level.deliveryMethod = DeliveryMethod.ReliableSequenced;
            level.forOwnerOnly = false;
            exp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            exp.forOwnerOnly = false;
            currentHp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentHp.forOwnerOnly = false;
            currentMp.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentMp.forOwnerOnly = false;
            currentFood.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentFood.forOwnerOnly = false;
            currentWater.deliveryMethod = DeliveryMethod.ReliableSequenced;
            currentWater.forOwnerOnly = false;
            equipWeapons.deliveryMethod = DeliveryMethod.ReliableSequenced;
            equipWeapons.forOwnerOnly = false;
            isHidding.deliveryMethod = DeliveryMethod.ReliableSequenced;
            isHidding.forOwnerOnly = false;
            movementState.deliveryMethod = DeliveryMethod.Sequenced;
            movementState.forOwnerOnly = false;

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
            RegisterNetFunction(NetFuncAttackWithoutAimPosition);
            RegisterNetFunction<Vector3>(NetFuncAttackWithAimPosition);
            RegisterNetFunction<int>(NetFuncUseSkillWithoutAimPosition);
            RegisterNetFunction<int, Vector3>(NetFuncUseSkillWithAimPosition);
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
