using UnityEngine;
using System.Collections.Generic;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class DamageableHitBox : MonoBehaviour, IDamageableEntity, IActivatePressActivatableEntity, IClickActivatableEntity, IHoldClickActivatableEntity
    {
        [System.Serializable]
        public struct TransformHistory
        {
            public long Time { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
            public Bounds Bounds { get; set; }
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
        public IActivatePressActivatableEntity ActivatePressActivatableEntity
        {
            get
            {
                if (DamageableEntity is IActivatePressActivatableEntity)
                    return DamageableEntity as IActivatePressActivatableEntity;
                return null;
            }
        }
        public IClickActivatableEntity ClickActivatableEntity
        {
            get
            {
                if (DamageableEntity is IClickActivatableEntity)
                    return DamageableEntity as IClickActivatableEntity;
                return null;
            }
        }
        public IHoldClickActivatableEntity HoldClickActivatableEntity
        {
            get
            {
                if (DamageableEntity is IHoldClickActivatableEntity)
                    return DamageableEntity as IHoldClickActivatableEntity;
                return null;
            }
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
        public Bounds Bounds
        {
            get
            {
                if (CacheCollider)
                    return CacheCollider.bounds;
                if (CacheCollider2D)
                    return CacheCollider2D.bounds;
                return new Bounds(transform.position, Vector3.one);
            }
        }

        protected bool isSetup;
        protected Vector3 defaultLocalPosition;
        protected Quaternion defaultLocalRotation;
        protected List<TransformHistory> histories = new List<TransformHistory>();

#if UNITY_EDITOR
        [Header("Rewind Debugging")]
        public Color debugHistoryColor = new Color(0, 1, 0, 0.25f);
        public Color debugRewindColor = new Color(0, 0, 1, 0.5f);
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
                return;
            }
            CacheCollider2D = GetComponent<Collider2D>();
            if (CacheCollider2D)
            {
                CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
                CacheRigidbody2D.gravityScale = 0;
                CacheRigidbody2D.isKinematic = true;
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
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            foreach (TransformHistory history in histories)
            {
                Gizmos.color = debugHistoryColor;
                Gizmos.DrawWireCube(history.Bounds.center, history.Bounds.size);
            }
            Gizmos.matrix = oldGizmosMatrix;
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

        public TransformHistory GetTransformHistory(long currentTime, long rewindTime)
        {
            if (histories.Count == 0)
            {
                return new TransformHistory()
                {
                    Time = currentTime,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Bounds = Bounds,
                };
            }
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
                    // No stored history, so use current value
                    afterRewind = new TransformHistory()
                    {
                        Time = currentTime,
                        Position = transform.position,
                        Rotation = transform.rotation,
                        Bounds = Bounds,
                    };
                }
            }
            long durationToRewindTime = rewindTime - beforeRewind.Time;
            long durationBetweenRewindTime = afterRewind.Time - beforeRewind.Time;
            float lerpProgress = (float)durationToRewindTime / (float)durationBetweenRewindTime;
            return new TransformHistory()
            {
                Time = rewindTime,
                Position = Vector3.Lerp(beforeRewind.Position, afterRewind.Position, lerpProgress),
                Rotation = Quaternion.Slerp(beforeRewind.Rotation, afterRewind.Rotation, lerpProgress),
                Bounds = new Bounds(Vector3.Lerp(beforeRewind.Bounds.center, afterRewind.Bounds.center, lerpProgress), Vector3.Lerp(beforeRewind.Bounds.size, afterRewind.Bounds.size, lerpProgress)),
            };
        }

        public void Rewind(long currentTime, long rewindTime)
        {
            TransformHistory transformHistory = GetTransformHistory(currentTime, rewindTime);
            transform.position = transformHistory.Position;
            transform.rotation = transformHistory.Rotation;
        }

        public void Restore()
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
                Bounds = Bounds,
            });
        }

        public float GetActivatableDistance()
        {
            if (ActivatePressActivatableEntity != null)
                return ActivatePressActivatableEntity.GetActivatableDistance();
            if (ClickActivatableEntity != null)
                return ClickActivatableEntity.GetActivatableDistance();
            if (HoldClickActivatableEntity != null)
                return HoldClickActivatableEntity.GetActivatableDistance();
            return 0f;
        }

        public bool ShouldBeAttackTarget()
        {
            if (ActivatePressActivatableEntity != null)
                return ActivatePressActivatableEntity.ShouldBeAttackTarget();
            if (ClickActivatableEntity != null)
                return ClickActivatableEntity.ShouldBeAttackTarget();
            if (HoldClickActivatableEntity != null)
                return HoldClickActivatableEntity.ShouldBeAttackTarget();
            return true;
        }

        public bool CanKeyPressActivate()
        {
            if (ActivatePressActivatableEntity != null)
                return ActivatePressActivatableEntity.CanKeyPressActivate();
            return false;
        }

        public void OnKeyPressActivate()
        {
            if (ActivatePressActivatableEntity != null)
                ActivatePressActivatableEntity.OnKeyPressActivate();
        }

        public bool CanClickActivate()
        {
            if (ClickActivatableEntity != null)
                return ClickActivatableEntity.CanClickActivate();
            return false;
        }

        public void OnClickActivate()
        {
            if (ClickActivatableEntity != null)
                ClickActivatableEntity.OnClickActivate();
        }

        public bool CanHoldClickActivate()
        {
            if (HoldClickActivatableEntity != null)
                return HoldClickActivatableEntity.CanHoldClickActivate();
            return false;
        }

        public void OnHoldClickActivate()
        {
            if (HoldClickActivatableEntity != null)
                HoldClickActivatableEntity.OnHoldClickActivate();
        }
    }
}
