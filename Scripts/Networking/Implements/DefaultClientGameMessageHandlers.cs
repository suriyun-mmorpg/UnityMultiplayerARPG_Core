using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientGameMessageHandlers : MonoBehaviour, IClientGameMessageHandlers
    {
        public void RegisterGameMessageHandlers(LiteNetLibManager.LiteNetLibManager manager)
        {
            manager.RegisterClientMessage(GameNetworkingConsts.GameMessage, HandleGameMessageAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.UpdateParty, HandleUpdatePartyAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.UpdateGuild, HandleUpdateGuildAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.UpdateFriends, HandleUpdateFriendsAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyRewardExp, HandleNotifyRewardExpAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyRewardGold, HandleNotifyRewardGoldAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyRewardItem, HandleNotifyRewardItemAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyStorageOpened, HandleNotifyStorageOpenedAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyStorageClosed, HandleNotifyStorageClosedAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyStorageItemsUpdated, HandleNotifyStorageItemsUpdatedAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyPartyInvitation, HandleNotifyPartyInvitationAtClient);
            manager.RegisterClientMessage(GameNetworkingConsts.NotifyGuildInvitation, HandleNotifyGuildInvitationAtClient);
        }

        protected void HandleGameMessageAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientReceiveGameMessage(messageHandler.ReadMessage<GameMessage>());
        }

        protected void HandleNotifyRewardExpAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardExp(messageHandler.Reader.GetPackedInt());
        }

        protected void HandleNotifyRewardGoldAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardGold(messageHandler.Reader.GetPackedInt());
        }

        protected void HandleNotifyRewardItemAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardItem(
                messageHandler.Reader.GetPackedInt(),
                messageHandler.Reader.GetPackedShort());
        }

        protected void HandleNotifyStorageOpenedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageOpened(
                (StorageType)messageHandler.Reader.GetByte(),
                messageHandler.Reader.GetString(),
                messageHandler.Reader.GetPackedUInt(),
                messageHandler.Reader.GetPackedShort(),
                messageHandler.Reader.GetPackedShort());
        }

        protected void HandleNotifyStorageClosedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageClosed();
        }

        protected void HandleNotifyStorageItemsUpdatedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageItemsUpdated(messageHandler.Reader.GetList<CharacterItem>());
        }

        protected void HandleNotifyPartyInvitationAtClient(MessageHandlerData messageHandler)
        {
            ClientPartyActions.NotifyPartyInvitation(messageHandler.ReadMessage<PartyInvitationData>());
        }

        protected void HandleNotifyGuildInvitationAtClient(MessageHandlerData messageHandler)
        {
            ClientGuildActions.NotifyGuildInvitation(messageHandler.ReadMessage<GuildInvitationData>());
        }

        protected void HandleUpdatePartyMemberAtClient(MessageHandlerData messageHandler)
        {
            if (GameInstance.ClientPartyHandlers.ClientParty != null)
                GameInstance.ClientPartyHandlers.ClientParty.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientPartyActions.NotifyPartyUpdated(GameInstance.ClientPartyHandlers.ClientParty);
        }

        protected void HandleUpdatePartyAtClient(MessageHandlerData messageHandler)
        {
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (message.type == UpdatePartyMessage.UpdateType.Create)
            {
                GameInstance.ClientPartyHandlers.ClientParty = new PartyData(message.id, message.shareExp, message.shareItem, message.characterId);
            }
            else if (GameInstance.ClientPartyHandlers.ClientParty != null && GameInstance.ClientPartyHandlers.ClientParty.id == message.id)
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        GameInstance.ClientPartyHandlers.ClientParty.SetLeader(message.characterId);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        GameInstance.ClientPartyHandlers.ClientParty.Setting(message.shareExp, message.shareItem);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        GameInstance.ClientPartyHandlers.ClientParty = null;
                        break;
                }
            }
            ClientPartyActions.NotifyPartyUpdated(GameInstance.ClientPartyHandlers.ClientParty);
        }

        protected void HandleUpdateGuildMemberAtClient(MessageHandlerData messageHandler)
        {
            if (GameInstance.ClientGuildHandlers.ClientGuild != null)
                GameInstance.ClientGuildHandlers.ClientGuild.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientGuildActions.NotifyGuildUpdated(GameInstance.ClientGuildHandlers.ClientGuild);
        }

        protected void HandleUpdateGuildAtClient(MessageHandlerData messageHandler)
        {
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (message.type == UpdateGuildMessage.UpdateType.Create)
            {
                GameInstance.ClientGuildHandlers.ClientGuild = new GuildData(message.id, message.guildName, message.characterId);
            }
            else if (GameInstance.ClientGuildHandlers.ClientGuild != null && GameInstance.ClientGuildHandlers.ClientGuild.id == message.id)
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        GameInstance.ClientGuildHandlers.ClientGuild.SetLeader(message.characterId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        GameInstance.ClientGuildHandlers.ClientGuild.guildMessage = message.guildMessage;
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        GameInstance.ClientGuildHandlers.ClientGuild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        GameInstance.ClientGuildHandlers.ClientGuild.SetMemberRole(message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        GameInstance.ClientGuildHandlers.ClientGuild.SetSkillLevel(message.dataId, message.level);
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                    case UpdateGuildMessage.UpdateType.SetGold:
                        GameInstance.ClientGuildHandlers.ClientGuild.gold = message.gold;
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        GameInstance.ClientGuildHandlers.ClientGuild.level = message.level;
                        GameInstance.ClientGuildHandlers.ClientGuild.exp = message.exp;
                        GameInstance.ClientGuildHandlers.ClientGuild.skillPoint = message.skillPoint;
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        GameInstance.ClientGuildHandlers.ClientGuild = null;
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                }
            }
            ClientGuildActions.NotifyGuildUpdated(GameInstance.ClientGuildHandlers.ClientGuild);
        }

        protected void HandleUpdateFriendsAtClient(MessageHandlerData messageHandler)
        {
            ClientFriendActions.NotifyFriendsUpdated(messageHandler.Reader.GetArray<SocialCharacterData>());
        }
    }
}
