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
            if (s_cancellationTokenSources.ContainsKey(id))
                s_cancellationTokenSources[id].Cancel();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            try
            {
                s_cancellationTokenSources[id] = cancellationTokenSource;
                // Delay 10 seconds before remove the validating data
                await UniTask.Delay(10 * 1000, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
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

            string id = MakeValidateId(attacker.ObjectId, randomSeed);
            CreateValidatingData(id);
            s_validatingHits[id].Attacker = attacker;
            s_validatingHits[id].TriggerDurations = triggerDurations;
            s_validatingHits[id].FireSpread = fireSpread;
            s_validatingHits[id].DamageInfo = damageInfo;
            s_validatingHits[id].DamageAmounts = damageAmounts;
            s_validatingHits[id].Weapon = weapon;
            s_validatingHits[id].Skill = skill;
            s_validatingHits[id].SkillLevel = skillLevel;
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
            CreateValidatingData(id);

            string hitId = MakeHitId(triggerIndex, spreadIndex);
            if (s_validatingHits[id].Origins.ContainsKey(hitId))
                return;

            s_validatingHits[id].Origins.Add(hitId, new HitOriginData()
            {
                TriggerIndex = triggerIndex,
                Position = position,
                Direction = direction,
            });

            if (s_validatingHits[id].Pendings.ContainsKey(hitId))
                PerformValidation(attacker, id, randomSeed, s_validatingHits[id].Pendings[hitId]);
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

            for (int i = 0; i < message.Hits.Count; ++i)
            {
                PerformValidation(attacker, id, message.RandomSeed, message.Hits[i]);
            }
        }

        public void ClearData()
        {
            s_validatingHits.Clear();
            s_registeringHits.Clear();
        }

        private bool PerformValidation(BaseGameEntity attacker, string id, int simulateSeed, HitData hitData)
        {
            if (attacker == null)
                return false;

            string hitId = MakeHitId(hitData.TriggerIndex, hitData.SpreadIndex);
            HitValidateData validateData;
            if (!s_validatingHits.TryGetValue(id, out validateData) || !validateData.Origins.ContainsKey(hitId))
            {
                // Invalid spread index
                CreateValidatingData(id);
                validateData.Pendings.Add(hitId, hitData);
                return false;
            }
            validateData.Pendings.Remove(hitId);

            HitOriginData hitOriginData = validateData.Origins[hitId];
            uint objectId = hitData.ObjectId;
            int hitBoxIndex = hitData.HitBoxIndex;
            if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(objectId, out DamageableEntity damageableEntity) ||
                hitBoxIndex < 0 || hitBoxIndex >= damageableEntity.HitBoxes.Length)
            {
                // Can't find target or invalid hitbox
                return false;
            }

            if (!validateData.HitsCount.TryGetValue(hitId, out int hitCount))
            {
                // Set hit count to 0, if it is not in collection
                hitCount = 0;
            }

            if (validateData.DamageInfo.IsHitReachedMax(hitCount))
            {
                // Can't hit because it is reaching max amount of objects that can be hit
                return false;
            }

            string hitObjectId = MakeHitObjectId(hitData.TriggerIndex, hitData.SpreadIndex, hitData.ObjectId);
            DamageableHitBox hitBox = damageableEntity.HitBoxes[hitBoxIndex];
            // Valiate hitting
            if (validateData.HitObjects.Contains(hitObjectId) || !IsHit(attacker, hitOriginData, hitData, hitBox))
            {
                // Already validate or not hit
                return false;
            }

            // Yes, it is hit
            hitBox.ReceiveDamage(attacker.EntityTransform.position, attacker.GetInfo(), validateData.DamageAmounts, validateData.Weapon, validateData.Skill, validateData.SkillLevel, simulateSeed);
            validateData.HitsCount[hitId] = ++hitCount;
            validateData.HitObjects.Add(hitObjectId);
            return true;
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
