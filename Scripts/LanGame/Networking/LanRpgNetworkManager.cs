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
            if (playerIdentity == null)
                return;
            var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
            playerCharacterEntity.DeserializeCharacterData(reader);
            // Notify clients that this character is spawn or dead
            if (playerCharacterEntity.CurrentHp > 0)
                playerCharacterEntity.RequestOnRespawn(true);
            else
                playerCharacterEntity.RequestOnDead(true);
            // Load world by owner character id
            if (playerCharacterEntity.IsOwnerClient)
            {
                var worldSaveData = new WorldSaveData();
                worldSaveData.LoadPersistentData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
                StartCoroutine(SpawnBuildings(worldSaveData));
            }
        }

        IEnumerator SpawnBuildings(WorldSaveData worldSaveData)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            foreach (var building in worldSaveData.buildings)
            {
                CreateBuildingEntity(building, true);
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
