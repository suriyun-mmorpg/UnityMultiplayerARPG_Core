using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Profiling;
using Cysharp.Threading.Tasks;

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

        public enum EnableGmCommandType
        {
            Everyone,
            HostOnly,
        }

        public float autoSaveDuration = 2f;
        public GameStartType startType;
        public PlayerCharacterData selectedCharacter;
        public EnableGmCommandType enableGmCommands;
        private float lastSaveTime;
        private Vector3? teleportPosition;
        private readonly Dictionary<long, PlayerCharacterData> pendingSpawnPlayerCharacters = new Dictionary<long, PlayerCharacterData>();

        public LiteNetLibDiscovery CacheDiscovery { get; private set; }
        public BaseGameSaveSystem SaveSystem { get { return GameInstance.Singleton.SaveSystem; } }

        protected override void Awake()
        {
            base.Awake();
            CacheDiscovery = gameObject.GetOrAddComponent<LiteNetLibDiscovery>();
            // Server Handlers
            ServerUserHandlers = gameObject.GetOrAddComponent<IServerUserHandlers, DefaultServerUserHandlers>();
            ServerGameMessageHandlers = gameObject.GetOrAddComponent<IServerGameMessageHandlers, DefaultServerGameMessageHandlers>();
            ServerStorageHandlers = gameObject.GetOrAddComponent<IServerStorageHandlers, LanRpgServerStorageHandlers>();
            ServerPartyHandlers = gameObject.GetOrAddComponent<IServerPartyHandlers, DefaultServerPartyHandlers>();
            ServerGuildHandlers = gameObject.GetOrAddComponent<IServerGuildHandlers, DefaultServerGuildHandlers>();
            // Server Message Handlers
            ServerCashShopMessageHandlers = gameObject.GetOrAddComponent<IServerCashShopMessageHandlers, LanRpgServerCashShopMessageHandlers>();
            ServerStorageMessageHandlers = gameObject.GetOrAddComponent<IServerStorageMessageHandlers, LanRpgServerStorageMessageHandlers>();
            ServerInventoryMessageHandlers = gameObject.GetOrAddComponent<IServerInventoryMessageHandlers, DefaultServerInventoryMessageHandlers>();
            ServerPartyMessageHandlers = gameObject.GetOrAddComponent<IServerPartyMessageHandlers, LanRpgServerPartyMessageHandlers>();
            ServerGuildMessageHandlers = gameObject.GetOrAddComponent<IServerGuildMessageHandlers, LanRpgServerGuildMessageHandlers>();
            ServerBankMessageHandlers = gameObject.GetOrAddComponent<IServerBankMessageHandlers, LanRpgServerBankMessageHandlers>();
            // Client handlers
            ClientCashShopHandlers = gameObject.GetOrAddComponent<IClientCashShopHandlers, DefaultClientCashShopHandlers>();
            ClientMailHandlers = gameObject.GetOrAddComponent<IClientMailHandlers, DefaultClientMailHandlers>();
            ClientStorageHandlers = gameObject.GetOrAddComponent<IClientStorageHandlers, DefaultClientStorageHandlers>();
            ClientInventoryHandlers = gameObject.GetOrAddComponent<IClientInventoryHandlers, DefaultClientInventoryHandlers>();
            ClientPartyHandlers = gameObject.GetOrAddComponent<IClientPartyHandlers, DefaultClientPartyHandlers>();
            ClientGuildHandlers = gameObject.GetOrAddComponent<IClientGuildHandlers, DefaultClientGuildHandlers>();
            ClientFriendHandlers = gameObject.GetOrAddComponent<IClientFriendHandlers, DefaultClientFriendHandlers>();
            ClientBankHandlers = gameObject.GetOrAddComponent<IClientBankHandlers, DefaultClientBankHandlers>();
            ClientUserHandlers = gameObject.GetOrAddComponent<IClientUserHandlers, DefaultClientUserHandlers>();
        }

        public void StartGame()
        {
            NetworkSetting gameServiceConnection = CurrentGameInstance.NetworkSetting;
            switch (startType)
            {
                case GameStartType.Host:
                    SetMapInfo(selectedCharacter.CurrentMapName);
                    Assets.onlineScene.SceneName = CurrentMapInfo.GetSceneName();
                    networkPort = gameServiceConnection.networkPort;
                    maxConnections = gameServiceConnection.maxConnections;
                    StartHost(false);
                    // Set discovery data by selected character
                    CacheDiscovery.data = JsonUtility.ToJson(new DiscoveryData()
                    {
                        id = selectedCharacter.Id,
                        characterName = selectedCharacter.CharacterName,
                        level = selectedCharacter.Level
                    });
                    // Stop discovery client because game started
                    CacheDiscovery.StopClient();
                    // Start discovery server to allow clients to connect
                    CacheDiscovery.StartServer();
                    break;
                case GameStartType.SinglePlayer:
                    SetMapInfo(selectedCharacter.CurrentMapName);
                    Assets.onlineScene.SceneName = CurrentMapInfo.GetSceneName();
                    StartHost(true);
                    // Stop discovery client because game started
                    CacheDiscovery.StopClient();
                    break;
                case GameStartType.Client:
                    networkPort = gameServiceConnection.networkPort;
                    StartClient();
                    // Stop discovery client because game started
                    CacheDiscovery.StopClient();
                    break;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SaveSystem.OnServerStart();
        }

        protected override async UniTask PreSpawnEntities()
        {
            await SaveSystem.PreSpawnEntities(selectedCharacter, BuildingEntities, ServerStorageHandlers.GetAllStorageItems());
        }

        public override void OnStopHost()
        {
            base.OnStopHost();
            // Stop both client and server
            CacheDiscovery.StopClient();
            CacheDiscovery.StopServer();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            float tempUnscaledTime = Time.unscaledTime;
            if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
            {
                Profiler.BeginSample("LanRpgNetworkManager - Save Data");
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && IsClientConnected)
                {
                    SaveSystem.SaveCharacter(owningCharacter);
                    if (IsServer)
                    {
                        SaveSystem.SaveWorld(owningCharacter, BuildingEntities);
                        SaveSystem.SaveStorage(owningCharacter, ServerStorageHandlers.GetAllStorageItems());
                    }
                }
                Profiler.EndSample();
                lastSaveTime = tempUnscaledTime;
            }

            if (IsServer && pendingSpawnPlayerCharacters.Count > 0 && isReadyToInstantiatePlayers)
            {
                // Spawn pending player characters
                LiteNetLibPlayer player;
                foreach (KeyValuePair<long, PlayerCharacterData> spawnPlayerCharacter in pendingSpawnPlayerCharacters)
                {
                    if (!Players.TryGetValue(spawnPlayerCharacter.Key, out player))
                        continue;
                    player.IsReady = true;
                    SpawnPlayerCharacter(spawnPlayerCharacter.Key, spawnPlayerCharacter.Value);
                }
                pendingSpawnPlayerCharacters.Clear();
            }
        }

        public override void UnregisterPlayerCharacter(long connectionId)
        {
            BasePlayerCharacterEntity playerCharacter;
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out playerCharacter))
                ServerStorageHandlers.CloseStorage(playerCharacter).Forget();
            base.UnregisterPlayerCharacter(connectionId);
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            UnregisterPlayerCharacter(connectionId);
            base.OnPeerDisconnected(connectionId, disconnectInfo);
        }

        public override void SerializeClientReadyData(NetDataWriter writer)
        {
            selectedCharacter.SerializeCharacterData(writer);
        }

        public override async UniTask<bool> DeserializeClientReadyData(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader)
        {
            await UniTask.Yield();

            if (!isReadyToInstantiatePlayers)
            {
                // Not ready to instantiate objects, add spawning player character to pending dictionary
                if (LogDev) Logging.Log("[LanRpgNetworkManager] Not ready to deserializing client ready extra");
                if (!pendingSpawnPlayerCharacters.ContainsKey(connectionId))
                    pendingSpawnPlayerCharacters.Add(connectionId, new PlayerCharacterData().DeserializeCharacterData(reader));
                return false;
            }

            if (LogDev) Logging.Log("[LanRpgNetworkManager] Deserializing client ready extra");
            SpawnPlayerCharacter(connectionId, new PlayerCharacterData().DeserializeCharacterData(reader));
            return true;
        }

        private void SpawnPlayerCharacter(long connectionId, PlayerCharacterData playerCharacterData)
        {
            BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
            // If it is not allow this character data, disconnect user
            if (entityPrefab == null)
            {
                Logging.LogError("[LanRpgNetworkManager] Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                return;
            }
            if (!CurrentMapInfo.Id.Equals(playerCharacterData.CurrentMapName))
                playerCharacterData.CurrentPosition = teleportPosition.HasValue ? teleportPosition.Value : CurrentMapInfo.StartPosition;
            Quaternion characterRotation = Quaternion.identity;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                characterRotation = Quaternion.Euler(playerCharacterData.CurrentRotation);
            GameObject spawnObj = Instantiate(entityPrefab.gameObject, playerCharacterData.CurrentPosition, characterRotation);
            BasePlayerCharacterEntity playerCharacterEntity = spawnObj.GetComponent<BasePlayerCharacterEntity>();
            playerCharacterData.CloneTo(playerCharacterEntity);
            Assets.NetworkSpawn(spawnObj, 0, connectionId);

            // Set user Id
            playerCharacterEntity.UserId = playerCharacterEntity.Id;

            // Enable GM commands in Singleplayer / LAN mode
            // TODO: Don't use fixed user level
            if (enableGmCommands == EnableGmCommandType.Everyone)
                playerCharacterEntity.UserLevel = 1;

            // Load data for first character (host)
            if (ServerUserHandlers.PlayerCharactersCount == 0)
            {
                if (enableGmCommands == EnableGmCommandType.HostOnly)
                    playerCharacterEntity.UserLevel = 1;
            }

            // Summon saved summons
            for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
            {
                CharacterSummon summon = playerCharacterEntity.Summons[i];
                summon.Summon(playerCharacterEntity, summon.Level, summon.summonRemainsDuration, summon.Exp, summon.CurrentHp, summon.CurrentMp);
                playerCharacterEntity.Summons[i] = summon;
            }

            // Summon saved mount entity
            if (GameInstance.VehicleEntities.ContainsKey(playerCharacterData.MountDataId))
                playerCharacterEntity.Mount(GameInstance.VehicleEntities[playerCharacterData.MountDataId]);

            // Force make caches, to calculate current stats to fill empty slots items
            playerCharacterEntity.ForceMakeCaches();
            playerCharacterEntity.FillEmptySlots();

            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
                playerCharacterEntity.CallAllOnRespawn();
            else
                playerCharacterEntity.CallAllOnDead();

            // Register player, will use registered player to send chat / player messages
            RegisterPlayerCharacter(connectionId, playerCharacterEntity);

            SocialCharacterData[] members;
            // Set guild id
            if (ServerGuildHandlers.GuildsCount > 0)
            {
                foreach (GuildData guild in ServerGuildHandlers.GetGuilds())
                {
                    members = guild.GetMembers();
                    for (int i = 0; i < members.Length; ++i)
                    {
                        if (members[i].id.Equals(playerCharacterEntity.Id))
                        {
                            playerCharacterEntity.GuildId = guild.id;
                            break;
                        }
                    }
                    if (playerCharacterEntity.GuildId > 0)
                        break;
                }
            }
            // Set party id
            if (ServerPartyHandlers.PartiesCount > 0)
            {
                foreach (PartyData party in ServerPartyHandlers.GetParties())
                {
                    members = party.GetMembers();
                    for (int i = 0; i < members.Length; ++i)
                    {
                        if (members[i].id.Equals(playerCharacterEntity.Id))
                        {
                            playerCharacterEntity.PartyId = party.id;
                            break;
                        }
                    }
                    if (playerCharacterEntity.PartyId > 0)
                        break;
                }
            }
        }

        protected override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName) || (mapName.Equals(CurrentMapInfo.Id) && !IsInstanceMap()))
            {
                if (overrideRotation)
                    playerCharacterEntity.CurrentRotation = rotation;
                playerCharacterEntity.Teleport(position);
                return;
            }

            BaseMapInfo mapInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                playerCharacterEntity.IsServer &&
                playerCharacterEntity.IsOwnerClient &&
                GameInstance.MapInfos.TryGetValue(mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Save data before warp
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                SaveSystem.SaveWorld(owningCharacter, BuildingEntities);
                SaveSystem.SaveStorage(owningCharacter, ServerStorageHandlers.GetAllStorageItems());
                BuildingEntities.Clear();
                ServerStorageHandlers.ClearStorage();
                SetMapInfo(mapInfo);
                teleportPosition = position;
                if (owningCharacter != null)
                {
                    selectedCharacter = owningCharacter.CloneTo(selectedCharacter);
                    selectedCharacter.CurrentMapName = mapInfo.Id;
                    selectedCharacter.CurrentPosition = position;
                    if (overrideRotation)
                        selectedCharacter.CurrentRotation = rotation;
                    SaveSystem.SaveCharacter(selectedCharacter);
                }
                // Unregister all players characters to register later after map changed
                foreach (LiteNetLibPlayer player in GetPlayers())
                {
                    UnregisterPlayerCharacter(player.ConnectionId);
                }
                if (owningCharacter != null)
                {
                    // Destroy owning character to avoid save while warp
                    owningCharacter.NetworkDestroy();
                }
                ServerSceneChange(mapInfo.Scene);
            }
        }

        protected override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            // For now just warp follow host
            // TODO: May add instance by load scene additive and offsets for LAN mode
            WarpCharacter(playerCharacterEntity, mapName, position, overrideRotation, rotation);
        }

        protected override bool IsInstanceMap()
        {
            return false;
        }
    }
}
