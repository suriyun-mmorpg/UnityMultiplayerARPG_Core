using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterHitBox : MonoBehaviour, IDamageableEntity
    {
        public BaseCharacterEntity characterEntity;
        public float damageRate = 1f;

        public uint ObjectId { get { return characterEntity.ObjectId; } }
        public int CurrentHp { get { return characterEntity.CurrentHp; } set { characterEntity.CurrentHp = value; } }
        public Transform OpponentAimTransform { get { return characterEntity.OpponentAimTransform; } }
        public BaseGameEntity Entity { get { return characterEntity; } }

        public bool IsDead()
        {
            return characterEntity.IsDead();
        }

        public void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            characterEntity.ReceiveDamageFunction(attacker, weapon, damageAmounts, skill, skillLevel);
        }

        public bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return characterEntity.CanReceiveDamageFrom(attacker);
        }

        public void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            characterEntity.PlayHitEffects(damageElements, skill);
        }
    }
}
