using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class AreaDamageEntity : BaseDamageEntity
    {
        public UnityEvent onDestroy;

        private float applyDuration;
        private float lastAppliedTime;
        private readonly Dictionary<uint, IDamageableEntity> receivingDamageEntities = new Dictionary<uint, IDamageableEntity>();

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        public void Setup(
            IGameEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            float areaDuration,
            float applyDuration)
        {
            base.Setup(attacker, weapon, damageAmounts, skill, skillLevel);
            PushBack(areaDuration);
            this.applyDuration = applyDuration;
            lastAppliedTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime - lastAppliedTime >= applyDuration)
            {
                lastAppliedTime = Time.unscaledTime;
                foreach (IDamageableEntity entity in receivingDamageEntities.Values)
                {
                    if (entity == null)
                        continue;

                    ApplyDamageTo(entity);
                }
            }
        }

        protected override void OnPushBack()
        {
            if (onDestroy != null)
                onDestroy.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            if (attacker != null && attacker.GetGameObject() == other)
                return;

            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            if (target == null)
                return;

            if (receivingDamageEntities.ContainsKey(target.GetObjectId()))
                return;

            receivingDamageEntities.Add(target.GetObjectId(), target);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerExit(GameObject other)
        {
            if (attacker != null && attacker.GetGameObject() == other)
                return;

            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            if (target == null)
                return;

            if (!receivingDamageEntities.ContainsKey(target.GetObjectId()))
                return;

            receivingDamageEntities.Remove(target.GetObjectId());
        }
    }
}
