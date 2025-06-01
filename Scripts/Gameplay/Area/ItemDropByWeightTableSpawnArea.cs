using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class ItemDropByWeightTableSpawnArea : GameSpawnArea
    {
        [Header("Drop settings")]
        public ItemRandomByWeightTable weightTable;
        [FormerlySerializedAs("respawnPickedupDelay")]
        public float respawnPickedupDelayMin = 10f;
        public float respawnPickedupDelayMax = 10f;
        public float droppedItemDestroyDelay = 300f;
        public RewardGivenType rewardGivenType = RewardGivenType.KillMonster;

        protected float _respawnPendingEntitiesTimer = 0f;
        protected List<CharacterItem> _pending = new List<CharacterItem>();
        protected CancellationTokenSource _cancellationSource;

        protected override void Awake()
        {
            base.Awake();
            _cancellationSource = new CancellationTokenSource();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            weightTable = null;
            _cancellationSource?.Cancel();
            _cancellationSource = null;
            _pending?.Clear();
            _pending = null;
        }

        protected override void LateUpdate()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;

            base.LateUpdate();

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
            int amount = GetRandomedSpawnAmount();
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
            try
            {
                await UniTask.Delay(Mathf.RoundToInt(delay * 1000), cancellationToken: _cancellationSource.Token);
            }
            catch
            {
                return;
            }
            if (!AbleToSpawn())
            {
                return;
            }
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
                        _subscribeHandler.AddEntity(newEntity);
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
                        _subscribeHandler.AddEntity(newEntity);
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
                        _subscribeHandler.AddEntity(newEntity);
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
                        _subscribeHandler.AddEntity(newEntity);
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
            if (!AbleToSpawn())
            {
                return;
            }
            weightTable.RandomItem((item, level, amount) =>
            {
                Spawn(
                    CharacterItem.Create(item, level, amount),
                    Random.Range(respawnPickedupDelayMin, respawnPickedupDelayMax));
            });
        }
    }
}
