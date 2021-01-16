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
            if (GameInstance.ClientParty != null)
                GameInstance.ClientParty.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientPartyActions.NotifyPartyUpdated(GameInstance.ClientParty);
        }

        public void HandleUpdateParty(MessageHandlerData messageHandler)
        {
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (message.type == UpdatePartyMessage.UpdateType.Create)
            {
                GameInstance.ClientParty = new PartyData(message.id, message.shareExp, message.shareItem, message.characterId);
            }
            else if (GameInstance.ClientParty != null && GameInstance.ClientParty.id == message.id)
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        GameInstance.ClientParty.SetLeader(message.characterId);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        GameInstance.ClientParty.Setting(message.shareExp, message.shareItem);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        GameInstance.ClientParty = null;
                        break;
                }
            }
            ClientPartyActions.NotifyPartyUpdated(GameInstance.ClientParty);
        }

        public void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
            if (GameInstance.ClientGuild != null)
                GameInstance.ClientGuild.UpdateSocialGroupMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientGuildActions.NotifyGuildUpdated(GameInstance.ClientGuild);
        }

        public void HandleUpdateGuild(MessageHandlerData messageHandler)
        {
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (message.type == UpdateGuildMessage.UpdateType.Create)
            {
                GameInstance.ClientGuild = new GuildData(message.id, message.guildName, message.characterId);
            }
            else if (GameInstance.ClientGuild != null && GameInstance.ClientGuild.id == message.id)
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        GameInstance.ClientGuild.SetLeader(message.characterId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        GameInstance.ClientGuild.guildMessage = message.guildMessage;
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        GameInstance.ClientGuild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        GameInstance.ClientGuild.SetMemberRole(message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        GameInstance.ClientGuild.SetSkillLevel(message.dataId, message.level);
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                    case UpdateGuildMessage.UpdateType.SetGold:
                        GameInstance.ClientGuild.gold = message.gold;
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        GameInstance.ClientGuild.level = message.level;
                        GameInstance.ClientGuild.exp = message.exp;
                        GameInstance.ClientGuild.skillPoint = message.skillPoint;
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        GameInstance.ClientGuild = null;
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                }
            }
            ClientGuildActions.NotifyGuildUpdated(GameInstance.ClientGuild);
        }

        public void HandleUpdateFriends(MessageHandlerData messageHandler)
        {
            ClientFriendActions.NotifyFriendsUpdated(messageHandler.Reader.GetArray<SocialCharacterData>());
        }
    }
}
