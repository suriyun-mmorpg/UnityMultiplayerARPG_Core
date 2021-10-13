using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public class ItemDropEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[1000];

        [Category("Relative GameObjects/Transforms")]
        [Tooltip("Item's `dropModel` will be instantiated to this transform for items which drops from characters")]
        [SerializeField]
        protected Transform modelContainer;

        [Category(5, "Respawn Settings")]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onItemDropDestroy` event before it's going to be destroyed from the game.")]
        [SerializeField]
        protected float destroyDelay = 0f;
        [SerializeField]
        protected float destroyRespawnDelay = 5f;

        [Category(99, "Events")]
        [SerializeField]
        protected UnityEvent onItemDropDestroy;

        [Category(6, "Drop Settings")]
        [Tooltip("Max kind of items that will be dropped in ground")]
        [SerializeField]
        protected byte maxDropItems = 5;
        [ArrayElementTitle("item")]
        [SerializeField]
        protected ItemDrop[] randomItems;
        [SerializeField]
        protected ItemDropTable itemDropTable;

        [System.NonSerialized]
        private List<ItemDrop> cacheRandomItems;
        public List<ItemDrop> CacheRandomItems
        {
            get
            {
                if (cacheRandomItems == null)
                {
                    int i = 0;
                    cacheRandomItems = new List<ItemDrop>();
                    if (randomItems != null &&
                        randomItems.Length > 0)
                    {
                        for (i = 0; i < randomItems.Length; ++i)
                        {
                            if (randomItems[i].item == null ||
                                randomItems[i].maxAmount <= 0 ||
                                randomItems[i].dropRate <= 0)
                                continue;
                            cacheRandomItems.Add(randomItems[i]);
                        }
                    }
                    if (itemDropTable != null &&
                        itemDropTable.randomItems != null &&
                        itemDropTable.randomItems.Length > 0)
                    {
                        for (i = 0; i < itemDropTable.randomItems.Length; ++i)
                        {
                            if (itemDropTable.randomItems[i].item == null ||
                                itemDropTable.randomItems[i].maxAmount <= 0 ||
                                itemDropTable.randomItems[i].dropRate <= 0)
                                continue;
                            cacheRandomItems.Add(itemDropTable.randomItems[i]);
                        }
                    }
                }
                return cacheRandomItems;
            }
        }
        public bool PutOnPlaceholder { get; protected set; }
        public List<CharacterItem> DropItems { get; protected set; }
        public HashSet<uint> Looters { get; protected set; }
        public GameSpawnArea<ItemDropEntity> SpawnArea { get; protected set; }
        public ItemDropEntity SpawnPrefab { get; protected set; }
        public short SpawnLevel { get; protected set; }
        public Vector3 SpawnPosition { get; protected set; }
        public float DestroyDelay { get { return destroyDelay; } }
        public float DestroyRespawnDelay { get { return destroyRespawnDelay; } }
        private GameObject dropModel;

        public override string EntityTitle
        {
            get
            {
                BaseItem item;
                if (ItemDropData.putOnPlaceholder && GameInstance.Items.TryGetValue(ItemDropData.dataId, out item))
                    return item.Title;
                return base.EntityTitle;
            }
        }

        public Transform CacheModelContainer
        {
            get
            {
                if (modelContainer == null)
                    modelContainer = GetComponent<Transform>();
                return modelContainer;
            }
        }

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldItemDropData itemDropData = new SyncFieldItemDropData();
        public ItemDropData ItemDropData
        {
            get { return itemDropData.Value; }
            set { itemDropData.Value = value; }
        }

        // Private variables
        protected bool isPickedUp;
        protected float dropTime;

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            // Add items from drop table
            List<BaseItem> items = new List<BaseItem>();
            foreach (var randomItem in CacheRandomItems)
            {
                if (randomItem.item == null)
                    continue;
                items.Add(randomItem.item);
            }
            GameInstance.AddItems(items);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.itemDropTag;
            gameObject.layer = CurrentGameInstance.itemDropLayer;
        }

        protected virtual void InitDropItems()
        {
            if (!IsServer)
                return;
            isPickedUp = false;
            if (!PutOnPlaceholder)
            {
                // Random drop items
                DropItems = new List<CharacterItem>();
                Looters = new HashSet<uint>();
                if (CacheRandomItems.Count > 0)
                {
                    ItemDrop randomItem;
                    for (int countDrops = 0; countDrops < CacheRandomItems.Count && countDrops < maxDropItems; ++countDrops)
                    {
                        randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                        if (Random.value > randomItem.dropRate)
                            continue;
                        DropItems.Add(CharacterItem.Create(randomItem.item.DataId, 1, (short)Random.Range(randomItem.minAmount <= 0 ? 1 : randomItem.minAmount, randomItem.maxAmount)));
                    }
                    if (DropItems.Count == 0)
                    {
                        randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                        DropItems.Add(CharacterItem.Create(randomItem.item.DataId, 1, (short)Random.Range(randomItem.minAmount <= 0 ? 1 : randomItem.minAmount, randomItem.maxAmount)));
                    }
                }
            }
            if (DropItems == null || DropItems.Count == 0 ||
                !GameInstance.Items.ContainsKey(DropItems[0].dataId))
            {
                NetworkDestroy(1f);
                return;
            }
            ItemDropData = new ItemDropData()
            {
                putOnPlaceholder = PutOnPlaceholder,
                dataId = DropItems[0].dataId,
                level = DropItems[0].level,
                amount = DropItems[0].amount,
            };
            if (PutOnPlaceholder)
            {
                // Destroy later by duration value
                NetworkDestroy(CurrentGameInstance.itemAppearDuration);
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            itemDropData.deliveryMethod = DeliveryMethod.ReliableOrdered;
            itemDropData.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public virtual void SetSpawnArea(GameSpawnArea<ItemDropEntity> spawnArea, ItemDropEntity spawnPrefab, short spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = spawnPrefab;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            itemDropData.onChange += OnItemDropDataChange;
            InitDropItems();
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            itemDropData.onChange -= OnItemDropDataChange;
        }

        [AllRpc]
        protected virtual void AllOnItemDropDestroy()
        {
            if (onItemDropDestroy != null)
                onItemDropDestroy.Invoke();
        }

        public void CallAllOnItemDropDestroy()
        {
            RPC(AllOnItemDropDestroy);
        }

        protected virtual void OnItemDropDataChange(bool isInitial, ItemDropData itemDropData)
        {
            // Instantiate model at clients
            if (!IsClient)
                return;
            if (dropModel != null)
                Destroy(dropModel);
            BaseItem item;
            if (CacheModelContainer != null && itemDropData.putOnPlaceholder &&
                GameInstance.Items.TryGetValue(itemDropData.dataId, out item) &&
                item.DropModel != null)
            {
                dropModel = Instantiate(item.DropModel, CacheModelContainer);
                dropModel.gameObject.SetLayerRecursively(CurrentGameInstance.itemDropLayer, true);
                dropModel.gameObject.SetActive(true);
                dropModel.RemoveComponentsInChildren<Collider>(false);
                dropModel.transform.localPosition = Vector3.zero;
            }
        }

        public bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if ((Looters == null || Looters.Count == 0 || Looters.Contains(baseCharacterEntity.ObjectId) ||
                Time.unscaledTime - dropTime > CurrentGameInstance.itemLootLockDuration) &&
                !isPickedUp)
                return true;
            return false;
        }

        public void PickedUp()
        {
            if (!IsServer)
                return;
            if (isPickedUp)
                return;
            // Mark as picked up
            isPickedUp = true;
            // Tell clients that the item drop destroy to play animation at client
            CallAllOnItemDropDestroy();
            // Respawning later
            if (SpawnArea != null)
                SpawnArea.Spawn(SpawnPrefab, SpawnLevel, DestroyDelay + DestroyRespawnDelay);
            else if (Identity.IsSceneObject)
                RespawnRoutine(DestroyDelay + DestroyRespawnDelay).Forget();
            // Destroy this entity
            NetworkDestroy(destroyDelay);
        }

        protected async UniTaskVoid RespawnRoutine(float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                CacheTransform.position,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
            InitDropItems();
        }

        public static ItemDropEntity DropItem(BaseGameEntity dropper, CharacterItem dropData, IEnumerable<uint> looters)
        {
            return DropItem(GameInstance.Singleton.itemDropEntityPrefab, dropper, dropData, looters);
        }

        public static ItemDropEntity DropItem(ItemDropEntity prefab, BaseGameEntity dropper, CharacterItem dropData, IEnumerable<uint> looters)
        {
            Vector3 dropPosition = dropper.CacheTransform.position;
            Quaternion dropRotation = Quaternion.identity;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    // Random position around dropper with its height
                    dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, GROUND_DETECTION_Y_OFFSETS, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    // Random rotation
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                    break;
                case DimensionType.Dimension2D:
                    // Random position around dropper
                    dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    break;
            }
            return DropItem(prefab, dropPosition, dropRotation, dropData, looters);
        }

        public static ItemDropEntity DropItem(ItemDropEntity prefab, Vector3 dropPosition, Quaternion dropRotation, CharacterItem dropItem, IEnumerable<uint> looters = null)
        {
            if (prefab == null)
                return null;

            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                // Find drop position on ground
                dropPosition = PhysicUtils.FindGroundedPosition(dropPosition, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetItemDropGroundDetectionLayerMask());
            }
            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                prefab.Identity.HashAssetId,
                dropPosition, dropRotation);
            ItemDropEntity itemDropEntity = spawnObj.GetComponent<ItemDropEntity>();
            itemDropEntity.PutOnPlaceholder = true;
            itemDropEntity.DropItems = new List<CharacterItem> { dropItem };
            itemDropEntity.Looters = new HashSet<uint>(looters);
            itemDropEntity.isPickedUp = false;
            itemDropEntity.dropTime = Time.unscaledTime;
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            itemDropEntity.InitDropItems();
            return itemDropEntity;
        }
    }
}
