using UnityEngine;
using System.Collections.Generic;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class DamageableHitBox : MonoBehaviour, IDamageableEntity
    {
        [SerializeField]
        protected DamageableEntity entity;
        [SerializeField]
        protected float damageRate = 1f;
        public int CurrentHp { get { return entity.CurrentHp; } set { entity.CurrentHp = value; } }
        public Transform OpponentAimTransform { get { return entity.OpponentAimTransform; } }
        public BaseGameEntity Entity { get { return entity.Entity; } }
        public LiteNetLibIdentity Identity { get { return entity.Identity; } }

        protected virtual void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<DamageableEntity>();
            if (entity != null)
            {
                gameObject.tag = entity.GetGameObject().tag;
                gameObject.layer = entity.GetGameObject().layer;
            }
        }

        public virtual bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            return entity.CanReceiveDamageFrom(attacker);
        }

        public virtual void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            entity.PlayHitEffects(damageElements, skill);
        }

        public virtual void ReceiveDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            if (!entity.IsServer || this.IsDead() || !CanReceiveDamageFrom(attacker))
                return;
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            entity.ApplyDamage(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }
    }
}
