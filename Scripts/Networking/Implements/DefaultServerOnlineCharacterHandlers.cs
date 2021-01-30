using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerOnlineCharacterHandlers : MonoBehaviour, IServerOnlineCharacterHandlers
    {
        public static readonly ConcurrentDictionary<string, float> OnlineCharacterIds = new ConcurrentDictionary<string, float>();

        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool IsCharacterOnline(string characterId)
        {
            float time;
            return OnlineCharacterIds.TryGetValue(characterId, out time) && Time.unscaledTime - time <= 5f;
        }

        public void HandleRequestOnlineCharacter(MessageHandlerData messageHandler)
        {
            string characterId = messageHandler.Reader.GetString();
            if (IsCharacterOnline(characterId))
            {
                // Notify back online character
                Manager.ServerSendPacket(messageHandler.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
                {
                    writer.Put(characterId);
                });
            }
        }

        public void MarkOnlineCharacter(string characterId)
        {
            OnlineCharacterIds.TryRemove(characterId, out _);
            OnlineCharacterIds.TryAdd(characterId, Time.unscaledTime);
        }

        public void ClearOnlineCharacters()
        {
            OnlineCharacterIds.Clear();
        }
    }
}
