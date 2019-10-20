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
            // Sync fields
            dataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            dataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            factionId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            factionId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            statPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            statPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            skillPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            skillPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            gold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            gold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            userGold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userGold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            userCash.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userCash.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            partyId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            partyId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            guildId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            guildId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isWarping.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isWarping.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            // Sync lists
            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
            storageItems.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            dataId.onChange += OnDataIdChange;
            factionId.onChange += OnFactionIdChange;
            statPoint.onChange += OnStatPointChange;
            skillPoint.onChange += OnSkillPointChange;
            gold.onChange += OnGoldChange;
            userGold.onChange += OnUserGoldChange;
            userCash.onChange += OnUserCashChange;
            partyId.onChange += OnPartyIdChange;
            guildId.onChange += OnGuildIdChange;
            isWarping.onChange += OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
            storageItems.onOperation += OnStorageItemsOperation;
            // Register Network functions
            RegisterNetFunction<PackedUInt>(NetFuncSetTargetEntity);
            RegisterNetFunction<short>(NetFuncUseItem);
            RegisterNetFunction<short, bool, Vector3>(NetFuncUseSkillItem);
            RegisterNetFunction<short, short>(NetFuncSwapOrMergeItem);
            RegisterNetFunction<int>(NetFuncAddAttribute);
            RegisterNetFunction<int>(NetFuncAddSkill);
            RegisterNetFunction<int>(NetFuncAddGuildSkill);
            RegisterNetFunction<int>(NetFuncUseGuildSkill);
            RegisterNetFunction(NetFuncRespawn);
            RegisterNetFunction<string, byte, string>(NetFuncAssignHotkey);
            RegisterNetFunction<PackedUInt>(NetFuncNpcActivate);
            RegisterNetFunction<int>(NetFuncShowNpcDialog);
            RegisterNetFunction(NetFuncShowNpcRefine);
            RegisterNetFunction<byte>(NetFuncSelectNpcDialogMenu);
            RegisterNetFunction<short, short>(NetFuncBuyNpcItem);
            RegisterNetFunction(NetFuncEnterWarp);
            RegisterNetFunction<short, Vector3, Quaternion, PackedUInt>(NetFuncBuild);
            RegisterNetFunction<PackedUInt>(NetFuncDestroyBuilding);
            RegisterNetFunction<short, short>(NetFuncSellItem);
            RegisterNetFunction<byte, short>(NetFuncRefineItem);
            RegisterNetFunction<byte, short, int>(NetFuncEnhanceSocketItem);
            RegisterNetFunction<byte, short>(NetFuncRepairItem);
            RegisterNetFunction<PackedUInt>(NetFuncSendDealingRequest);
            RegisterNetFunction<PackedUInt>(NetFuncReceiveDealingRequest);
            RegisterNetFunction(NetFuncAcceptDealingRequest);
            RegisterNetFunction(NetFuncDeclineDealingRequest);
            RegisterNetFunction<PackedUInt>(NetFuncAcceptedDealingRequest);
            RegisterNetFunction<short, short>(NetFuncSetDealingItem);
            RegisterNetFunction<int>(NetFuncSetDealingGold);
            RegisterNetFunction(NetFuncLockDealing);
            RegisterNetFunction(NetFuncConfirmDealing);
            RegisterNetFunction(NetFuncCancelDealing);
            RegisterNetFunction<byte>(NetFuncUpdateDealingState);
            RegisterNetFunction<byte>(NetFuncUpdateAnotherDealingState);
            RegisterNetFunction<int>(NetFuncUpdateDealingGold);
            RegisterNetFunction<int>(NetFuncUpdateAnotherDealingGold);
            RegisterNetFunction<DealingCharacterItems>(NetFuncUpdateDealingItems);
            RegisterNetFunction<DealingCharacterItems>(NetFuncUpdateAnotherDealingItems);
            RegisterNetFunction<bool, bool>(NetFuncCreateParty);
            RegisterNetFunction<string>(NetFuncChangePartyLeader);
            RegisterNetFunction<bool, bool>(NetFuncPartySetting);
            RegisterNetFunction<PackedUInt>(NetFuncSendPartyInvitation);
            RegisterNetFunction<PackedUInt>(NetFuncReceivePartyInvitation);
            RegisterNetFunction(NetFuncAcceptPartyInvitation);
            RegisterNetFunction(NetFuncDeclinePartyInvitation);
            RegisterNetFunction<string>(NetFuncKickFromParty);
            RegisterNetFunction(NetFuncLeaveParty);
            RegisterNetFunction<string>(NetFuncCreateGuild);
            RegisterNetFunction<string>(NetFuncChangeGuildLeader);
            RegisterNetFunction<string>(NetFuncSetGuildMessage);
            RegisterNetFunction<byte, string, bool, bool, byte>(NetFuncSetGuildRole);
            RegisterNetFunction<string, byte>(NetFuncSetGuildMemberRole);
            RegisterNetFunction<PackedUInt>(NetFuncSendGuildInvitation);
            RegisterNetFunction<PackedUInt>(NetFuncReceiveGuildInvitation);
            RegisterNetFunction(NetFuncAcceptGuildInvitation);
            RegisterNetFunction(NetFuncDeclineGuildInvitation);
            RegisterNetFunction<string>(NetFuncKickFromGuild);
            RegisterNetFunction(NetFuncLeaveGuild);
            RegisterNetFunction<byte, short, short>(NetFuncShowStorage);
            RegisterNetFunction<short, short, short>(NetFuncMoveItemToStorage);
            RegisterNetFunction<short, short, short>(NetFuncMoveItemFromStorage);
            RegisterNetFunction<short, short>(NetFuncSwapOrMergeStorageItem);
            RegisterNetFunction<int>(NetFuncDepositGold);
            RegisterNetFunction<int>(NetFuncWithdrawGold);
            RegisterNetFunction<int>(NetFuncDepositGuildGold);
            RegisterNetFunction<int>(NetFuncWithdrawGuildGold);
            RegisterNetFunction<PackedUInt>(NetFuncOpenStorage);
            RegisterNetFunction(NetFuncCloseStorage);
            RegisterNetFunction<PackedUInt>(NetFuncToggleDoor);
            RegisterNetFunction<PackedUInt, int>(NetFuncCraftItemByWorkbench);
            RegisterNetFunction<string>(NetFuncFindCharacters);
            RegisterNetFunction<string>(NetFuncAddFriend);
            RegisterNetFunction<string>(NetFuncRemoveFriend);
            RegisterNetFunction(NetFuncGetFriends);
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
                        BasePlayerCharacterController controller = Instantiate(controllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else if (gameInstance.defaultControllerPrefab != null)
                    {
                        BasePlayerCharacterController controller = Instantiate(gameInstance.defaultControllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else
                        Debug.LogWarning("[BasePlayerCharacterEntity] `controllerPrefab` is empty so it cannot be instantiated");
                }
                if (gameInstance.owningCharacterObjects != null && gameInstance.owningCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in gameInstance.owningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, CacheTransform.position, CacheTransform.rotation, CacheTransform);
                    }
                }
                if (gameInstance.owningCharacterMiniMapObjects != null && gameInstance.owningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in gameInstance.owningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUITransform.position, MiniMapUITransform.rotation, MiniMapUITransform);
                    }
                }
                if (gameInstance.owningCharacterUI != null)
                    InstantiateUI(gameInstance.owningCharacterUI);
            }
            else
            {
                if (gameInstance.nonOwningCharacterMiniMapObjects != null && gameInstance.nonOwningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in gameInstance.nonOwningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUITransform.position, MiniMapUITransform.rotation, MiniMapUITransform);
                    }
                }
                if (gameInstance.nonOwningCharacterUI != null)
                    InstantiateUI(gameInstance.nonOwningCharacterUI);
            }
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            dataId.onChange -= OnDataIdChange;
            factionId.onChange -= OnFactionIdChange;
            statPoint.onChange -= OnStatPointChange;
            skillPoint.onChange -= OnSkillPointChange;
            gold.onChange -= OnGoldChange;
            userGold.onChange -= OnUserGoldChange;
            userCash.onChange -= OnUserCashChange;
            partyId.onChange -= OnPartyIdChange;
            guildId.onChange -= OnGuildIdChange;
            isWarping.onChange -= OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;
            storageItems.onOperation -= OnStorageItemsOperation;

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }
    }
}
