using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public sealed class ItemDropEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public CharacterItem dropData;
        public HashSet<uint> looters;
        public Transform modelContainer;
        private float dropTime;
        private bool isPickedUp;

        [SerializeField]
        private SyncFieldInt itemDataId = new SyncFieldInt();

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

        public override string Title
        {
            get
            {
                BaseItem item = Item;
                return item == null ? LanguageManager.GetUnknowTitle() : item.Title;
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

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.itemDropTag;
            gameObject.layer = CurrentGameInstance.itemDropLayer;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            if (IsServer)
            {
                int id = dropData.dataId;
                dropTime = Time.unscaledTime;
                if (!GameInstance.Items.ContainsKey(id))
                    NetworkDestroy();
                itemDataId.Value = id;
                NetworkDestroy(CurrentGameInstance.itemAppearDuration);
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            itemDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            itemDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            itemDataId.onChange += OnItemDataIdChange;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            itemDataId.onChange -= OnItemDataIdChange;
        }

        private void OnItemDataIdChange(bool isInitial, int itemDataId)
        {
            BaseItem item;
            if (GameInstance.Items.TryGetValue(itemDataId, out item) && item.DropModel != null)
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
            if (looters == null ||
                looters.Contains(baseCharacterEntity.ObjectId) ||
                Time.unscaledTime - dropTime > CurrentGameInstance.itemLootLockDuration ||
                isPickedUp)
                return true;
            return false;
        }

        public void MarkAsPickedUp()
        {
            isPickedUp = true;
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
                // Random drop position around character
                // Raycast to find hit floor
                Vector3? aboveHitPoint = null;
                Vector3? underHitPoint = null;
                int raycastLayerMask = gameInstance.GetItemDropGroundDetectionLayerMask();
                RaycastHit tempHit;
                if (Physics.Raycast(dropPosition, Vector3.up, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                    aboveHitPoint = tempHit.point;
                if (Physics.Raycast(dropPosition, Vector3.down, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                    underHitPoint = tempHit.point;
                // Set drop position to nearest hit point
                if (aboveHitPoint.HasValue && underHitPoint.HasValue)
                {
                    if (Vector3.Distance(dropPosition, aboveHitPoint.Value) < Vector3.Distance(dropPosition, underHitPoint.Value))
                        dropPosition = aboveHitPoint.Value;
                    else
                        dropPosition = underHitPoint.Value;
                }
                else if (aboveHitPoint.HasValue)
                    dropPosition = aboveHitPoint.Value;
                else if (underHitPoint.HasValue)
                    dropPosition = underHitPoint.Value;
            }
            GameObject spawnObj = Instantiate(gameInstance.itemDropEntityPrefab.gameObject, dropPosition, dropRotation);
            ItemDropEntity itemDropEntity = spawnObj.GetComponent<ItemDropEntity>();
            itemDropEntity.dropData = dropData;
            itemDropEntity.looters = new HashSet<uint>(looters);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemDropEntity;
        }
    }
}
