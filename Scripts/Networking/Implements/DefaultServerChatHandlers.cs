using LiteNetLib;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerChatHandlers : MonoBehaviour, IServerChatHandlers
    {
        public const float CHAT_DELAY = 3f;
        private ConcurrentDictionary<string, float> _characterChatTimes = new ConcurrentDictionary<string, float>();
        private ConcurrentDictionary<string, int> _characterChatFloods = new ConcurrentDictionary<string, int>();
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void OnChatMessage(ChatMessage message)
        {
            message.timestamp = BaseGameNetworkManager.Singleton.ServerTimestamp;
            long connectionId;
            switch (message.channel)
            {
                case ChatChannel.Local:
                    IPlayerCharacterData playerCharacter = null;
                    if (!string.IsNullOrEmpty(message.senderName))
                        GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(message.senderName, out playerCharacter);
                    if (message.sendByServer || playerCharacter != null)
                    {
                        BasePlayerCharacterEntity playerCharacterEntity = playerCharacter == null ? null : playerCharacter as BasePlayerCharacterEntity;
                        if (message.sendByServer || (playerCharacterEntity != null &&
                            GameInstance.Singleton.GMCommands.IsGMCommand(message.message, out string gmCommand) &&
                            GameInstance.Singleton.GMCommands.CanUseGMCommand(playerCharacterEntity.UserLevel, gmCommand)))
                        {
                            // If it's gm command and sender's user level > 0, handle gm commands
                            string response = GameInstance.Singleton.GMCommands.HandleGMCommand(message.senderName, playerCharacterEntity, message.message);
                            if (!string.IsNullOrEmpty(response))
                            {
                                Manager.ServerSendPacket(playerCharacterEntity.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, new ChatMessage()
                                {
                                    channel = ChatChannel.System,
                                    message = response,
                                    timestamp = message.timestamp,
                                });
                            }
                        }
                        else
                        {
                            if (playerCharacterEntity != null)
                            {
                                // Send messages to nearby characters
                                List<BasePlayerCharacterEntity> receivers = playerCharacterEntity.FindEntities<BasePlayerCharacterEntity>(GameInstance.Singleton.localChatDistance, false, true, true, true, GameInstance.Singleton.playerLayer.Mask | GameInstance.Singleton.playingLayer.Mask);
                                foreach (BasePlayerCharacterEntity receiver in receivers)
                                {
                                    Manager.ServerSendPacket(receiver.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                                }
                                // Send messages to sender
                                Manager.ServerSendPacket(playerCharacterEntity.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                            else
                            {
                                // Character is not the entity, assume that player enter chat message in lobby, so broadcast message to other players in the lobby
                                Manager.ServerSendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.Global:
                    if (!string.IsNullOrEmpty(message.senderName))
                    {
                        // Send message to all clients
                        Manager.ServerSendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Whisper:
                    if (GameInstance.ServerUserHandlers.TryGetConnectionIdByName(message.senderName, out connectionId))
                    {
                        // If found sender send whisper message to sender
                        Manager.ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    if (!string.IsNullOrEmpty(message.receiverName) && !message.receiverName.Equals(message.senderName) &&
                        GameInstance.ServerUserHandlers.TryGetConnectionIdByName(message.receiverName, out connectionId))
                    {
                        // If found receiver send whisper message to receiver
                        Manager.ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Party:
                    if (GameInstance.ServerPartyHandlers.TryGetParty(message.channelId, out PartyData party))
                    {
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out connectionId))
                            {
                                // If party member is online, send party message to the member
                                Manager.ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.Guild:
                    if (GameInstance.ServerGuildHandlers.TryGetGuild(message.channelId, out GuildData guild))
                    {
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out connectionId))
                            {
                                // If guild member is online, send guild message to the member
                                Manager.ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.System:
                    // Send message to all clients
                    Manager.ServerSendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    break;
            }
            if (!string.IsNullOrEmpty(message.senderId))
            {
                _characterChatFloods[message.senderId] = 0;
                _characterChatTimes[message.senderId] = Time.unscaledTime;
            }
        }

        public bool CanSendSystemAnnounce(string senderName)
        {
            // TODO: Don't use fixed user level
            BasePlayerCharacterEntity playerCharacter;
            return (!string.IsNullOrEmpty(senderName) &&
                    GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(senderName, out playerCharacter) &&
                    playerCharacter.UserLevel > 0) ||
                    BaseGameNetworkManager.CHAT_SYSTEM_ANNOUNCER_SENDER.Equals(senderName);
        }

        public bool ChatTooFast(string senderId)
        {
            if (!_characterChatTimes.TryGetValue(senderId, out float time))
                return false;
            return Time.unscaledTime - time < CHAT_DELAY;
        }

        public void ChatFlooded(string senderId)
        {
            if (!_characterChatFloods.TryGetValue(senderId, out int floodCount))
                floodCount = 0;
            floodCount++;
            _characterChatFloods[senderId] = floodCount;
        }
    }
}
