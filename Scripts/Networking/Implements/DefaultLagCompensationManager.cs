using LiteNetLibManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultLagCompensationManager : MonoBehaviour, ILagCompensationManager
    {
        private readonly Dictionary<uint, DamageableHitBox[]> HitBoxes = new Dictionary<uint, DamageableHitBox[]>();

        public int maxHistorySize = 16;
        public int MaxHistorySize { get { return maxHistorySize; } }

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

        public bool SimulateHitBoxes(long connectionId, Action action)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer || !BaseGameNetworkManager.Singleton.ContainsPlayer(connectionId) || action == null)
                return false;
            LiteNetLibPlayer player = BaseGameNetworkManager.Singleton.GetPlayer(connectionId);
            long rtt = player.Rtt;
            if (rtt <= 0)
                return false;
            List<DamageableHitBox> hitBoxes = new List<DamageableHitBox>();
            foreach (uint subscribingObjectId in player.GetSubscribingObjectIds())
            {
                if (!HitBoxes.ContainsKey(subscribingObjectId))
                    continue;
                hitBoxes.AddRange(HitBoxes[subscribingObjectId]);
            }
            for (int i = 0; i < hitBoxes.Count; ++i)
            {
                hitBoxes[i].Reverse(rtt);
            }
            action.Invoke();
            for (int i = 0; i < hitBoxes.Count; ++i)
            {
                hitBoxes[i].ResetTransform();
            }
            return true;
        }

        private void Update()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;
            foreach (DamageableHitBox[] hitBoxesArray in HitBoxes.Values)
            {
                foreach (DamageableHitBox hitBox in hitBoxesArray)
                {
                    hitBox.AddTransformHistory();
                }
            }
        }
    }
}
