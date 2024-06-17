using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropByWeightTableSpawnArea : GameSpawnArea
    {
        public ItemRandomByWeightTable weightTable;
        public int amount = 5;
        public float respawnPickedupDelay = 10f;
        public float respawnPendingEntitiesDelay = 5f;

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
                weightTable.RandomItem((item, amount) =>
                {
                    Spawn(CharacterItem.Create(item, 1, amount), 0f);
                });
            }
        }

        public virtual Coroutine Spawn(CharacterItem item, float delay)
        {
            return StartCoroutine(SpawnRoutine(item, delay));
        }

        IEnumerator SpawnRoutine(CharacterItem item, float delay)
        {
            if (item.IsEmptySlot())
                yield break;
            yield return new WaitForSecondsRealtime(delay);
            ItemDropEntity newEntity = ItemDropEntity.Drop(null, RewardGivenType.None, item, new string[0]);
            if (newEntity == null)
                AddPending(item);
            else
                newEntity.OnPickedUp.AddListener(OnPickedUp);
        }

        protected virtual void AddPending(CharacterItem item)
        {
            _pending.Add(item);
        }

        protected virtual void OnPickedUp()
        {
            weightTable.RandomItem((item, amount) =>
            {
                Spawn(CharacterItem.Create(item, 1, amount), respawnPickedupDelay);
            });
        }
    }
}
