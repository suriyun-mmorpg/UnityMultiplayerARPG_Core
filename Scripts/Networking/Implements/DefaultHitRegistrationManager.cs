using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public struct HitInfo
        {
            public BaseCharacterEntity attacker;
            public Dictionary<DamageElement, MinMaxFloat> damageAmounts;
            public CharacterItem weapon;
            public BaseSkill skill;
            public short skillLevel;
            public int randomSeed;
        }

        private Dictionary<string, HitInfo> registeredHitInfos = new Dictionary<string, HitInfo>();
        private Dictionary<string, int> registeredHitCounts = new Dictionary<string, int>();

        public void PrepareHitRegistration(DamageInfo damageInfo, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can prepare hit registration
                return;
            }

            string id = MakeId(attacker.Id, randomSeed);
            if (!registeredHitInfos.ContainsKey(id))
            {
                registeredHitInfos.Add(id, new HitInfo()
                {
                    attacker = attacker,
                    damageAmounts = damageAmounts,
                    weapon = weapon,
                    skill = skill,
                    skillLevel = skillLevel,
                    randomSeed = randomSeed,
                });
                DelayUnregisterHitInfo(id);
            }
            if (!registeredHitCounts.ContainsKey(id))
            {
                // Store hit count it will be used for collection removing later (when client send hit register to server)
                registeredHitCounts.Add(id, 0);
            }
            registeredHitCounts[id]++;
        }

        public void PerformHitRegistration(BaseCharacterEntity attacker, Vector3 origin, Vector3 direction, DamageableEntity target, byte hitBoxIndex, Vector3 hitPoint, int randomSeed)
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
            {
                // Only server can perform hit registration
                return;
            }

            string id = MakeId(attacker.Id, randomSeed);
            if (!registeredHitInfos.ContainsKey(id) ||
                !registeredHitCounts.ContainsKey(id))
            {
                // No hit info
                return;
            }

            if (target.HitBoxes.Length >= hitBoxIndex)
            {
                // Invalid hit box index
                return;
            }

            // TODO: Validate hitting

            // Trust the client
            HitInfo hitInfo = registeredHitInfos[id];
            EntityInfo instigator = attacker.GetInfo();
            target.HitBoxes[hitBoxIndex].ReceiveDamage(attacker.CacheTransform.position, instigator, hitInfo.damageAmounts, hitInfo.weapon, hitInfo.skill, hitInfo.skillLevel, randomSeed);
        }

        private async void DelayUnregisterHitInfo(string id)
        {
            await UniTask.Delay(200);
            registeredHitInfos.Remove(id);
            registeredHitCounts.Remove(id);
        }

        private string MakeId(string attackerId, int randomSeed)
        {
            return attackerId + "_" + randomSeed; ;
        }
    }
}
