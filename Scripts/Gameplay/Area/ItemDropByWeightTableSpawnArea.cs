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
        public RewardGivenType rewardGivenType = RewardGivenType.KillMonster;

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
            if (GetRandomPosition(out Vector3 dropPosition))
            {
                Quaternion dropRotation = Quaternion.identity;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                {
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                }

                BaseItem itemData = item.GetItem();
                if (GameInstance.Singleton.IsExpDropRepresentItem(itemData))
                {
                    ExpDropEntity prefab = await CurrentGameInstance.GetLoadedExpDropEntityPrefab();
                    if (prefab != null)
                    {
                        ExpDropEntity newEntity = BaseRewardDropEntity.Drop(prefab, dropPosition, dropRotation, 1f, rewardGivenType, 1, 1, item.amount, System.Array.Empty<string>(), -1);
                        newEntity.onNetworkDestroy -= NewEntity_onNetworkDestroy;
                        newEntity.onNetworkDestroy += NewEntity_onNetworkDestroy;
                    }
                }
                else if (GameInstance.Singleton.IsGoldDropRepresentItem(itemData))
                {
                    GoldDropEntity prefab = await CurrentGameInstance.GetLoadedGoldDropEntityPrefab();
                    if (prefab != null)
                    {
                        GoldDropEntity newEntity = BaseRewardDropEntity.Drop(prefab, dropPosition, dropRotation, 1f, rewardGivenType, 1, 1, item.amount, System.Array.Empty<string>(), -1);
                        newEntity.onNetworkDestroy -= NewEntity_onNetworkDestroy;
                        newEntity.onNetworkDestroy += NewEntity_onNetworkDestroy;
                    }
                }
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
                else if (GameInstance.Singleton.IsCurrencyDropRepresentItem(itemData, out Currency currency))
                {
                    CurrencyDropEntity prefab = await CurrentGameInstance.GetLoadedCurrencyDropEntityPrefab();
                    if (prefab != null)
                    {
                        CurrencyDropEntity newEntity = BaseRewardDropEntity.Drop(prefab, dropPosition, dropRotation, 1f, rewardGivenType, 1, 1, item.amount, System.Array.Empty<string>(), -1);
                        newEntity.Currency = currency;
                        newEntity.onNetworkDestroy -= NewEntity_onNetworkDestroy;
                        newEntity.onNetworkDestroy += NewEntity_onNetworkDestroy;
                    }
                }
#endif
                else
                {
                    ItemDropEntity prefab = await CurrentGameInstance.GetLoadedItemDropEntityPrefab();
                    if (prefab != null)
                    {
                        ItemDropEntity newEntity = ItemDropEntity.Drop(prefab, dropPosition, dropRotation, rewardGivenType, item, System.Array.Empty<string>(), -1);
                        newEntity.onNetworkDestroy -= NewEntity_onNetworkDestroy;
                        newEntity.onNetworkDestroy += NewEntity_onNetworkDestroy;
                    }
                }
            }
            else
            {
                // Unable to spawn?, add to pending list
                AddPending(item);
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
