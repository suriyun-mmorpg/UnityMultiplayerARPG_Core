using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static partial class GameNetworkTransportExtensions
    {
        public static void SendEnterChat(this TransportHandler transportHandler, long? connectionId, ushort msgType, ChatChannel channel, string message, string senderName, string receiverName, int channelId)
        {
            var chatMessage = new ChatMessage();
            chatMessage.channel = channel;
            chatMessage.message = message;
            chatMessage.sender = senderName;
            chatMessage.receiver = receiverName;
            chatMessage.channelId = channelId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, chatMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, chatMessage.Serialize);
        }

        public static void SendAddSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, int level)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            updateMessage.id = id;
            updateMessage.CharacterId = characterId;
            updateMessage.data = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
            };
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendUpdateSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool isOnline, string characterId, string characterName, int dataId, int level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Update;
            updateMessage.id = id;
            updateMessage.CharacterId = characterId;
            updateMessage.isOnline = isOnline;
            updateMessage.data = new SocialCharacterData()
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
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendRemoveSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            updateMessage.id = id;
            updateMessage.CharacterId = characterId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendCreateParty(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem, string characterId)
        {
            var updateMessage = new UpdatePartyMessage();
            updateMessage.type = UpdatePartyMessage.UpdateType.Create;
            updateMessage.id = id;
            updateMessage.shareExp = shareExp;
            updateMessage.shareItem = shareItem;
            updateMessage.characterId = characterId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendChangePartyLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var updateMessage = new UpdatePartyMessage();
            updateMessage.type = UpdatePartyMessage.UpdateType.ChangeLeader;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendPartySetting(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem)
        {
            var updateMessage = new UpdatePartyMessage();
            updateMessage.type = UpdatePartyMessage.UpdateType.Setting;
            updateMessage.id = id;
            updateMessage.shareExp = shareExp;
            updateMessage.shareItem = shareItem;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendPartyTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            var updateMessage = new UpdatePartyMessage();
            updateMessage.type = UpdatePartyMessage.UpdateType.Terminate;
            updateMessage.id = id;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendCreateGuild(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string guildName, string characterId)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.Create;
            updateMessage.id = id;
            updateMessage.guildName = guildName;
            updateMessage.characterId = characterId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendChangeGuildLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.ChangeLeader;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendSetGuildMessage(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string message)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage;
            updateMessage.id = id;
            updateMessage.guildMessage = message;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendSetGuildRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.SetGuildRole;
            updateMessage.id = id;
            updateMessage.guildRole = guildRole;
            updateMessage.roleName = roleName;
            updateMessage.canInvite = canInvite;
            updateMessage.canKick = canKick;
            updateMessage.shareExpPercentage = shareExpPercentage;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendSetGuildMemberRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, byte guildRole)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.SetGuildMemberRole;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
            updateMessage.guildRole = guildRole;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendGuildTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.Terminate;
            updateMessage.id = id;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }

        public static void SendGuildLevelExpSkillPoint(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, short level, int exp, short skillPoint)
        {
            var updateMessage = new UpdateGuildMessage();
            updateMessage.type = UpdateGuildMessage.UpdateType.LevelExpSkillPoint;
            updateMessage.id = id;
            updateMessage.level = level;
            updateMessage.exp = exp;
            updateMessage.skillPoint = skillPoint;
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, updateMessage.Serialize);
        }
    }
}
