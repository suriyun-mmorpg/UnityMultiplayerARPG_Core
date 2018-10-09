using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public sealed class WarpPortalEntity : BaseGameEntity
    {
        [Tooltip("Signal to tell players that their character can warp")]
        public GameObject[] warpSignals;
        public bool warpImmediatelyWhenEnter;
        public UnityScene mapScene;
        public Vector3 position;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            foreach (var warpSignal in warpSignals)
            {
                if (warpSignal != null)
                    warpSignal.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            // Improve performance by tags
            if (!other.CompareTag(GameInstance.Singleton.playerTag))
                return;

            var playerCharacterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (warpImmediatelyWhenEnter && IsServer)
                EnterWarp(playerCharacterEntity);

            if (!warpImmediatelyWhenEnter)
            {
                playerCharacterEntity.warpingPortal = this;

                if (playerCharacterEntity == BasePlayerCharacterController.OwningCharacter)
                {
                    foreach (var warpSignal in warpSignals)
                    {
                        if (warpSignal != null)
                            warpSignal.SetActive(true);
                    }
                }
            }
        }

        private void TriggerExit(GameObject other)
        {
            // Improve performance by tags
            if (!other.CompareTag(GameInstance.Singleton.playerTag))
                return;

            var playerCharacterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (playerCharacterEntity == BasePlayerCharacterController.OwningCharacter)
            {
                playerCharacterEntity.warpingPortal = null;

                foreach (var warpSignal in warpSignals)
                {
                    if (warpSignal != null)
                        warpSignal.SetActive(false);
                }
            }
        }

        public void EnterWarp(BasePlayerCharacterEntity playerCharacterEntity)
        {
            GameManager.WarpCharacter(playerCharacterEntity, mapScene, position);
        }
    }
}
