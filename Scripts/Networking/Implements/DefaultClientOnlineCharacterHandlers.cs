using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultClientOnlineCharacterHandlers : MonoBehaviour, IClientOnlineCharacterHandlers
    {
        public static readonly ConcurrentDictionary<string, float> OnlineRequestTimes = new ConcurrentDictionary<string, float>();
        public static readonly ConcurrentDictionary<string, float> OnlineCharacterIds = new ConcurrentDictionary<string, float>();

        public const float OnlineDuration = 5f;
        public const float RequestDuration = 5f * 0.75f;

        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool IsCharacterOnline(string characterId)
        {
            float time;
            return OnlineCharacterIds.TryGetValue(characterId, out time) && Time.unscaledTime - time <= OnlineDuration;
        }

        public void RequestOnlineCharacter(string characterId)
        {
            float lastRequestTime;
            if (OnlineRequestTimes.TryGetValue(characterId, out lastRequestTime) &&
                Time.unscaledTime - lastRequestTime <= RequestDuration)
            {
                // Requested too frequently, so skip it
                return;
            }

            OnlineRequestTimes.TryRemove(characterId, out _);
            OnlineRequestTimes.TryAdd(characterId, Time.unscaledTime);

            Manager.ClientSendPacket(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
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
