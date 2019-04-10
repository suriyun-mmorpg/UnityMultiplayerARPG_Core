using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            // Sync fields
            dataId.deliveryMethod = DeliveryMethod.ReliableSequenced;
            dataId.forOwnerOnly = false;
            statPoint.deliveryMethod = DeliveryMethod.ReliableSequenced;
            statPoint.forOwnerOnly = true;
            skillPoint.deliveryMethod = DeliveryMethod.ReliableSequenced;
            skillPoint.forOwnerOnly = true;
            gold.deliveryMethod = DeliveryMethod.ReliableSequenced;
            gold.forOwnerOnly = true;
            userGold.deliveryMethod = DeliveryMethod.ReliableSequenced;
            userGold.forOwnerOnly = true;
            userCash.deliveryMethod = DeliveryMethod.ReliableSequenced;
            userCash.forOwnerOnly = true;
            partyId.deliveryMethod = DeliveryMethod.ReliableSequenced;
            partyId.forOwnerOnly = false;
            guildId.deliveryMethod = DeliveryMethod.ReliableSequenced;
            guildId.forOwnerOnly = false;
            isWarping.deliveryMethod = DeliveryMethod.ReliableSequenced;
            isWarping.forOwnerOnly = true;
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
            RegisterNetFunction<short, short>(NetFuncSwapOrMergeItem);
            RegisterNetFunction<int>(NetFuncAddAttribute);
            RegisterNetFunction<int>(NetFuncAddSkill);
            RegisterNetFunction<int>(NetFuncAddGuildSkill);
            RegisterNetFunction<int>(NetFuncUseGuildSkill);
            RegisterNetFunction(NetFuncRespawn);
            RegisterNetFunction<string, byte, int>(NetFuncAssignHotkey);
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
            RegisterNetFunction<int>(NetFuncDepositGold);
            RegisterNetFunction<int>(NetFuncWithdrawGold);
            RegisterNetFunction<int>(NetFuncDepositGuildGold);
            RegisterNetFunction<int>(NetFuncWithdrawGuildGold);
            RegisterNetFunction<PackedUInt>(NetFuncOpenStorage);
            RegisterNetFunction(NetFuncCloseStorage);
            RegisterNetFunction<PackedUInt>(NetFuncToggleDoor);
            RegisterNetFunction<PackedUInt, int>(NetFuncCraftItemByWorkbench);
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
