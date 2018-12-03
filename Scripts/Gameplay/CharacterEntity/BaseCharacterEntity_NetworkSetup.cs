using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            id.sendOptions = SendOptions.ReliableOrdered;
            id.forOwnerOnly = false;
            characterName.sendOptions = SendOptions.ReliableOrdered;
            characterName.forOwnerOnly = false;
            level.sendOptions = SendOptions.ReliableOrdered;
            level.forOwnerOnly = false;
            exp.sendOptions = SendOptions.ReliableOrdered;
            exp.forOwnerOnly = false;
            currentHp.sendOptions = SendOptions.ReliableOrdered;
            currentHp.forOwnerOnly = false;
            currentMp.sendOptions = SendOptions.ReliableOrdered;
            currentMp.forOwnerOnly = false;
            equipWeapons.sendOptions = SendOptions.ReliableOrdered;
            equipWeapons.forOwnerOnly = false;
            isHidding.sendOptions = SendOptions.ReliableOrdered;
            isHidding.forOwnerOnly = false;
            petEntityId.sendOptions = SendOptions.ReliableOrdered;
            petEntityId.forOwnerOnly = true;

            attributes.forOwnerOnly = false;
            skills.forOwnerOnly = true;
            skillUsages.forOwnerOnly = true;
            buffs.forOwnerOnly = false;
            equipItems.forOwnerOnly = false;
            nonEquipItems.forOwnerOnly = true;
            summonEntityIds.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnIdChange;
            characterName.onChange += OnCharacterNameChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            currentHp.onChange += OnCurrentHpChange;
            currentMp.onChange += OnCurrentMpChange;
            equipWeapons.onChange += OnEquipWeaponsChange;
            isHidding.onChange += OnIsHiddingChange;
            petEntityId.onChange += OnPetEntityIdChange;
            // On list changes events
            attributes.onOperation += OnAttributesOperation;
            skills.onOperation += OnSkillsOperation;
            skillUsages.onOperation += OnSkillUsagesOperation;
            buffs.onOperation += OnBuffsOperation;
            equipItems.onOperation += OnEquipItemsOperation;
            nonEquipItems.onOperation += OnNonEquipItemsOperation;
            summonEntityIds.onOperation += OnSummonEntityIdsOperation;
            // Register Network functions
            RegisterNetFunction(NetFuncAttack);
            RegisterNetFunction<Vector3, int>(NetFuncUseSkill);
            RegisterNetFunction<int>(NetFuncUseItem);
            RegisterNetFunction<byte, int, byte>(NetFuncPlayActionAnimation);
            RegisterNetFunction<PackedUInt>(NetFuncPickupItem);
            RegisterNetFunction<ushort, short>(NetFuncDropItem);
            RegisterNetFunction<ushort, string>(NetFuncEquipItem);
            RegisterNetFunction<string>(NetFuncUnEquipItem);
            RegisterNetFunction(NetFuncOnDead);
            RegisterNetFunction(NetFuncOnRespawn);
            RegisterNetFunction(NetFuncOnLevelUp);
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            id.onChange -= OnIdChange;
            characterName.onChange -= OnCharacterNameChange;
            level.onChange -= OnLevelChange;
            exp.onChange -= OnExpChange;
            currentHp.onChange -= OnCurrentHpChange;
            currentMp.onChange -= OnCurrentMpChange;
            equipWeapons.onChange -= OnEquipWeaponsChange;
            isHidding.onChange -= OnIsHiddingChange;
            petEntityId.onChange -= OnPetEntityIdChange;
            // On list changes events
            attributes.onOperation -= OnAttributesOperation;
            skills.onOperation -= OnSkillsOperation;
            skillUsages.onOperation -= OnSkillUsagesOperation;
            buffs.onOperation -= OnBuffsOperation;
            equipItems.onOperation -= OnEquipItemsOperation;
            nonEquipItems.onOperation -= OnNonEquipItemsOperation;
            summonEntityIds.onOperation -= OnSummonEntityIdsOperation;

            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
        }
    }
}
