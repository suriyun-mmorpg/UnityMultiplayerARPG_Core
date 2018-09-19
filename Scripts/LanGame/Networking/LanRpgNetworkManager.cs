using System.Collections;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class LanRpgNetworkManager : BaseGameNetworkManager
    {
        public static LanRpgNetworkManager Singleton { get; protected set; }
        public enum GameStartType
        {
            Client,
            Host,
            SinglePlayer,
        }

        public float autoSaveDuration = 2f;
        public GameStartType startType;
        public PlayerCharacterData selectedCharacter;
        private float lastSaveTime;
        private int nextPartyId = 1;

        protected override void Awake()
        {
            Singleton = this;
            doNotDestroyOnSceneChanges = true;
            base.Awake();
        }

        public void StartGame()
        {
            var gameServiceConnection = gameInstance.NetworkSetting;
            switch (startType)
            {
                case GameStartType.Host:
                    networkPort = gameServiceConnection.networkPort;
                    maxConnections = gameServiceConnection.maxConnections;
                    StartHost(false);
                    break;
                case GameStartType.SinglePlayer:
                    StartHost(true);
                    break;
                case GameStartType.Client:
                    networkPort = gameServiceConnection.networkPort;
                    StartClient();
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();
            tempUnscaledTime = Time.unscaledTime;
            if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
            {
                Profiler.BeginSample("LanRpgNetworkManager - Save Data");
                var owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && IsNetworkActive)
                {
                    owningCharacter.SavePersistentCharacterData();
                    if (IsServer)
                        SaveWorld();
                }
                Profiler.EndSample();
                lastSaveTime = tempUnscaledTime;
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            nextPartyId = 1;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var connectId = peer.ConnectId;
            UnregisterPlayerCharacter(peer);
            base.OnPeerDisconnected(peer, disconnectInfo);
        }

        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            selectedCharacter.SerializeCharacterData(writer);
        }

        public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetPeer peer, NetDataReader reader)
        {
            if (LogDev) Debug.Log("[LanRpgNetworkManager] Deserializing client ready extra");
            var playerCharacterData = new PlayerCharacterData().DeserializeCharacterData(reader);
            var dataId = playerCharacterData.DataId;
            PlayerCharacter playerCharacter;
            if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out playerCharacter) || playerCharacter.entityPrefab == null)
            {
                Debug.LogError("[LanRpgNetworkManager] Cannot find player character with data Id: " + dataId);
                return;
            }
            var playerCharacterPrefab = playerCharacter.entityPrefab;
            var identity = Assets.NetworkSpawn(playerCharacterPrefab.Identity.HashAssetId, playerCharacterData.CurrentPosition, Quaternion.identity, 0, peer.ConnectId);
            var playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
            playerCharacterData.CloneTo(playerCharacterEntity);
            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
                playerCharacterEntity.RequestOnRespawn();
            else
                playerCharacterEntity.RequestOnDead();
            RegisterPlayerCharacter(peer, playerCharacterEntity);
            // Load world by owner character id
            if (playerCharacterEntity.IsOwnerClient)
            {
                var worldSaveData = new WorldSaveData();
                worldSaveData.LoadPersistentData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
                StartCoroutine(SpawnBuildingsAndHarvestables(worldSaveData));
            }
        }

        IEnumerator SpawnBuildingsAndHarvestables(WorldSaveData worldSaveData)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            // Spawn buildings
            foreach (var building in worldSaveData.buildings)
            {
                CreateBuildingEntity(building, true);
            }
            // Spawn harvestables
            var harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (var harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
        }

        private void SaveWorld()
        {
            // Save building entities / Tree / Rocks
            var playerCharacterEntity = BasePlayerCharacterController.OwningCharacter;
            var worldSaveData = new WorldSaveData();
            foreach (var building in buildingEntities.Values)
            {
                worldSaveData.buildings.Add(new BuildingSaveData()
                {
                    Id = building.Id,
                    ParentId = building.ParentId,
                    DataId = building.DataId,
                    Position = building.Position,
                    Rotation = building.Rotation,
                    CurrentHp = building.CurrentHp,
                    CreatorId = building.CreatorId,
                    CreatorName = building.CreatorName,
                });
            }
            worldSaveData.SavePersistentWorldData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
        }

        #region Implement Abstract Functions
        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
            {
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
                return;
            }
        }

        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = nextPartyId++;
            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
        }

        public override void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            party.Setting(shareExp, shareItem);
            parties[partyId] = party;
        }

        public override void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            var partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            if (party.CountMember() == gameInstance.maxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return;
            }
            party.AddMember(acceptCharacterEntity);
            parties[partyId] = party;
            acceptCharacterEntity.PartyId = partyId;
        }

        public override void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
                memberCharacterEntity.PartyId = 0;
            party.RemoveMember(characterId);
            parties[partyId] = party;
        }

        public override void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (party.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in party.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                        memberCharacterEntity.PartyId = 0;
                }
                parties.Remove(partyId);
            }
            else
            {
                playerCharacterEntity.PartyId = 0;
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
            }
        }
        #endregion
    }
}
