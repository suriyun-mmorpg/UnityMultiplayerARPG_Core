using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Profiling;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public partial class LanRpgNetworkManager : BaseGameNetworkManager, IServerStorageHandlers
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
        private int nextPartyId = 1;
        private int nextGuildId = 1;
        private Vector3? teleportPosition;
        private readonly Dictionary<long, PlayerCharacterData> pendingSpawnPlayerCharacters = new Dictionary<long, PlayerCharacterData>();

        public LiteNetLibDiscovery CacheDiscovery { get; private set; }
        public BaseGameSaveSystem SaveSystem { get { return GameInstance.Singleton.SaveSystem; } }

        protected override void Awake()
        {
            base.Awake();
            CacheDiscovery = gameObject.GetOrAddComponent<LiteNetLibDiscovery>();
            CashShopRequestHandlers = gameObject.GetOrAddComponent<IServerCashShopMessageHandlers, LanRpgCashShopMessageHandlers>();
            CashShopRequestHandlers.ServerPlayerCharacterHandlers = this;
            StorageRequestHandlers = gameObject.GetOrAddComponent<IServerStorageMessageHandlers, LanRpgStorageMessageHandlers>();
            StorageRequestHandlers.ServerPlayerCharacterHandlers = this;
            StorageRequestHandlers.ServerStorageHandlers = this;
            InventoryRequestHandlers = gameObject.GetOrAddComponent<IServerInventoryMessageHandlers, DefaultServerInventoryMessageHandlers>();
            InventoryRequestHandlers.ServerPlayerCharacterHandlers = this;
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
                        SaveSystem.SaveStorage(owningCharacter, storageItems);
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
            if (this.TryGetPlayerCharacter(connectionId, out playerCharacter))
                CloseStorage(playerCharacter).Forget();
            base.UnregisterPlayerCharacter(connectionId);
        }

        protected override void Clean()
        {
            base.Clean();
            nextPartyId = 1;
            nextGuildId = 1;
            storageItems.Clear();
            usingStorageCharacters.Clear();
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
            GameObject spawnObj = Instantiate(entityPrefab.gameObject, playerCharacterData.CurrentPosition, Quaternion.Euler(playerCharacterData.CurrentRotation));
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
            if (PlayerCharactersCount == 0)
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
            RegisterPlayerCharacter(playerCharacterEntity);

            SocialCharacterData[] members;
            // Set guild id
            if (Guilds.Count > 0)
            {
                foreach (GuildData guild in Guilds.Values)
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
            if (Parties.Count > 0)
            {
                foreach (PartyData party in Parties.Values)
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

            long connectionId = playerCharacterEntity.ConnectionId;
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
                SaveSystem.SaveStorage(owningCharacter, storageItems);
                BuildingEntities.Clear();
                storageItems.Clear();
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

        #region Implement Abstract Functions
        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            CreateParty(playerCharacterEntity, shareExp, shareItem, nextPartyId++);
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            CreateGuild(playerCharacterEntity, guildName, nextGuildId++);
        }

        public override void DepositGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            if (playerCharacterEntity.Gold - amount >= 0)
            {
                playerCharacterEntity.Gold -= amount;
                playerCharacterEntity.UserGold = playerCharacterEntity.UserGold.Increase(amount);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
        }

        public override void WithdrawGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            if (playerCharacterEntity.UserGold - amount >= 0)
            {
                playerCharacterEntity.UserGold -= amount;
                playerCharacterEntity.Gold = playerCharacterEntity.Gold.Increase(amount);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
        }

        public override void DepositGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            GuildData guild;
            if (Guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
            {
                if (playerCharacterEntity.Gold - amount >= 0)
                {
                    playerCharacterEntity.Gold -= amount;
                    guild.gold += amount;
                    Guilds[playerCharacterEntity.GuildId] = guild;
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }

        public override void WithdrawGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            GuildData guild;
            if (Guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
            {
                if (guild.gold - amount >= 0)
                {
                    guild.gold -= amount;
                    playerCharacterEntity.Gold = playerCharacterEntity.Gold.Increase(amount);
                    Guilds[playerCharacterEntity.GuildId] = guild;
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }

        public override void FindCharacters(BasePlayerCharacterEntity finder, string characterName)
        {
            List<SocialCharacterData> socialCharacters = new List<SocialCharacterData>();
            BasePlayerCharacterEntity findResult;
            if (this.TryGetPlayerCharacterByName(characterName, out findResult))
                socialCharacters.Add(SocialCharacterData.Create(findResult));
            this.SendSocialMembers(finder.ConnectionId, MsgTypes.UpdateFoundCharacters, socialCharacters.ToArray());
        }

        public override void AddFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
            // Service not available for Lan mode
            SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.ServiceNotAvailable);
        }

        public override void RemoveFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
            // Service not available for Lan mode
            SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.ServiceNotAvailable);
        }

        public override void GetFriends(BasePlayerCharacterEntity playerCharacterEntity)
        {
            // Service not available for Lan mode
            SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.ServiceNotAvailable);
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            SaveSystem.OnServerStart();
        }

        protected override async UniTask PreSpawnEntities()
        {
            await SaveSystem.PreSpawnEntities(selectedCharacter, BuildingEntities, storageItems);
        }
        #endregion
    }
}
