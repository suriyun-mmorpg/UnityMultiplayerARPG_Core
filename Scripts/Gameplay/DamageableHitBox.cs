using UnityEngine;
using System.Collections.Generic;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        protected virtual void Awake()
        {
            if (entity == null)
                entity = GetComponentInParent<DamageableEntity>();
            if (entity != null)
            {
                gameObject.tag = entity.GetGameObject().tag;
                gameObject.layer = entity.GetGameObject().layer;
            }
        }

        public virtual bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            return entity.CanReceiveDamageFrom(instigator);
        }

        public virtual void ReceiveDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            if (!entity.IsServer || this.IsDead() || !CanReceiveDamageFrom(instigator))
                return;
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            entity.ApplyDamage(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }

        public EntityInfo GetInfo()
        {
            return entity.GetInfo();
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Handles.Label(transform.position, name + "(HitBox)");
        }
#endif
    }
}
