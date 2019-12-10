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

        private void Awake()
        {
            gameObject.layer = 2;   // Ignore raycast
        }

        private void Start()
        {
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
            Destroy(gameObject, areaDuration);
            this.applyDuration = applyDuration;
        }

        private void OnDestroy()
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
            if (attacker != null && attacker.Entity.gameObject == other)
                return;

            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            if (target == null)
                return;

            if (receivingDamageEntities.ContainsKey(target.ObjectId))
                return;

            receivingDamageEntities.Add(target.ObjectId, target);
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
            if (attacker != null && attacker.Entity.gameObject == other)
                return;

            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            if (target == null)
                return;

            if (!receivingDamageEntities.ContainsKey(target.ObjectId))
                return;

            receivingDamageEntities.Remove(target.ObjectId);
        }
    }
}
