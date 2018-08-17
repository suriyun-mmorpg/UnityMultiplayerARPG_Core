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
            dataId.sendOptions = SendOptions.ReliableOrdered;
            dataId.forOwnerOnly = false;
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

            attributes.forOwnerOnly = false;
            skills.forOwnerOnly = true;
            buffs.forOwnerOnly = false;
            equipItems.forOwnerOnly = false;
            nonEquipItems.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnIdChange;
            dataId.onChange += OnDataIdChange;
            characterName.onChange += OnCharacterNameChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            currentHp.onChange += OnCurrentHpChange;
            currentMp.onChange += OnCurrentMpChange;
            equipWeapons.onChange += OnEquipWeaponsChange;
            isHidding.onChange += OnIsHiddingChange;
            // On list changes events
            attributes.onOperation += OnAttributesOperation;
            skills.onOperation += OnSkillsOperation;
            buffs.onOperation += OnBuffsOperation;
            equipItems.onOperation += OnEquipItemsOperation;
            nonEquipItems.onOperation += OnNonEquipItemsOperation;
            // Register Network functions
            RegisterNetFunction("Attack", new LiteNetLibFunction(NetFuncAttack));
            RegisterNetFunction("UseSkill", new LiteNetLibFunction<NetFieldVector3, NetFieldInt>((position, skillIndex) => NetFuncUseSkill(position, skillIndex)));
            RegisterNetFunction("UseItem", new LiteNetLibFunction<NetFieldInt>((itemIndex) => NetFuncUseItem(itemIndex)));
            RegisterNetFunction("PlayActionAnimation", new LiteNetLibFunction<NetFieldByte, NetFieldInt, NetFieldByte>((animActionType, dataId, index) => NetFuncPlayActionAnimation((AnimActionType)animActionType.Value, dataId, index)));
            RegisterNetFunction("PlayEffect", new LiteNetLibFunction<NetFieldPackedUInt>((effectId) => NetFuncPlayEffect(effectId)));
            RegisterNetFunction("PickupItem", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncPickupItem(objectId)));
            RegisterNetFunction("DropItem", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((index, amount) => NetFuncDropItem(index, amount)));
            RegisterNetFunction("EquipItem", new LiteNetLibFunction<NetFieldInt, NetFieldString>((nonEquipIndex, equipPosition) => NetFuncEquipItem(nonEquipIndex, equipPosition)));
            RegisterNetFunction("UnEquipItem", new LiteNetLibFunction<NetFieldString>((fromEquipPosition) => NetFuncUnEquipItem(fromEquipPosition)));
            RegisterNetFunction("OnDead", new LiteNetLibFunction(NetFuncOnDead));
            RegisterNetFunction("OnRespawn", new LiteNetLibFunction(NetFuncOnRespawn));
            RegisterNetFunction("OnLevelUp", new LiteNetLibFunction(NetFuncOnLevelUp));
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            dataId.onChange -= OnDataIdChange;
            equipWeapons.onChange -= OnEquipWeaponsChange;
            // On list changes events
            attributes.onOperation -= OnAttributesOperation;
            skills.onOperation -= OnSkillsOperation;
            buffs.onOperation -= OnBuffsOperation;
            equipItems.onOperation -= OnEquipItemsOperation;
            nonEquipItems.onOperation -= OnNonEquipItemsOperation;

            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
        }
    }
}
