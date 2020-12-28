using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerGameMessageHandlers : MonoBehaviour, IServerGameMessageHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void SendGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.GameMessage, message);
        }

        public void NotifyRewardExp(long connectionId, int exp)
        {
            if (exp <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardExp, (writer) =>
            {
                writer.PutPackedInt(exp);
            });
        }

        public void NotifyRewardGold(long connectionId, int gold)
        {
            if (gold <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardGold, (writer) =>
            {
                writer.PutPackedInt(gold);
            });
        }

        public void NotifyRewardItem(long connectionId, int dataId, short amount)
        {
            if (amount <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardItem, (writer) =>
            {
                writer.PutPackedInt(dataId);
                writer.PutPackedShort(amount);
            });
        }

        // Storage
        public void NotifyStorageItems(long connectionId, List<CharacterItem> storageItems)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyStorageItemsUpdated, (writer) =>
            {
                writer.PutList(storageItems);
            });
        }

        public void NotifyStorageOpened(long connectionId, StorageType storageType, string storageOwnerId, uint objectId, short weightLimit, short slotLimit)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyStorageOpened, (writer) =>
            {
                writer.Put((byte)storageType);
                writer.Put(storageOwnerId);
                writer.PutPackedUInt(objectId);
                writer.PutPackedShort(weightLimit);
                writer.PutPackedShort(slotLimit);
            });
        }

        public void NotifyStorageClosed(long connectionId)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyStorageClosed);
        }

        // Party
        public void SendSetPartyData(long connectionId, int id, bool shareExp, bool shareItem, string leaderId)
        {
            Manager.SendCreateParty(connectionId, GameNetworkingConsts.UpdateParty, id, shareExp, shareItem, leaderId);
        }

        public void SendSetPartyLeader(long connectionId, int id, string leaderId)
        {
            Manager.SendChangePartyLeader(connectionId, GameNetworkingConsts.UpdateParty, id, leaderId);
        }

        public void SendSetPartySetting(long connectionId, int id, bool shareExp, bool shareItem)
        {
            Manager.SendPartySetting(connectionId, GameNetworkingConsts.UpdateParty, id, shareExp, shareItem);
        }

        public void SendClearPartyData(long connectionId, int id)
        {
            Manager.SendPartyTerminate(connectionId, GameNetworkingConsts.UpdateParty, id);
        }

        public void SendAddPartyMember(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            Manager.SendAddSocialMember(connectionId, GameNetworkingConsts.UpdatePartyMember, id, characterId, characterName, dataId, level);
        }

        public void SendUpdatePartyMember(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            Manager.SendUpdateSocialMember(connectionId, GameNetworkingConsts.UpdatePartyMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }

        public void SendRemovePartyMember(long connectionId, int id, string characterId)
        {
            Manager.SendRemoveSocialMember(connectionId, GameNetworkingConsts.UpdatePartyMember, id, characterId);
        }

        public void SendNotifyPartyInvitation(long connectionId, PartyInvitationData invitation)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyPartyInvitation, invitation);
        }

        // Guild
        public void SendSetGuildData(long connectionId, int id, string guildName, string leaderId)
        {
            Manager.SendCreateGuild(connectionId, GameNetworkingConsts.UpdateGuild, id, guildName, leaderId);
        }

        public void SendSetGuildLeader(long connectionId, int id, string leaderId)
        {
            Manager.SendChangeGuildLeader(connectionId, GameNetworkingConsts.UpdateGuild, id, leaderId);
        }

        public void SendSetGuildMessage(long connectionId, int id, string message)
        {
            Manager.SendSetGuildMessage(connectionId, GameNetworkingConsts.UpdateGuild, id, message);
        }

        public void SendSetGuildRole(long connectionId, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            Manager.SendSetGuildRole(connectionId, GameNetworkingConsts.UpdateGuild, id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public void SendSetGuildMemberRole(long connectionId, int id, string characterId, byte guildRole)
        {
            Manager.SendSetGuildMemberRole(connectionId, GameNetworkingConsts.UpdateGuild, id, characterId, guildRole);
        }

        public void SendClearGuildData(long connectionId, int id)
        {
            Manager.SendGuildTerminate(connectionId, GameNetworkingConsts.UpdateGuild, id);
        }

        public void SendAddGuildMember(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            Manager.SendAddSocialMember(connectionId, GameNetworkingConsts.UpdateGuildMember, id, characterId, characterName, dataId, level);
        }

        public void SendUpdateGuildMember(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            Manager.SendUpdateSocialMember(connectionId, GameNetworkingConsts.UpdateGuildMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }

        public void SendRemoveGuildMember(long connectionId, int id, string characterId)
        {
            Manager.SendRemoveSocialMember(connectionId, GameNetworkingConsts.UpdateGuildMember, id, characterId);
        }

        public void SendSetGuildSkillLevel(long connectionId, int id, int dataId, short level)
        {
            Manager.SendSetGuildSkillLevel(connectionId, GameNetworkingConsts.UpdateGuild, id, dataId, level);
        }

        public void SendSetGuildGold(long connectionId, int id, int gold)
        {
            Manager.SendSetGuildGold(connectionId, GameNetworkingConsts.UpdateGuild, id, gold);
        }

        public void SendSetGuildLevelExpSkillPoint(long connectionId, int id, short level, int exp, short skillPoint)
        {
            Manager.SendGuildLevelExpSkillPoint(connectionId, GameNetworkingConsts.UpdateGuild, id, level, exp, skillPoint);
        }

        public void SendNotifyGuildInvitation(long connectionId, GuildInvitationData invitation)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyGuildInvitation, invitation);
        }

        // Friends
        public void SendSetFriends(long connectionId, SocialCharacterData[] friends)
        {
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.UpdateFriends, (writer) =>
            {
                writer.PutArray(friends);
            });
        }
    }
}
