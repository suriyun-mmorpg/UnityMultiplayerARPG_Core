using Cysharp.Text;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public const int MAX_VALIDATE_QUEUE_SIZE = 16;

        public float hitValidationBuffer = 2f;
        protected GameObject _hitBoxObject;
        protected Transform _hitBoxTransform;

        protected static readonly Dictionary<string, HitValidateData> s_validatingHits = new Dictionary<string, HitValidateData>();
        protected static readonly Dictionary<uint, Queue<string>> s_removingQueues = new Dictionary<uint, Queue<string>>();
        protected static readonly List<HitRegisterData> s_registeringHits = new List<HitRegisterData>();

        void Start()
        {
            _hitBoxObject = new GameObject("_testHitBox");
            _hitBoxTransform = _hitBoxObject.transform;
            _hitBoxTransform.parent = transform;
        }

        void OnDestroy()
        {
            if (_hitBoxObject != null)
                Destroy(_hitBoxObject);
        }

        private void AppendValidatingData(uint objectId, string id, HitValidateData hitValidateData)
        {
            if (!s_removingQueues.ContainsKey(objectId))
                s_removingQueues[objectId] = new Queue<string>();
            while (s_removingQueues[objectId].Count >= MAX_VALIDATE_QUEUE_SIZE)
            {
                s_validatingHits.Remove(s_removingQueues[objectId].Dequeue());
            }
            s_removingQueues[objectId].Enqueue(id);
            s_validatingHits[id] = hitValidateData;
        }

        public HitValidateData GetHitValidateData(BaseGameEntity attacker, int simulateSeed)
        {
            string id = MakeValidateId(attacker.ObjectId, simulateSeed);
            if (s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
                return hitValidateData;
            return null;
        }    

        public void PrepareHitRegValidation(BaseGameEntity attacker, int simulateSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, bool isLeftHand, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            string id = MakeValidateId(attacker.ObjectId, simulateSeed);
            bool appending = false;
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
            {
                hitValidateData = new HitValidateData();
                appending = true;
            }

            hitValidateData.Attacker = attacker;
            hitValidateData.TriggerDurations = triggerDurations;
            hitValidateData.FireSpread = fireSpread;
            hitValidateData.DamageInfo = damageInfo;
            hitValidateData.BaseDamageAmounts = damageAmounts;
            hitValidateData.IsLeftHand = isLeftHand;
            hitValidateData.Weapon = weapon;
            hitValidateData.Skill = skill;
            hitValidateData.SkillLevel = skillLevel;
            if (!appending)
            {
                // Just update the data
                s_validatingHits[id] = hitValidateData;
            }
            else
            {
                // Addpend new data
                AppendValidatingData(attacker.ObjectId, id, hitValidateData);
            }
        }

        public void ConfirmHitRegValidation(BaseGameEntity attacker, int simulateSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts)
        {
            string id = MakeValidateId(attacker.ObjectId, simulateSeed);
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
            {
                Logging.LogError($"Cannot confirm validation data, there is no prepared validation data");
                return;
            }

            // Make sure it won't increase damage to the wrong collction
            Dictionary<DamageElement, MinMaxFloat> confirmedDamageAmounts = hitValidateData.BaseDamageAmounts == null ? new Dictionary<DamageElement, MinMaxFloat>() : new Dictionary<DamageElement, MinMaxFloat>(hitValidateData.BaseDamageAmounts);
            // Increase damage amounts
            if (increaseDamageAmounts != null && increaseDamageAmounts.Count > 0)
                confirmedDamageAmounts = GameDataHelpers.CombineDamages(confirmedDamageAmounts, increaseDamageAmounts);
            hitValidateData.ConfirmedDamageAmounts[triggerIndex] = confirmedDamageAmounts;

            if (hitValidateData.Pendings.TryGetValue(triggerIndex, out List<HitRegisterData> hits))
            {
                for (int i = 0; i < hits.Count; ++i)
                {
                    PerformValidation(attacker, hits[i]);
                }
                hitValidateData.Pendings.Remove(triggerIndex);
            }
        }

        public void PrepareHitRegData(HitRegisterData hitRegisterData)
        {
            s_registeringHits.Add(hitRegisterData);
        }

        public bool PerformValidation(BaseGameEntity attacker, HitRegisterData hitData)
        {
            if (attacker == null)
                return false;

            string id = MakeValidateId(attacker.ObjectId, hitData.SimulateSeed);
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
            {
                // No validating data
                hitValidateData = new HitValidateData();
                if (!hitValidateData.Pendings.ContainsKey(hitData.TriggerIndex))
                    hitValidateData.Pendings[hitData.TriggerIndex] = new List<HitRegisterData>();
                hitValidateData.Pendings[hitData.TriggerIndex].Add(hitData);
                AppendValidatingData(attacker.ObjectId, id, hitValidateData);
                return false;
            }

            if (!hitValidateData.ConfirmedDamageAmounts.ContainsKey(hitData.TriggerIndex))
            {
                // No confirmed validating data
                if (!hitValidateData.Pendings.ContainsKey(hitData.TriggerIndex))
                    hitValidateData.Pendings[hitData.TriggerIndex] = new List<HitRegisterData>();
                hitValidateData.Pendings[hitData.TriggerIndex].Add(hitData);
                return false;
            }

            uint objectId = hitData.HitObjectId;
            int hitBoxIndex = hitData.HitBoxIndex;
            if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(objectId, out DamageableEntity damageableEntity) ||
                hitBoxIndex < 0 || hitBoxIndex >= damageableEntity.HitBoxes.Length)
            {
                // Can't find target or invalid hitbox
                return false;
            }

            string hitObjectId = MakeHitObjectId(hitData.TriggerIndex, hitData.SpreadIndex, hitData.HitObjectId);
            if (hitValidateData.HitObjects.Contains(hitObjectId))
            {
                // Already hit
                return false;
            }

            DamageableHitBox hitBox = damageableEntity.HitBoxes[hitBoxIndex];
            if (!hitValidateData.DamageInfo.IsHitValid(hitValidateData, hitData, hitBox))
            {
                // Not valid
                return false;
            }

            if (!IsHit(attacker, hitValidateData, hitData, hitBox))
            {
                // Not hit
                return false;
            }

            string hitId = MakeHitRegId(hitData.TriggerIndex, hitData.SpreadIndex);
            if (!hitValidateData.HitsCount.TryGetValue(hitId, out int hitCount))
            {
                // Set hit count to 0, if it is not in collection
                hitCount = 0;
            }
            hitValidateData.HitsCount[hitId] = ++hitCount;

            // Yes, it is hit
            hitBox.ReceiveDamage(attacker.EntityTransform.position, attacker.GetInfo(), hitValidateData.BaseDamageAmounts, hitValidateData.Weapon, hitValidateData.Skill, hitValidateData.SkillLevel, hitData.SimulateSeed);
            hitValidateData.HitObjects.Add(hitObjectId);
            return true;
        }

        private bool IsHit(BaseGameEntity attacker, HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox)
        {
            long timestamp = BaseGameNetworkManager.Singleton.Timestamp;
            long halfRtt = attacker.Player != null ? (attacker.Player.Rtt / 2) : 0;
            long targetTime = timestamp - halfRtt;
            DamageableHitBox.TransformHistory transformHistory = hitBox.GetTransformHistory(timestamp, targetTime);
            _hitBoxTransform.position = transformHistory.Bounds.center;
            _hitBoxTransform.rotation = transformHistory.Rotation;
            Vector3 alignedHitPoint = _hitBoxTransform.InverseTransformPoint(hitData.HitOrigin);
            float maxExtents = Mathf.Max(transformHistory.Bounds.extents.x, transformHistory.Bounds.extents.y, transformHistory.Bounds.extents.z);
            return Vector3.Distance(Vector3.zero, alignedHitPoint) <= maxExtents + hitValidationBuffer;
        }

        private static string MakeValidateId(uint attackerId, int simulateSeed)
        {
            return ZString.Concat(attackerId, "_", simulateSeed);
        }

        private static string MakeHitRegId(byte triggerIndex, byte spreadIndex)
        {
            return ZString.Concat(triggerIndex, "_", spreadIndex);
        }

        private static string MakeHitObjectId(byte triggerIndex, byte spreadIndex, uint objectId)
        {
            return ZString.Concat(triggerIndex, "_", spreadIndex, "_", objectId);
        }

        public void ClearData()
        {
            s_validatingHits.Clear();
            s_removingQueues.Clear();
            s_registeringHits.Clear();
        }
    }
}
