using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public class ItemDropEntity : BaseGameEntity, IPickupActivatableEntity
    {
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;
        private static readonly RaycastHit[] s_findGroundRaycastHits = new RaycastHit[4];

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
        private List<ItemDrop> _cacheRandomItems;
        public List<ItemDrop> CacheRandomItems
        {
            get
            {
                if (_cacheRandomItems == null)
                {
                    int i = 0;
                    _cacheRandomItems = new List<ItemDrop>();
                    if (randomItems != null &&
                        randomItems.Length > 0)
                    {
                        for (i = 0; i < randomItems.Length; ++i)
                        {
                            if (randomItems[i].item == null ||
                                randomItems[i].maxAmount <= 0 ||
                                randomItems[i].dropRate <= 0)
                                continue;
                            _cacheRandomItems.Add(randomItems[i]);
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
                            _cacheRandomItems.Add(itemDropTable.randomItems[i]);
                        }
                    }
                }
                return _cacheRandomItems;
            }
        }
        public bool PutOnPlaceholder { get; protected set; }
        public RewardGivenType GivenType { get; protected set; }
        public List<CharacterItem> DropItems { get; protected set; }
        public HashSet<string> Looters { get; protected set; }
        public GameSpawnArea<ItemDropEntity> SpawnArea { get; protected set; }
        public ItemDropEntity SpawnPrefab { get; protected set; }
        public int SpawnLevel { get; protected set; }
        public Vector3 SpawnPosition { get; protected set; }
        public float DestroyDelay { get { return destroyDelay; } }
        public float DestroyRespawnDelay { get { return destroyRespawnDelay; } }

        private GameObject _dropModel;

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

        private bool isModelContainerValidated = false;
        public Transform ModelContainer
        {
            get
            {
                if (!isModelContainerValidated)
                {
                    if (modelContainer == null || modelContainer == transform)
                    {
                        modelContainer = new GameObject("_ModelContainer").transform;
                        modelContainer.transform.SetParent(transform);
                        modelContainer.transform.localPosition = Vector3.zero;
                        modelContainer.transform.localRotation = Quaternion.identity;
                        modelContainer.transform.localScale = Vector3.one;
                    }
                    isModelContainerValidated = true;
                }
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
        protected float appearDuration;

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
            ModelContainer.gameObject.SetActive(false);
        }

        protected override void EntityOnDisable()
        {
            base.EntityOnDisable();
            ModelContainer.gameObject.SetActive(false);
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
                Looters = new HashSet<string>();
                if (CacheRandomItems.Count > 0)
                {
                    ItemDrop randomItem;
                    for (int countDrops = 0; countDrops < CacheRandomItems.Count && countDrops < maxDropItems; ++countDrops)
                    {
                        randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                        if (Random.value > randomItem.dropRate)
                            continue;
                        DropItems.Add(CharacterItem.Create(randomItem.item.DataId, 1, Random.Range(randomItem.minAmount <= 0 ? 1 : randomItem.minAmount, randomItem.maxAmount)));
                    }
                    if (DropItems.Count == 0)
                    {
                        randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                        DropItems.Add(CharacterItem.Create(randomItem.item.DataId, 1, Random.Range(randomItem.minAmount <= 0 ? 1 : randomItem.minAmount, randomItem.maxAmount)));
                    }
                }
            }
            if (DropItems == null || DropItems.Count == 0)
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
                NetworkDestroy(appearDuration);
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            itemDropData.deliveryMethod = DeliveryMethod.ReliableOrdered;
            itemDropData.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public virtual void SetSpawnArea(GameSpawnArea<ItemDropEntity> spawnArea, ItemDropEntity spawnPrefab, int spawnLevel, Vector3 spawnPosition)
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
            // Activate container to show item drop model
            ModelContainer.gameObject.SetActive(true);
            if (_dropModel != null)
                Destroy(_dropModel);
            BaseItem item;
            if (itemDropData.putOnPlaceholder && GameInstance.Items.TryGetValue(itemDropData.dataId, out item) && item.DropModel != null)
            {
                _dropModel = Instantiate(item.DropModel, ModelContainer);
                _dropModel.gameObject.SetLayerRecursively(CurrentGameInstance.itemDropLayer, true);
                _dropModel.gameObject.SetActive(true);
                _dropModel.RemoveComponentsInChildren<Collider>(false);
                _dropModel.transform.localPosition = Vector3.zero;
            }
        }

        public bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if ((Looters == null || Looters.Count == 0 || Looters.Contains(baseCharacterEntity.Id) ||
                Time.unscaledTime - dropTime > CurrentGameInstance.itemLootLockDuration) && !isPickedUp)
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
                EntityTransform.position,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
            InitDropItems();
        }

        public static ItemDropEntity DropItem(BaseGameEntity dropper, RewardGivenType givenType, CharacterItem dropData, IEnumerable<string> looters)
        {
            return DropItem(GameInstance.Singleton.itemDropEntityPrefab, dropper, givenType, dropData, looters, GameInstance.Singleton.itemAppearDuration);
        }

        public static ItemDropEntity DropItem(ItemDropEntity prefab, BaseGameEntity dropper, RewardGivenType givenType, CharacterItem dropData, IEnumerable<string> looters, float appearDuration)
        {
            Vector3 dropPosition = dropper.EntityTransform.position;
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
            return DropItem(prefab, dropPosition, dropRotation, givenType, dropData, looters, appearDuration);
        }

        public static ItemDropEntity DropItem(ItemDropEntity prefab, Vector3 dropPosition, Quaternion dropRotation, RewardGivenType givenType, CharacterItem dropItem, IEnumerable<string> looters, float appearDuration)
        {
            if (prefab == null)
                return null;

            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                // Find drop position on ground
                dropPosition = PhysicUtils.FindGroundedPosition(dropPosition, s_findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetItemDropGroundDetectionLayerMask());
            }
            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                prefab.Identity.HashAssetId,
                dropPosition, dropRotation);
            ItemDropEntity itemDropEntity = spawnObj.GetComponent<ItemDropEntity>();
            itemDropEntity.PutOnPlaceholder = true;
            itemDropEntity.GivenType = givenType;
            itemDropEntity.DropItems = new List<CharacterItem> { dropItem };
            itemDropEntity.Looters = new HashSet<string>(looters);
            itemDropEntity.isPickedUp = false;
            itemDropEntity.dropTime = Time.unscaledTime;
            itemDropEntity.appearDuration = appearDuration;
            itemDropEntity.InitDropItems();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemDropEntity;
        }

        public override bool SetAsTargetInOneClick()
        {
            return true;
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.pickUpItemDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return true;
        }

        public virtual bool CanPickupActivate()
        {
            return true;
        }

        public virtual void OnPickupActivate()
        {
            GameInstance.PlayingCharacterEntity.CallServerPickupItem(ObjectId);
        }
    }
}
