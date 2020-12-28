using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientGameMessageHandlers : MonoBehaviour, IClientGameMessageHandlers
    {
        public void HandleGameMessage(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientReceiveGameMessage(messageHandler.ReadMessage<GameMessage>());
        }

        public void HandleNotifyRewardExp(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardExp(messageHandler.Reader.GetPackedInt());
        }

        public void HandleNotifyRewardGold(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardGold(messageHandler.Reader.GetPackedInt());
        }

        public void HandleNotifyRewardItem(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardItem(
                messageHandler.Reader.GetPackedInt(),
                messageHandler.Reader.GetPackedShort());
        }

        public void HandleNotifyStorageOpened(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageOpened(
                (StorageType)messageHandler.Reader.GetByte(),
                messageHandler.Reader.GetString(),
                messageHandler.Reader.GetPackedUInt(),
                messageHandler.Reader.GetPackedShort(),
                messageHandler.Reader.GetPackedShort());
        }

        public void HandleNotifyStorageClosed(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageClosed();
        }

        public void HandleNotifyStorageItems(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageItemsUpdated(messageHandler.Reader.GetList<CharacterItem>());
        }

        public void HandleNotifyPartyInvitation(MessageHandlerData messageHandler)
        {
            ClientPartyActions.NotifyPartyInvitation(messageHandler.ReadMessage<PartyInvitationData>());
        }

        public void HandleNotifyGuildInvitation(MessageHandlerData messageHandler)
        {
            ClientGuildActions.NotifyGuildInvitation(messageHandler.ReadMessage<GuildInvitationData>());
        }

        public void HandleUpdatePartyMember(MessageHandlerData messageHandler)
        {
            if (GameInstance.ClientPartyHandlers.ClientParty != null)
                GameInstance.ClientPartyHandlers.ClientParty.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientPartyActions.NotifyPartyUpdated(GameInstance.ClientPartyHandlers.ClientParty);
        }

        public void HandleUpdateParty(MessageHandlerData messageHandler)
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

        public void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
            if (GameInstance.ClientGuildHandlers.ClientGuild != null)
                GameInstance.ClientGuildHandlers.ClientGuild.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientGuildActions.NotifyGuildUpdated(GameInstance.ClientGuildHandlers.ClientGuild);
        }

        public void HandleUpdateGuild(MessageHandlerData messageHandler)
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

        public void HandleUpdateFriends(MessageHandlerData messageHandler)
        {
            ClientFriendActions.NotifyFriendsUpdated(messageHandler.Reader.GetArray<SocialCharacterData>());
        }
    }
}
