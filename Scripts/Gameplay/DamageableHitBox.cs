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
        [System.Serializable]
        public struct TransformHistory
        {
            public long Time { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        [SerializeField]
        protected float damageRate = 1f;

        public DamageableEntity DamageableEntity { get; private set; }
        public BaseGameEntity Entity { get { return DamageableEntity.Entity; } }
        public int CurrentHp
        {
            get
            {
                return !isSetup ? 0 : DamageableEntity.CurrentHp;
            }
            set
            {
                if (isSetup)
                    DamageableEntity.CurrentHp = value;
            }
        }
        public bool IsInSafeArea
        {
            get
            {
                return !isSetup ? false : DamageableEntity.IsInSafeArea;
            }
            set
            {
                if (isSetup)
                    DamageableEntity.IsInSafeArea = value;
            }
        }
        public Transform OpponentAimTransform { get { return DamageableEntity.OpponentAimTransform; } }
        public LiteNetLibIdentity Identity { get { return DamageableEntity.Identity; } }
        public int Index { get; private set; }

        private bool isSetup;
        private Vector3 defaultLocalPosition;
        private Quaternion defaultLocalRotation;
        private List<TransformHistory> histories = new List<TransformHistory>();

        public virtual void Setup(DamageableEntity entity, int index)
        {
            isSetup = true;
            DamageableEntity = entity;
            gameObject.tag = entity.gameObject.tag;
            gameObject.layer = entity.gameObject.layer;
            defaultLocalPosition = transform.localPosition;
            defaultLocalRotation = transform.localRotation;
            Index = index;
        }

        public virtual bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            return DamageableEntity.CanReceiveDamageFrom(instigator);
        }

        public virtual void ReceiveDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            if (!DamageableEntity.IsServer || this.IsDead() || !CanReceiveDamageFrom(instigator))
                return;
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            DamageableEntity.ApplyDamage(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }

        public EntityInfo GetInfo()
        {
            return DamageableEntity.GetInfo();
        }

        internal void Reverse(long duration)
        {
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
            transform.localPosition = defaultLocalPosition;
            transform.localRotation = defaultLocalRotation;
        }

        public void AddTransformHistory()
        {
            if (histories.Count == BaseGameNetworkManager.Singleton.LagCompensationManager.MaxHistorySize)
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
