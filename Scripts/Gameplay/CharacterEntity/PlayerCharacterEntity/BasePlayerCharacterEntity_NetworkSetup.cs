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
            RegisterNetFunction<ushort, ushort>(NetFuncSwapOrMergeItem);
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
            RegisterNetFunction<ushort, short>(NetFuncBuyNpcItem);
            RegisterNetFunction(NetFuncEnterWarp);
            RegisterNetFunction<ushort, Vector3, Quaternion, PackedUInt>(NetFuncBuild);
            RegisterNetFunction<PackedUInt>(NetFuncDestroyBuild);
            RegisterNetFunction<ushort, short>(NetFuncSellItem);
            RegisterNetFunction<ushort>(NetFuncRefineItem);
            RegisterNetFunction<PackedUInt>(NetFuncSendDealingRequest);
            RegisterNetFunction<PackedUInt>(NetFuncReceiveDealingRequest);
            RegisterNetFunction(NetFuncAcceptDealingRequest);
            RegisterNetFunction(NetFuncDeclineDealingRequest);
            RegisterNetFunction<PackedUInt>(NetFuncAcceptedDealingRequest);
            RegisterNetFunction<ushort, short>(NetFuncSetDealingItem);
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
                    else if (GameInstance.defaultControllerPrefab != null)
                    {
                        BasePlayerCharacterController controller = Instantiate(GameInstance.defaultControllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else
                        Debug.LogWarning("[BasePlayerCharacterEntity] `controllerPrefab` is empty so it cannot be instantiated");
                }
                if (GameInstance.owningCharacterObjects != null && GameInstance.owningCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in GameInstance.owningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, CacheTransform.position, CacheTransform.rotation, CacheTransform);
                    }
                }
                if (GameInstance.owningCharacterMiniMapObjects != null && GameInstance.owningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in GameInstance.owningCharacterMiniMapObjects)
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
                if (GameInstance.nonOwningCharacterMiniMapObjects != null && GameInstance.nonOwningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in GameInstance.nonOwningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapElementContainer.position, MiniMapElementContainer.rotation, MiniMapElementContainer);
                    }
                }
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
