using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public override void SetTargetEntity(BaseGameEntity entity)
        {
            if (IsOwnerClient && !IsServer)
                CallNetFunction(NetFuncSetTargetEntity, FunctionReceivers.Server, new PackedUInt(entity == null ? 0 : entity.ObjectId));
            base.SetTargetEntity(entity);
        }

        public bool RequestUseItem(short index)
        {
            if (!CanUseItem())
                return false;
            CallNetFunction(NetFuncUseItem, FunctionReceivers.Server, index);
            return true;
        }

        public bool ValidateRequestUseSkillItem(short index, bool isLeftHand)
        {
            if (!CanUseItem())
                return false;

            if (index >= nonEquipItems.Count)
                return false;

            CharacterItem characterItem = nonEquipItems[index];
            if (characterItem.IsLock())
                return false;

            Item item = characterItem.GetSkillItem();
            if (item == null)
                return false;

            BaseSkill skill = item.skillLevel.skill;
            short skillLevel = item.skillLevel.level;
            if (skill == null)
                return false;

            int dataId = skill.DataId;
            float currentTime = Time.unscaledTime;
            if (!requestUseSkillErrorTime.ContainsKey(dataId))
                requestUseSkillErrorTime[dataId] = currentTime;

            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType, true))
            {
                if (!IsOwnerClient)
                    return false;

                if (Time.unscaledTime - requestUseSkillErrorTime[dataId] >= COMBATANT_MESSAGE_DELAY)
                {
                    requestUseSkillErrorTime[dataId] = Time.unscaledTime;
                    gameManager.ClientReceiveGameMessage(new GameMessage() { type = gameMessageType });
                }
                return false;
            }

            CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
            if (skill.IsAttack() && !ValidateAmmo(weapon))
            {
                if (!IsOwnerClient)
                    return false;

                if (Time.unscaledTime - requestUseSkillErrorTime[dataId] >= COMBATANT_MESSAGE_DELAY)
                {
                    requestUseSkillErrorTime[dataId] = Time.unscaledTime;
                    gameManager.ClientReceiveGameMessage(new GameMessage() { type = GameMessage.Type.NoAmmo });
                }
                return false;
            }

            return true;
        }
        
        public bool RequestUseSkillItem(short index, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            CallNetFunction(NetFuncUseSkillItem, FunctionReceivers.Server, index, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestSwapOrMergeItem(short fromIndex, short toIndex)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncSwapOrMergeItem, FunctionReceivers.Server, fromIndex, toIndex);
            return true;
        }

        public bool RequestAddAttribute(int dataId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncAddAttribute, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestAddSkill(int dataId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncAddSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestAddGuildSkill(int dataId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncAddGuildSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestUseGuildSkill(int dataId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncUseGuildSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestRespawn()
        {
            if (!IsDead())
                return false;
            CallNetFunction(NetFuncRespawn, FunctionReceivers.Server);
            return true;
        }

        public bool RequestAssignHotkey(string hotkeyId, HotkeyType type, string id)
        {
            CallNetFunction(NetFuncAssignHotkey, FunctionReceivers.Server, hotkeyId, (byte)type, id);
            return true;
        }

        public bool RequestNpcActivate(uint objectId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncNpcActivate, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestShowNpcDialog(int dataId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcDialog, ConnectionId, dataId);
            return true;
        }

        public bool RequestShowNpcRefine()
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcRefine, ConnectionId);
            return true;
        }

        public bool RequestSelectNpcDialogMenu(byte menuIndex)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncSelectNpcDialogMenu, FunctionReceivers.Server, menuIndex);
            return true;
        }

        public bool RequestBuyNpcItem(short itemIndex, short amount)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncBuyNpcItem, FunctionReceivers.Server, itemIndex, amount);
            return true;
        }

        public bool RequestEnterWarp()
        {
            if (!CanDoActions() || warpingPortal == null)
                return false;
            CallNetFunction(NetFuncEnterWarp, FunctionReceivers.Server);
            return true;
        }

        public bool RequestBuild(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(NetFuncBuild, FunctionReceivers.Server, itemIndex, position, rotation, new PackedUInt(parentObjectId));
            return true;
        }

        public bool RequestDestroyBuilding(uint objectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(NetFuncDestroyBuilding, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestSellItem(short nonEquipIndex, short amount)
        {
            if (IsDead() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(NetFuncSellItem, FunctionReceivers.Server, nonEquipIndex, amount);
            return true;
        }

        public bool RequestRefineItem(byte byteInventoryType, short index)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncRefineItem, FunctionReceivers.Server, byteInventoryType, index);
            return true;
        }

        public bool RequestEnhanceSocketItem(byte byteInventoryType, short index, int enhancerId)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncEnhanceSocketItem, FunctionReceivers.Server, byteInventoryType, index, enhancerId);
            return true;
        }

        public bool RequestRepairItem(byte byteInventoryType, short index)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncRepairItem, FunctionReceivers.Server, byteInventoryType, index);
            return true;
        }

        public bool RequestSendDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncSendDealingRequest, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceiveDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncReceiveDealingRequest, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptDealingRequest()
        {
            CallNetFunction(NetFuncAcceptDealingRequest, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclineDealingRequest()
        {
            CallNetFunction(NetFuncDeclineDealingRequest, FunctionReceivers.Server);
            return true;
        }

        public bool RequestAcceptedDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncAcceptedDealingRequest, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestSetDealingItem(short itemIndex, short amount)
        {
            CallNetFunction(NetFuncSetDealingItem, FunctionReceivers.Server, itemIndex, amount);
            return true;
        }

        public bool RequestSetDealingGold(int dealingGold)
        {
            CallNetFunction(NetFuncSetDealingGold, FunctionReceivers.Server, dealingGold);
            return true;
        }

        public bool RequestLockDealing()
        {
            CallNetFunction(NetFuncLockDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestConfirmDealing()
        {
            CallNetFunction(NetFuncConfirmDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestCancelDealing()
        {
            CallNetFunction(NetFuncCancelDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestUpdateDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateDealingState, ConnectionId, (byte)state);
            return true;
        }

        public bool RequestUpdateAnotherDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingState, ConnectionId, (byte)state);
            return true;
        }

        public bool RequestUpdateDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateDealingGold, ConnectionId, gold);
            return true;
        }

        public bool RequestUpdateAnotherDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingGold, ConnectionId, gold);
            return true;
        }

        public bool RequestUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool RequestUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool RequestCreateParty(bool shareExp, bool shareItem)
        {
            CallNetFunction(NetFuncCreateParty, FunctionReceivers.Server, shareExp, shareItem);
            return true;
        }

        public bool RequestChangePartyLeader(string characterId)
        {
            CallNetFunction(NetFuncChangePartyLeader, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestPartySetting(bool shareExp, bool shareItem)
        {
            CallNetFunction(NetFuncPartySetting, FunctionReceivers.Server, shareExp, shareItem);
            return true;
        }

        public bool RequestSendPartyInvitation(uint objectId)
        {
            CallNetFunction(NetFuncSendPartyInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceivePartyInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceivePartyInvitation, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptPartyInvitation()
        {
            CallNetFunction(NetFuncAcceptPartyInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclinePartyInvitation()
        {
            CallNetFunction(NetFuncDeclinePartyInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestKickFromParty(string characterId)
        {
            CallNetFunction(NetFuncKickFromParty, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestLeaveParty()
        {
            CallNetFunction(NetFuncLeaveParty, FunctionReceivers.Server);
            return true;
        }

        public bool RequestCreateGuild(string guildName)
        {
            CallNetFunction(NetFuncCreateGuild, FunctionReceivers.Server, guildName);
            return true;
        }

        public bool RequestChangeGuildLeader(string characterId)
        {
            CallNetFunction(NetFuncChangeGuildLeader, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestSetGuildMessage(string guildMessage)
        {
            CallNetFunction(NetFuncSetGuildMessage, FunctionReceivers.Server, guildMessage);
            return true;
        }

        public bool RequestSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CallNetFunction(NetFuncSetGuildRole, FunctionReceivers.Server, guildRole, name, canInvite, canKick, shareExpPercentage);
            return true;
        }

        public bool RequestSetGuildMemberRole(string characterId, byte guildRole)
        {
            CallNetFunction(NetFuncSetGuildMemberRole, FunctionReceivers.Server, characterId, guildRole);
            return true;
        }

        public bool RequestSendGuildInvitation(uint objectId)
        {
            CallNetFunction(NetFuncSendGuildInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceiveGuildInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceiveGuildInvitation, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptGuildInvitation()
        {
            CallNetFunction(NetFuncAcceptGuildInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclineGuildInvitation()
        {
            CallNetFunction(NetFuncDeclineGuildInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestKickFromGuild(string characterId)
        {
            CallNetFunction(NetFuncKickFromGuild, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestLeaveGuild()
        {
            CallNetFunction(NetFuncLeaveGuild, FunctionReceivers.Server);
            return true;
        }

        public bool RequestMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
            CallNetFunction(NetFuncMoveItemToStorage, FunctionReceivers.Server, nonEquipIndex, amount, storageItemIndex);
            return true;
        }

        public bool RequestMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
            CallNetFunction(NetFuncMoveItemFromStorage, FunctionReceivers.Server, storageItemIndex, amount, nonEquipIndex);
            return true;
        }

        public bool RequestSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
            CallNetFunction(NetFuncSwapOrMergeStorageItem, FunctionReceivers.Server, fromIndex, toIndex);
            return true;
        }

        public bool RequestDepositGold(int amount)
        {
            CallNetFunction(NetFuncDepositGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestWithdrawGold(int amount)
        {
            CallNetFunction(NetFuncWithdrawGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestDepositGuildGold(int amount)
        {
            CallNetFunction(NetFuncDepositGuildGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestWithdrawGuildGold(int amount)
        {
            CallNetFunction(NetFuncWithdrawGuildGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestOpenStorage(uint objectId)
        {
            CallNetFunction(NetFuncOpenStorage, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestCloseStorage()
        {
            CallNetFunction(NetFuncCloseStorage, FunctionReceivers.Server);
            return true;
        }

        public bool RequestToggleDoor(uint objectId)
        {
            CallNetFunction(NetFuncToggleDoor, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestCraftItemByWorkbench(uint objectId, int dataId)
        {
            CallNetFunction(NetFuncCraftItemByWorkbench, FunctionReceivers.Server, new PackedUInt(objectId), dataId);
            return true;
        }

        public bool RequestFindCharacters(string characterName)
        {
            CallNetFunction(NetFuncFindCharacters, FunctionReceivers.Server, characterName);
            return true;
        }

        public bool RequestAddFriend(string friendCharacterId)
        {
            CallNetFunction(NetFuncAddFriend, FunctionReceivers.Server, friendCharacterId);
            return true;
        }

        public bool RequestRemoveFriend(string friendCharacterId)
        {
            CallNetFunction(NetFuncRemoveFriend, FunctionReceivers.Server, friendCharacterId);
            return true;
        }

        public bool RequestGetFriends()
        {
            CallNetFunction(NetFuncGetFriends, FunctionReceivers.Server);
            return true;
        }
    }
}
