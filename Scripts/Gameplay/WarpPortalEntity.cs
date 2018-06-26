using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public sealed class WarpPortalEntity : RpgNetworkEntity
    {
        [Tooltip("Signal to tell players that their character can warp")]
        public GameObject[] warpSignals;
        public bool warpImmediatelyWhenEnter;
        public UnityScene mapScene;
        public Vector3 position;

        protected override void Awake()
        {
            base.Awake();
            foreach (var warpSignal in warpSignals)
            {
                if (warpSignal != null)
                    warpSignal.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var playerCharacterEntity = other.GetComponent<PlayerCharacterEntity>();
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

        private void OnTriggerExit(Collider other)
        {
            var playerCharacterEntity = other.GetComponent<PlayerCharacterEntity>();
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

        public void EnterWarp(PlayerCharacterEntity playerCharacterEntity)
        {
            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
                manager.WarpCharacter(playerCharacterEntity, mapScene, position);
        }
    }
}
