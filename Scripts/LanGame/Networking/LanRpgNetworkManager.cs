using System.Collections;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class LanRpgNetworkManager : BaseGameNetworkManager
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
            if (Time.unscaledTime - lastSaveTime > autoSaveDuration)
            {
                var owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && IsNetworkActive)
                {
                    owningCharacter.SavePersistentCharacterData();
                    if (IsServer)
                        SaveWorld();
                }
                lastSaveTime = Time.unscaledTime;
            }
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
            identity.SendInitSyncFields(peer);
            identity.SendInitSyncLists(peer);
            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
                playerCharacterEntity.RequestOnRespawn(true);
            else
                playerCharacterEntity.RequestOnDead(true);
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
    }
}
