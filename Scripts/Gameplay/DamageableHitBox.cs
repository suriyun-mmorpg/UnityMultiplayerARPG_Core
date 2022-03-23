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
        protected HitBoxPosition position;

        [SerializeField]
        protected float damageRate = 1f;

        public DamageableEntity DamageableEntity { get; private set; }
        public BaseGameEntity Entity
        {
            get { return DamageableEntity.Entity; }
        }
        public bool IsImmune
        {
            get { return DamageableEntity.IsImmune; }
        }
        public int CurrentHp
        {
            get { return DamageableEntity.CurrentHp; }
            set { DamageableEntity.CurrentHp = value; }
        }
        public bool IsInSafeArea
        {
            get { return DamageableEntity.IsInSafeArea; }
            set { DamageableEntity.IsInSafeArea = value; }
        }
        public Transform OpponentAimTransform
        {
            get { return DamageableEntity.OpponentAimTransform; }
        }
        public LiteNetLibIdentity Identity
        {
            get { return DamageableEntity.Identity; }
        }
        public Collider CacheCollider { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public Collider2D CacheCollider2D { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }
        public byte Index { get; private set; }

        protected bool isSetup;
        protected Vector3 defaultLocalPosition;
        protected Quaternion defaultLocalRotation;
        protected List<TransformHistory> histories = new List<TransformHistory>();

#if UNITY_EDITOR
        [Header("Rewind Debugging")]
        public Color debugHistoryColor = new Color(0, 1, 0, 0.25f);
        public Color debugRewindColor = new Color(0, 0, 1, 0.5f);
        private Vector3? debugRewindPosition;
        private Quaternion? debugRewindRotation;
        private Vector3? debugRewindCenter;
        private Vector3? debugRewindSize;
#endif

        private void Awake()
        {
            DamageableEntity = GetComponentInParent<DamageableEntity>();
            CacheCollider = GetComponent<Collider>();
            if (CacheCollider)
            {
                CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
                CacheRigidbody.useGravity = false;
                CacheRigidbody.isKinematic = true;
#if UNITY_EDITOR
                debugRewindCenter = CacheCollider.bounds.center - transform.position;
                debugRewindSize = CacheCollider.bounds.size;
#endif
                return;
            }
            CacheCollider2D = GetComponent<Collider2D>();
            if (CacheCollider2D)
            {
                CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
                CacheRigidbody2D.gravityScale = 0;
                CacheRigidbody2D.isKinematic = true;
#if UNITY_EDITOR
                debugRewindCenter = CacheCollider2D.bounds.center - transform.position;
                debugRewindSize = CacheCollider2D.bounds.size;
#endif
            }
        }

        public virtual void Setup(byte index)
        {
            isSetup = true;
            gameObject.tag = DamageableEntity.gameObject.tag;
            gameObject.layer = DamageableEntity.gameObject.layer;
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
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
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
            ReceiveDamageWithoutConditionCheck(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public virtual void ReceiveDamageWithoutConditionCheck(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            List<DamageElement> keys = new List<DamageElement>(damageAmounts.Keys);
            foreach (DamageElement key in keys)
            {
                damageAmounts[key] = damageAmounts[key] * damageRate;
            }
            DamageableEntity.ApplyDamage(position, fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }

        public EntityInfo GetInfo()
        {
            return DamageableEntity.GetInfo();
        }

        internal void Rewind(long currentTime, long rewindTime)
        {
            TransformHistory beforeRewind = default;
            TransformHistory afterRewind = default;
            for (int i = 0; i < histories.Count; ++i)
            {
                if (beforeRewind.Time > 0 && beforeRewind.Time <= rewindTime && histories[i].Time >= rewindTime)
                {
                    afterRewind = histories[i];
                    break;
                }
                else
                {
                    beforeRewind = histories[i];
                }
                if (histories.Count - 1 == i)
                {
                    afterRewind = new TransformHistory()
                    {
                        Position = transform.position,
                        Rotation = transform.rotation,
                        Time = currentTime,
                    };
                }
            }
            long durationToRewindTime = rewindTime - beforeRewind.Time;
            long durationBetweenRewindTime = afterRewind.Time - beforeRewind.Time;
            float lerpProgress = (float)durationToRewindTime / (float)durationBetweenRewindTime;
            transform.position = Vector3.Lerp(beforeRewind.Position, afterRewind.Position, lerpProgress);
            transform.rotation = Quaternion.Slerp(beforeRewind.Rotation, afterRewind.Rotation, lerpProgress);
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

        public void AddTransformHistory(long time)
        {
            if (histories.Count == BaseGameNetworkManager.Singleton.LagCompensationManager.MaxHistorySize)
                histories.RemoveAt(0);
            histories.Add(new TransformHistory()
            {
                Time = time,
                Position = transform.position,
                Rotation = transform.rotation,
            });
        }
    }
}
