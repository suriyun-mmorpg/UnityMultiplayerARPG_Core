using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientOnlineCharacterHandlers : MonoBehaviour, IClientOnlineCharacterHandlers
    {
        public static readonly ConcurrentDictionary<string, float> OnlineRequestTimes = new ConcurrentDictionary<string, float>();
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

        public void RequestOnlineCharacter(string characterId)
        {
            float lastRequestTime;
            if (OnlineRequestTimes.TryGetValue(characterId, out lastRequestTime) &&
                Time.unscaledTime - lastRequestTime <= 2.5f)
            {
                // Requested too frequently, so skip it
                return;
            }

            OnlineRequestTimes.TryRemove(characterId, out _);
            OnlineRequestTimes.TryAdd(characterId, Time.unscaledTime);

            Manager.ClientSendPacket(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
            {
                writer.Put(characterId);
            });
        }

        public void HandleNotifyOnlineCharacter(MessageHandlerData messageHandler)
        {
            string characterId = messageHandler.Reader.GetString();
            OnlineCharacterIds.TryRemove(characterId, out _);
            OnlineCharacterIds.TryAdd(characterId, Time.unscaledTime);
        }

        public void ClearOnlineCharacters()
        {
            OnlineRequestTimes.Clear();
            OnlineCharacterIds.Clear();
        }
    }
}
