using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class ItemDropEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public const int FIND_GROUND_RAYCAST_HIT_SIZE = 10;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];
        [Header("Generic settings")]
        [Header("Monster Character Settings")]
        [Tooltip("The title which will be used with item drop entity which placed into the scene (not drops from characters)")]
        [SerializeField]
        protected string itemTitle;
        [Tooltip("Item titles by language keys")]
        [SerializeField]
        protected LanguageData[] itemTitles;
        [Tooltip("Item's `dropModel` will be instantiated to this transform for items which drops from characters")]
        [SerializeField]
        protected Transform modelContainer;
        [Header("Respawn settings")]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onItemDropDestroy` event before it's going to be destroyed from the game.")]
        [SerializeField]
        protected float destroyDelay = 0f;
        [SerializeField]
        protected float destroyRespawnDelay = 5f;
        [SerializeField]
        protected UnityEvent onItemDropDestroy;
        [Header("Drop items settings")]
        [Tooltip("Max kind of items that will be dropped in ground")]
        [SerializeField]
        protected byte maxDropItems = 5;
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
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
                    cacheRandomItems = new List<ItemDrop>(randomItems);
                    if (itemDropTable != null)
                        cacheRandomItems.AddRange(itemDropTable.randomItems);
                }
                return cacheRandomItems;
            }
        }
        public List<CharacterItem> DropItems { get; protected set; }
        public HashSet<uint> Looters { get; protected set; }
        public ItemDropSpawnArea SpawnArea { get; protected set; }
        public Vector3 SpawnPosition { get; protected set; }

        protected int? characterDropItemId;
        protected bool isPickedUp;
        protected float dropTime;

        [SerializeField]
        protected SyncFieldInt itemDataId = new SyncFieldInt();

        public BaseItem Item
        {
            get
            {
                BaseItem item;
                if (GameInstance.Items.TryGetValue(itemDataId, out item))
                    return item;
                return null;
            }
        }

        public string ItemTitle
        {
            get { return Language.GetText(itemTitles, itemTitle); }
        }

        public override string Title
        {
            get
            {
                BaseItem item = Item;
                return item == null ? ItemTitle : item.Title;
            }
            set { }
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
            if (IsServer)
            {
                isPickedUp = false;
                if (characterDropItemId.HasValue)
                {
                    // Item drop from character, so set item data id to instantiate drop model at clients
                    if (!GameInstance.Items.ContainsKey(characterDropItemId.Value))
                        NetworkDestroy();
                    itemDataId.Value = characterDropItemId.Value;
                    NetworkDestroy(CurrentGameInstance.itemAppearDuration);
                }
                else
                {
                    // Random drop items
                    DropItems = new List<CharacterItem>();
                    Looters = new HashSet<uint>();
                    ItemDrop randomItem;
                    for (int countDrops = 0; countDrops < CacheRandomItems.Count && countDrops < maxDropItems; ++countDrops)
                    {
                        randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                        if (randomItem.item == null ||
                            randomItem.amount == 0 ||
                            Random.value > randomItem.dropRate)
                            continue;
                        DropItems.Add(CharacterItem.Create(randomItem.item.DataId, 1, randomItem.amount));
                    }
                }
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            itemDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            itemDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public virtual void SetSpawnArea(ItemDropSpawnArea spawnArea, Vector3 spawnPosition)
        {
            this.SpawnArea = spawnArea;
            this.SpawnPosition = spawnPosition;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            itemDataId.onChange += OnItemDataIdChange;
            RegisterNetFunction(NetFuncOnItemDropDestroy);
            InitDropItems();
        }

        protected virtual void NetFuncOnItemDropDestroy()
        {
            if (onItemDropDestroy != null)
                onItemDropDestroy.Invoke();
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            itemDataId.onChange -= OnItemDataIdChange;
        }

        protected virtual void OnItemDataIdChange(bool isInitial, int itemDataId)
        {
            // Instantiate model at clients
            if (!IsClient)
                return;
            BaseItem item;
            if (CacheModelContainer != null && GameInstance.Items.TryGetValue(itemDataId, out item) && item.DropModel != null)
            {
                GameObject model = Instantiate(item.DropModel, CacheModelContainer);
                model.gameObject.SetLayerRecursively(CurrentGameInstance.itemDropLayer, true);
                model.gameObject.SetActive(true);
                model.RemoveComponentsInChildren<Collider>(false);
                model.transform.localPosition = Vector3.zero;
            }
        }

        public bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if ((Looters == null ||
                Looters.Contains(baseCharacterEntity.ObjectId) ||
                Time.unscaledTime - dropTime > CurrentGameInstance.itemLootLockDuration) &&
                !isPickedUp)
                return true;
            return false;
        }

        public void PickedUp()
        {
            if (!IsServer)
                return;

            // Mark as picked up
            isPickedUp = true;

            // Tell clients that the item drop destroy to play animation at client
            CallNetFunction(NetFuncOnItemDropDestroy, FunctionReceivers.All);

            // Destroy and Respawn
            if (SpawnArea != null)
                SpawnArea.Spawn(destroyDelay + destroyRespawnDelay);
            else if (Identity.IsSceneObject)
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(destroyDelay);
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(destroyDelay + destroyRespawnDelay);
            InitDropItems();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                CacheTransform.position,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public static ItemDropEntity DropItem(BaseGameEntity dropper, CharacterItem dropData, IEnumerable<uint> looters)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            Vector3 dropPosition = dropper.CacheTransform.position;
            Quaternion dropRotation = Quaternion.identity;
            switch (gameInstance.DimensionType)
            {
                case DimensionType.Dimension2D:
                    // Random position around character
                    dropPosition = dropPosition + new Vector3(Random.Range(-1f, 1f) * gameInstance.dropDistance, Random.Range(-1f, 1f) * gameInstance.dropDistance);
                    break;
                case DimensionType.Dimension3D:
                    // Random position around character
                    dropPosition = dropPosition + new Vector3(Random.Range(-1f, 1f) * gameInstance.dropDistance, 0, Random.Range(-1f, 1f) * gameInstance.dropDistance);
                    // Random rotation
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                    break;
            }
            return DropItem(dropPosition, dropRotation, dropData, looters);
        }

        public static ItemDropEntity DropItem(Vector3 dropPosition, Quaternion dropRotation, CharacterItem dropData, IEnumerable<uint> looters = null)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            if (gameInstance.itemDropEntityPrefab == null)
                return null;

            if (gameInstance.DimensionType == DimensionType.Dimension3D)
            {
                // Find drop position on ground
                dropPosition = PhysicUtils.FindGroundedPosition(dropPosition, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, gameInstance.GetItemDropGroundDetectionLayerMask());
            }
            GameObject spawnObj = Instantiate(gameInstance.itemDropEntityPrefab.gameObject, dropPosition, dropRotation);
            ItemDropEntity itemDropEntity = spawnObj.GetComponent<ItemDropEntity>();
            itemDropEntity.DropItems = new List<CharacterItem> { dropData };
            itemDropEntity.Looters = new HashSet<uint>(looters);
            itemDropEntity.characterDropItemId = dropData.dataId;
            itemDropEntity.isPickedUp = false;
            itemDropEntity.dropTime = Time.unscaledTime;
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemDropEntity;
        }
    }
}
