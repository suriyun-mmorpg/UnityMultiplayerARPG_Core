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
        public BaseGameEntity Entity
        {
            get
            {
                if (isSetup)
                    return DamageableEntity.Entity;
                return null;
            }
        }
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
        public Transform OpponentAimTransform
        {
            get
            {
                if (isSetup)
                    return DamageableEntity.OpponentAimTransform;
                return null;
            }
        }
        public LiteNetLibIdentity Identity
        {
            get
            {
                if (isSetup)
                    return DamageableEntity.Identity;
                return null;
            }
        }
        public int Index { get; private set; }

        private bool isSetup;
        private Vector3 defaultLocalPosition;
        private Quaternion defaultLocalRotation;
        private List<TransformHistory> histories = new List<TransformHistory>();

#if UNITY_EDITOR
        [Header("Rewind Debugging")]
        public Color debugHistoryColor = new Color(0, 1, 0, 0.04f);
        public Color debugRewindColor = new Color(0, 0, 1, 0.04f);
        private Vector3? debugRewindPosition;
        private Quaternion? debugRewindRotation;
        private Vector3? debugRewindCenter;
        private Vector3? debugRewindSize;
#endif

#if UNITY_EDITOR
        private void Awake()
        {
            Collider debugCollider = GetComponent<Collider>();
            Collider2D debugCollider2D = GetComponent<Collider2D>();
            if (debugCollider)
            {
                debugRewindCenter = debugCollider.bounds.center - transform.position;
                debugRewindSize = debugCollider.bounds.size;
            }
            else if (debugCollider2D)
            {
                debugRewindCenter = debugCollider.bounds.center - transform.position;
                debugRewindSize = debugCollider2D.bounds.size;
            }
        }
#endif

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

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (debugRewindCenter.HasValue &&
                debugRewindSize.HasValue)
            {
                Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
                foreach (TransformHistory history in histories)
                {
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(history.Position + debugRewindCenter.Value, history.Rotation, debugRewindSize.Value);
                    Gizmos.color = debugHistoryColor;
                    Gizmos.matrix = transformMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
                if (debugRewindPosition.HasValue &&
                    debugRewindRotation.HasValue)
                {
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(debugRewindPosition.Value + debugRewindCenter.Value, debugRewindRotation.Value, debugRewindSize.Value);
                    Gizmos.color = debugRewindColor;
                    Gizmos.matrix = transformMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
                Gizmos.matrix = oldGizmosMatrix;
            }
            Handles.Label(transform.position, name + "(HitBox)");
        }
#endif

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

        internal void Rewind(long rewindTime)
        {
            long currentTime = BaseGameNetworkManager.Singleton.ServerTimestamp;

            TransformHistory beforeReversedData = default;
            TransformHistory afterReversedData = default;
            for (int i = 0; i < histories.Count; ++i)
            {
                if (beforeReversedData.Time <= rewindTime && histories[i].Time >= rewindTime)
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
#if UNITY_EDITOR
            debugRewindPosition = transform.position;
            debugRewindRotation = transform.rotation;
#endif
        }

        internal void Restore()
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
    }
}
