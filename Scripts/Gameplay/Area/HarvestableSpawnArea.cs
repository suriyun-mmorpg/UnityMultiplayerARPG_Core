using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class HarvestableSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        public HarvestableEntity harvestableEntity;
        public short amount = 1;

        private int pending;

        private GameInstance gameInstance { get { return GameInstance.Singleton; } }

        public void RegisterAssets()
        {
            if (harvestableEntity != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(harvestableEntity.Identity);
        }

        public void SpawnAll()
        {
            if (harvestableEntity != null)
            {
                for (int i = 0; i < amount; ++i)
                {
                    Spawn(0);
                }
            }
        }

        public void Spawn(float delay)
        {
            StartCoroutine(SpawnRoutine(delay));
        }

        IEnumerator SpawnRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            float colliderDetectionRadius = harvestableEntity.colliderDetectionRadius;
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            bool overlapEntities = false;
            Collider[] overlaps = Physics.OverlapSphere(spawnPosition, colliderDetectionRadius);
            foreach (Collider overlap in overlaps)
            {
                if (overlap.gameObject.layer == gameInstance.characterLayer ||
                    overlap.gameObject.layer == gameInstance.itemDropLayer ||
                    overlap.gameObject.layer == gameInstance.buildingLayer ||
                    overlap.gameObject.layer == gameInstance.harvestableLayer)
                {
                    overlapEntities = true;
                    break;
                }
            }
            if (!overlapEntities)
            {
                LiteNetLibIdentity identity = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(harvestableEntity.Identity, spawnPosition, spawnRotation);
                if (identity != null)
                {
                    HarvestableEntity entity = identity.GetComponent<HarvestableEntity>();
                    entity.SetSpawnArea(this, spawnPosition);
                }
            }
            else
            {
                ++pending;
                Debug.LogWarning("[HarvestableSpawnArea(" + name + ")] Cannot spawn harvestable it is collided to another entities, pending harvestable amount " + pending);
            }
        }

        public override int GroundLayerMask
        {
            get { return gameInstance.GetHarvestableSpawnGroundDetectionLayerMask(); }
        }
    }
}
