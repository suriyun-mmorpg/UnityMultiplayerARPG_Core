using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            statPoint.sendOptions = SendOptions.ReliableOrdered;
            statPoint.forOwnerOnly = true;
            skillPoint.sendOptions = SendOptions.ReliableOrdered;
            skillPoint.forOwnerOnly = true;
            gold.sendOptions = SendOptions.ReliableOrdered;
            gold.forOwnerOnly = true;
            partyId.sendOptions = SendOptions.ReliableOrdered;
            partyId.forOwnerOnly = true;
            guildId.sendOptions = SendOptions.ReliableOrdered;
            guildId.forOwnerOnly = true;

            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            statPoint.onChange += OnStatPointChange;
            skillPoint.onChange += OnSkillPointChange;
            gold.onChange += OnGoldChange;
            partyId.onChange += OnPartyIdChange;
            guildId.onChange += OnGuildIdChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
            // Register Network functions
            RegisterNetFunction("SwapOrMergeItem", new LiteNetLibFunction<NetFieldUShort, NetFieldUShort>((fromIndex, toIndex) => NetFuncSwapOrMergeItem(fromIndex, toIndex)));
            RegisterNetFunction("AddAttribute", new LiteNetLibFunction<NetFieldInt>((dataId) => NetFuncAddAttribute(dataId)));
            RegisterNetFunction("AddSkill", new LiteNetLibFunction<NetFieldInt>((dataId) => NetFuncAddSkill(dataId)));
            RegisterNetFunction("AddGuildSkill", new LiteNetLibFunction<NetFieldInt>((dataId) => NetFuncAddGuildSkill(dataId)));
            RegisterNetFunction("Respawn", new LiteNetLibFunction(NetFuncRespawn));
            RegisterNetFunction("AssignHotkey", new LiteNetLibFunction<NetFieldString, NetFieldByte, NetFieldInt>((hotkeyId, type, dataId) => NetFuncAssignHotkey(hotkeyId, type, dataId)));
            RegisterNetFunction("NpcActivate", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncNpcActivate(objectId)));
            RegisterNetFunction("ShowNpcDialog", new LiteNetLibFunction<NetFieldInt>((dataId) => NetFuncShowNpcDialog(dataId)));
            RegisterNetFunction("SelectNpcDialogMenu", new LiteNetLibFunction<NetFieldByte>((menuIndex) => NetFuncSelectNpcDialogMenu(menuIndex)));
            RegisterNetFunction("BuyNpcItem", new LiteNetLibFunction<NetFieldUShort, NetFieldShort>((itemIndex, amount) => NetFuncBuyNpcItem(itemIndex, amount)));
            RegisterNetFunction("EnterWarp", new LiteNetLibFunction(() => NetFuncEnterWarp()));
            RegisterNetFunction("Build", new LiteNetLibFunction<NetFieldInt, NetFieldVector3, NetFieldQuaternion, NetFieldPackedUInt>((itemIndex, position, rotation, parentObjectId) => NetFuncBuild(itemIndex, position, rotation, parentObjectId)));
            RegisterNetFunction("DestroyBuild", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncDestroyBuild(objectId)));
            RegisterNetFunction("SellItem", new LiteNetLibFunction<NetFieldUShort, NetFieldShort>((nonEquipIndex, amount) => NetFuncSellItem(nonEquipIndex, amount)));
            RegisterNetFunction("RefineItem", new LiteNetLibFunction<NetFieldUShort>((nonEquipIndex) => NetFuncRefineItem(nonEquipIndex)));
            RegisterNetFunction("SendDealingRequest", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSendDealingRequest(objectId)));
            RegisterNetFunction("ReceiveDealingRequest", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncReceiveDealingRequest(objectId)));
            RegisterNetFunction("AcceptDealingRequest", new LiteNetLibFunction(NetFuncAcceptDealingRequest));
            RegisterNetFunction("DeclineDealingRequest", new LiteNetLibFunction(NetFuncDeclineDealingRequest));
            RegisterNetFunction("AcceptedDealingRequest", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncAcceptedDealingRequest(objectId)));
            RegisterNetFunction("SetDealingItem", new LiteNetLibFunction<NetFieldUShort, NetFieldShort>((itemIndex, amount) => NetFuncSetDealingItem(itemIndex, amount)));
            RegisterNetFunction("SetDealingGold", new LiteNetLibFunction<NetFieldInt>((gold) => NetFuncSetDealingGold(gold)));
            RegisterNetFunction("LockDealing", new LiteNetLibFunction(NetFuncLockDealing));
            RegisterNetFunction("ConfirmDealing", new LiteNetLibFunction(NetFuncConfirmDealing));
            RegisterNetFunction("CancelDealing", new LiteNetLibFunction(NetFuncCancelDealing));
            RegisterNetFunction("UpdateDealingState", new LiteNetLibFunction<NetFieldByte>((byteState) => NetFuncUpdateDealingState((DealingState)byteState.Value)));
            RegisterNetFunction("UpdateAnotherDealingState", new LiteNetLibFunction<NetFieldByte>((byteState) => NetFuncUpdateAnotherDealingState((DealingState)byteState.Value)));
            RegisterNetFunction("UpdateDealingGold", new LiteNetLibFunction<NetFieldInt>((gold) => NetFuncUpdateDealingGold(gold)));
            RegisterNetFunction("UpdateAnotherDealingGold", new LiteNetLibFunction<NetFieldInt>((gold) => NetFuncUpdateAnotherDealingGold(gold)));
            RegisterNetFunction("UpdateDealingItems", new LiteNetLibFunction<NetFieldDealingCharacterItems>((items) => NetFuncUpdateDealingItems(items)));
            RegisterNetFunction("UpdateAnotherDealingItems", new LiteNetLibFunction<NetFieldDealingCharacterItems>((items) => NetFuncUpdateAnotherDealingItems(items)));
            RegisterNetFunction("CreateParty", new LiteNetLibFunction<NetFieldBool, NetFieldBool>((shareExp, shareItem) => NetFuncCreateParty(shareExp, shareItem)));
            RegisterNetFunction("ChangePartyLeader", new LiteNetLibFunction<NetFieldString>((characterId) => NetFuncChangePartyLeader(characterId)));
            RegisterNetFunction("PartySetting", new LiteNetLibFunction<NetFieldBool, NetFieldBool>((shareExp, shareItem) => NetFuncPartySetting(shareExp, shareItem)));
            RegisterNetFunction("SendPartyInvitation", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSendPartyInvitation(objectId)));
            RegisterNetFunction("ReceivePartyInvitation", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncReceivePartyInvitation(objectId)));
            RegisterNetFunction("AcceptPartyInvitation", new LiteNetLibFunction(NetFuncAcceptPartyInvitation));
            RegisterNetFunction("DeclinePartyInvitation", new LiteNetLibFunction(NetFuncDeclinePartyInvitation));
            RegisterNetFunction("KickFromParty", new LiteNetLibFunction<NetFieldString>((characterId) => NetFuncKickFromParty(characterId)));
            RegisterNetFunction("LeaveParty", new LiteNetLibFunction(NetFuncLeaveParty));
            RegisterNetFunction("CreateGuild", new LiteNetLibFunction<NetFieldString>((guildName) => NetFuncCreateGuild(guildName)));
            RegisterNetFunction("ChangeGuildLeader", new LiteNetLibFunction<NetFieldString>((characterId) => NetFuncChangeGuildLeader(characterId)));
            RegisterNetFunction("SetGuildMessage", new LiteNetLibFunction<NetFieldString>((message) => NetFuncSetGuildMessage(message)));
            RegisterNetFunction("SetGuildRole", new LiteNetLibFunction<NetFieldByte, NetFieldString, NetFieldBool, NetFieldBool, NetFieldByte>((guildRole, name, canInvite, canKick, shareExpPercentage) => NetFuncSetGuildRole(guildRole, name, canInvite, canKick, shareExpPercentage)));
            RegisterNetFunction("SetGuildMemberRole", new LiteNetLibFunction<NetFieldString, NetFieldByte>((characterId, role) => NetFuncSetGuildMemberRole(characterId, role)));
            RegisterNetFunction("SendGuildInvitation", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSendGuildInvitation(objectId)));
            RegisterNetFunction("ReceiveGuildInvitation", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncReceiveGuildInvitation(objectId)));
            RegisterNetFunction("AcceptGuildInvitation", new LiteNetLibFunction(NetFuncAcceptGuildInvitation));
            RegisterNetFunction("DeclineGuildInvitation", new LiteNetLibFunction(NetFuncDeclineGuildInvitation));
            RegisterNetFunction("KickFromGuild", new LiteNetLibFunction<NetFieldString>((characterId) => NetFuncKickFromGuild(characterId)));
            RegisterNetFunction("LeaveGuild", new LiteNetLibFunction(NetFuncLeaveGuild));
        }

        protected override void EntityOnSetOwnerClient()
        {
            base.EntityOnSetOwnerClient();

            // Setup relates elements
            if (IsOwnerClient)
            {
                if (BasePlayerCharacterController.Singleton == null)
                {
                    if (controllerPrefab != null)
                    {
                        var controller = Instantiate(controllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else if (GameInstance.defaultControllerPrefab != null)
                    {
                        var controller = Instantiate(GameInstance.defaultControllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else
                        Debug.LogWarning("[BasePlayerCharacterEntity] `controllerPrefab` is empty so it cannot be instantiated");
                }
                if (GameInstance.owningCharacterObjects != null && GameInstance.owningCharacterObjects.Length > 0)
                {
                    foreach (var obj in GameInstance.owningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, CacheTransform.position, CacheTransform.rotation, CacheTransform);
                    }
                }
                if (GameInstance.owningCharacterMiniMapObjects != null && GameInstance.owningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (var obj in GameInstance.owningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapElementContainer.position, MiniMapElementContainer.rotation, MiniMapElementContainer);
                    }
                }
                if (GameInstance.owningCharacterUI != null)
                    InstantiateUI(GameInstance.owningCharacterUI);
            }
            else
            {
                if (GameInstance.nonOwningCharacterUI != null)
                    InstantiateUI(GameInstance.nonOwningCharacterUI);
            }
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            statPoint.onChange -= OnStatPointChange;
            skillPoint.onChange -= OnSkillPointChange;
            gold.onChange += OnGoldChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }
    }
}
