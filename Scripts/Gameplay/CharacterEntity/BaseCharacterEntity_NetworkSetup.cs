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
            id.sendOptions = SendOptions.ReliableOrdered;
            id.forOwnerOnly = false;
            level.sendOptions = SendOptions.ReliableOrdered;
            level.forOwnerOnly = false;
            exp.sendOptions = SendOptions.ReliableOrdered;
            exp.forOwnerOnly = false;
            currentHp.sendOptions = SendOptions.ReliableOrdered;
            currentHp.forOwnerOnly = false;
            currentMp.sendOptions = SendOptions.ReliableOrdered;
            currentMp.forOwnerOnly = false;
            currentFood.sendOptions = SendOptions.ReliableOrdered;
            currentFood.forOwnerOnly = false;
            currentWater.sendOptions = SendOptions.ReliableOrdered;
            currentWater.forOwnerOnly = false;
            equipWeapons.sendOptions = SendOptions.ReliableOrdered;
            equipWeapons.forOwnerOnly = false;
            isHidding.sendOptions = SendOptions.ReliableOrdered;
            isHidding.forOwnerOnly = false;

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
            RegisterNetFunction<Vector3>(NetFuncUpdateAimPosition);
            RegisterNetFunction(NetFuncDismissAimPosition);
            RegisterNetFunction(NetFuncAttack);
            RegisterNetFunction<int>(NetFuncUseSkill);
            RegisterNetFunction<short>(NetFuncUseItem);
            RegisterNetFunction<byte, int, byte>(NetFuncPlayActionAnimation);
            RegisterNetFunction<PackedUInt>(NetFuncPickupItem);
            RegisterNetFunction<short, short>(NetFuncDropItem);
            RegisterNetFunction<short, byte, short>(NetFuncEquipItem);
            RegisterNetFunction<byte, short>(NetFuncUnEquipItem);
            RegisterNetFunction(NetFuncOnDead);
            RegisterNetFunction(NetFuncOnRespawn);
            RegisterNetFunction(NetFuncOnLevelUp);
            RegisterNetFunction<PackedUInt>(NetFuncUnSummon);
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
