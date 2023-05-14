using UnityEngine;
using System.Collections.Generic;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class DamageableHitBox : MonoBehaviour, IDamageableEntity, IBaseActivatableEntity, IActivatableEntity, IHoldActivatableEntity
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
        public Transform EntityTransform
        {
            get { return DamageableEntity.EntityTransform; }
        }
        public GameObject EntityGameObject
        {
            get { return DamageableEntity.EntityGameObject; }
        }
        public IBaseActivatableEntity BaseActivatableEntity
        {
            get
            {
                if (DamageableEntity is IBaseActivatableEntity)
                    return DamageableEntity as IBaseActivatableEntity;
                return null;
            }
        }
        public IActivatableEntity ActivatableEntity
        {
            get
            {
                if (DamageableEntity is IActivatableEntity)
                    return DamageableEntity as IActivatableEntity;
                return null;
            }
        }
        public IHoldActivatableEntity HoldActivatableEntity
        {
            get
            {
                if (DamageableEntity is IHoldActivatableEntity)
                    return DamageableEntity as IHoldActivatableEntity;
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
        public Transform CacheTransform { get; private set; }
        public Collider CacheCollider { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public Collider2D CacheCollider2D { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }
        public byte Index { get; private set; }
        public Bounds Bounds
        {
            get
            {
                return new Bounds()
                {
                    center = CacheTransform.position + (Quaternion.LookRotation(CacheTransform.forward, CacheTransform.up) * _boundsOffset),
                    size = new Vector3(
                        CacheTransform.lossyScale.x * _boundsSize.x,
                        CacheTransform.lossyScale.y * _boundsSize.y,
                        CacheTransform.lossyScale.z * _boundsSize.z),
                };
            }
        }

        protected bool _isSetup;
        protected Vector3 _defaultLocalPosition;
        protected Quaternion _defaultLocalRotation;
        protected List<TransformHistory> _histories = new List<TransformHistory>();
        protected Vector3 _boundsOffset;
        protected Vector3 _boundsSize;

#if UNITY_EDITOR
        [Header("Rewind Debugging")]
        public Color debugHistoryColor = new Color(0, 1, 0, 0.25f);
        public Color debugRewindColor = new Color(0, 0, 1, 0.5f);
#endif

        private void Awake()
        {
            DamageableEntity = GetComponentInParent<DamageableEntity>();
            CacheTransform = transform;
            CacheCollider = GetComponent<Collider>();
            if (CacheCollider != null)
            {
                if (CacheCollider is BoxCollider boxCollider)
                {
                    _boundsOffset = boxCollider.center;
                    _boundsSize = boxCollider.size;
                }
                else if (CacheCollider is SphereCollider sphereCollider)
                {
                    _boundsOffset = sphereCollider.center;
                    _boundsSize = sphereCollider.radius * Vector3.one * 2f;
                }
                else if (CacheCollider is CapsuleCollider capsuleCollider)
                {
                    _boundsOffset = capsuleCollider.center;
                    _boundsSize = capsuleCollider.radius * Vector3.one * 2f;
                    switch (capsuleCollider.direction)
                    {
                        case 0:
                            // X
                            _boundsSize = new Vector3(capsuleCollider.height, _boundsSize.y, _boundsSize.z);
                            break;

                        case 2:
                            // Z
                            _boundsSize = new Vector3(_boundsSize.x, _boundsSize.y, capsuleCollider.height);
                            break;
                        default:
                            // Y
                            _boundsSize = new Vector3(_boundsSize.x, capsuleCollider.height, _boundsSize.z);
                            break;
                    }
                }
                else
                {
                    Logging.LogError(ToString(), "Only `BoxCollider`, `SphereCollider` and `CapsuleCollider` can be used for damageable hit box (3D games)");
                    return;
                }
                CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
                CacheRigidbody.useGravity = false;
                CacheRigidbody.isKinematic = true;
                return;
            }
            CacheCollider2D = GetComponent<Collider2D>();
            if (CacheCollider2D != null)
            {
                if (CacheCollider2D is BoxCollider2D boxCollider2D)
                {
                    _boundsOffset = boxCollider2D.offset;
                    _boundsSize = boxCollider2D.size;
                }
                else if (CacheCollider2D is CircleCollider2D circleCollider2D)
                {
                    _boundsOffset = circleCollider2D.offset;
                    _boundsSize = circleCollider2D.radius * Vector3.one * 2f;
                }
                else if (CacheCollider2D is CapsuleCollider2D capsuleCollider2D)
                {
                    _boundsOffset = capsuleCollider2D.offset;
                    switch (capsuleCollider2D.direction)
                    {
                        case CapsuleDirection2D.Vertical:
                            if (capsuleCollider2D.size.x >= capsuleCollider2D.size.y)
                                _boundsSize = capsuleCollider2D.size.x * Vector3.one;
                            else
                                _boundsSize = new Vector3(capsuleCollider2D.size.x, capsuleCollider2D.size.y);
                            break;
                        case CapsuleDirection2D.Horizontal:
                            if (capsuleCollider2D.size.y >= capsuleCollider2D.size.x)
                                _boundsSize = capsuleCollider2D.size.y * Vector3.one;
                            else
                                _boundsSize = new Vector3(capsuleCollider2D.size.y, capsuleCollider2D.size.x);
                            break;
                    }
                }
                else
                {
                    Logging.LogError(ToString(), "Only `BoxCollider2D`, `CircleCollider2D` and `CapsuleCollider2D` can be used for damageable hit box (2D games)");
                    return;
                }
                CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
                CacheRigidbody2D.gravityScale = 0;
                CacheRigidbody2D.isKinematic = true;
            }
        }

        public virtual void Setup(byte index)
        {
            _isSetup = true;
            gameObject.tag = DamageableEntity.gameObject.tag;
            gameObject.layer = DamageableEntity.gameObject.layer;
            _defaultLocalPosition = transform.localPosition;
            _defaultLocalRotation = transform.localRotation;
            Index = index;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            foreach (TransformHistory history in _histories)
            {
                Gizmos.color = debugHistoryColor;
                Gizmos.matrix = Matrix4x4.TRS(history.Bounds.center, history.Rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, history.Bounds.size);
            }
            Gizmos.matrix = oldGizmosMatrix;
            Handles.Label(transform.position, name + "(HitBox)");
        }
#endif

        public virtual bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            return DamageableEntity.CanReceiveDamageFrom(instigator);
        }

        public virtual void ReceiveDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed)
        {
            if (!DamageableEntity.IsServer || this.IsDead() || !CanReceiveDamageFrom(instigator))
                return;
            ReceiveDamageWithoutConditionCheck(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);
        }

        public virtual void ReceiveDamageWithoutConditionCheck(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed)
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

        public bool IsHide()
        {
            return DamageableEntity.IsHide();
        }

        public TransformHistory GetTransformHistory(long currentTime, long rewindTime)
        {
            if (_histories.Count == 0)
            {
                return new TransformHistory()
                {
                    Time = currentTime,
                    Position = CacheTransform.position,
                    Rotation = CacheTransform.rotation,
                    Bounds = Bounds,
                };
            }
            TransformHistory beforeRewind = default;
            TransformHistory afterRewind = default;
            for (int i = 0; i < _histories.Count; ++i)
            {
                if (beforeRewind.Time > 0 && beforeRewind.Time <= rewindTime && _histories[i].Time >= rewindTime)
                {
                    afterRewind = _histories[i];
                    break;
                }
                else
                {
                    beforeRewind = _histories[i];
                }
                if (_histories.Count - 1 == i)
                {
                    // No stored history, so use current value
                    afterRewind = new TransformHistory()
                    {
                        Time = currentTime,
                        Position = CacheTransform.position,
                        Rotation = CacheTransform.rotation,
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
            CacheTransform.position = transformHistory.Position;
            CacheTransform.rotation = transformHistory.Rotation;
        }

        public void Restore()
        {
            CacheTransform.localPosition = _defaultLocalPosition;
            CacheTransform.localRotation = _defaultLocalRotation;
        }

        public void AddTransformHistory(long time)
        {
            if (_histories.Count == BaseGameNetworkManager.Singleton.LagCompensationManager.MaxHistorySize)
                _histories.RemoveAt(0);
            _histories.Add(new TransformHistory()
            {
                Time = time,
                Position = CacheTransform.position,
                Rotation = CacheTransform.rotation,
                Bounds = Bounds,
            });
        }

        public bool SetAsTargetInOneClick()
        {
            if (BaseActivatableEntity != null)
                return BaseActivatableEntity.SetAsTargetInOneClick();
            return false;
        }

        public bool NotBeingSelectedOnClick()
        {
            if (BaseActivatableEntity != null)
                return BaseActivatableEntity.NotBeingSelectedOnClick();
            return false;
        }

        public float GetActivatableDistance()
        {
            if (BaseActivatableEntity != null)
                return BaseActivatableEntity.GetActivatableDistance();
            return 0f;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            if (BaseActivatableEntity != null)
                return BaseActivatableEntity.ShouldClearTargetAfterActivated();
            return false;
        }

        public bool ShouldBeAttackTarget()
        {
            if (ActivatableEntity != null)
                return ActivatableEntity.ShouldBeAttackTarget();
            return true;
        }

        public virtual bool ShouldNotActivateAfterFollowed()
        {
            if (ActivatableEntity != null)
                return ActivatableEntity.ShouldNotActivateAfterFollowed();
            return false;
        }

        public bool CanActivate()
        {
            if (ActivatableEntity != null)
                return ActivatableEntity.CanActivate();
            return false;
        }

        public void OnActivate()
        {
            if (ActivatableEntity != null)
                ActivatableEntity.OnActivate();
        }

        public bool CanHoldActivate()
        {
            if (HoldActivatableEntity != null)
                return HoldActivatableEntity.CanHoldActivate();
            return false;
        }

        public void OnHoldActivate()
        {
            if (HoldActivatableEntity != null)
                HoldActivatableEntity.OnHoldActivate();
        }
    }
}
