using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class WarpPortalEntity : BaseGameEntity
    {
        [Category(5, "Warp Portal Settings")]
        [Tooltip("Signal to tell players that their character can enter the portal")]
        public GameObject[] warpSignals;
        [Tooltip("If this is `TRUE`, character will warp immediately when enter this warp portal")]
        public bool warpImmediatelyWhenEnter;
        [FormerlySerializedAs("type")]
        public WarpPortalType warpPortalType;
        [Tooltip("Map which character will warp to when use the warp portal, leave this empty to warp character to other position in the same map")]
        [FormerlySerializedAs("mapInfo")]
        public BaseMapInfo warpToMapInfo;
        [Tooltip("Position which character will warp to when use the warp portal")]
        [FormerlySerializedAs("position")]
        public Vector3 warpToPosition;
        [Tooltip("If this is `TRUE` it will change character's rotation when warp")]
        public bool warpOverrideRotation;
        [Tooltip("This will be used if `warpOverrideRotation` is `TRUE` to change character's rotation when warp")]
        public Vector3 warpToRotation;

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
                if (playerCharacterEntity == GameInstance.PlayingCharacterEntity)
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

            if (playerCharacterEntity == GameInstance.PlayingCharacterEntity)
            {
                foreach (GameObject warpSignal in warpSignals)
                {
                    if (warpSignal != null)
                        warpSignal.SetActive(false);
                }
            }
        }

        public virtual void EnterWarp(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (warpToMapInfo == null)
                CurrentGameManager.WarpCharacter(warpPortalType, playerCharacterEntity, string.Empty, warpToPosition, warpOverrideRotation, warpToRotation);
            else
                CurrentGameManager.WarpCharacter(warpPortalType, playerCharacterEntity, warpToMapInfo.Id, warpToPosition, warpOverrideRotation, warpToRotation);
        }
    }
}
