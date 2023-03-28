using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerCharacterHandlers : MonoBehaviour, IServerCharacterHandlers
    {
        public static readonly ConcurrentDictionary<string, float> OnlineCharacterIds = new ConcurrentDictionary<string, float>();

        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void HandleRequestOnlineCharacter(MessageHandlerData messageHandler)
        {
            string characterId = messageHandler.Reader.GetString();
            if (string.IsNullOrEmpty(characterId))
                return;
            if (OnlineCharacterIds.TryGetValue(characterId, out float lastOnlineTime))
            {
                // Notify back online character
                Manager.ServerSendPacket(messageHandler.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
                {
                    writer.Put(characterId);
                    writer.PutPackedInt(Mathf.FloorToInt(Time.unscaledTime - lastOnlineTime));
                });
            }
            // NOTE: For MMO games, it should get offline offsets from database for exact offline offsets
        }

        public void MarkOnlineCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return;
            OnlineCharacterIds.TryRemove(characterId, out _);
            OnlineCharacterIds.TryAdd(characterId, Time.unscaledTime);
        }

        public void ClearOnlineCharacters()
        {
            OnlineCharacterIds.Clear();
        }

        public void Respawn(int option, IPlayerCharacterData playerCharacter)
        {
            GameInstance.Singleton.GameplayRule.OnCharacterRespawn(playerCharacter);
            WarpPortalType respawnPortalType = WarpPortalType.Default;
            string respawnMapName = playerCharacter.RespawnMapName;
            Vector3 respawnPosition = playerCharacter.RespawnPosition;
            bool respawnOverrideRotation = false;
            Vector3 respawnRotation = Vector3.zero;
            if (BaseGameNetworkManager.CurrentMapInfo != null)
                BaseGameNetworkManager.CurrentMapInfo.GetRespawnPoint(playerCharacter, out respawnPortalType, out respawnMapName, out respawnPosition, out respawnOverrideRotation, out respawnRotation);
            if (playerCharacter is BasePlayerCharacterEntity entity)
            {
                switch (respawnPortalType)
                {
                    case WarpPortalType.Default:
                        BaseGameNetworkManager.Singleton.WarpCharacter(entity, respawnMapName, respawnPosition, respawnOverrideRotation, respawnRotation);
                        break;
                    case WarpPortalType.EnterInstance:
                        BaseGameNetworkManager.Singleton.WarpCharacterToInstance(entity, respawnMapName, respawnPosition, respawnOverrideRotation, respawnRotation);
                        break;
                }
                entity.OnRespawn();
            }
            else
            {
                playerCharacter.CurrentMapName = respawnMapName;
                playerCharacter.CurrentPosition = respawnPosition;
            }
        }
    }
}
