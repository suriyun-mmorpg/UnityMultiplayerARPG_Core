using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        private Dictionary<int, List<HitRegisterData>> preparedHits = new Dictionary<int, List<HitRegisterData>>();
        private Dictionary<string, List<HitRegisterData>> registeredHits = new Dictionary<string, List<HitRegisterData>>();

        public void Validate(DamageInfo damageInfo, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can prepare hit registration
                return;
            }

            string id = MakeId(attacker.Id, randomSeed);
            if (!registeredHits.ContainsKey(id))
            {
                // The hits not registered yet, assume that it doesn't hit
                // TODO: May performs casting by server
                return;
            }

            if (registeredHits[id].Count <= 0)
            {
                // No more hitting
                return;
            }

            DamageableEntity damageableEntity;
            if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(registeredHits[id][0].HitObjectId, out damageableEntity) ||
                damageableEntity.HitBoxes.Length >= registeredHits[id][0].HitBoxIndex)
            {
                // Can't find target or invalid hitbox
                return;
            }

            // TODO: Valiate hitting

            // Yes, it is hit
            damageableEntity.HitBoxes[registeredHits[id][0].HitBoxIndex].ReceiveDamage(attacker.CacheTransform.position, attacker.GetInfo(), damageAmounts, weapon, skill, skillLevel, randomSeed);
            registeredHits[id].RemoveAt(0);
        }

        public void Register(BaseCharacterEntity attacker, HitRegisterMessage message)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can perform hit registration
                return;
            }
            string id = MakeId(attacker.Id, message.RandomSeed);
            if (!registeredHits.ContainsKey(id))
            {
                registeredHits.Add(id, message.Hits);
                DelayUnregisterHits(id);
            }
        }

        public void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, AimPosition aimPosition, uint hitObjectId, byte hitBoxIndex, Vector3 hitPoint)
        {
            if (!attacker.IsOwnerClient)
            {
                // Only owner client can prepare to register
                return;
            }
            if (!preparedHits.ContainsKey(randomSeed))
                preparedHits.Add(randomSeed, new List<HitRegisterData>());
            preparedHits[randomSeed].Add(new HitRegisterData()
            {
                AimPosition = aimPosition,
                HitObjectId = hitObjectId,
                HitBoxIndex = hitBoxIndex,
                HitPoint = hitPoint,
            });
        }

        private void FixedUpdate()
        {
            if (preparedHits.Count > 0)
            {
                foreach (KeyValuePair<int, List<HitRegisterData>> kv in preparedHits)
                {
                    // Send register message to server
                    BaseGameNetworkManager.Singleton.ClientSendPacket(BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, LiteNetLib.DeliveryMethod.ReliableUnordered, GameNetworkingConsts.HitRegistration, new HitRegisterMessage()
                    {
                        RandomSeed = kv.Key,
                        Hits = kv.Value,
                    });
                }
                preparedHits.Clear();
            }
        }

        private async void DelayUnregisterHits(string id)
        {
            await UniTask.Delay(200);
            registeredHits.Remove(id);
        }

        private string MakeId(string attackerId, int randomSeed)
        {
            return attackerId + "_" + randomSeed;
        }
    }
}
