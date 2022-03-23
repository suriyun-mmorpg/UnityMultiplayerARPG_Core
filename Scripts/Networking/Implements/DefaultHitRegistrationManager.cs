using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        private Dictionary<string, List<HitRegisterData>> registeredHits = new Dictionary<string, List<HitRegisterData>>();

        public void ValidateHit(DamageInfo damageInfo, byte hitIndex, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
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

            if (registeredHits[id].Count >= hitIndex)
            {
                // Invalid hit index
                return;
            }

            if (!registeredHits[id][hitIndex].IsHit)
            {
                // Not hit
                return;
            }

            DamageableEntity damageableEntity;
            if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(registeredHits[id][hitIndex].HitObjectId, out damageableEntity) ||
                damageableEntity.HitBoxes.Length >= registeredHits[id][hitIndex].HitBoxIndex)
            {
                // Can't find target or invalid hitbox
                return;
            }

            // TODO: Valiate hitting

            // Yes, it is hit
            damageableEntity.HitBoxes[registeredHits[id][hitIndex].HitBoxIndex].ReceiveDamage(attacker.CacheTransform.position, attacker.GetInfo(), damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public void RegisterHit(BaseCharacterEntity attacker, HitRegisterMessage message)
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

        private async void DelayUnregisterHits(string id)
        {
            await UniTask.Delay(200);
            registeredHits.Remove(id);
        }

        private string MakeId(string attackerId, int randomSeed)
        {
            return attackerId + "_" + randomSeed; ;
        }
    }
}
