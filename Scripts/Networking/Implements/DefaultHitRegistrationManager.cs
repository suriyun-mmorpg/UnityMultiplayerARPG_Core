using Cysharp.Text;
using Cysharp.Threading.Tasks;
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
        
        public void PrepareHitRegValidation(BaseGameEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            // Only server can prepare hit registration
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null)
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

        public void ConfirmHitRegValidation(BaseGameEntity attacker, int randomSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts)
        {
            resultDamageAmounts = null;
            // Only server can modify damage amounts
            if (!BaseGameNetworkManager.Singleton.IsServer || attacker == null)
                return false;

            string id = MakeValidateId(attacker.ObjectId, randomSeed);
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData))
                return false;

            if (increaseDamageAmounts != null && increaseDamageAmounts.Count > 0)
                hitValidateData.DamageAmounts = GameDataHelpers.CombineDamages(hitValidateData.DamageAmounts, increaseDamageAmounts);

            resultDamageAmounts = hitValidateData.DamageAmounts;
            return true;
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

            string id = MakeValidateId(attacker.ObjectId, message.RandomSeed);
            if (!s_validatingHits.ContainsKey(id))
                return;

            for (int i = 0; i < message.Hits.Count; ++i)
            {
                PerformValidation(attacker, id, message.RandomSeed, message.Hits[i]);
            }
        }

        private bool PerformValidation(BaseGameEntity attacker, string id, int simulateSeed, HitRegisterData hitData)
        {
            if (attacker == null)
                return false;

            string hitId = MakeHitRegId(hitData.TriggerIndex, hitData.SpreadIndex);
            if (!s_validatingHits.TryGetValue(id, out HitValidateData hitValidateData) || !hitValidateData.Origins.ContainsKey(hitId))
            {
                // Invalid spread index
                CreateValidatingData(id);
                hitValidateData.Pendings[hitId] = hitData;
                return false;
            }
            hitValidateData.Pendings.Remove(hitId);

            HitOriginData hitOriginData = hitValidateData.Origins[hitId];
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
            long timestamp = BaseGameNetworkManager.Singleton.Timestamp;
            if (!hitValidateData.DamageInfo.IsHitValid(hitValidateData, hitData, hitBox, hitId, hitObjectId, timestamp))
            {
                // Not valid
                return false;
            }

            if (!IsHit(attacker, hitValidateData, hitOriginData, hitData, hitBox, timestamp))
            {
                // Not hit
                return false;
            }

            if (!hitValidateData.HitsCount.TryGetValue(hitId, out int hitCount))
            {
                // Set hit count to 0, if it is not in collection
                hitCount = 0;
            }

            // Yes, it is hit
            hitBox.ReceiveDamage(attacker.EntityTransform.position, attacker.GetInfo(), hitValidateData.DamageAmounts, hitValidateData.Weapon, hitValidateData.Skill, hitValidateData.SkillLevel, simulateSeed);
            hitValidateData.HitsCount[hitId] = ++hitCount;
            hitValidateData.HitObjects.Add(hitObjectId);
            return true;
        }

        private bool IsHit(BaseGameEntity attacker, HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox, long timestamp)
        {
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

        private static string MakeValidateId(uint attackerId, int randomSeed)
        {
            return ZString.Concat(attackerId, "_", randomSeed);
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
