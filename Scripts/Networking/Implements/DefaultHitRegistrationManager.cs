using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public float hitValidationBuffer = 2f;
        private static Dictionary<int, List<HitRegisterData>> prepareHits = new Dictionary<int, List<HitRegisterData>>();
        private static Dictionary<string, List<HitRegisterData>> registerHits = new Dictionary<string, List<HitRegisterData>>();
        private static Dictionary<string, HitValidateData> validateHits = new Dictionary<string, HitValidateData>();

        public void Validate(DamageInfo damageInfo, int randomSeed, byte fireSpread, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can prepare hit registration
                return;
            }

            if (damageInfo.damageType == DamageType.Missile || damageInfo.damageType == DamageType.Throwable)
            {
                // Don't validate damage entity based damage types
                return;
            }

            string id = MakeId(attacker.Id, randomSeed);
            Debug.LogError("Validate " + id);
            if (!validateHits.ContainsKey(id))
            {
                validateHits.Add(id, new HitValidateData()
                {
                    FireSpread = fireSpread,
                    Attacker = attacker,
                    DamageAmounts = damageAmounts,
                    Weapon = weapon,
                    Skill = skill,
                    SkillLevel = skillLevel,
                });
                DelayRemoveValidateHits(id);
                PerformValidation(id, randomSeed);
            }
        }

        public void Register(BaseCharacterEntity attacker, HitRegisterMessage message)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can perform hit registration
                return;
            }

            string id = MakeId(attacker.Id, message.RandomSeed);
            if (!registerHits.ContainsKey(id))
            {
                registerHits.Add(id, message.Hits);
                DelayRemoveRegisterHits(id);
                PerformValidation(id, message.RandomSeed);
            }
        }

        private void PerformValidation(string id, int randomSeed)
        {
            if (!registerHits.ContainsKey(id) || !validateHits.ContainsKey(id))
                return;

            while (registerHits[id].Count > 0)
            {
                if (registerHits[id].Count > validateHits[id].FireSpread + 1)
                {
                    // Over firespread? player try to hack?, don't allow it
                    registerHits[id].RemoveAt(0);
                    continue;
                }
                DamageableEntity damageableEntity;
                for (int i = 0; i < registerHits[id][0].HitDataCollection.Count; ++i)
                {
                    if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(registerHits[id][0].HitDataCollection[i].HitObjectId, out damageableEntity) ||
                        registerHits[id][0].HitDataCollection[i].HitBoxIndex >= damageableEntity.HitBoxes.Length)
                    {
                        // Can't find target or invalid hitbox
                        registerHits[id].RemoveAt(0);
                        continue;
                    }
                    DamageableHitBox hitBox = damageableEntity.HitBoxes[registerHits[id][0].HitDataCollection[i].HitBoxIndex];
                    // Valiate hitting
                    if (IsHit(validateHits[id], registerHits[id][0], registerHits[id][0].HitDataCollection[i], hitBox))
                    {
                        // Yes, it is hit
                        hitBox.ReceiveDamage(validateHits[id].Attacker.CacheTransform.position, validateHits[id].Attacker.GetInfo(), validateHits[id].DamageAmounts, validateHits[id].Weapon, validateHits[id].Skill, validateHits[id].SkillLevel, randomSeed);
                    }
                }
                registerHits[id].RemoveAt(0);
            }

            registerHits.Remove(id);
            validateHits.Remove(id);
        }

        private bool IsHit(HitValidateData validateHitData, HitRegisterData registerData, HitData hitData, DamageableHitBox hitBox)
        {
            long halfRtt = validateHitData.Attacker.Player != null ? (validateHitData.Attacker.Player.Rtt / 2) : 0;
            long serverTime = BaseGameNetworkManager.Singleton.ServerTimestamp;
            long targetTime = serverTime - halfRtt;
            DamageableHitBox.TransformHistory transformHistory = hitBox.GetTransformHistory(serverTime, targetTime);
            bool isHit = Vector3.Distance(hitData.HitPoint, transformHistory.Position) <= Mathf.Max(transformHistory.Bounds.extents.x, transformHistory.Bounds.extents.y, transformHistory.Bounds.extents.z) + hitValidationBuffer;
            return isHit;
        }

        public void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, AimPosition aimPosition, List<HitData> hitDataCollection)
        {
            if (!attacker.IsOwnerClient)
            {
                // Only owner client can prepare to register
                return;
            }

            if (!prepareHits.ContainsKey(randomSeed))
                prepareHits.Add(randomSeed, new List<HitRegisterData>());

            prepareHits[randomSeed].Add(new HitRegisterData()
            {
                AimPosition = aimPosition,
                HitDataCollection = hitDataCollection,
            });
        }

        private void FixedUpdate()
        {
            if (prepareHits.Count > 0)
            {
                foreach (KeyValuePair<int, List<HitRegisterData>> kv in prepareHits)
                {
                    // Send register message to server
                    BaseGameNetworkManager.Singleton.ClientSendPacket(BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, LiteNetLib.DeliveryMethod.ReliableUnordered, GameNetworkingConsts.HitRegistration, new HitRegisterMessage()
                    {
                        RandomSeed = kv.Key,
                        Hits = kv.Value,
                    });
                }
                prepareHits.Clear();
            }
        }

        private static async void DelayRemoveValidateHits(string id)
        {
            await UniTask.Delay(200);
            validateHits.Remove(id);
        }

        private static async void DelayRemoveRegisterHits(string id)
        {
            await UniTask.Delay(200);
            registerHits.Remove(id);
        }

        private static string MakeId(string attackerId, int randomSeed)
        {
            return attackerId + "_" + randomSeed;
        }
    }
}
