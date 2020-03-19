using UnityEngine;

namespace MultiplayerARPG
{
    public class WarpPortalEntity : BaseGameEntity, IUnHittable
    {
        [Tooltip("Signal to tell players that their character can enter the portal")]
        public GameObject[] warpSignals;
        public bool warpImmediatelyWhenEnter;
        public WarpPortalType type;
        [Tooltip("Leave this empty to warp character to other position in the same map")]
        public BaseMapInfo mapInfo;
        public Vector3 position;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            foreach (GameObject warpSignal in warpSignals)
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

            BasePlayerCharacterEntity playerCharacterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (warpImmediatelyWhenEnter && IsServer)
                EnterWarp(playerCharacterEntity);

            if (!warpImmediatelyWhenEnter)
            {
                playerCharacterEntity.WarpingPortal = this;

                if (playerCharacterEntity == BasePlayerCharacterController.OwningCharacter)
                {
                    foreach (GameObject warpSignal in warpSignals)
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

            BasePlayerCharacterEntity playerCharacterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (playerCharacterEntity == BasePlayerCharacterController.OwningCharacter)
            {
                playerCharacterEntity.WarpingPortal = null;

                foreach (GameObject warpSignal in warpSignals)
                {
                    if (warpSignal != null)
                        warpSignal.SetActive(false);
                }
            }
        }

        public void EnterWarp(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (mapInfo == null)
                CurrentGameManager.WarpCharacter(type, playerCharacterEntity, string.Empty, position);
            else
                CurrentGameManager.WarpCharacter(type, playerCharacterEntity, mapInfo.Id, position);
        }
    }
}
