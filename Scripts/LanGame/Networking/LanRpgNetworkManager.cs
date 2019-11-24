using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public sealed partial class LanRpgNetworkManager : BaseGameNetworkManager
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

        public struct PendingSpawnPlayerCharacter
        {
            public long connectionId;
            public PlayerCharacterData playerCharacterData;
        }

        public float autoSaveDuration = 2f;
        public GameStartType startType;
        public PlayerCharacterData selectedCharacter;
        public EnableGmCommandType enableGmCommands;
        private float lastSaveTime;
        private int nextPartyId = 1;
        private int nextGuildId = 1;
        private Vector3? teleportPosition;
        private readonly WorldSaveData worldSaveData = new WorldSaveData();
        private readonly StorageSaveData storageSaveData = new StorageSaveData();
        private readonly Dictionary<StorageId, List<CharacterItem>> storageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private readonly Dictionary<StorageId, HashSet<uint>> usingStorageCharacters = new Dictionary<StorageId, HashSet<uint>>();
        private readonly List<PendingSpawnPlayerCharacter> pendingSpawnPlayerCharacters = new List<PendingSpawnPlayerCharacter>();
        private bool isInstantiateSceneObjects;

        private LiteNetLibDiscovery cacheDiscovery;
        public LiteNetLibDiscovery CacheDiscovery
        {
            get
            {
                if (cacheDiscovery == null)
                    cacheDiscovery = GetComponent<LiteNetLibDiscovery>();
                if (cacheDiscovery == null)
                    cacheDiscovery = gameObject.AddComponent<LiteNetLibDiscovery>();
                return cacheDiscovery;
            }
        }

        public void StartGame()
        {
            NetworkSetting gameServiceConnection = gameInstance.NetworkSetting;
            switch (startType)
            {
                case GameStartType.Host:
                    SetMapInfo(selectedCharacter.CurrentMapName);
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

        protected override void Update()
        {
            base.Update();
            float tempUnscaledTime = Time.unscaledTime;
            if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
            {
                Profiler.BeginSample("LanRpgNetworkManager - Save Data");
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && IsClientConnected)
                {
                    owningCharacter.SavePersistentCharacterData();
                    if (IsServer)
                    {
                        SaveWorld();
                        SaveStorage();
                    }
                }
                Profiler.EndSample();
                lastSaveTime = tempUnscaledTime;
            }

            if (IsServer && pendingSpawnPlayerCharacters.Count > 0 && IsReadyToInstantiateObjects())
            {
                // Spawn pending player characters
                foreach (PendingSpawnPlayerCharacter spawnPlayerCharacter in pendingSpawnPlayerCharacters)
                {
                    SpawnPlayerCharacter(spawnPlayerCharacter.connectionId, spawnPlayerCharacter.playerCharacterData);
                }
                pendingSpawnPlayerCharacters.Clear();
            }
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

        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            selectedCharacter.SerializeCharacterData(writer);
        }

        public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader)
        {
            if (!IsReadyToInstantiateObjects())
            {
                // Not ready to instantiate objects, add spawning player character to pending dictionary
                if (LogDev) Debug.Log("[LanRpgNetworkManager] Not ready to deserializing client ready extra");
                pendingSpawnPlayerCharacters.Add(new PendingSpawnPlayerCharacter()
                {
                    connectionId = connectionId,
                    playerCharacterData = new PlayerCharacterData().DeserializeCharacterData(reader)
                });
                return;
            }

            if (LogDev) Debug.Log("[LanRpgNetworkManager] Deserializing client ready extra");
            SpawnPlayerCharacter(connectionId, new PlayerCharacterData().DeserializeCharacterData(reader));
        }

        private void SpawnPlayerCharacter(long connectionId, PlayerCharacterData playerCharacterData)
        {
            BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
            // If it is not allow this character data, disconnect user
            if (entityPrefab == null)
            {
                Debug.LogError("[LanRpgNetworkManager] Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                return;
            }
            if (!CurrentMapInfo.Id.Equals(playerCharacterData.CurrentMapName))
                playerCharacterData.CurrentPosition = teleportPosition.HasValue ? teleportPosition.Value : CurrentMapInfo.startPosition;
            GameObject spawnObj = Instantiate(entityPrefab.gameObject, playerCharacterData.CurrentPosition, Quaternion.identity);
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
            if (playerCharacters.Count == 0)
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

            // Force make caches, to calculate current stats to fill empty slots items
            playerCharacterEntity.ForceMakeCaches();
            playerCharacterEntity.FillEmptySlots();

            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
                playerCharacterEntity.RequestOnRespawn();
            else
                playerCharacterEntity.RequestOnDead();

            // Register player, will use registered player to send chat / player messages
            RegisterPlayerCharacter(playerCharacterEntity);
        }

        private void SaveWorld()
        {
            if (!isInstantiateSceneObjects)
                return;
            // Save building entities / Tree / Rocks
            BasePlayerCharacterEntity playerCharacterEntity = BasePlayerCharacterController.OwningCharacter;
            worldSaveData.buildings.Clear();
            foreach (BuildingEntity buildingEntity in buildingEntities.Values)
            {
                if (buildingEntity == null) continue;
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
            worldSaveData.SavePersistentData(playerCharacterEntity.Id, CurrentMapInfo.Id);
        }

        private void SaveStorage()
        {
            if (!isInstantiateSceneObjects)
                return;
            BasePlayerCharacterEntity playerCharacterEntity = BasePlayerCharacterController.OwningCharacter;
            storageSaveData.storageItems.Clear();
            foreach (StorageId key in storageItems.Keys)
            {
                foreach (CharacterItem item in storageItems[key])
                {
                    storageSaveData.storageItems.Add(new StorageCharacterItem()
                    {
                        storageType = key.storageType,
                        storageOwnerId = key.storageOwnerId,
                        characterItem = item,
                    });
                }
            }
            storageSaveData.SavePersistentData(playerCharacterEntity.Id);
        }

        protected override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName))
            {
                playerCharacterEntity.Teleport(position);
                return;
            }

            long connectionId = playerCharacterEntity.ConnectionId;
            MapInfo mapInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                playerCharacters.ContainsKey(connectionId) &&
                playerCharacterEntity.IsServer &&
                playerCharacterEntity.IsOwnerClient &&
                GameInstance.MapInfos.TryGetValue(mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Save data before warp
                SaveWorld();
                SaveStorage();
                buildingEntities.Clear();
                storageItems.Clear();
                SetMapInfo(mapInfo);
                teleportPosition = position;
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null)
                {
                    selectedCharacter = owningCharacter.CloneTo(selectedCharacter);
                    selectedCharacter.CurrentMapName = mapInfo.Id;
                    selectedCharacter.CurrentPosition = position;
                    selectedCharacter.SavePersistentCharacterData();
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
                ServerSceneChange(mapInfo.scene);
            }
        }

        #region Implement Singleplayer / Lan - in-app purchasing
        protected override void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            // Set response data
            ResponseCashShopInfoMessage.Error error = ResponseCashShopInfoMessage.Error.None;
            int cash = 0;
            List<int> cashShopItemIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                // Set cash shop item ids
                cashShopItemIds.AddRange(GameInstance.CashShopItems.Keys);
            }
            // Send response message
            ResponseCashShopInfoMessage responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashShopItemIds = cashShopItemIds.ToArray();
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected override void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashShopBuyMessage message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            // Set response data
            ResponseCashShopBuyMessage.Error error = ResponseCashShopBuyMessage.Error.None;
            int dataId = message.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    error = ResponseCashShopBuyMessage.Error.ItemNotFound;
                }
                else if (cash < cashShopItem.sellPrice)
                {
                    // Not enough cash
                    error = ResponseCashShopBuyMessage.Error.NotEnoughCash;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.receiveItems))
                {
                    // Cannot carry all rewards
                    error = ResponseCashShopBuyMessage.Error.CannotCarryAllRewards;
                }
                else
                {
                    // Decrease cash amount
                    cash -= cashShopItem.sellPrice;
                    playerCharacter.UserCash = cash;
                    // Increase character gold
                    playerCharacter.Gold += cashShopItem.receiveGold;
                    // Increase character item
                    foreach (ItemAmount receiveItem in cashShopItem.receiveItems)
                    {
                        if (receiveItem.item == null || receiveItem.amount <= 0) continue;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
                    }
                    playerCharacter.FillEmptySlots();
                }
            }
            // Send response message
            ResponseCashShopBuyMessage responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected override void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            // Set response data
            ResponseCashPackageInfoMessage.Error error = ResponseCashPackageInfoMessage.Error.None;
            int cash = 0;
            List<int> cashPackageIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                // Set cash package ids
                cashPackageIds.AddRange(GameInstance.CashPackages.Keys);
            }
            // Send response message
            ResponseCashPackageInfoMessage responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashPackageIds = cashPackageIds.ToArray();
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected override void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashPackageBuyValidationMessage message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            // TODO: Validate purchasing at server side
            // Set response data
            ResponseCashPackageBuyValidationMessage.Error error = ResponseCashPackageBuyValidationMessage.Error.None;
            int dataId = message.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                {
                    // Cannot find package
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
                }
                else
                {
                    // Increase cash amount
                    cash += cashPackage.cashAmount;
                    playerCharacter.UserCash = cash;
                }
            }
            // Send response message
            ResponseCashPackageBuyValidationMessage responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageBuyValidation, responseMessage);
        }
        #endregion

        #region Implement Abstract Functions
        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            CreateParty(playerCharacterEntity, shareExp, shareItem, nextPartyId++);
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            CreateGuild(playerCharacterEntity, guildName, nextGuildId++);
        }

        public override void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!CanAccessStorage(playerCharacterEntity, playerCharacterEntity.CurrentStorageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!storageItems.ContainsKey(playerCharacterEntity.CurrentStorageId))
                storageItems[playerCharacterEntity.CurrentStorageId] = new List<CharacterItem>();
            if (!usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId] = new HashSet<uint>();
            usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Add(playerCharacterEntity.ObjectId);
            // Prepare storage data
            Storage storage = GetStorage(playerCharacterEntity.CurrentStorageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            storageItems[playerCharacterEntity.CurrentStorageId].FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage items
            playerCharacterEntity.StorageItems = storageItems[playerCharacterEntity.CurrentStorageId];
        }

        public override void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Remove(playerCharacterEntity.ObjectId);
            playerCharacterEntity.StorageItems.Clear();
        }

        public override void MoveItemToStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short nonEquipIndex, short amount, short storageItemIndex)
        {
            if (!CanAccessStorage(playerCharacterEntity, playerCharacterEntity.CurrentStorageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!storageItems.ContainsKey(storageId))
                storageItems[storageId] = new List<CharacterItem>();
            List<CharacterItem> storageItemList = storageItems[storageId];
            if (nonEquipIndex < 0 || nonEquipIndex >= playerCharacterEntity.NonEquipItems.Count)
            {
                // Don't do anything, if non equip item index is invalid
                return;
            }
            // Prepare storage data
            Storage storage = GetStorage(storageId);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem movingItem = playerCharacterEntity.NonEquipItems[nonEquipIndex].Clone();
            movingItem.amount = amount;
            if (storageItemIndex < 0 ||
                storageItemIndex >= storageItemList.Count ||
                storageItemList[storageItemIndex].dataId == movingItem.dataId)
            {
                // Add to storage or merge
                bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                    movingItem.dataId, movingItem.amount, isLimitWeight, weightLimit,
                    storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
                if (!isOverwhelming && storageItemList.IncreaseItems(movingItem))
                {
                    // Decrease from inventory
                    playerCharacterEntity.DecreaseItemsByIndex(nonEquipIndex, amount);
                }
            }
            else
            {
                // Swapping
                CharacterItem storageItem = storageItemList[storageItemIndex];
                CharacterItem nonEquipItem = playerCharacterEntity.NonEquipItems[nonEquipIndex];

                storageItemList[storageItemIndex] = nonEquipItem;
                playerCharacterEntity.NonEquipItems[nonEquipIndex] = storageItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
        }

        public override void MoveItemFromStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short storageItemIndex, short amount, short nonEquipIndex)
        {
            if (!CanAccessStorage(playerCharacterEntity, playerCharacterEntity.CurrentStorageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!storageItems.ContainsKey(storageId))
                storageItems[storageId] = new List<CharacterItem>();
            List<CharacterItem> storageItemList = storageItems[storageId];
            if (storageItemIndex < 0 || storageItemIndex >= storageItemList.Count)
            {
                // Don't do anything, if storage item index is invalid
                return;
            }
            // Prepare storage data
            Storage storage = GetStorage(storageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem movingItem = storageItemList[storageItemIndex].Clone();
            movingItem.amount = amount;
            if (nonEquipIndex < 0 ||
                nonEquipIndex >= playerCharacterEntity.NonEquipItems.Count ||
                playerCharacterEntity.NonEquipItems[nonEquipIndex].dataId == movingItem.dataId)
            {
                // Add to inventory or merge
                bool isOverwhelming = playerCharacterEntity.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                if (!isOverwhelming && playerCharacterEntity.IncreaseItems(movingItem))
                {
                    // Decrease from storage
                    storageItemList.DecreaseItemsByIndex(storageItemIndex, amount);
                }
            }
            else
            {
                // Swapping
                CharacterItem storageItem = storageItemList[storageItemIndex];
                CharacterItem nonEquipItem = playerCharacterEntity.NonEquipItems[nonEquipIndex];

                storageItemList[storageItemIndex] = nonEquipItem;
                playerCharacterEntity.NonEquipItems[nonEquipIndex] = storageItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
        }

        public override void SwapOrMergeStorageItem(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short fromIndex, short toIndex)
        {
            if (!CanAccessStorage(playerCharacterEntity, playerCharacterEntity.CurrentStorageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!storageItems.ContainsKey(storageId))
                storageItems[storageId] = new List<CharacterItem>();
            List<CharacterItem> storageItemList = storageItems[storageId];
            if (fromIndex >= storageItemList.Count ||
                toIndex >= storageItemList.Count)
            {
                // Don't do anything, if storage item index is invalid
                return;
            }
            // Prepare storage data
            Storage storage = GetStorage(storageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem fromItem = storageItemList[fromIndex];
            CharacterItem toItem = storageItemList[toIndex];

            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    storageItemList[fromIndex] = CharacterItem.Empty;
                    storageItemList[toIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    storageItemList[fromIndex] = fromItem;
                    storageItemList[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                storageItemList[fromIndex] = toItem;
                storageItemList[toIndex] = fromItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
        }

        public override bool IsStorageEntityOpen(StorageEntity storageEntity)
        {
            if (storageEntity == null)
                return false;
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            return usingStorageCharacters.ContainsKey(id) &&
                usingStorageCharacters[id].Count > 0;
        }

        private void UpdateStorageItemsToCharacters(HashSet<uint> objectIds, List<CharacterItem> storageItems)
        {
            PlayerCharacterEntity playerCharacterEntity;
            foreach (uint objectId in objectIds)
            {
                if (Assets.TryGetSpawnedObject(objectId, out playerCharacterEntity))
                {
                    // Update storage items
                    playerCharacterEntity.StorageItems = storageItems;
                }
            }
        }

        public override void DepositGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            if (playerCharacterEntity.Gold - amount >= 0)
            {
                playerCharacterEntity.Gold -= amount;
                playerCharacterEntity.UserGold += amount;
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
        }

        public override void WithdrawGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            if (playerCharacterEntity.UserGold - amount >= 0)
            {
                playerCharacterEntity.UserGold -= amount;
                playerCharacterEntity.Gold += amount;
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
        }

        public override void DepositGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            GuildData guild;
            if (guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
            {
                if (playerCharacterEntity.Gold - amount >= 0)
                {
                    playerCharacterEntity.Gold -= amount;
                    guild.gold += amount;
                    guilds[playerCharacterEntity.GuildId] = guild;
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
            if (guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
            {
                if (guild.gold - amount >= 0)
                {
                    guild.gold -= amount;
                    playerCharacterEntity.Gold += amount;
                    guilds[playerCharacterEntity.GuildId] = guild;
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }

        public override void FindCharacters(BasePlayerCharacterEntity playerCharacterEntity, string characterName)
        {
            List<SocialCharacterData> socialCharacters = new List<SocialCharacterData>();
            foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
            {
                if (playerCharacter.Id.Equals(playerCharacterEntity.Id) ||
                    !playerCharacter.CharacterName.Equals(characterName))
                    continue;
                socialCharacters.Add(new SocialCharacterData()
                {
                    id = playerCharacter.Id,
                    characterName = playerCharacter.CharacterName,
                    dataId = playerCharacter.DataId,
                    level = playerCharacter.Level,
                });
            }
            Server.SendSocialMembers(playerCharacterEntity.ConnectionId, MsgTypes.UpdateFoundCharacters, socialCharacters.ToArray());
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

        protected override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            // For now just warp follow host
            // TODO: May add instance by load scene additive and offsets for LAN mode
            WarpCharacter(playerCharacterEntity, mapName, position);
        }

        protected override bool IsInstanceMap()
        {
            return false;
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            isInstantiateSceneObjects = false;
            StartCoroutine(OnServerOnlineSceneLoadedRoutine());
        }

        private IEnumerator OnServerOnlineSceneLoadedRoutine()
        {
            while (!IsReadyToInstantiateObjects())
            {
                yield return null;
            }
            // Load and Spawn buildings
            worldSaveData.LoadPersistentData(selectedCharacter.Id, CurrentMapInfo.Id);
            yield return null;
            foreach (BuildingSaveData building in worldSaveData.buildings)
            {
                CreateBuildingEntity(building, true);
            }
            // Load storage data
            storageSaveData.LoadPersistentData(selectedCharacter.Id);
            yield return null;
            foreach (StorageCharacterItem storageItem in storageSaveData.storageItems)
            {
                StorageId storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                if (!storageItems.ContainsKey(storageId))
                    storageItems[storageId] = new List<CharacterItem>();
                storageItems[storageId].Add(storageItem.characterItem);
            }
            // Spawn harvestables
            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
            isInstantiateSceneObjects = true;
        }
        #endregion
    }
}
