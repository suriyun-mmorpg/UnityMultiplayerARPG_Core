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

        public float autoSaveDuration = 2f;
        public GameStartType startType;
        public PlayerCharacterData selectedCharacter;
        public bool enableGmCommands;
        private float lastSaveTime;
        private int nextPartyId = 1;
        private int nextGuildId = 1;
        private Vector3? teleportPosition;
        private readonly WorldSaveData worldSaveData = new WorldSaveData();
        private readonly StorageSaveData storageSaveData = new StorageSaveData();
        private readonly Dictionary<StorageId, List<CharacterItem>> storageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private readonly Dictionary<StorageId, HashSet<uint>> usingStorageCharacters = new Dictionary<StorageId, HashSet<uint>>();

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
                    break;
                case GameStartType.SinglePlayer:
                    SetMapInfo(selectedCharacter.CurrentMapName);
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
            if (!CurrentMapInfo.Id.Equals(playerCharacterData.CurrentMapName))
                playerCharacterData.CurrentPosition = teleportPosition.HasValue ? teleportPosition.Value : CurrentMapInfo.startPosition;
            LiteNetLibIdentity identity = Assets.NetworkSpawn(entityPrefab.Identity.HashAssetId, playerCharacterData.CurrentPosition, Quaternion.identity, 0, connectionId);
            BasePlayerCharacterEntity playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
            playerCharacterData.CloneTo(playerCharacterEntity);

            // Set user Id
            playerCharacterEntity.UserId = playerCharacterEntity.Id;

            // Enable GM commands in Singleplayer / LAN mode
            // TODO: Don't use fixed user level
            if (enableGmCommands)
                playerCharacterEntity.UserLevel = 1;

            // Load world / storage for first character (host)
            if (playerCharacters.Count == 0)
            {
                worldSaveData.LoadPersistentData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
                storageSaveData.LoadPersistentData(playerCharacterEntity.Id);
                foreach (StorageCharacterItem storageItem in storageSaveData.storageItems)
                {
                    StorageId storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                    if (!storageItems.ContainsKey(storageId))
                        storageItems[storageId] = new List<CharacterItem>();
                    storageItems[storageId].Add(storageItem.characterItem);
                }
                StartCoroutine(SpawnBuildingsAndHarvestables(worldSaveData));
            }

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
            worldSaveData.SavePersistentData(playerCharacterEntity.Id, playerCharacterEntity.CurrentMapName);
        }

        private void SaveStorage()
        {
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
            base.WarpCharacter(playerCharacterEntity, mapName, position);
            long connectionId = playerCharacterEntity.ConnectionId;
            MapInfo mapInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectionId) &&
                playerCharacterEntity.IsServer &&
                playerCharacterEntity.IsOwnerClient &&
                GameInstance.MapInfos.TryGetValue(mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                SetMapInfo(mapInfo);
                teleportPosition = position;
                // Unregister all players characters to register later after map changed
                foreach (LiteNetLibPlayer player in GetPlayers())
                {
                    UnregisterPlayerCharacter(player.ConnectionId);
                }
                ServerSceneChange(mapInfo.scene);
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

        public override void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            StorageId storageId = playerCharacterEntity.CurrentStorageId;
            if (storageId.storageType == StorageType.Guild &&
                !guilds.ContainsKey(playerCharacterEntity.GuildId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return;
            }
            if (!storageItems.ContainsKey(storageId))
                storageItems[storageId] = new List<CharacterItem>();
            if (!usingStorageCharacters.ContainsKey(storageId))
                usingStorageCharacters[storageId] = new HashSet<uint>();
            usingStorageCharacters[storageId].Add(playerCharacterEntity.ObjectId);
            // Prepare storage data
            Storage storage = GetStorage(playerCharacterEntity.CurrentStorageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            CharacterDataExtension.FillEmptySlots(storageItems[storageId], isLimitSlot, slotLimit);
            // Update storage items
            playerCharacterEntity.StorageItems = storageItems[storageId];
        }

        public override void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            StorageId storageId = playerCharacterEntity.CurrentStorageId;
            usingStorageCharacters[storageId].Remove(playerCharacterEntity.ObjectId);
            playerCharacterEntity.StorageItems.Clear();
        }

        public override void MoveItemToStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short nonEquipIndex, short amount, short storageItemIndex)
        {
            if (storageId.storageType == StorageType.Guild &&
                !guilds.ContainsKey(playerCharacterEntity.GuildId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
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
                !storageItemList[storageItemIndex].NotEmptySlot() ||
                storageItemList[storageItemIndex].dataId == movingItem.dataId)
            {
                // Add to storage or merge
                bool isOverwhelming = CharacterDataExtension.IncreasingItemsWillOverwhelming(
                    storageItemList, movingItem.dataId, movingItem.amount, isLimitWeight, weightLimit,
                    CharacterDataExtension.GetTotalItemWeight(storageItemList), isLimitSlot, slotLimit);
                if (!isOverwhelming && CharacterDataExtension.IncreaseItems(storageItemList, movingItem))
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
            CharacterDataExtension.FillEmptySlots(storageItemList, isLimitSlot, slotLimit);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
        }

        public override void MoveItemFromStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short storageItemIndex, short amount, short nonEquipIndex)
        {
            if (storageId.storageType == StorageType.Guild &&
                !guilds.ContainsKey(playerCharacterEntity.GuildId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
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
                !playerCharacterEntity.NonEquipItems[nonEquipIndex].NotEmptySlot() ||
                playerCharacterEntity.NonEquipItems[nonEquipIndex].dataId == movingItem.dataId)
            {
                // Add to inventory or merge
                bool isOverwhelming = playerCharacterEntity.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                if (!isOverwhelming && playerCharacterEntity.IncreaseItems(movingItem))
                {
                    // Decrease from storage
                    CharacterDataExtension.DecreaseItemsByIndex(storageItemList, storageItemIndex, amount);
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
            CharacterDataExtension.FillEmptySlots(storageItemList, isLimitSlot, slotLimit);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
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
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
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
        #endregion
    }
}
