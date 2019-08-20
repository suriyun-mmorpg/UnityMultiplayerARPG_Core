using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static partial class GameNetworkTransportExtensions
    {
        private static void Send(TransportHandler transportHandler, long? connectionId, ushort msgType, INetSerializable message)
        {
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(DeliveryMethod.ReliableOrdered, msgType, message.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, DeliveryMethod.ReliableOrdered, msgType, message.Serialize);
        }

        public static void SendEnterChat(this TransportHandler transportHandler, long? connectionId, ushort msgType, ChatChannel channel, string message, string senderName, string receiverName, int channelId)
        {
            ChatMessage netMessage = new ChatMessage();
            netMessage.channel = channel;
            netMessage.message = message;
            netMessage.sender = senderName;
            netMessage.receiver = receiverName;
            netMessage.channelId = channelId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendAddSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, short level)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            netMessage.id = id;
            netMessage.data = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
            };
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendUpdateSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Update;
            netMessage.id = id;
            netMessage.isOnline = isOnline;
            netMessage.data = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
                currentHp = currentHp,
                maxHp = maxHp,
                currentMp = currentMp,
                maxMp = maxMp,
            };
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendRemoveSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            netMessage.id = id;
            netMessage.data.id = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendClearSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Clear;
            netMessage.id = id;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSocialMembers(this TransportHandler transportHandler, long? connectionId, ushort msgType, SocialCharacterData[] members)
        {
            UpdateSocialMembersMessage netMessage = new UpdateSocialMembersMessage();
            netMessage.members = members;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendCreateParty(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem, string characterId)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendChangePartyLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendPartySetting(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Setting;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendPartyTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendCreateGuild(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string guildName, string characterId)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.guildName = guildName;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendChangeGuildLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMessage(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string message)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage;
            netMessage.id = id;
            netMessage.guildMessage = message;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildRole;
            netMessage.id = id;
            netMessage.guildRole = guildRole;
            netMessage.roleName = roleName;
            netMessage.canInvite = canInvite;
            netMessage.canKick = canKick;
            netMessage.shareExpPercentage = shareExpPercentage;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMemberRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, byte guildRole)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMemberRole;
            netMessage.id = id;
            netMessage.characterId = characterId;
            netMessage.guildRole = guildRole;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendGuildTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendGuildLevelExpSkillPoint(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, short level, int exp, short skillPoint)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.LevelExpSkillPoint;
            netMessage.id = id;
            netMessage.level = level;
            netMessage.exp = exp;
            netMessage.skillPoint = skillPoint;
            Send(transportHandler, connectionId, msgType, netMessage);
        }
        
        public static void SendSetGuildSkillLevel(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, int dataId, short level)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetSkillLevel;
            netMessage.id = id;
            netMessage.dataId = dataId;
            netMessage.level = level;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildGold(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, int gold)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGold;
            netMessage.id = id;
            netMessage.gold = gold;
            Send(transportHandler, connectionId, msgType, netMessage);
        }
    }
}
