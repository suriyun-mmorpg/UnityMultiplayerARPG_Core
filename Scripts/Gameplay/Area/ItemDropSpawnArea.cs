using System.Collections;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropSpawnArea : GameSpawnArea<ItemDropEntity>
    {
        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddItemDropEntities(prefab);
        }

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, short level)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(prefab.gameObject, spawnPosition, spawnRotation);
            ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
            entity.gameObject.SetActive(false);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.SetSpawnArea(this, prefab, level, spawnPosition);
            return entity;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetItemDropGroundDetectionLayerMask(); }
        }
    }
}
