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

        public static void SendAddPartyMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, int level)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
            updateMessage.member = new SocialCharacterData()
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

        public static void SendRemovePartyMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
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

        public static void SendAddGuildMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, int level)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
            updateMessage.member = new SocialCharacterData()
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

        public static void SendRemoveGuildMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var updateMessage = new UpdateSocialMemberMessage();
            updateMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            updateMessage.id = id;
            updateMessage.characterId = characterId;
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
            updateMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage;
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
    }
}
