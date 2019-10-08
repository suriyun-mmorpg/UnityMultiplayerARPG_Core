using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class AreaBuffEntity : BaseBuffEntity
    {
        public UnityEvent onDestroy;

        private float applyDuration;
        private float lastAppliedTime;
        private readonly Dictionary<uint, BaseCharacterEntity> receivingBuffCharacters = new Dictionary<uint, BaseCharacterEntity>();

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
                foreach (BaseCharacterEntity entity in receivingBuffCharacters.Values)
                {
                    if (entity == null)
                        continue;

                    ApplyBuffTo(entity);
                }
            }
        }

        public void Setup(
            BaseCharacterEntity buffApplier,
            BaseSkill skill,
            short skillLevel,
            float areaDuration,
            float applyDuration)
        {
            base.Setup(buffApplier, skill, skillLevel);
            Destroy(gameObject, areaDuration);
            this.applyDuration = applyDuration;
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
            BaseCharacterEntity target = other.GetComponent<BaseCharacterEntity>();
            if (target == null)
                return;

            if (receivingBuffCharacters.ContainsKey(target.ObjectId))
                return;

            receivingBuffCharacters.Add(target.ObjectId, target);
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
            BaseCharacterEntity target = other.GetComponent<BaseCharacterEntity>();
            if (target == null)
                return;

            if (!receivingBuffCharacters.ContainsKey(target.ObjectId))
                return;

            receivingBuffCharacters.Remove(target.ObjectId);
        }
    }
}
