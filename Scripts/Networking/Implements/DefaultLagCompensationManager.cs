using LiteNetLibManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultLagCompensationManager : MonoBehaviour, ILagCompensationManager
    {
        private readonly Dictionary<uint, DamageableHitBox[]> HitBoxes = new Dictionary<uint, DamageableHitBox[]>();
        public float snapShotInterval = 0.06f;
        public int maxHistorySize = 16;
        public int MaxHistorySize { get { return maxHistorySize; } }

        private readonly List<DamageableHitBox> hitBoxes = new List<DamageableHitBox>();
        private float snapShotCountDown = 0f;

        public bool AddHitBoxes(uint objectId, DamageableHitBox[] hitBoxes)
        {
            if (HitBoxes.ContainsKey(objectId))
                return false;
            HitBoxes.Add(objectId, hitBoxes);
            return true;
        }

        public bool RemoveHitBoxes(uint objectId)
        {
            return HitBoxes.Remove(objectId);
        }

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
            hitBoxes.Clear();
            foreach (uint subscribingObjectId in player.GetSubscribingObjectIds())
            {
                if (HitBoxes.ContainsKey(subscribingObjectId))
                    hitBoxes.AddRange(HitBoxes[subscribingObjectId]);
            }
            long time = BaseGameNetworkManager.Singleton.ServerTimestamp;
            for (int i = 0; i < hitBoxes.Count; ++i)
            {
                if (hitBoxes[i] != null)
                    hitBoxes[i].Rewind(time, targetTime);
            }
            return true;
        }

        public void EndSimulateHitBoxes()
        {
            for (int i = 0; i < hitBoxes.Count; ++i)
            {
                if (hitBoxes[i] != null)
                    hitBoxes[i].Restore();
            }
        }

        private void LateUpdate()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;
            snapShotCountDown -= Time.unscaledDeltaTime;
            if (snapShotCountDown > 0)
                return;
            snapShotCountDown = snapShotInterval;
            long time = BaseGameNetworkManager.Singleton.ServerTimestamp;
            foreach (DamageableHitBox[] hitBoxesArray in HitBoxes.Values)
            {
                foreach (DamageableHitBox hitBox in hitBoxesArray)
                {
                    hitBox.AddTransformHistory(time);
                }
            }
        }
    }
}
