using Cysharp.Text;
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

        public void PrepareHitRegValidatation(DamageInfo damageInfo, int randomSeed, byte fireSpread, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel)
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
            validateHits[id] = new HitValidateData()
            {
                FireSpread = fireSpread,
                Attacker = attacker,
                DamageAmounts = damageAmounts,
                DamageInfo = damageInfo,
                Weapon = weapon,
                Skill = skill,
                SkillLevel = skillLevel,
            };
            PerformValidation(attacker, id, randomSeed);
        }

        public void Register(BaseCharacterEntity attacker, HitRegisterMessage message)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can perform hit registration
                return;
            }

            string id = MakeId(attacker.Id, message.RandomSeed);
            registerHits[id] = message.Hits;
            PerformValidation(attacker, id, message.RandomSeed);
        }

        private bool PerformValidation(BaseCharacterEntity attacker, string id, int randomSeed)
        {
            if (!registerHits.ContainsKey(id) || !validateHits.ContainsKey(id))
                return false;

            HashSet<string> hitHitboxes = new HashSet<string>();
            while (registerHits[id].Count > 0)
            {
                if (registerHits[id].Count > validateHits[id].FireSpread + 1)
                {
                    // Over firespread? player try to hack?, don't allow it
                    registerHits[id].RemoveAt(0);
                    continue;
                }
                hitHitboxes.Clear();
                for (int i = 0; i < registerHits[id][0].HitDataCollection.Count; ++i)
                {
                    if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(registerHits[id][0].HitDataCollection[i].HitObjectId, out DamageableEntity damageableEntity) ||
                        registerHits[id][0].HitDataCollection[i].HitBoxIndex < 0 || registerHits[id][0].HitDataCollection[i].HitBoxIndex >= damageableEntity.HitBoxes.Length)
                    {
                        // Can't find target or invalid hitbox
                        continue;
                    }
                    if (!validateHits[id].DamageInfo.IsHitReachedMax(hitHitboxes.Count))
                    {
                        // Can't hit because it is reaching max amount of objects that can be hit
                        continue;
                    }
                    string hitId = ZString.Concat(registerHits[id][0].HitDataCollection[i].HitObjectId, "_", registerHits[id][0].HitDataCollection[i].HitBoxIndex);
                    if (hitHitboxes.Contains(hitId))
                    {
                        // Already hit to the hitbox, shouldn't be hit again
                        continue;
                    }
                    hitHitboxes.Add(hitId);
                    DamageableHitBox hitBox = damageableEntity.HitBoxes[registerHits[id][0].HitDataCollection[i].HitBoxIndex];
                    // Valiate hitting
                    if (IsHit(attacker, registerHits[id][0], registerHits[id][0].HitDataCollection[i], hitBox))
                    {
                        // Yes, it is hit
                        hitBox.ReceiveDamage(validateHits[id].Attacker.EntityTransform.position, validateHits[id].Attacker.GetInfo(), validateHits[id].DamageAmounts, validateHits[id].Weapon, validateHits[id].Skill, validateHits[id].SkillLevel, randomSeed);
                    }
                }
                registerHits[id].RemoveAt(0);
            }

            registerHits.Remove(id);
            validateHits.Remove(id);
            return true;
        }

        private bool IsHit(BaseCharacterEntity attacker, HitRegisterData hitRegData, HitData hitData, DamageableHitBox hitBox)
        {
            long halfRtt = attacker.Player != null ? (attacker.Player.Rtt / 2) : 0;
            long serverTime = BaseGameNetworkManager.Singleton.ServerTimestamp;
            long targetTime = serverTime - halfRtt;
            DamageableHitBox.TransformHistory transformHistory = hitBox.GetTransformHistory(serverTime, targetTime);
            bool isHit = Vector3.Distance(hitData.HitPoint, transformHistory.Position) <= Mathf.Max(transformHistory.Bounds.extents.x, transformHistory.Bounds.extents.y, transformHistory.Bounds.extents.z) + hitValidationBuffer;
            return isHit;
        }

        public void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, Vector3 damagePosition, Vector3 damageDirection, List<HitData> hitDataCollection)
        {
            if (!prepareHits.ContainsKey(randomSeed))
                prepareHits.Add(randomSeed, new List<HitRegisterData>());

            prepareHits[randomSeed].Add(new HitRegisterData()
            {
                Position = damagePosition,
                Direction = damageDirection,
                HitDataCollection = hitDataCollection,
            });
        }

        public void SendHitRegToServer()
        {
            if (prepareHits.Count > 0)
            {
                foreach (KeyValuePair<int, List<HitRegisterData>> kv in prepareHits)
                {
                    // Send register message to server
                    BaseGameNetworkManager.Singleton.ClientSendPacket(BaseGameEntity.CLIENT_STATE_DATA_CHANNEL, LiteNetLib.DeliveryMethod.ReliableOrdered, GameNetworkingConsts.HitRegistration, new HitRegisterMessage()
                    {
                        RandomSeed = kv.Key,
                        Hits = kv.Value,
                    });
                }
                prepareHits.Clear();
            }
        }

        private static string MakeId(string attackerId, int randomSeed)
        {
            return ZString.Concat(attackerId, "_", randomSeed);
        }
    }
}
