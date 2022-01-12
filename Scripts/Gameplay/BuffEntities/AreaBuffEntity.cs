using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibIdentity))]
    public partial class AreaBuffEntity : BaseBuffEntity
    {
        public UnityEvent onDestroy;

        private LiteNetLibIdentity identity;
        public LiteNetLibIdentity Identity
        {
            get
            {
                if (identity == null)
                    identity = GetComponent<LiteNetLibIdentity>();
                return identity;
            }
        }

        protected float applyDuration;
        protected float lastAppliedTime;
        protected readonly Dictionary<uint, BaseCharacterEntity> receivingBuffCharacters = new Dictionary<uint, BaseCharacterEntity>();

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = PhysicLayers.IgnoreRaycast;
            Identity.onGetInstance.AddListener(OnGetInstance);
        }

        protected virtual void OnDestroy()
        {
            Identity.onGetInstance.RemoveListener(OnGetInstance);
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="buffApplier"></param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="areaDuration"></param>
        /// <param name="applyDuration"></param>
        public virtual void Setup(
            EntityInfo buffApplier,
            BaseSkill skill,
            short skillLevel,
            float areaDuration,
            float applyDuration)
        {
            base.Setup(buffApplier, skill, skillLevel);
            PushBack(areaDuration);
            this.applyDuration = applyDuration;
            lastAppliedTime = Time.unscaledTime;
        }

        protected virtual void Update()
        {
            if (Time.unscaledTime - lastAppliedTime >= applyDuration)
            {
                lastAppliedTime = Time.unscaledTime;
                foreach (BaseCharacterEntity entity in receivingBuffCharacters.Values)
                {
                    if (entity == null)
                        continue;

                    ApplyBuffTo(entity);
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        protected virtual void TriggerEnter(GameObject other)
        {
            BaseCharacterEntity target = other.GetComponent<BaseCharacterEntity>();
            if (target == null)
                return;

            if (receivingBuffCharacters.ContainsKey(target.ObjectId))
                return;

            receivingBuffCharacters.Add(target.ObjectId, target);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        protected virtual void TriggerExit(GameObject other)
        {
            BaseCharacterEntity target = other.GetComponent<BaseCharacterEntity>();
            if (target == null)
                return;

            if (!receivingBuffCharacters.ContainsKey(target.ObjectId))
                return;

            receivingBuffCharacters.Remove(target.ObjectId);
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Base Bufff Entity is null, this should not happens");
                return;
            }
            FxCollection.InitPrefab();
            Identity.PoolingSize = PoolSize;
        }

        protected override void PushBack()
        {
            OnPushBack();
            Identity.NetworkDestroy();
        }
    }
}
