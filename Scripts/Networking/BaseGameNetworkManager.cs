using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using LiteNetLibManager.SuperGrid2D;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager : LiteNetLibGameManager
    {
        public const string CHAT_SYSTEM_ANNOUNCER_SENDER = "SYSTEM_ANNOUNCER";
        public const float UPDATE_ONLINE_CHARACTER_DURATION = 1f;
        public const float UPDATE_TIME_OF_DAY_DURATION = 5f;
        public const string INSTANTIATES_OBJECTS_DELAY_STATE_KEY = "INSTANTIATES_OBJECTS_DELAY";
        public const float INSTANTIATES_OBJECTS_DELAY = 0.5f;

        protected static readonly NetDataWriter s_Writer = new NetDataWriter();

        public static BaseGameNetworkManager Singleton { get; protected set; }
        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
        // Others
        public ILagCompensationManager LagCompensationManager { get; protected set; }
        public IHitRegistrationManager HitRegistrationManager { get; protected set; }
        public BaseGameNetworkManagerComponent[] ManagerComponents { get; private set; }

        public static BaseMapInfo CurrentMapInfo { get; protected set; }
        public bool ShouldPhysicSyncTransforms { get; set; }
        public bool ShouldPhysicSyncTransforms2D { get; set; }

        public bool useUnityAutoPhysicSyncTransform = true;
        // Spawn entities events
        public LiteNetLibLoadSceneEvent onSpawnEntitiesStart = new LiteNetLibLoadSceneEvent();
        public LiteNetLibLoadSceneEvent onSpawnEntitiesProgress = new LiteNetLibLoadSceneEvent();
        public LiteNetLibLoadSceneEvent onSpawnEntitiesFinish = new LiteNetLibLoadSceneEvent();
        // Other events
        /// <summary>
        /// ConnectionID, PlayerCharacterEntity
        /// </summary>
        public event System.Action<long, BasePlayerCharacterEntity> onRegisterCharacter;
        /// <summary>
        /// ConnectionID, CharacterID, UserID
        /// </summary>
        public event System.Action<long, string, string> onUnregisterCharacter;
        /// <summary>
        /// ConnectionID, UserID
        /// </summary>
        public event System.Action<long, string> onRegisterUser;
        /// <summary>
        /// ConnectionID, UserID
        /// </summary>
        public event System.Action<long, string> onUnregisterUser;
        // Private variables
        protected float _updateOnlineCharactersCountDown;
        protected float _updateTimeOfDayCountDown;
        protected float _serverSceneLoadedTime;
        protected float _clientSceneLoadedTime;
        protected HashSet<BaseGameEntity> _setOfGameEntity = new HashSet<BaseGameEntity>();

        // Instantiate object allowing status
        /// <summary>
        /// For backward compatibility, should use `_serverReadyToInstantiateObjectsStates` instead.
        /// </summary>
        protected Dictionary<string, bool> _readyToInstantiateObjectsStates { get { return _serverReadyToInstantiateObjectsStates; } set { _serverReadyToInstantiateObjectsStates = value; } }
        protected Dictionary<string, bool> _serverReadyToInstantiateObjectsStates = new Dictionary<string, bool>();
        protected Dictionary<string, bool> _clientReadyToInstantiateObjectsStates = new Dictionary<string, bool>();

        /// <summary>
        /// For backward compatibility, should use `_isServerReadyToInstantiateObjects` instead.
        /// </summary>
        protected bool _isReadyToInstantiateObjects { get { return _isServerReadyToInstantiateObjects; } set { _isServerReadyToInstantiateObjects = value; } }
        protected bool _isServerReadyToInstantiateObjects;
        protected bool _isClientReadyToInstantiateObjects;

        /// <summary>
        /// For backward compatibility, should use `_isServerReadyToInstantiatePlayers` instead.
        /// </summary>
        protected bool _isReadyToInstantiatePlayers { get { return _isServerReadyToInstantiatePlayers; } set { _isServerReadyToInstantiatePlayers = value; } }
        protected bool _isServerReadyToInstantiatePlayers;

        protected override void Awake()
        {
            Singleton = this;
            doNotEnterGameOnConnect = false;
            doNotReadyOnSceneLoaded = true;
            doNotDestroyOnSceneChanges = true;
            LagCompensationManager = gameObject.GetOrAddComponent<ILagCompensationManager, DefaultLagCompensationManager>();
            HitRegistrationManager = gameObject.GetOrAddComponent<IHitRegistrationManager, DefaultHitRegistrationManager>();
            ManagerComponents = GetComponents<BaseGameNetworkManagerComponent>();
            // Force change physic auto sync transforms mode to manual
            Physics.autoSyncTransforms = useUnityAutoPhysicSyncTransform;
            Physics2D.autoSyncTransforms = useUnityAutoPhysicSyncTransform;
            // Setup character hidding condition
            LiteNetLibIdentity.ForceHideFunctions.Add(IsHideEntity);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Remove character hidding condition
            LiteNetLibIdentity.ForceHideFunctions.Remove(IsHideEntity);
        }

        protected bool IsHideEntity(LiteNetLibIdentity mustHideThis, LiteNetLibIdentity fromThis)
        {
            if (!mustHideThis.TryGetComponent(out BaseGameEntity mustHideThisEntity) ||
                !fromThis.TryGetComponent(out BaseGameEntity fromThisEntity))
                return false;
            return mustHideThisEntity.IsHideFrom(fromThisEntity);
        }

        protected override void Update()
        {
            // Network messages will be handled before update game entities (in base.Update())
            base.Update();
            float tempDeltaTime = Time.unscaledDeltaTime;
            if (IsServer)
            {
                _updateOnlineCharactersCountDown -= tempDeltaTime;
                if (_updateOnlineCharactersCountDown <= 0f)
                {
                    _updateOnlineCharactersCountDown = UPDATE_ONLINE_CHARACTER_DURATION;
                    UpdateOnlineCharacters();
                }
                _updateTimeOfDayCountDown -= tempDeltaTime;
                if (_updateTimeOfDayCountDown <= 0f)
                {
                    _updateTimeOfDayCountDown = UPDATE_TIME_OF_DAY_DURATION;
                    SendTimeOfDay();
                }
            }
            // Network messages were handled (in base.Update()), enity movement proceeded, it may have transform changing manually, and need to sync tranforms before update physic movement
            if (ShouldPhysicSyncTransforms && !Physics.autoSyncTransforms)
                Physics.SyncTransforms();
            ShouldPhysicSyncTransforms = false;
            if (ShouldPhysicSyncTransforms2D && !Physics2D.autoSyncTransforms)
                Physics2D.SyncTransforms();
            ShouldPhysicSyncTransforms2D = false;

            // Update game entity, it may update entities movement
            if (IsNetworkActive)
            {
                // Update day-night time on both client and server. It will sync from server some time to make sure that clients time of day won't very difference
                CurrentGameInstance.DayNightTimeUpdater.UpdateTimeOfDay(tempDeltaTime);
                foreach (BaseGameEntity entity in _setOfGameEntity)
                {
                    if (!entity.enabled)
                        continue;
                    entity.DoUpdate();
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (IsNetworkActive)
            {
                foreach (BaseGameEntity entity in _setOfGameEntity)
                {
                    if (!entity.enabled)
                        continue;
                    entity.DoLateUpdate();
                }
            }
        }

        protected override void OnServerUpdate(LogicUpdater updater)
        {
            base.OnServerUpdate(updater);
            Profiler.BeginSample("BaseGameNetworkManager - SendServerState");
            long timestamp = Timestamp;
            foreach (BaseGameEntity entity in _setOfGameEntity)
            {
                if (!entity.enabled)
                    continue;
                entity.SendServerState(timestamp);
            }
            Profiler.EndSample();
        }

        protected override void OnClientUpdate(LogicUpdater updater)
        {
            base.OnClientUpdate(updater);
            if (IsServer)
                return;
            Profiler.BeginSample("BaseGameNetworkManager - SendClientState");
            long timestamp = Timestamp;
            foreach (BaseGameEntity entity in _setOfGameEntity)
            {
                if (!entity.enabled)
                    continue;
                if (entity.IsOwnerClient)
                    entity.SendClientState(timestamp);
            }
            Profiler.EndSample();
        }

        public void RegisterGameEntity(BaseGameEntity gameEntity)
        {
            _setOfGameEntity.Add(gameEntity);
        }

        public void UnregisterGameEntity(BaseGameEntity gameEntity)
        {
            _setOfGameEntity.Remove(gameEntity);
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            RegisterHandlerMessages();
            // Keeping `RegisterClientMessages` and `RegisterServerMessages` for backward compatibility, can use any of below dev extension methods
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            this.InvokeInstanceDevExtMethods("RegisterMessages");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.RegisterMessages(this);
            }
        }

        protected virtual void Clean()
        {
            CleanHandlers();
            // Other components
            HitRegistrationManager.ClearData();
            CurrentMapInfo = null;
            _isServerReadyToInstantiateObjects = false;
            _isClientReadyToInstantiateObjects = false;
            _isServerReadyToInstantiatePlayers = false;
            _setOfGameEntity.Clear();
            PoolSystem.Clear();
            ClientBankActions.Clean();
            ClientCashShopActions.Clean();
            ClientCharacterActions.Clean();
            ClientFriendActions.Clean();
            ClientGachaActions.Clean();
            ClientGenericActions.Clean();
            ClientGuildActions.Clean();
            ClientInventoryActions.Clean();
            ClientMailActions.Clean();
            ClientPartyActions.Clean();
            ClientStorageActions.Clean();
            // Extensions
            this.InvokeInstanceDevExtMethods("Clean");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.Clean(this);
            }
        }

        public override bool StartServer()
        {
            // Prepare grid manager
            GridManager gridManager = gameObject.GetComponent<GridManager>();
            if (gridManager != null)
            {
                // Make sure that grid manager -> axis mode set correctly for current dimension type
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    gridManager.axisMode = GridManager.EAxisMode.XZ;
                else
                    gridManager.axisMode = GridManager.EAxisMode.XY;
            }
            InitPrefabs();
            return base.StartServer();
        }

        public override void OnStartServer()
        {
            SetServerHandlersRef();
            this.InvokeInstanceDevExtMethods("OnStartServer");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnStartServer(this);
            }
            CurrentGameInstance.DayNightTimeUpdater.InitTimeOfDay(this);
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            this.InvokeInstanceDevExtMethods("OnStopServer");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnStopServer(this);
            }
            Clean();
            base.OnStopServer();
        }

        public override bool StartClient(string networkAddress, int networkPort)
        {
            // Server will call init prefabs function too, so don't call it again
            if (!IsServer)
                InitPrefabs();
            return base.StartClient(networkAddress, networkPort);
        }

        public override void OnStartClient(LiteNetLibClient client)
        {
            SetClientHandlersRef();
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnStartClient(this, client);
            }
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            this.InvokeInstanceDevExtMethods("OnStopClient");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnStopClient(this);
            }
            ClientGenericActions.ClientStopped();
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        public override void OnClientConnected()
        {
            ClientGenericActions.ClientConnected();
            base.OnClientConnected();
        }

        public override void OnClientDisconnected(DisconnectReason reason, SocketError socketError, byte[] data)
        {
            UITextKeys message = UITextKeys.NONE;
            if (data != null && data.Length > 0)
            {
                NetDataReader reader = new NetDataReader(data);
                message = (UITextKeys)reader.GetPackedUShort();
            }
            UISceneGlobal.Singleton.ShowDisconnectDialog(reason, socketError, message);
            ClientGenericActions.ClientDisconnected(reason, socketError, message);
        }

        public override void OnPeerConnected(long connectionId)
        {
            this.InvokeInstanceDevExtMethods("OnPeerConnected", connectionId);
            base.OnPeerConnected(connectionId);
        }

        protected override UniTaskVoid HandleEnterGameRequest(RequestHandlerData requestHandler, EnterGameRequestMessage request, RequestProceedResultDelegate<EnterGameResponseMessage> result)
        {
            result += (responseCode, response, extraResponseSerializer) =>
            {
                if (responseCode == AckResponseCode.Success)
                {
                    SendMapInfo(requestHandler.ConnectionId);
                    SendTimeOfDay(requestHandler.ConnectionId);
                }
            };
            return base.HandleEnterGameRequest(requestHandler, request, result);
        }

        protected virtual void UpdateOnlineCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            ServerCharacterHandlers.MarkOnlineCharacter(playerCharacterEntity.Id);
        }

        protected virtual void UpdateOnlineCharacters()
        {
            Dictionary<long, PartyData> updatingPartyMembers = new Dictionary<long, PartyData>();
            Dictionary<long, GuildData> updatingGuildMembers = new Dictionary<long, GuildData>();

            PartyData tempParty;
            GuildData tempGuild;
            foreach (BasePlayerCharacterEntity playerCharacter in ServerUserHandlers.GetPlayerCharacters())
            {
                if (playerCharacter == null)
                    continue;

                UpdateOnlineCharacter(playerCharacter);

                if (playerCharacter.PartyId > 0 && ServerPartyHandlers.TryGetParty(playerCharacter.PartyId, out tempParty) && tempParty != null)
                {
                    tempParty.UpdateMember(playerCharacter);
                    if (!updatingPartyMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingPartyMembers.Add(playerCharacter.ConnectionId, tempParty);
                }

                if (playerCharacter.GuildId > 0 && ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out tempGuild) && tempGuild != null)
                {
                    tempGuild.UpdateMember(playerCharacter);
                    if (!updatingGuildMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingGuildMembers.Add(playerCharacter.ConnectionId, tempGuild);
                }
            }

            foreach (long connectionId in updatingPartyMembers.Keys)
            {
                ServerGameMessageHandlers.SendUpdatePartyMembersToOne(connectionId, updatingPartyMembers[connectionId]);
            }

            foreach (long connectionId in updatingGuildMembers.Keys)
            {
                ServerGameMessageHandlers.SendUpdateGuildMembersToOne(connectionId, updatingGuildMembers[connectionId]);
            }
        }

        protected virtual void HandleWarpAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientWarp();
        }

        protected virtual void HandleChatAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientReceiveChatMessage(messageHandler.ReadMessage<ChatMessage>());
        }

        protected void HandleUpdateDayNightTimeAtClient(MessageHandlerData messageHandler)
        {
            // Don't set time of day again at server
            if (IsServer)
                return;
            UpdateTimeOfDayMessage message = messageHandler.ReadMessage<UpdateTimeOfDayMessage>();
            CurrentGameInstance.DayNightTimeUpdater.SetTimeOfDay(message.timeOfDay);
        }

        protected void HandleUpdateMapInfoAtClient(MessageHandlerData messageHandler)
        {
            // Don't set map info again at server
            if (IsServer)
                return;
            UpdateMapInfoMessage message = messageHandler.ReadMessage<UpdateMapInfoMessage>();
            SetMapInfo(message.mapName);
            if (CurrentMapInfo == null)
            {
                Logging.LogError(LogTag, $"Cannot find map info: {message.mapName}, it will create new map info to use, it can affect players' experience.");
                CurrentMapInfo = ScriptableObject.CreateInstance<MapInfo>();
                CurrentMapInfo.Id = message.mapName;
                return;
            }
            if (!CurrentMapInfo.GetType().FullName.Equals(message.className))
            {
                Logging.LogError(LogTag, $"Invalid map info expect: {message.className}, found {CurrentMapInfo.GetType().FullName}, it can affect players' experience.");
                return;
            }
            CurrentMapInfo.Deserialize(messageHandler.Reader);
            this.InvokeInstanceDevExtMethods("ReadMapInfoExtra", messageHandler.Reader);
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.ReadMapInfoExtra(this, messageHandler.Reader);
            }
        }

        protected void HandleServerEntityStateAtClient(MessageHandlerData messageHandler)
        {
            uint objectId = messageHandler.Reader.GetPackedUInt();
            long peerTimestamp = messageHandler.Reader.GetPackedLong();
            if (Assets.TryGetSpawnedObject(objectId, out BaseGameEntity gameEntity))
                gameEntity.ReadServerStateAtClient(peerTimestamp, messageHandler.Reader);
        }

        protected virtual void HandleChatAtServer(MessageHandlerData messageHandler)
        {
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>().FillChannelId();
            // Get character
            IPlayerCharacterData playerCharacter = null;
            if (!string.IsNullOrEmpty(message.senderName))
                ServerUserHandlers.TryGetPlayerCharacterByName(message.senderName, out playerCharacter);
            // Set guild data
            if (playerCharacter != null)
            {
                if (ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guildData))
                {
                    message.guildId = playerCharacter.GuildId;
                    message.guildName = guildData.guildName;
                }
            }
            // Character muted?
            if (!message.sendByServer && playerCharacter != null && playerCharacter.IsMuting())
            {
                long connectionId;
                if (ServerUserHandlers.TryGetConnectionId(playerCharacter.Id, out connectionId))
                {
                    ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, new ChatMessage()
                    {
                        channel = ChatChannel.System,
                        message = "You have been muted.",
                    });
                }
                return;
            }
            if (message.channel != ChatChannel.System || ServerChatHandlers.CanSendSystemAnnounce(message.senderName))
            {
                ServerChatHandlers.OnChatMessage(message);
                ServerLogHandlers.LogEnterChat(message);
            }
        }

        protected void HandleClientEntityStateAtServer(MessageHandlerData messageHandler)
        {
            uint objectId = messageHandler.Reader.GetPackedUInt();
            long peerTimestamp = messageHandler.Reader.GetPackedLong();
            if (Assets.TryGetSpawnedObject(objectId, out BaseGameEntity gameEntity) && gameEntity.Identity.ConnectionId == messageHandler.ConnectionId)
                gameEntity.ReadClientStateAtServer(peerTimestamp, messageHandler.Reader);
        }

        public virtual void InitPrefabs()
        {
            Assets.addressableOfflineScene = null;
#if !EXCLUDE_PREFAB_REFS
            Assets.offlineScene = default;
#endif
            if (CurrentGameInstance.GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene))
            {
                Assets.addressableOfflineScene = addressableScene;
            }
#if !EXCLUDE_PREFAB_REFS
            else
            {
                Assets.offlineScene = scene;
            }
#endif
            // Prepare networking prefabs
#if !EXCLUDE_PREFAB_REFS
            Assets.playerPrefab = null;
            HashSet<LiteNetLibIdentity> spawnablePrefabs = new HashSet<LiteNetLibIdentity>(Assets.spawnablePrefabs);
            if (CurrentGameInstance.itemDropEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.itemDropEntityPrefab.Identity);
            if (CurrentGameInstance.expDropEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.expDropEntityPrefab.Identity);
            if (CurrentGameInstance.goldDropEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.goldDropEntityPrefab.Identity);
            if (CurrentGameInstance.currencyDropEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.currencyDropEntityPrefab.Identity);
            if (CurrentGameInstance.warpPortalEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.warpPortalEntityPrefab.Identity);
            if (CurrentGameInstance.playerCorpsePrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.playerCorpsePrefab.Identity);
            if (CurrentGameInstance.monsterCorpsePrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.monsterCorpsePrefab.Identity);
            foreach (BaseCharacterEntity entry in GameInstance.CharacterEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (VehicleEntity entry in GameInstance.VehicleEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (WarpPortalEntity entry in GameInstance.WarpPortalEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (NpcEntity entry in GameInstance.NpcEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (BuildingEntity entry in GameInstance.BuildingEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (LiteNetLibIdentity identity in GameInstance.OtherNetworkObjectPrefabs.Values)
            {
                spawnablePrefabs.Add(identity);
            }
            Assets.spawnablePrefabs = new LiteNetLibIdentity[spawnablePrefabs.Count];
            spawnablePrefabs.CopyTo(Assets.spawnablePrefabs);
#endif

            Assets.addressablePlayerPrefab = null;
            HashSet<AssetReferenceLiteNetLibIdentity> addressableSpawnablePrefabs = new HashSet<AssetReferenceLiteNetLibIdentity>(Assets.addressableSpawnablePrefabs);
            if (CurrentGameInstance.addressableItemDropEntityPrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableItemDropEntityPrefab);
            if (CurrentGameInstance.addressableExpDropEntityPrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableExpDropEntityPrefab);
            if (CurrentGameInstance.addressableGoldDropEntityPrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableGoldDropEntityPrefab);
            if (CurrentGameInstance.addressableCurrencyDropEntityPrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableCurrencyDropEntityPrefab);
            if (CurrentGameInstance.addressableWarpPortalEntityPrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableWarpPortalEntityPrefab);
            if (CurrentGameInstance.addressablePlayerCorpsePrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressablePlayerCorpsePrefab);
            if (CurrentGameInstance.addressableMonsterCorpsePrefab.IsDataValid())
                addressableSpawnablePrefabs.Add(CurrentGameInstance.addressableMonsterCorpsePrefab);
            foreach (AssetReferenceBaseCharacterEntity entry in GameInstance.AddressableCharacterEntities.Values)
            {
                addressableSpawnablePrefabs.Add(entry);
            }
            foreach (AssetReferenceVehicleEntity entry in GameInstance.AddressableVehicleEntities.Values)
            {
                addressableSpawnablePrefabs.Add(entry);
            }
            foreach (AssetReferenceWarpPortalEntity entry in GameInstance.AddressableWarpPortalEntities.Values)
            {
                addressableSpawnablePrefabs.Add(entry);
            }
            foreach (AssetReferenceNpcEntity entry in GameInstance.AddressableNpcEntities.Values)
            {
                addressableSpawnablePrefabs.Add(entry);
            }
            foreach (AssetReferenceBuildingEntity entry in GameInstance.AddressableBuildingEntities.Values)
            {
                addressableSpawnablePrefabs.Add(entry);
            }
            foreach (AssetReferenceLiteNetLibIdentity identity in GameInstance.AddressableOtherNetworkObjectPrefabs.Values)
            {
                addressableSpawnablePrefabs.Add(identity);
            }
            Assets.addressableSpawnablePrefabs = new AssetReferenceLiteNetLibIdentity[addressableSpawnablePrefabs.Count];
            addressableSpawnablePrefabs.CopyTo(Assets.addressableSpawnablePrefabs);

            this.InvokeInstanceDevExtMethods("InitPrefabs");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.InitPrefabs(this);
            }
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void RegisterEntities()
        {
            MonsterSpawnArea[] monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
            foreach (MonsterSpawnArea monsterSpawnArea in monsterSpawnAreas)
            {
                monsterSpawnArea.RegisterPrefabs();
            }

            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.RegisterPrefabs();
            }

            ItemDropSpawnArea[] itemDropSpawnAreas = FindObjectsOfType<ItemDropSpawnArea>();
            foreach (ItemDropSpawnArea itemDropSpawnArea in itemDropSpawnAreas)
            {
                itemDropSpawnArea.RegisterPrefabs();
            }

            // Register scene entities
            GameInstance.AddCharacterEntities(FindObjectsOfType<BaseMonsterCharacterEntity>());
            GameInstance.AddHarvestableEntities(FindObjectsOfType<HarvestableEntity>());
            GameInstance.AddItemDropEntities(FindObjectsOfType<ItemDropEntity>());

            PoolSystem.Clear();
            foreach (IPoolDescriptor poolingObject in GameInstance.PoolingObjectPrefabs)
            {
                if (!IsClient && (poolingObject is GameEffect || poolingObject is ProjectileEffect))
                    continue;
                PoolSystem.InitPool(poolingObject);
            }
            System.GC.Collect();
        }

        protected override void HandleClientReadyResponse(ResponseHandlerData responseHandler, AckResponseCode responseCode, EmptyMessage response)
        {
            base.HandleClientReadyResponse(responseHandler, responseCode, response);
            if (responseCode != AckResponseCode.Success)
                OnClientConnectionRefused();
        }

        public override void OnClientConnectionRefused()
        {
            base.OnClientConnectionRefused();
            UISceneGlobal.Singleton.ShowDisconnectDialog(DisconnectReason.ConnectionRejected, SocketError.Success, UITextKeys.NONE);
        }

        public override void OnClientOnlineSceneLoaded()
        {
            this.InvokeInstanceDevExtMethods("OnClientOnlineSceneLoaded");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnClientOnlineSceneLoaded(this);
            }
            _clientSceneLoadedTime = Time.unscaledTime;
            // Server will register entities later, so don't register entities now
            if (!IsServer)
                RegisterEntities();
            ProceedUntilClientReady().Forget();
        }

        public override void OnServerOnlineSceneLoaded()
        {
            this.InvokeInstanceDevExtMethods("OnServerOnlineSceneLoaded");
            foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
            {
                component.OnServerOnlineSceneLoaded(this);
            }
            _serverSceneLoadedTime = Time.unscaledTime;
            _serverReadyToInstantiateObjectsStates.Clear();
            _isServerReadyToInstantiateObjects = false;
            _isServerReadyToInstantiatePlayers = false;
            SpawnEntities().Forget();
        }

        public override void ServerSceneChange(ServerSceneInfo serverSceneInfo)
        {
            if (!IsServer)
                return;
            _serverReadyToInstantiateObjectsStates.Clear();
            _isServerReadyToInstantiateObjects = false;
            _isServerReadyToInstantiatePlayers = false;
            base.ServerSceneChange(serverSceneInfo);
        }

        protected virtual async UniTaskVoid SpawnEntities()
        {
            do { await UniTask.Delay(100); } while (!IsServerReadyToInstantiateObjects());
            float progress = 0f;
            string sceneName = SceneManager.GetActiveScene().name;
            await UniTask.NextFrame();
            onSpawnEntitiesStart.Invoke(sceneName, false, true, progress);
            await PreSpawnEntities();
            RegisterEntities();
            await UniTask.NextFrame();
            int i;
            LiteNetLibIdentity spawnObj;
            // Spawn Warp Portals
            if (LogInfo)
                Logging.Log(LogTag, "Spawning warp portals");
            if (GameInstance.MapWarpPortals.Count > 0)
            {
                if (GameInstance.MapWarpPortals.TryGetValue(CurrentMapInfo.Id, out List<WarpPortal> mapWarpPortals))
                {
                    WarpPortal warpPortal;
                    WarpPortalEntity warpPortalPrefab;
                    AssetReferenceWarpPortalEntity addressableWarpPortalPrefab;
                    WarpPortalEntity warpPortalEntity;
                    for (i = 0; i < mapWarpPortals.Count; ++i)
                    {
                        warpPortal = mapWarpPortals[i];
#if !EXCLUDE_PREFAB_REFS
                        warpPortalPrefab = warpPortal.entityPrefab != null ? warpPortal.entityPrefab : CurrentGameInstance.warpPortalEntityPrefab;
#else
                        warpPortalPrefab = null;
#endif
                        addressableWarpPortalPrefab = warpPortal.addressableEntityPrefab.IsDataValid() ? warpPortal.addressableEntityPrefab : CurrentGameInstance.addressableWarpPortalEntityPrefab;
                        spawnObj = null;
                        if (warpPortalPrefab != null)
                        {
                            spawnObj = Assets.GetObjectInstance(
                                warpPortalPrefab.Identity.HashAssetId, warpPortal.position,
                                Quaternion.Euler(warpPortal.rotation));
                        }
                        else if (addressableWarpPortalPrefab.IsDataValid())
                        {
                            spawnObj = Assets.GetObjectInstance(
                                addressableWarpPortalPrefab.HashAssetId, warpPortal.position,
                                Quaternion.Euler(warpPortal.rotation));
                        }
                        if (spawnObj != null)
                        {
                            warpPortalEntity = spawnObj.GetComponent<WarpPortalEntity>();
                            warpPortalEntity.WarpPortalType = warpPortal.warpPortalType;
                            warpPortalEntity.WarpToMapInfo = warpPortal.warpToMapInfo;
                            warpPortalEntity.WarpToPosition = warpPortal.warpToPosition;
                            warpPortalEntity.WarpOverrideRotation = warpPortal.warpOverrideRotation;
                            warpPortalEntity.WarpToRotation = warpPortal.warpToRotation;
                            warpPortalEntity.WarpPointsByCondition = warpPortal.warpPointsByCondition;
                            Assets.NetworkSpawn(spawnObj);
                        }
                        await UniTask.NextFrame();
                        progress = 0f + ((float)i / (float)mapWarpPortals.Count * 0.25f);
                        onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
                    }
                }
            }
            await UniTask.NextFrame();
            progress = 0.25f;
            onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
            // Spawn Npcs
            if (LogInfo)
                Logging.Log(LogTag, "Spawning NPCs");
            if (GameInstance.MapNpcs.Count > 0)
            {
                if (GameInstance.MapNpcs.TryGetValue(CurrentMapInfo.Id, out List<Npc> mapNpcs))
                {
                    Npc npc;
                    NpcEntity npcPrefab;
                    AssetReferenceNpcEntity addressableNpcPrefab;
                    NpcEntity npcEntity;
                    for (i = 0; i < mapNpcs.Count; ++i)
                    {
                        npc = mapNpcs[i];
#if !EXCLUDE_PREFAB_REFS
                        npcPrefab = npc.entityPrefab;
#else
                        npcPrefab = null;
#endif
                        addressableNpcPrefab = npc.addressableEntityPrefab;
                        spawnObj = null;
                        if (npcPrefab != null)
                        {
                            spawnObj = Assets.GetObjectInstance(
                                npcPrefab.Identity.HashAssetId, npc.position,
                                Quaternion.Euler(npc.rotation));
                        }
                        else if (addressableNpcPrefab.IsDataValid())
                        {
                            spawnObj = Assets.GetObjectInstance(
                                addressableNpcPrefab.HashAssetId, npc.position,
                                Quaternion.Euler(npc.rotation));
                        }
                        if (spawnObj != null)
                        {
                            npcEntity = spawnObj.GetComponent<NpcEntity>();
                            npcEntity.Title = npc.title;
                            npcEntity.StartDialog = npc.startDialog;
                            npcEntity.Graph = npc.graph;
                            Assets.NetworkSpawn(spawnObj);
                        }
                        await UniTask.NextFrame();
                        progress = 0.25f + ((float)i / (float)mapNpcs.Count * 0.25f);
                        onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
                    }
                }
            }
            await UniTask.NextFrame();
            progress = 0.5f;
            onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
            // Spawn entities
            if (LogInfo)
                Logging.Log(LogTag, "Spawning entities");
            List<GameSpawnArea> gameSpawnAreas = GenericUtils.GetComponentsFromAllLoadedScenes<GameSpawnArea>(false);
            for (i = 0; i < gameSpawnAreas.Count; ++i)
            {
                gameSpawnAreas[i].SpawnAll();
                await UniTask.NextFrame();
                progress = 0.5f + ((float)i / (float)gameSpawnAreas.Count * 0.5f);
                onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
            }
            await UniTask.NextFrame();
            progress = 1f;
            onSpawnEntitiesProgress.Invoke(sceneName, false, true, progress);
            // If it's server (not host) spawn simple camera controller
            if (!IsClient && GameInstance.Singleton.serverCharacterPrefab != null &&
                SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
            {
                if (LogInfo)
                    Logging.Log(LogTag, "Spawning server character");
                Instantiate(GameInstance.Singleton.serverCharacterPrefab, CurrentMapInfo.StartPosition, Quaternion.identity);
            }
            await UniTask.NextFrame();
            // Entities were spawned
            progress = 1f;
            onSpawnEntitiesFinish.Invoke(sceneName, false, true, progress);
            await PostSpawnEntities();
            _isServerReadyToInstantiatePlayers = true;
        }

        protected virtual UniTask PreSpawnEntities()
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask PostSpawnEntities()
        {
            return UniTask.CompletedTask;
        }

        public bool IsServerReadyToInstantiateObjects()
        {
            if (!_isServerReadyToInstantiateObjects)
            {
                _serverReadyToInstantiateObjectsStates[INSTANTIATES_OBJECTS_DELAY_STATE_KEY] = Time.unscaledTime - _serverSceneLoadedTime >= INSTANTIATES_OBJECTS_DELAY;
                // NOTE: Make it works with old version 
                this.InvokeInstanceDevExtMethods("UpdateReadyToInstantiateObjectsStates", _serverReadyToInstantiateObjectsStates);
                this.InvokeInstanceDevExtMethods("UpdateServerReadyToInstantiateObjectsStates", _serverReadyToInstantiateObjectsStates);
                foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
                {
                    component.UpdateReadyToInstantiateObjectsStates(this, _serverReadyToInstantiateObjectsStates);
                    component.UpdateServerReadyToInstantiateObjectsStates(this, _serverReadyToInstantiateObjectsStates);
                }
                foreach (bool value in _serverReadyToInstantiateObjectsStates.Values)
                {
                    if (!value)
                        return false;
                }
                _isServerReadyToInstantiateObjects = true;
            }
            return true;
        }

        protected virtual async UniTaskVoid ProceedUntilClientReady()
        {
            do { await UniTask.Delay(100); } while (!IsClientReadyToInstantiateObjects());
            SendClientReady();
        }

        public bool IsClientReadyToInstantiateObjects()
        {
            if (!_isClientReadyToInstantiateObjects)
            {
                _clientReadyToInstantiateObjectsStates[INSTANTIATES_OBJECTS_DELAY_STATE_KEY] = Time.unscaledTime - _clientSceneLoadedTime >= INSTANTIATES_OBJECTS_DELAY;
                this.InvokeInstanceDevExtMethods("UpdateClientReadyToInstantiateObjectsStates", _clientReadyToInstantiateObjectsStates);
                foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
                {
                    component.UpdateClientReadyToInstantiateObjectsStates(this, _clientReadyToInstantiateObjectsStates);
                }
                foreach (bool value in _clientReadyToInstantiateObjectsStates.Values)
                {
                    if (!value)
                        return false;
                }
                _isClientReadyToInstantiateObjects = true;
            }
            return true;
        }

        public virtual void RegisterPlayerCharacter(long connectionId, BasePlayerCharacterEntity playerCharacter)
        {
            bool success = ServerUserHandlers.AddPlayerCharacter(connectionId, playerCharacter);
            if (success)
            {
                ServerLogHandlers.LogEnterGame(playerCharacter);
                onRegisterCharacter?.Invoke(connectionId, playerCharacter);
            }
        }

        public virtual void UnregisterPlayerCharacter(long connectionId)
        {
            ServerStorageHandlers.CloseStorage(connectionId).Forget();
            bool success = ServerUserHandlers.RemovePlayerCharacter(connectionId, out string characterId, out string userId);
            if (success)
            {
                if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out IPlayerCharacterData playerCharacter))
                    ServerLogHandlers.LogExitGame(characterId, userId);
                onUnregisterCharacter?.Invoke(connectionId, characterId, userId);
            }
        }

        public virtual void RegisterUserId(long connectionId, string userId)
        {
            bool success = ServerUserHandlers.AddUserId(connectionId, userId);
            if (success)
                onRegisterUser?.Invoke(connectionId, userId);
        }

        public virtual void UnregisterUserId(long connectionId)
        {
            bool success = ServerUserHandlers.RemoveUserId(connectionId, out string userId);
            if (success)
                onUnregisterUser?.Invoke(connectionId, userId);
        }

        public virtual BuildingEntity CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            LiteNetLibIdentity spawnObj;
            if (GameInstance.AddressableBuildingEntities.TryGetValue(saveData.EntityId, out AssetReferenceBuildingEntity addressablePrefab))
            {
                spawnObj = Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId,
                    saveData.Position, Quaternion.Euler(saveData.Rotation));
            }
            else if (GameInstance.BuildingEntities.TryGetValue(saveData.EntityId, out BuildingEntity prefab))
            {
                spawnObj = Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    saveData.Position, Quaternion.Euler(saveData.Rotation));
            }
            else
            {
                return null;
            }

            if (spawnObj == null)
            {
                return null;
            }

            BuildingEntity buildingEntity = spawnObj.GetComponent<BuildingEntity>();
            buildingEntity.Id = saveData.Id;
            buildingEntity.ParentId = saveData.ParentId;
            buildingEntity.CurrentHp = saveData.CurrentHp;
            buildingEntity.RemainsLifeTime = saveData.RemainsLifeTime;
            buildingEntity.IsLocked = saveData.IsLocked;
            buildingEntity.LockPassword = saveData.LockPassword;
            buildingEntity.CreatorId = saveData.CreatorId;
            buildingEntity.CreatorName = saveData.CreatorName;
            buildingEntity.ExtraData = saveData.ExtraData;
            Assets.NetworkSpawn(spawnObj);
            ServerBuildingHandlers.AddBuilding(buildingEntity.Id, buildingEntity);
            buildingEntity.CallRpcOnBuildingConstruct();
            return buildingEntity;
        }

        public virtual void DestroyBuildingEntity(string id, bool isSceneObject)
        {
            if (!isSceneObject)
                ServerBuildingHandlers.RemoveBuilding(id);
        }

        public void SetMapInfo(string mapName)
        {
            if (!GameInstance.MapInfos.TryGetValue(mapName, out BaseMapInfo mapInfo))
            {
                CurrentMapInfo = null;
                return;
            }
            SetMapInfo(mapInfo);
        }

        public void SetMapInfo(BaseMapInfo mapInfo)
        {
            if (mapInfo == null)
                return;
            CurrentMapInfo = mapInfo;
            SendMapInfo();
        }

        public void SendMapInfo()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendMapInfo(connectionId);
            }
        }

        public void SendMapInfo(long connectionId)
        {
            if (!IsServer || CurrentMapInfo == null)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.UpdateMapInfo, new UpdateMapInfoMessage()
            {
                mapName = CurrentMapInfo.Id,
                className = CurrentMapInfo.GetType().FullName,
            }, (writer) =>
            {
                CurrentMapInfo.Serialize(writer);
                this.InvokeInstanceDevExtMethods("WriteMapInfoExtra", writer);
                foreach (BaseGameNetworkManagerComponent component in ManagerComponents)
                {
                    component.WriteMapInfoExtra(this, writer);
                }
            });
        }

        public void SendTimeOfDay()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendTimeOfDay(connectionId);
            }
        }

        public void SendTimeOfDay(long connectionId)
        {
            if (!IsServer)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.Unreliable, GameNetworkingConsts.UpdateTimeOfDay, new UpdateTimeOfDayMessage()
            {
                timeOfDay = CurrentGameInstance.DayNightTimeUpdater.TimeOfDay,
            });
        }

        public void ServerSendSystemAnnounce(string message)
        {
            if (!IsServer)
                return;
            s_Writer.Reset();
            s_Writer.Put(new ChatMessage()
            {
                channel = ChatChannel.System,
                senderName = CHAT_SYSTEM_ANNOUNCER_SENDER,
                message = message,
                sendByServer = true,
            });
            HandleChatAtServer(new MessageHandlerData(GameNetworkingConsts.Chat, Server, -1, new NetDataReader(s_Writer.Data)));
        }

        public void ServerSendLocalMessage(string sender, string message)
        {
            if (!IsServer)
                return;
            s_Writer.Reset();
            s_Writer.Put(new ChatMessage()
            {
                channel = ChatChannel.Local,
                senderName = sender,
                message = message,
                sendByServer = true,
            });
            HandleChatAtServer(new MessageHandlerData(GameNetworkingConsts.Chat, Server, -1, new NetDataReader(s_Writer.Data)));
        }

        public void KickClient(long connectionId, UITextKeys message)
        {
            if (!IsServer)
                return;
            s_Writer.Reset();
            s_Writer.PutPackedUShort((ushort)message);
            KickClient(connectionId, s_Writer.Data);
        }
    }
}
