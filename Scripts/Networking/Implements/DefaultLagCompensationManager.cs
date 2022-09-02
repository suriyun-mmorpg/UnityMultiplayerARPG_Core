using LiteNetLibManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultLagCompensationManager : MonoBehaviour, ILagCompensationManager
    {
        [SerializeField]
        private float snapShotInterval = 0.06f;
        public float SnapShotInterval { get { return snapShotInterval; } }

        [SerializeField]
        private int maxHistorySize = 16;
        public int MaxHistorySize { get { return maxHistorySize; } }

        private Dictionary<uint, DamageableEntity> damageableEntities = new Dictionary<uint, DamageableEntity>();
        private List<DamageableEntity> simulatedDamageableEntities = new List<DamageableEntity>();
        private float lastHistoryStoreTime;

        public bool SimulateHitBoxes(long connectionId, long targetTime, Action action)
        {
            if (action == null || !BeginSimlateHitBoxes(connectionId, targetTime))
                return false;
            action.Invoke();
            EndSimulateHitBoxes();
            return true;
        }

        public bool SimulateHitBoxesByHalfRtt(long connectionId, Action action)
        {
            if (action == null || !BeginSimlateHitBoxesByHalfRtt(connectionId))
                return false;
            action.Invoke();
            EndSimulateHitBoxes();
            return true;
        }

        public bool BeginSimlateHitBoxes(long connectionId, long targetTime)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer || !BaseGameNetworkManager.Singleton.ContainsPlayer(connectionId))
                return false;
            LiteNetLibPlayer player = BaseGameNetworkManager.Singleton.GetPlayer(connectionId);
            return InternalBeginSimlateHitBoxes(player, targetTime);
        }

        public bool BeginSimlateHitBoxesByHalfRtt(long connectionId)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer || !BaseGameNetworkManager.Singleton.ContainsPlayer(connectionId))
                return false;
            LiteNetLibPlayer player = BaseGameNetworkManager.Singleton.GetPlayer(connectionId);
            long targetTime = BaseGameNetworkManager.Singleton.ServerTimestamp - (player.Rtt / 2);
            return InternalBeginSimlateHitBoxes(player, targetTime);
        }

        private bool InternalBeginSimlateHitBoxes(LiteNetLibPlayer player, long targetTime)
        {
            foreach (uint subscribingObjectId in player.GetSubscribingObjectIds())
            {
                if (damageableEntities.ContainsKey(subscribingObjectId))
                {
                    damageableEntities[subscribingObjectId].RewindHitBoxes(targetTime);
                    if (!simulatedDamageableEntities.Contains(damageableEntities[subscribingObjectId]))
                        simulatedDamageableEntities.Add(damageableEntities[subscribingObjectId]);
                }
            }
            return true;
        }

        public void EndSimulateHitBoxes()
        {
            for (int i = 0; i < simulatedDamageableEntities.Count; ++i)
            {
                if (simulatedDamageableEntities[i] != null)
                    simulatedDamageableEntities[i].RestoreHitBoxes();
            }
            simulatedDamageableEntities.Clear();
        }

        public void AddDamageableEntity(DamageableEntity entity)
        {
            damageableEntities[entity.ObjectId] = entity;
        }

        public void RemoveDamageableEntity(DamageableEntity entity)
        {
            damageableEntities.Remove(entity.ObjectId);
        }

        private void LateUpdate()
        {
            float currentTime = Time.unscaledTime;
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;
            if (currentTime - lastHistoryStoreTime < SnapShotInterval)
                return;
            lastHistoryStoreTime = currentTime;
            long serverTimestamp = BaseGameNetworkManager.Singleton.ServerTimestamp;
            foreach (DamageableEntity entity in damageableEntities.Values)
            {
                if (entity.Identity.CountSubscribers() > 0)
                    entity.AddHitBoxesTransformHistory(serverTimestamp);
            }
        }
    }
}
