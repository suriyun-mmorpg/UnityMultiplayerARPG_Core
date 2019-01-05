using System.Collections;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public partial class LanRpgNetworkManager : BaseGameNetworkManager
    {
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
        private int nextGuildId = 1;
        private Vector3? teleportPosition;

        public void StartGame()
        {
            NetworkSetting gameServiceConnection = gameInstance.NetworkSetting;
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
            float tempUnscaledTime = Time.unscaledTime;
            if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
            {
                Profiler.BeginSample("LanRpgNetworkManager - Save Data");
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && IsNetworkActive)
                {
                    selectedCharacter = owningCharacter.CloneTo(selectedCharacter);
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
            nextGuildId = 1;
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            UnregisterPlayerCharacter(connectionId);
            base.OnPeerDisconnected(connectionId, disconnectInfo);
        }

        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            selectedCharacter.SerializeCharacterData(writer);
        }

        public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader)
        {
            if (LogDev) Debug.Log("[LanRpgNetworkManager] Deserializing client ready extra");
            PlayerCharacterData playerCharacterData = new PlayerCharacterData().DeserializeCharacterData(reader);
            BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
            // If it is not allow this character data, disconnect user
            if (entityPrefab == null)
            {
                Debug.LogError("[LanRpgNetworkManager] Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                return;
            }
            if (!SceneManager.GetActiveScene().name.Equals(playerCharacterData.CurrentMapName))
                playerCharacterData.CurrentPosition = teleportPosition.HasValue ? teleportPosition.Value : CurrentMapInfo.startPosition;
            LiteNetLibIdentity identity = Assets.NetworkSpawn(entityPrefab.Identity.HashAssetId, playerCharacterData.CurrentPosition, Quaternion.identity, 0, connectionId);
            BasePlayerCharacterEntity playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
            playerCharacterData.CloneTo(playerCharacterEntity);
            // Summon saved summons
            for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
            {
                CharacterSummon summon = playerCharacterEntity.Summons[i];
                summon.Summon(playerCharacterEntity, summon.Level, summon.summonRemainsDuration, summon.Exp, summon.CurrentHp, summon.CurrentMp);
                playerCharacterEntity.Summons[i] = summon;
            }
            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
                playerCharacterEntity.RequestOnRespawn();
            else
                playerCharacterEntity.RequestOnDead();
            // Load world for first character (host)
            if (playerCharacters.Count == 0)
            {
                WorldSaveData worldSaveData = new WorldSaveData();
                worldSaveData.LoadPersistentData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
                StartCoroutine(SpawnBuildingsAndHarvestables(worldSaveData));
            }
            // Register player, will use registered player to send chat / player messages
            RegisterPlayerCharacter(connectionId, playerCharacterEntity);
        }

        IEnumerator SpawnBuildingsAndHarvestables(WorldSaveData worldSaveData)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            // Spawn buildings
            foreach (BuildingSaveData building in worldSaveData.buildings)
            {
                CreateBuildingEntity(building, true);
            }
            // Spawn harvestables
            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
        }

        private void SaveWorld()
        {
            // Save building entities / Tree / Rocks
            BasePlayerCharacterEntity playerCharacterEntity = BasePlayerCharacterController.OwningCharacter;
            WorldSaveData worldSaveData = new WorldSaveData();
            foreach (BuildingEntity buildingEntity in buildingEntities.Values)
            {
                worldSaveData.buildings.Add(new BuildingSaveData()
                {
                    Id = buildingEntity.Id,
                    ParentId = buildingEntity.ParentId,
                    DataId = buildingEntity.DataId,
                    Position = buildingEntity.Position,
                    Rotation = buildingEntity.Rotation,
                    CurrentHp = buildingEntity.CurrentHp,
                    CreatorId = buildingEntity.CreatorId,
                    CreatorName = buildingEntity.CreatorName,
                });
            }
            worldSaveData.SavePersistentWorldData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
        }

        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            base.WarpCharacter(playerCharacterEntity, mapName, position);
            long connectId = playerCharacterEntity.ConnectionId;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectId) &&
                playerCharacterEntity.IsServer &&
                playerCharacterEntity.IsOwnerClient)
            {
                teleportPosition = position;
                ServerSceneChange(mapName);
            }
        }

        #region Implement Abstract Functions
        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            CreateParty(playerCharacterEntity, shareExp, shareItem, nextPartyId++);
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            CreateGuild(playerCharacterEntity, guildName, nextGuildId++);
        }
        #endregion
    }
}
