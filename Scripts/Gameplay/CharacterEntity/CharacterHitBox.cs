using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterHitBox : DamageableHitBox<BaseCharacterEntity>
    {
        public float damageRate = 1f;

        public override void ReceiveDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            entity.ReceiveDamageFunction(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }
    }
}
