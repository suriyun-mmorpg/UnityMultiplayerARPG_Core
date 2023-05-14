using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine.Profiling;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;
using Newtonsoft.Json;

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

        [Header("Lan RPG")]
        public float autoSaveDuration = 2f;
        public GameStartType startType;
        public PlayerCharacterData selectedCharacter;
        public List<CharacterBuff> selectedCharacterSummonBuffs;
        public List<CharacterItem> selectedCharacterStorageItems;
        public EnableGmCommandType enableGmCommands;
        private float _lastSaveTime;
        private Vector3? _teleportPosition;
        private readonly Dictionary<long, PlayerCharacterData> _pendingSpawnPlayerCharacters = new Dictionary<long, PlayerCharacterData>();
        private readonly Dictionary<long, List<CharacterBuff>> _pendingSpawnPlayerCharacterSummonBuffs = new Dictionary<long, List<CharacterBuff>>();

        public LiteNetLibDiscovery CacheDiscovery { get; private set; }
        public BaseGameSaveSystem SaveSystem { get { return GameInstance.Singleton.SaveSystem; } }

        protected override void Awake()
        {
            CacheDiscovery = gameObject.GetOrAddComponent<LiteNetLibDiscovery>();
            // Server Handlers
            ServerUserHandlers = gameObject.GetOrAddComponent<IServerUserHandlers, DefaultServerUserHandlers>();
            ServerBuildingHandlers = gameObject.GetOrAddComponent<IServerBuildingHandlers, DefaultServerBuildingHandlers>();
            ServerCharacterHandlers = gameObject.GetOrAddComponent<IServerCharacterHandlers, DefaultServerCharacterHandlers>();
            ServerGameMessageHandlers = gameObject.GetOrAddComponent<IServerGameMessageHandlers, DefaultServerGameMessageHandlers>();
            ServerStorageHandlers = gameObject.GetOrAddComponent<IServerStorageHandlers, LanRpgServerStorageHandlers>();
            ServerPartyHandlers = gameObject.GetOrAddComponent<IServerPartyHandlers, DefaultServerPartyHandlers>();
            ServerGuildHandlers = gameObject.GetOrAddComponent<IServerGuildHandlers, DefaultServerGuildHandlers>();
            ServerChatHandlers = gameObject.GetOrAddComponent<IServerChatHandlers, DefaultServerChatHandlers>();
            // Server Message Handlers
            ServerCashShopMessageHandlers = gameObject.GetOrAddComponent<IServerCashShopMessageHandlers, LanRpgServerCashShopMessageHandlers>();
            ServerStorageMessageHandlers = gameObject.GetOrAddComponent<IServerStorageMessageHandlers, LanRpgServerStorageMessageHandlers>();
            ServerCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerCharacterMessageHandlers, DefaultServerCharacterMessageHandlers>();
            ServerInventoryMessageHandlers = gameObject.GetOrAddComponent<IServerInventoryMessageHandlers, DefaultServerInventoryMessageHandlers>();
            ServerPartyMessageHandlers = gameObject.GetOrAddComponent<IServerPartyMessageHandlers, LanRpgServerPartyMessageHandlers>();
            ServerGuildMessageHandlers = gameObject.GetOrAddComponent<IServerGuildMessageHandlers, LanRpgServerGuildMessageHandlers>();
            ServerGachaMessageHandlers = gameObject.GetOrAddComponent<IServerGachaMessageHandlers, LanRpgServerGachaMessageHandlers>();
            ServerBankMessageHandlers = gameObject.GetOrAddComponent<IServerBankMessageHandlers, LanRpgServerBankMessageHandlers>();
            ServerOnlineCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerOnlineCharacterMessageHandlers, DefaultServerOnlineCharacterMessageHandlers>();
            // Client handlers
            ClientCashShopHandlers = gameObject.GetOrAddComponent<IClientCashShopHandlers, DefaultClientCashShopHandlers>();
            ClientMailHandlers = gameObject.GetOrAddComponent<IClientMailHandlers, DefaultClientMailHandlers>();
            ClientStorageHandlers = gameObject.GetOrAddComponent<IClientStorageHandlers, DefaultClientStorageHandlers>();
            ClientCharacterHandlers = gameObject.GetOrAddComponent<IClientCharacterHandlers, DefaultClientCharacterHandlers>();
            ClientInventoryHandlers = gameObject.GetOrAddComponent<IClientInventoryHandlers, DefaultClientInventoryHandlers>();
            ClientPartyHandlers = gameObject.GetOrAddComponent<IClientPartyHandlers, DefaultClientPartyHandlers>();
            ClientGuildHandlers = gameObject.GetOrAddComponent<IClientGuildHandlers, DefaultClientGuildHandlers>();
            ClientGachaHandlers = gameObject.GetOrAddComponent<IClientGachaHandlers, DefaultClientGachaHandlers>();
            ClientFriendHandlers = gameObject.GetOrAddComponent<IClientFriendHandlers, DefaultClientFriendHandlers>();
            ClientBankHandlers = gameObject.GetOrAddComponent<IClientBankHandlers, DefaultClientBankHandlers>();
            ClientOnlineCharacterHandlers = gameObject.GetOrAddComponent<IClientOnlineCharacterHandlers, DefaultClientOnlineCharacterHandlers>();
            ClientChatHandlers = gameObject.GetOrAddComponent<IClientChatHandlers, DefaultClientChatHandlers>();
            ClientGameMessageHandlers = gameObject.GetOrAddComponent<IClientGameMessageHandlers, DefaultClientGameMessageHandlers>();
            base.Awake();
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
                    CacheDiscovery.data = JsonConvert.SerializeObject(new DiscoveryData()
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

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            ClientStorageActions.onNotifyStorageItemsUpdated += NotifyStorageItemsUpdated;
        }

        public override void OnStopHost()
        {
            // Stop both client and server
            CacheDiscovery.StopClient();
            CacheDiscovery.StopServer();
            base.OnStopHost();
        }

        public override void OnStopClient()
        {
            Save();
            base.OnStopClient();
            ClientStorageActions.onNotifyStorageItemsUpdated -= NotifyStorageItemsUpdated;
        }

        private void NotifyStorageItemsUpdated(List<CharacterItem> storageItems)
        {
            selectedCharacterStorageItems = storageItems;
        }

        protected override void HandleServerSceneChange(MessageHandlerData messageHandler)
        {
            if (!IsServer)
            {
                Save();
                SaveSystem.OnSceneChanging();
            }
            base.HandleServerSceneChange(messageHandler);
        }

        protected override async UniTask PreSpawnEntities()
        {
            await SaveSystem.PreSpawnEntities(selectedCharacter, ServerStorageHandlers.GetAllStorageItems());
        }

        public void Save(System.Action<IPlayerCharacterData> onBeforeSaveCharacter = null, bool saveWorld = true, bool saveStorage = true)
        {
            Profiler.BeginSample("LanRpgNetworkManager - Save Data");
            BasePlayerCharacterEntity playingCharacter = GameInstance.PlayingCharacterEntity;
            if (playingCharacter != null)
            {
                selectedCharacter = playingCharacter.CloneTo(selectedCharacter);
                if (onBeforeSaveCharacter != null)
                    onBeforeSaveCharacter.Invoke(selectedCharacter);
                SaveSystem.SaveCharacter(selectedCharacter);
                SaveSystem.SaveSummonBuffs(selectedCharacter, new List<CharacterSummon>(selectedCharacter.Summons));
                if (IsServer)
                {
                    if (saveWorld)
                        SaveSystem.SaveWorld(selectedCharacter, ServerBuildingHandlers.GetBuildings());
                    if (saveStorage)
                        SaveSystem.SaveStorage(selectedCharacter, ServerStorageHandlers.GetAllStorageItems());
                }
                else
                {
                    if (saveStorage)
                        SaveSystem.SavePlayerStorage(selectedCharacter, selectedCharacterStorageItems);
                }
            }
            Profiler.EndSample();
        }

        protected override void Update()
        {
            base.Update();
            float tempTime = Time.unscaledTime;
            if (tempTime - _lastSaveTime > autoSaveDuration)
            {
                Save();
                _lastSaveTime = tempTime;
            }
        }

        protected override void OnServerUpdate(LogicUpdater updater)
        {
            base.OnServerUpdate(updater);
            if (_pendingSpawnPlayerCharacters.Count > 0 && _isReadyToInstantiatePlayers)
            {
                // Spawn pending player characters
                LiteNetLibPlayer player;
                foreach (KeyValuePair<long, PlayerCharacterData> spawnPlayerCharacter in _pendingSpawnPlayerCharacters)
                {
                    if (!Players.TryGetValue(spawnPlayerCharacter.Key, out player))
                        continue;
                    player.IsReady = true;
                    SpawnPlayerCharacter(spawnPlayerCharacter.Key, spawnPlayerCharacter.Value, _pendingSpawnPlayerCharacterSummonBuffs[spawnPlayerCharacter.Key]);
                }
                _pendingSpawnPlayerCharacters.Clear();
                _pendingSpawnPlayerCharacterSummonBuffs.Clear();
            }
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectReason reason, SocketError socketError)
        {
            base.OnPeerDisconnected(connectionId, reason, socketError);
            UnregisterPlayerCharacter(connectionId);
        }

        public override void SerializeClientReadyData(NetDataWriter writer)
        {
            GameInstance.SelectedCharacterId = selectedCharacter.Id;
            GameInstance.PlayingCharacter = selectedCharacter;
            selectedCharacterSummonBuffs = SaveSystem.LoadSummonBuffs(selectedCharacter);
            selectedCharacterStorageItems = SaveSystem.LoadPlayerStorage(selectedCharacter);
            selectedCharacter.SerializeCharacterData(writer);
            writer.PutList(selectedCharacterSummonBuffs);
            writer.PutList(selectedCharacterStorageItems);
        }

        public override async UniTask<bool> DeserializeClientReadyData(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader)
        {
            await UniTask.Yield();
            PlayerCharacterData playerCharacterData = new PlayerCharacterData().DeserializeCharacterData(reader);
            List<CharacterBuff> playerSummonBuffs = reader.GetList<CharacterBuff>();
            List<CharacterItem> playerStorageItems = reader.GetList<CharacterItem>();
            ServerStorageHandlers.SetStorageItems(new StorageId(StorageType.Player, playerCharacterData.Id), playerStorageItems);
            if (!_isReadyToInstantiatePlayers)
            {
                // Not ready to instantiate objects, add spawning player character to pending dictionary
                if (LogDev) Logging.Log(LogTag, "Not ready to deserializing client ready extra");
                if (!_pendingSpawnPlayerCharacters.ContainsKey(connectionId))
                    _pendingSpawnPlayerCharacters.Add(connectionId, playerCharacterData);
                if (!_pendingSpawnPlayerCharacterSummonBuffs.ContainsKey(connectionId))
                    _pendingSpawnPlayerCharacterSummonBuffs.Add(connectionId, playerSummonBuffs);
                return true;
            }
            if (LogDev) Logging.Log(LogTag, "Deserializing client ready extra");
            SpawnPlayerCharacter(connectionId, playerCharacterData, playerSummonBuffs);
            return true;
        }

        private void SpawnPlayerCharacter(long connectionId, PlayerCharacterData playerCharacterData, List<CharacterBuff> summonBuffs)
        {
            BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
            // If it is not allow this character data, disconnect user
            if (entityPrefab == null)
            {
                Logging.LogError(LogTag, "Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                return;
            }
            if (!CurrentMapInfo.Id.Equals(playerCharacterData.CurrentMapName))
                playerCharacterData.CurrentPosition = _teleportPosition.HasValue ? _teleportPosition.Value : CurrentMapInfo.StartPosition;
            Quaternion characterRotation = Quaternion.identity;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                characterRotation = Quaternion.Euler(playerCharacterData.CurrentRotation);
            LiteNetLibIdentity spawnObj = Assets.GetObjectInstance(
                entityPrefab.Identity.HashAssetId,
                playerCharacterData.CurrentPosition,
                characterRotation);
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

            // Force make caches, to calculate current stats to fill empty slots items
            playerCharacterEntity.ForceMakeCaches();
            playerCharacterEntity.FillEmptySlots();

            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
            {
                playerCharacterEntity.CallAllOnRespawn();
                // Summon saved mount entity
                if (GameInstance.VehicleEntities.ContainsKey(playerCharacterData.MountDataId))
                    playerCharacterEntity.Mount(GameInstance.VehicleEntities[playerCharacterData.MountDataId]);
                // Summon monsters
                for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
                {
                    CharacterSummon summon = playerCharacterEntity.Summons[i];
                    summon.Summon(playerCharacterEntity, summon.level, summon.summonRemainsDuration, summon.exp, summon.currentHp, summon.currentMp);
                    for (int j = 0; j < summonBuffs.Count; ++j)
                    {
                        if (summonBuffs[j].id.StartsWith(i.ToString()))
                        {
                            summon.CacheEntity.Buffs.Add(summonBuffs[j]);
                            summonBuffs.RemoveAt(j);
                            j--;
                        }
                    }
                    playerCharacterEntity.Summons[i] = summon;
                }
            }
            else
            {
                playerCharacterEntity.CallAllOnDead();
            }

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

        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName) || (mapName.Equals(CurrentMapInfo.Id) && !IsInstanceMap()))
            {
                if (overrideRotation)
                    playerCharacterEntity.CurrentRotation = rotation;
                playerCharacterEntity.Teleport(position, Quaternion.Euler(playerCharacterEntity.CurrentRotation));
                return;
            }

            if (!string.IsNullOrEmpty(mapName) && playerCharacterEntity.IsServer && playerCharacterEntity.IsOwnerClient &&
                GameInstance.MapInfos.TryGetValue(mapName, out BaseMapInfo mapInfo) && mapInfo.IsSceneSet())
            {
                // Save data before warp
                BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;
                SaveSystem.SaveWorld(owningCharacter, ServerBuildingHandlers.GetBuildings());
                SaveSystem.SaveStorage(owningCharacter, ServerStorageHandlers.GetAllStorageItems());
                ServerBuildingHandlers.ClearBuildings();
                ServerStorageHandlers.ClearStorage();
                SetMapInfo(mapInfo);
                _teleportPosition = position;
                Save((savingCharacter) =>
                {
                    savingCharacter.CurrentMapName = mapInfo.Id;
                    savingCharacter.CurrentPosition = position;
                    if (overrideRotation)
                        savingCharacter.CurrentRotation = rotation;
                }, false, false);
                SaveSystem.OnSceneChanging();
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

        public override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
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
