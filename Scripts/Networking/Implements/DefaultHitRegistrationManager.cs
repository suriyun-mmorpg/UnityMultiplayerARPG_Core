using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public float hitValidationBuffer = 2f;
        protected GameObject _hitBoxObject;
        protected Transform _hitBoxTransform;

        protected static readonly Dictionary<string, HitValidateData> s_validatingHits = new Dictionary<string, HitValidateData>();
        protected static readonly Dictionary<int, List<HitData>> s_registeringHits = new Dictionary<int, List<HitData>>();
        protected static readonly Dictionary<string, CancellationTokenSource> s_cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

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

        protected static async void DelayClearData(string id)
        {
            if (s_cancellationTokenSources.ContainsKey(id))
                s_cancellationTokenSources[id].Cancel();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            try
            {
                s_cancellationTokenSources[id] = cancellationTokenSource;
                await UniTask.Delay(30 * 1000, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
                s_validatingHits.Remove(id);
                if (s_cancellationTokenSources.ContainsKey(id))
                    s_cancellationTokenSources.Remove(id);
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

        public bool WillProceedHitRegByClient<T>(T damageEntity, EntityInfo attackerInfo) where T : BaseDamageEntity
        {
            if (damageEntity == null || !attackerInfo.TryGetEntity(out BaseGameEntity attacker))
                return false;
            if (attacker.IsOwnerHost || attacker.IsOwnedByServer)
                return false;
            return damageEntity is MissileDamageEntity;
        }

        public void PrepareHitRegValidatation(BaseGameEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            // Only server can prepare hit registration
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null)
                return;

            // Don't validate some damage types
            if (damageInfo.damageType == DamageType.Throwable)
                return;

            // Validating or not?
            if (damageInfo.damageType == DamageType.Custom && (damageInfo.customDamageInfo == null || !damageInfo.customDamageInfo.ValidatedByHitRegistrationManager()))
                return;

            if (triggerDurations == null || triggerDurations.Length <= 0)
                return;

            List<HitOriginData>[] origins = new List<HitOriginData>[triggerDurations.Length];
            for (int i = 0; i < triggerDurations.Length; ++i)
            {
                origins[i] = new List<HitOriginData>(fireSpread + 1);
            }

            string id = MakeValidateId(attacker.ObjectId, randomSeed);
            s_validatingHits[id] = new HitValidateData()
            {
                Attacker = attacker,
                TriggerDurations = triggerDurations,
                FireSpread = fireSpread,
                DamageInfo = damageInfo,
                DamageAmounts = damageAmounts,
                Weapon = weapon,
                Skill = skill,
                SkillLevel = skillLevel,
                Origins = origins,
            };
            DelayClearData(id);
        }

        public void IncreasePreparedDamageAmounts(BaseGameEntity attacker, int randomSeed, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts)
        {
            // Only server can modify damage amounts
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null || increaseDamageAmounts == null || increaseDamageAmounts.Count == 0)
                return;

            string id = MakeValidateId(attacker.ObjectId, randomSeed);
            if (!s_validatingHits.ContainsKey(id))
                return;

            s_validatingHits[id].DamageAmounts = GameDataHelpers.CombineDamages(s_validatingHits[id].DamageAmounts, increaseDamageAmounts);
        }

        public void PrepareHitRegOrigin(BaseGameEntity attacker, int randomSeed, byte triggerIndex, byte spreadIndex, Vector3 position, Vector3 direction)
        {
            string id = MakeValidateId(attacker.ObjectId, randomSeed);
            if (!s_validatingHits.ContainsKey(id))
                return;

            if (triggerIndex < 0 || triggerIndex >= s_validatingHits[id].Origins.Length)
                return;

            if (s_validatingHits[id].Origins[triggerIndex].Count >= s_validatingHits[id].FireSpread + 1)
                return;

            s_validatingHits[id].Origins[triggerIndex].Add(new HitOriginData()
            {
                TriggerIndex = triggerIndex,
                Position = position,
                Direction = direction,
            });
        }

        public void PrepareToRegister(int randomSeed, byte triggerIndex, byte spreadIndex, uint objectId, byte hitBoxIndex, Vector3 hitPoint)
        {
            if (!s_registeringHits.ContainsKey(randomSeed))
                s_registeringHits[randomSeed] = new List<HitData>();

            s_registeringHits[randomSeed].Add(new HitData()
            {
                TriggerIndex = triggerIndex,
                SpreadIndex = spreadIndex,
                ObjectId = objectId,
                HitBoxIndex = hitBoxIndex,
                HitPoint = hitPoint,
            });
        }

        public void SendHitRegToServer()
        {
            if (s_registeringHits.Count <= 0)
                return;

            foreach (KeyValuePair<int, List<HitData>> kv in s_registeringHits)
            {
                // Send register message to server
                BaseGameNetworkManager.Singleton.ClientSendPacket(BaseGameEntity.STATE_DATA_CHANNEL, LiteNetLib.DeliveryMethod.ReliableOrdered, GameNetworkingConsts.HitRegistration, new HitRegisterMessage()
                {
                    RandomSeed = kv.Key,
                    Hits = kv.Value,
                });
            }
            s_registeringHits.Clear();
        }

        public void Register(BaseGameEntity attacker, HitRegisterMessage message)
        {
            // Only server can perform hit registration
            if (attacker == null || !BaseGameNetworkManager.Singleton.IsServer)
                return;

            string id = MakeValidateId(attacker.ObjectId, message.RandomSeed);
            if (!s_validatingHits.ContainsKey(id))
                return;

            PerformValidation(attacker, id, message.RandomSeed, message.Hits);
        }

        public void ClearData()
        {
            s_validatingHits.Clear();
            s_registeringHits.Clear();
        }

        private void PerformValidation(BaseGameEntity attacker, string id, int simulateSeed, List<HitData> hits)
        {
            if (attacker == null || !s_validatingHits.ContainsKey(id))
                return;

            HitValidateData validateData = s_validatingHits[id];

            for (int i = 0; i < hits.Count; ++i)
            {
                HitData hitData = hits[i];
                if (hitData.TriggerIndex >= validateData.Origins.Length)
                {
                    // Invalid trigger index
                    continue;
                }

                if (hitData.SpreadIndex >= validateData.Origins[hitData.TriggerIndex].Count)
                {
                    // Invalid spread index
                    continue;
                }

                HitOriginData hitOriginData = validateData.Origins[hitData.TriggerIndex][hitData.SpreadIndex];
                uint objectId = hitData.ObjectId;
                int hitBoxIndex = hitData.HitBoxIndex;
                if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(objectId, out DamageableEntity damageableEntity) ||
                    hitBoxIndex < 0 || hitBoxIndex >= damageableEntity.HitBoxes.Length)
                {
                    // Can't find target or invalid hitbox
                    continue;
                }

                string hitId = MakeHitId(hitData.TriggerIndex, hitData.SpreadIndex);
                if (!validateData.HitsCount.TryGetValue(hitId, out int hitCount))
                {
                    // Set hit count to 0, if it is not in collection
                    hitCount = 0;
                }

                if (validateData.DamageInfo.IsHitReachedMax(hitCount))
                {
                    // Can't hit because it is reaching max amount of objects that can be hit
                    continue;
                }

                string hitObjectId = MakeHitObjectId(hitData.TriggerIndex, hitData.SpreadIndex, hitData.ObjectId);
                DamageableHitBox hitBox = damageableEntity.HitBoxes[hitBoxIndex];
                // Valiate hitting
                if (!validateData.HitObjects.Contains(hitObjectId) && IsHit(attacker, hitOriginData, hitData, hitBox))
                {
                    // Yes, it is hit
                    hitBox.ReceiveDamage(attacker.EntityTransform.position, attacker.GetInfo(), validateData.DamageAmounts, validateData.Weapon, validateData.Skill, validateData.SkillLevel, simulateSeed);
                    validateData.HitsCount[hitId] = ++hitCount;
                    validateData.HitObjects.Add(hitObjectId);
                }
            }
        }

        private bool IsHit(BaseGameEntity attacker, HitOriginData hitOriginData, HitData hitData, DamageableHitBox hitBox)
        {
            long halfRtt = attacker.Player != null ? (attacker.Player.Rtt / 2) : 0;
            long serverTime = BaseGameNetworkManager.Singleton.ServerTimestamp;
            long targetTime = serverTime - halfRtt;
            DamageableHitBox.TransformHistory transformHistory = hitBox.GetTransformHistory(serverTime, targetTime);
            _hitBoxTransform.position = transformHistory.Bounds.center;
            _hitBoxTransform.rotation = transformHistory.Rotation;
            Vector3 alignedHitPoint = _hitBoxTransform.InverseTransformPoint(hitData.HitPoint);
            bool isHit = Vector3.Distance(Vector3.zero, alignedHitPoint) <= Mathf.Max(transformHistory.Bounds.extents.x, transformHistory.Bounds.extents.y, transformHistory.Bounds.extents.z) + hitValidationBuffer;
            return isHit;
        }

        private static string MakeValidateId(uint attackerId, int randomSeed)
        {
            return ZString.Concat(attackerId, "_", randomSeed);
        }

        private static string MakeHitId(byte triggerIndex, byte spreadIndex)
        {
            return ZString.Concat(triggerIndex, "_", spreadIndex);
        }

        private static string MakeHitObjectId(byte triggerIndex, byte spreadIndex, uint objectId)
        {
            return ZString.Concat(triggerIndex, "_", spreadIndex, "_", objectId);
        }
    }
}
