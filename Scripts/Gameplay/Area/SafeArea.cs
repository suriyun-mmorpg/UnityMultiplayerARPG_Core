using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class SafeArea : MonoBehaviour
    {
        private void Awake()
        {
            // Set layer to ignore raycast
            gameObject.layer = 2;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(GameInstance.Singleton.playerTag) && !other.CompareTag(GameInstance.Singleton.monsterTag))
                return;
            
            BasePlayerCharacterEntity characterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (characterEntity == null)
                return;

            characterEntity.isInSafeArea = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(GameInstance.Singleton.playerTag) && !other.CompareTag(GameInstance.Singleton.monsterTag))
                return;

            BasePlayerCharacterEntity characterEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (characterEntity == null)
                return;

            characterEntity.isInSafeArea = false;
        }
    }
}
