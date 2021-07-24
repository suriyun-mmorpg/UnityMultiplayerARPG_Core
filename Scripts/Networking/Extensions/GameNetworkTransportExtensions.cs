using LiteNetLib;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public static partial class GameNetworkTransportExtensions
    {
        private static void Send(LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, INetSerializable message)
        {
            if (!connectionId.HasValue)
                manager.ClientSendPacket(0, DeliveryMethod.ReliableOrdered, msgType, message.Serialize);
            else
                manager.ServerSendPacket(connectionId.Value, 0, DeliveryMethod.ReliableOrdered, msgType, message.Serialize);
        }

        public static void SendEnterChat(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, ChatChannel channel, string message, string senderName, string receiverName, int channelId)
        {
            ChatMessage netMessage = new ChatMessage();
            netMessage.channel = channel;
            netMessage.message = message;
            netMessage.sender = senderName;
            netMessage.receiver = receiverName;
            netMessage.channelId = channelId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendAddSocialMember(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, short level)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            netMessage.socialId = id;
            netMessage.character = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
            };
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendUpdateSocialMember(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, SocialCharacterData member)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Update;
            netMessage.socialId = id;
            netMessage.character = member;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendRemoveSocialMember(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            netMessage.socialId = id;
            netMessage.character.id = characterId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendClearSocialMember(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id)
        {
            UpdateSocialMemberMessage netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Clear;
            netMessage.socialId = id;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSocialMembers(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, SocialCharacterData[] members)
        {
            UpdateSocialMembersMessage netMessage = new UpdateSocialMembersMessage();
            netMessage.members = members;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendCreateParty(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem, string characterId)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            netMessage.characterId = characterId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendChangePartyLeader(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendPartySetting(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Setting;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendPartyTerminate(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id)
        {
            UpdatePartyMessage netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendCreateGuild(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string guildName, string characterId)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.guildName = guildName;
            netMessage.characterId = characterId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendChangeGuildLeader(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string characterId)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMessage(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string message)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage;
            netMessage.id = id;
            netMessage.guildMessage = message;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMessage2(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string message)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage2;
            netMessage.id = id;
            netMessage.guildMessage = message;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildRole(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildRole;
            netMessage.id = id;
            netMessage.guildRole = guildRole;
            netMessage.roleName = roleName;
            netMessage.canInvite = canInvite;
            netMessage.canKick = canKick;
            netMessage.shareExpPercentage = shareExpPercentage;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMemberRole(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string characterId, byte guildRole)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMemberRole;
            netMessage.id = id;
            netMessage.characterId = characterId;
            netMessage.guildRole = guildRole;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendGuildTerminate(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildLevelExpSkillPoint(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, short level, int exp, short skillPoint)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.LevelExpSkillPoint;
            netMessage.id = id;
            netMessage.level = level;
            netMessage.exp = exp;
            netMessage.skillPoint = skillPoint;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildSkillLevel(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, int dataId, short level)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetSkillLevel;
            netMessage.id = id;
            netMessage.dataId = dataId;
            netMessage.level = level;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildGold(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, int gold)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGold;
            netMessage.id = id;
            netMessage.gold = gold;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildScore(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, int score)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetScore;
            netMessage.id = id;
            netMessage.score = score;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildOptions(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, string options)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetOptions;
            netMessage.id = id;
            netMessage.options = options;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildAutoAcceptRequests(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, bool autoAcceptRequests)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetAutoAcceptRequests;
            netMessage.id = id;
            netMessage.autoAcceptRequests = autoAcceptRequests;
            Send(manager, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildRank(this LiteNetLibManager.LiteNetLibManager manager, long? connectionId, ushort msgType, int id, int rank)
        {
            UpdateGuildMessage netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetRank;
            netMessage.id = id;
            netMessage.rank = rank;
            Send(manager, connectionId, msgType, netMessage);
        }
    }
}
