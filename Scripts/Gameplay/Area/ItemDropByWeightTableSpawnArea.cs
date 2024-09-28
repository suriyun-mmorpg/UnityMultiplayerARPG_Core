using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropByWeightTableSpawnArea : GameSpawnArea
    {
        public ItemRandomByWeightTable weightTable;
        public float respawnPickedupDelay = 10f;
        public float droppedItemDestroyDelay = 300f;

        protected float _respawnPendingEntitiesTimer = 0f;
        protected readonly List<CharacterItem> _pending = new List<CharacterItem>();

        protected virtual void LateUpdate()
        {
            if (_pending.Count > 0)
            {
                _respawnPendingEntitiesTimer += Time.deltaTime;
                if (_respawnPendingEntitiesTimer >= respawnPendingEntitiesDelay)
                {
                    _respawnPendingEntitiesTimer = 0f;
                    foreach (CharacterItem pendingEntry in _pending)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Logging.LogWarning(ToString(), $"Spawning pending items, Item: {pendingEntry.dataId}, Amount: {pendingEntry.amount}.");
#endif
                        Spawn(pendingEntry, 0);
                    }
                    _pending.Clear();
                }
            }
        }

        public override void SpawnAll()
        {
            for (int i = 0; i < amount; ++i)
            {
                if (weightTable == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Logging.LogWarning(ToString(), $"Unable to spawn item, table is empty.");
#endif
                    continue;
                }
                weightTable.RandomItem((item, level, amount) =>
                {
                    Spawn(CharacterItem.Create(item, level, amount), 0f);
                });
            }
        }

        public virtual async void Spawn(CharacterItem item, float delay)
        {
            if (item.IsEmptySlot())
            {
                return;
            }
            await UniTask.Delay(Mathf.RoundToInt(delay * 1000));
            ItemDropEntity newEntity = null;
            if (GetRandomPosition(out Vector3 dropPosition))
            {
                Quaternion dropRotation = Quaternion.identity;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                {
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                }
                ItemDropEntity loadedPrefab = await CurrentGameInstance.GetLoadedItemDropEntityPrefab();
                if (loadedPrefab != null)
                    newEntity = ItemDropEntity.Drop(loadedPrefab, dropPosition, dropRotation, RewardGivenType.None, item, System.Array.Empty<string>(), -1);
            }
            if (newEntity == null)
            {
                AddPending(item);
            }
            else
            {
                // TODO: fix this later
                //if (droppedItemDestroyDelay >= 0)
                //    newEntity.NetworkDestroy(droppedItemDestroyDelay);
                newEntity.onNetworkDestroy -= NewEntity_onNetworkDestroy;
                newEntity.onNetworkDestroy += NewEntity_onNetworkDestroy;
            }
        }

        protected virtual void AddPending(CharacterItem item)
        {
            _pending.Add(item);
        }

        protected virtual void NewEntity_onNetworkDestroy(byte reasons)
        {
            weightTable.RandomItem((item, level, amount) =>
            {
                Spawn(CharacterItem.Create(item, level, amount), respawnPickedupDelay);
            });
        }
    }
}
