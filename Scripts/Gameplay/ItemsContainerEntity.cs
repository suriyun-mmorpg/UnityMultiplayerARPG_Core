using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemsContainerEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;
        public const int FIND_GROUND_RAYCAST_HIT_SIZE = 10;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];

        protected SyncListCharacterItem items = new SyncListCharacterItem();
        public SyncListCharacterItem Items
        {
            get { return items; }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            items.forOwnerOnly = false;
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
            GameObject spawnObj = Instantiate(prefab.gameObject, dropPosition, dropRotation);
            ItemsContainerEntity itemsContainerEntity = spawnObj.GetComponent<ItemsContainerEntity>();
            itemsContainerEntity.Items.AddRange(dropItems);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemsContainerEntity;
        }
    }
}
