using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class AreaBuffEntity : BaseBuffEntity
    {
        public UnityEvent onDestroy;

        protected float applyDuration;
        protected float lastAppliedTime;
        protected readonly Dictionary<uint, BaseCharacterEntity> receivingBuffCharacters = new Dictionary<uint, BaseCharacterEntity>();

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        protected virtual void Start()
        {
            lastAppliedTime = Time.unscaledTime;
        }

        public virtual void Setup(
            BaseCharacterEntity buffApplier,
            BaseSkill skill,
            short skillLevel,
            float areaDuration,
            float applyDuration)
        {
            base.Setup(buffApplier, skill, skillLevel);
            PushBack(areaDuration);
            this.applyDuration = applyDuration;
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
    }
}
