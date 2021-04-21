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
        public const int MAX_HISTORY_SIZE = 16;

        [System.Serializable]
        struct TransformHistory
        {
            public long Time { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        [SerializeField]
        protected float damageRate = 1f;

        private DamageableEntity entity;
        public BaseGameEntity Entity { get { return entity.Entity; } }
        public int CurrentHp { get { return entity.CurrentHp; } set { entity.CurrentHp = value; } }
        public Transform OpponentAimTransform { get { return entity.OpponentAimTransform; } }
        public LiteNetLibIdentity Identity { get { return entity.Identity; } }
        public int Index { get; private set; }

        private Vector3 positionBeforeReverse;
        private Quaternion rotationBeforeReverse;
        private readonly List<TransformHistory> histories = new List<TransformHistory>();

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

        public virtual void Setup(DamageableEntity entity, int index)
        {
            this.entity = entity;
            Index = index;
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

        internal void Reverse(long duration)
        {
            positionBeforeReverse = transform.position;
            rotationBeforeReverse = transform.rotation;

            long currentTime = BaseGameNetworkManager.Singleton.ServerTimestamp;
            long reversedTime = currentTime - duration;

            TransformHistory beforeReversedData = default;
            TransformHistory afterReversedData = default;
            for (int i = 0; i < histories.Count; ++i)
            {
                if (beforeReversedData.Time <= reversedTime && histories[i].Time >= reversedTime)
                {
                    afterReversedData = histories[i];
                    break;
                }
                else
                {
                    beforeReversedData = histories[i];
                }
            }
            long durationBetweenReversedTime = afterReversedData.Time - beforeReversedData.Time;
            long durationToCurrentTime = currentTime - beforeReversedData.Time;
            float lerpProgress = (float)durationToCurrentTime / (float)durationBetweenReversedTime;
            transform.position = Vector3.Lerp(beforeReversedData.Position, afterReversedData.Position, lerpProgress);
            transform.rotation = Quaternion.Slerp(beforeReversedData.Rotation, afterReversedData.Rotation, lerpProgress);
        }

        internal void ResetTransform()
        {
            transform.position = positionBeforeReverse;
            transform.rotation = rotationBeforeReverse;
        }

        public void AddTransformHistory()
        {
            if (histories.Count == MAX_HISTORY_SIZE)
                histories.RemoveAt(0);
            histories.Add(new TransformHistory()
            {
                Time = BaseGameNetworkManager.Singleton.ServerTimestamp,
                Position = transform.position,
                Rotation = transform.rotation,
            });
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Handles.Label(transform.position, name + "(HitBox)");
        }
#endif
    }
}
