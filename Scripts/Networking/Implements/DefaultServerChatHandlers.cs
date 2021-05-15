using LiteNetLib;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerChatHandlers : MonoBehaviour, IServerChatHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void OnChatMessage(ChatMessage message)
        {
            BasePlayerCharacterEntity playerCharacter;
            switch (message.channel)
            {
                case ChatChannel.Local:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(message.sender, out playerCharacter))
                    {
                        string gmCommand;
                        if (GameInstance.Singleton.GMCommands.IsGMCommand(message.message, out gmCommand) &&
                            GameInstance.Singleton.GMCommands.CanUseGMCommand(playerCharacter, gmCommand))
                        {
                            // If it's gm command and sender's user level > 0, handle gm commands
                            GameInstance.Singleton.GMCommands.HandleGMCommand(message.sender, message.message);
                        }
                        else
                        {
                            // Send messages to nearby characters
                            List<BasePlayerCharacterEntity> receivers = playerCharacter.FindCharacters<BasePlayerCharacterEntity>(GameInstance.Singleton.localChatDistance, false, true, true, true);
                            foreach (BasePlayerCharacterEntity receiver in receivers)
                            {
                                Manager.ServerSendPacket(receiver.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                            // Send messages to sender
                            Manager.ServerSendPacket(playerCharacter.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                        }
                    }
                    break;
                case ChatChannel.Global:
                    if (!string.IsNullOrEmpty(message.sender))
                    {
                        // Send message to all clients
                        Manager.ServerSendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Whisper:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(message.sender, out playerCharacter))
                    {
                        // If found sender send whisper message to sender
                        Manager.ServerSendPacket(playerCharacter.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(message.receiver, out playerCharacter))
                    {
                        // If found receiver send whisper message to receiver
                        Manager.ServerSendPacket(playerCharacter.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Party:
                    PartyData party;
                    if (GameInstance.ServerPartyHandlers.TryGetParty(message.channelId, out party))
                    {
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                Manager.ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If party member is online, send party message to the member
                                Manager.ServerSendPacket(playerCharacter.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.Guild:
                    GuildData guild;
                    if (GameInstance.ServerGuildHandlers.TryGetGuild(message.channelId, out guild))
                    {
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                Manager.ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If guild member is online, send guild message to the member
                                Manager.ServerSendPacket(playerCharacter.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.System:
                    if (CanSendSystemAnnounce(message.sender))
                    {
                        // Send message to all clients
                        Manager.ServerSendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
            }
        }

        public bool CanSendSystemAnnounce(string sender)
        {
            // TODO: Don't use fixed user level
            BasePlayerCharacterEntity playerCharacter;
            return (!string.IsNullOrEmpty(sender) &&
                    GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(sender, out playerCharacter) &&
                    playerCharacter.UserLevel > 0) ||
                    BaseGameNetworkManager.CHAT_SYSTEM_ANNOUNCER_SENDER.Equals(sender);
        }
    }
}
