using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class ItemsContainerEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;

        [Category(5, "Items Container Settings")]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onItemDropDestroy` event before it's going to be destroyed from the game.")]
        [SerializeField]
        protected float destroyDelay = 0f;

        [Category("Events")]
        [SerializeField]
        protected UnityEvent onItemsContainerDestroy;

        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[1000];

        protected SyncListCharacterItem items = new SyncListCharacterItem();
        public SyncListCharacterItem Items
        {
            get { return items; }
        }

        // Private variables
        protected bool isDestroyed;

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            items.forOwnerOnly = false;
        }

        [AllRpc]
        protected virtual void AllOnItemsContainerDestroy()
        {
            if (onItemsContainerDestroy != null)
                onItemsContainerDestroy.Invoke();
        }

        public void CallAllOnItemDropDestroy()
        {
            RPC(AllOnItemsContainerDestroy);
        }

        public void PickedUp()
        {
            if (!IsServer)
                return;
            if (Items.Count > 0)
                return;
            if (isDestroyed)
                return;
            // Mark as destroyed
            isDestroyed = true;
            // Tell clients that the item drop destroy to play animation at client
            CallAllOnItemDropDestroy();
            // Destroy this entity
            NetworkDestroy(destroyDelay);
        }

        public static ItemsContainerEntity DropItems(ItemsContainerEntity prefab, BaseGameEntity dropper, IEnumerable<CharacterItem> dropItems)
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
            return DropItems(prefab, dropPosition, dropRotation, dropItems);
        }

        public static ItemsContainerEntity DropItems(ItemsContainerEntity prefab, Vector3 dropPosition, Quaternion dropRotation, IEnumerable<CharacterItem> dropItems)
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
            ItemsContainerEntity itemsContainerEntity = spawnObj.GetComponent<ItemsContainerEntity>();
            itemsContainerEntity.Items.AddRange(dropItems);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemsContainerEntity;
        }
    }
}
