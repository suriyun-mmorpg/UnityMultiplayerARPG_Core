using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public const int DELAY_TO_REMOVE_VALIDATING_DATA = 10 * 1000;

        public float hitValidationBuffer = 2f;
        protected GameObject _hitBoxObject;
        protected Transform _hitBoxTransform;

        protected static readonly Dictionary<string, HitValidateData> s_validatingHits = new Dictionary<string, HitValidateData>();
        protected static readonly List<HitRegisterData> s_registeringHits = new List<HitRegisterData>();
        protected static readonly Dictionary<string, CancellationTokenSource> s_removeValidatingCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        protected static readonly Dictionary<string, CancellationTokenSource> s_removePreparingCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

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

        protected static void CreateValidatingData(string id)
        {
            if (!s_validatingHits.ContainsKey(id))
            {
                s_validatingHits[id] = new HitValidateData();
                DelayRemoveValidatingData(id);
            }
        }

        protected static async void DelayRemoveValidatingData(string id)
        {
            if (s_removeValidatingCancellationTokenSources.ContainsKey(id))
                s_removeValidatingCancellationTokenSources[id].Cancel();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            try
            {
                s_removeValidatingCancellationTokenSources[id] = cancellationTokenSource;
                // Delay x seconds before remove the validating data
                await UniTask.Delay(DELAY_TO_REMOVE_VALIDATING_DATA, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
                s_validatingHits.Remove(id);
                if (s_removeValidatingCancellationTokenSources.ContainsKey(id))
                    s_removeValidatingCancellationTokenSources.Remove(id);
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
            }
            catch (System.Exception ex)
            {
                // Other errors
                Debug.LogException(ex);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        public void PrepareHitRegValidation(BaseGameEntity attacker, int simulateSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            // Only server can prepare hit registration
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null)
                return;

            if (triggerDurations == null || triggerDurations.Length <= 0)
                return;

            string id = MakeValidateId(attacker.ObjectId, simulateSeed);
            if (s_validatingHits.ContainsKey(id))
            {
                Logging.LogError($"Cannot prepare validation data, there is already has prepared validation data");
                return;
            }

            s_validatingHits[id] = new HitValidateData()
            {
                Attacker = attacker,
                TriggerDurations = triggerDurations,
                FireSpread = fireSpread,
                DamageInfo = damageInfo,
                BaseDamageAmounts = damageAmounts,
                Weapon = weapon,
                Skill = skill,
                SkillLevel = skillLevel,
            };
        }

        public void ConfirmHitRegValidation(BaseGameEntity attacker, int simulateSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts)
        {
            // Only server can confirm hit registration
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null)
                return;

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

        public void Register(BaseGameEntity attacker, HitRegisterMessage message)
        {
            // Only server can perform hit registration
            if (attacker == null || !BaseGameNetworkManager.Singleton.IsServer)
                return;

            for (int i = 0; i < message.Hits.Count; ++i)
            {
                PerformValidation(attacker, message.Hits[i]);
            }
        }

        private bool PerformValidation(BaseGameEntity attacker, HitRegisterData hitData)
        {
            if (attacker == null)
                return false;

            string id = MakeValidateId(attacker.ObjectId, hitData.SimulateSeed);
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
            {
                // No validating data
                Logging.LogError($"Cannot perform validation, there is no prepared validation data");
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
            Vector3 alignedHitPoint = _hitBoxTransform.InverseTransformPoint(hitData.Destination);
            float maxExtents = Mathf.Max(transformHistory.Bounds.extents.x, transformHistory.Bounds.extents.y, transformHistory.Bounds.extents.z);
            bool isHit = Vector3.Distance(Vector3.zero, alignedHitPoint) <= maxExtents + hitValidationBuffer;
            if (Vector3.Distance(hitData.Origin, hitData.Destination) > maxExtents + hitValidateData.DamageInfo.GetDistance())
            {
                // Too far, it should not hit
                return false;
            }
            return isHit;
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

        public int CountHitRegDataList()
        {
            return s_registeringHits.Count;
        }

        public List<HitRegisterData> GetHitRegDataList()
        {
            return s_registeringHits;
        }

        public void ClearHitRegData()
        {
            s_registeringHits.Clear();
        }

        public void ClearData()
        {
            s_validatingHits.Clear();
            s_registeringHits.Clear();
        }
    }
}
