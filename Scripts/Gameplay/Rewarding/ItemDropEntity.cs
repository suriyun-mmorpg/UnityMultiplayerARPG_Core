using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class ItemDropEntity : BaseGameEntity, IPickupActivatableEntity
    {
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
        [FormerlySerializedAs("onItemDropDestroy")]
        [SerializeField]
        protected UnityEvent onPickedUp = new UnityEvent();
        public UnityEvent OnPickedUp { get { return onPickedUp; } }

        [Category(6, "Drop Settings")]
        public ItemDropManager itemDropManager = new ItemDropManager();
        public ItemDropManager ItemDropManager { get { return itemDropManager; } }

        #region Being deprecated
        [HideInInspector]
        [SerializeField]
        [Tooltip("Max kind of items that will be dropped in ground")]
        protected byte maxDropItems = 5;

        [HideInInspector]
        [SerializeField]
        [ArrayElementTitle("item")]
        protected ItemDrop[] randomItems;

        [HideInInspector]
        [SerializeField]
        protected ItemDropTable itemDropTable;
        #endregion

        public bool PutOnPlaceholder { get; protected set; }

        public RewardGivenType GivenType { get; protected set; }

        public List<CharacterItem> DropItems { get; protected set; } = new List<CharacterItem>();

        public HashSet<string> Looters { get; protected set; } = new HashSet<string>();

        public GameSpawnArea<ItemDropEntity> SpawnArea { get; protected set; }

        public ItemDropEntity SpawnPrefab { get; protected set; }

        public GameSpawnArea<ItemDropEntity>.AddressablePrefab SpawnAddressablePrefab { get; protected set; }

        public int SpawnLevel { get; protected set; }

        public Vector3 SpawnPosition { get; protected set; }

        public float DestroyDelay
        {
            get { return destroyDelay; }
        }

        public float DestroyRespawnDelay
        {
            get { return destroyRespawnDelay; }
        }

        private GameObject _dropModel;

        public override string EntityTitle
        {
            get
            {
                if (ItemDropData.putOnPlaceholder && GameInstance.Items.TryGetValue(ItemDropData.characterItem.dataId, out BaseItem item))
                    return item.Title;
                return base.EntityTitle;
            }
        }

        private bool _isModelContainerValidated = false;
        public Transform ModelContainer
        {
            get
            {
                if (!_isModelContainerValidated)
                {
                    if (modelContainer == null || modelContainer == transform)
                    {
                        modelContainer = new GameObject("_ModelContainer").transform;
                        modelContainer.transform.SetParent(transform);
                        modelContainer.transform.localPosition = Vector3.zero;
                        modelContainer.transform.localRotation = Quaternion.identity;
                        modelContainer.transform.localScale = Vector3.one;
                    }
                    _isModelContainerValidated = true;
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
        protected bool _isPickedUp;
        protected float _dropTime;

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            ItemDropManager.PrepareRelatesData();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            bool hasChanges = false;
            if (MigrateItemDropData())
                hasChanges = true;
            if (hasChanges)
                EditorUtility.SetDirty(this);
        }
#endif

        private bool MigrateItemDropData()
        {
            bool hasChanges = false;
            if (randomItems != null && randomItems.Length > 0)
            {
                hasChanges = true;
                List<ItemDrop> list = new List<ItemDrop>(itemDropManager.randomItems);
                list.AddRange(randomItems);
                itemDropManager.randomItems = list.ToArray();
                randomItems = null;
            }
            if (itemDropTable != null)
            {
                hasChanges = true;
                List<ItemDropTable> list = new List<ItemDropTable>(itemDropManager.itemDropTables)
                {
                    itemDropTable
                };
                itemDropManager.itemDropTables = list.ToArray();
                itemDropTable = null;
            }
            if (maxDropItems > 0)
            {
                hasChanges = true;
                itemDropManager.maxDropItems = maxDropItems;
                maxDropItems = 0;
            }
            return hasChanges;
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

        public virtual void Init()
        {
            _isPickedUp = false;
            _dropTime = Time.unscaledTime;
            if (!PutOnPlaceholder)
            {
                DropItems.Clear();
                ItemDropManager.RandomItems((item, amount) =>
                {
                    DropItems.Add(CharacterItem.Create(item.DataId, 1, amount));
                });
            }
            if (DropItems.Count == 0)
            {
                // No drop items data, it may not setup properly
                return;
            }
            ItemDropData = new ItemDropData()
            {
                putOnPlaceholder = PutOnPlaceholder,
                characterItem = DropItems[0],
            };
        }

        public override void OnSetup()
        {
            base.OnSetup();
            itemDropData.onChange += OnItemDropDataChange;
            if (IsServer && IsSceneObject)
            {
                // Init just once when started, if this entity is scene object
                Init();
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
            SpawnAddressablePrefab = null;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        public virtual void SetSpawnArea(GameSpawnArea<ItemDropEntity> spawnArea, GameSpawnArea<ItemDropEntity>.AddressablePrefab spawnAddressablePrefab, int spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = null;
            SpawnAddressablePrefab = spawnAddressablePrefab;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            itemDropData.onChange -= OnItemDropDataChange;
        }

        public void CallRpcOnPickedUp()
        {
            RPC(RpcOnPickedUp);
        }

        [AllRpc]
        protected virtual void RpcOnPickedUp()
        {
            if (onPickedUp != null)
                onPickedUp.Invoke();
        }

        protected virtual void OnItemDropDataChange(bool isInitial, ItemDropData itemDropData)
        {
#if !UNITY_SERVER
            // Instantiate model at clients
            if (!IsClient)
                return;
            // Activate container to show item drop model
            ModelContainer.gameObject.SetActive(true);
            if (_dropModel != null)
                Destroy(_dropModel);
            if (itemDropData.putOnPlaceholder && GameInstance.Items.TryGetValue(itemDropData.characterItem.dataId, out BaseItem item) && item.DropModel != null)
            {
                _dropModel = Instantiate(item.DropModel, ModelContainer);
                _dropModel.gameObject.SetLayerRecursively(CurrentGameInstance.itemDropLayer, true);
                _dropModel.gameObject.SetActive(true);
                _dropModel.RemoveComponentsInChildren<Collider>(false);
                _dropModel.transform.localPosition = Vector3.zero;
            }
#endif
        }

        public bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if ((Looters.Count == 0 || Looters.Contains(baseCharacterEntity.Id) ||
                Time.unscaledTime - _dropTime > CurrentGameInstance.itemLootLockDuration) && !_isPickedUp)
                return true;
            return false;
        }

        public void PickedUp()
        {
            if (!IsServer)
                return;
            if (_isPickedUp)
                return;
            // Mark as picked up
            _isPickedUp = true;
            // Tell clients that the entity is picked up
            CallRpcOnPickedUp();
            // Respawning later
            if (SpawnArea != null)
                SpawnArea.Spawn(SpawnPrefab, SpawnAddressablePrefab, SpawnLevel, DestroyDelay + DestroyRespawnDelay);
            else if (Identity.IsSceneObject)
                RespawnRoutine(DestroyDelay + DestroyRespawnDelay).Forget();
            // Destroy this entity
            NetworkDestroy(destroyDelay);
        }

        /// <summary>
        /// This function will be called if this object is placed in scene networked object
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected async UniTaskVoid RespawnRoutine(float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            Looters.Clear();
            Init();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                EntityTransform.position,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public static ItemDropEntity Drop(BaseGameEntity dropper, RewardGivenType givenType, CharacterItem dropData, IEnumerable<string> looters)
        {
            ItemDropEntity entity = null;
            ItemDropEntity prefab;
#if !EXCLUDE_PREFAB_REFS
            prefab = GameInstance.Singleton.itemDropEntityPrefab;
#else
            prefab = null;
#endif
            if (prefab != null)
            {
                entity = Drop(prefab, dropper, givenType, dropData, looters, GameInstance.Singleton.itemAppearDuration);
            }
            else if (GameInstance.Singleton.addressableItemDropEntityPrefab.IsDataValid())
            {
                entity = Drop(GameInstance.Singleton.addressableItemDropEntityPrefab.GetOrLoadAsset<AssetReferenceItemDropEntity, ItemDropEntity>(), dropper, givenType, dropData, looters, GameInstance.Singleton.itemAppearDuration);
            }
            return entity;
        }

        public static ItemDropEntity Drop(ItemDropEntity prefab, BaseGameEntity dropper, RewardGivenType givenType, CharacterItem dropData, IEnumerable<string> looters, float appearDuration)
        {
            Vector3 dropPosition = dropper.EntityTransform.position;
            Quaternion dropRotation = Quaternion.identity;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    // Random position around dropper with its height
                    dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, 0f, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    // Random rotation
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                    break;
                case DimensionType.Dimension2D:
                    // Random position around dropper
                    dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    break;
            }
            return Drop(prefab, dropPosition, dropRotation, givenType, dropData, looters, appearDuration);
        }

        public static ItemDropEntity Drop(ItemDropEntity prefab, Vector3 dropPosition, Quaternion dropRotation, RewardGivenType givenType, CharacterItem dropItem, IEnumerable<string> looters, float appearDuration)
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
            ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
            entity.GivenType = givenType;
            entity.PutOnPlaceholder = true;
            entity.DropItems = new List<CharacterItem> { dropItem };
            entity.Looters = new HashSet<string>(looters);
            entity.Init();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.NetworkDestroy(appearDuration);
            return entity;
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
            GameInstance.PlayingCharacterEntity.CallCmdPickup(ObjectId);
        }

        public virtual bool ProceedPickingUpAtServer(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            if (!IsAbleToLoot(characterEntity))
            {
                message = UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT;
                return false;
            }
            if (CurrentGameInstance.itemLootRandomPartyMember && Time.unscaledTime - _dropTime < CurrentGameInstance.itemLootLockDuration)
            {
                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(Looters.ToArray()[Random.Range(0, Looters.Count)], out IPlayerCharacterData randomCharacter))
                {
                    characterEntity = randomCharacter as BaseCharacterEntity;
                }
            }
            if (characterEntity.IncreasingItemsWillOverwhelming(DropItems))
            {
                message = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }
            characterEntity.IncreaseItems(DropItems, characterItem => characterEntity.OnRewardItem(GivenType, characterItem));
            characterEntity.FillEmptySlots();
            PickedUp();
            message = UITextKeys.NONE;
            return true;
        }
    }
}
