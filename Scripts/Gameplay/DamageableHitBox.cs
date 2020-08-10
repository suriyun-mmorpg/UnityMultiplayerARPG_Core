using UnityEngine;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class DamageableHitBox<T> : MonoBehaviour, IDamageableEntity where T : IDamageableEntity
    {
        [SerializeField]
        protected T entity;
        public int CurrentHp { get { return entity.CurrentHp; } set { entity.CurrentHp = value; } }
        public Transform OpponentAimTransform { get { return entity.OpponentAimTransform; } }
        public BaseGameEntity Entity { get { return entity.Entity; } }
        public T TargetEntity { get { return entity; } }

        protected virtual void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<T>();
        }

        public virtual bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            return entity.CanReceiveDamageFrom(attacker);
        }

        public virtual bool IsDead()
        {
            return entity.IsDead();
        }

        public virtual void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            entity.PlayHitEffects(damageElements, skill);
        }

        public virtual void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            entity.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
        }
    }

    public class DamageableHitBox : DamageableHitBox<DamageableEntity> { }
}
