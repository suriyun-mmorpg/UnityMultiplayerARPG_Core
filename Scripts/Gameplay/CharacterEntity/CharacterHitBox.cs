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
        public BaseGameEntity Entity { get { return characterEntity; } }

        public bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return characterEntity.CanReceiveDamageFrom(attacker);
        }

        public bool IsDead()
        {
            return characterEntity.IsDead();
        }

        public void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            List<DamageElement> keys = new List<DamageElement>(allDamageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                allDamageAmounts[key] = allDamageAmounts[key] * damageRate;
            }
            characterEntity.ReceiveDamageFunction(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }
    }
}
