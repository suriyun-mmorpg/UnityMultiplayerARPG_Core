using System.Collections;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropSpawnArea : GameSpawnArea<ItemDropEntity>
    {
        public override void RegisterAssets()
        {
            base.RegisterAssets();
            GameInstance.AddItemDropEntities(asset);
        }

        protected override void SpawnInternal()
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(asset.gameObject, spawnPosition, spawnRotation);
            ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
            entity.gameObject.SetActive(false);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.SetSpawnArea(this, spawnPosition);
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetItemDropGroundDetectionLayerMask(); }
        }
    }
}
