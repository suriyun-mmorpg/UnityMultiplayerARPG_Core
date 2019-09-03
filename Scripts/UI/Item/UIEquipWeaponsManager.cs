using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIEquipWeaponsManager : MonoBehaviour
    {
        [Tooltip("Index of this array is equip weapons set")]
        public ActivatingGameObjects[] activatingGameObjects;
        
        private void Update()
        {
            for (byte i = 0; i < activatingGameObjects.Length; ++i)
            {
                for (byte j = 0; j < activatingGameObjects[i].gameObjects.Length; ++j)
                {
                    activatingGameObjects[i].gameObjects[j].SetActive(BasePlayerCharacterController.OwningCharacter.EquipWeaponSet == i);
                }
            }
        }

        public void OnClickSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            BasePlayerCharacterController.OwningCharacter.RequestSwitchEquipWeaponSet(equipWeaponSet);
        }

        [System.Serializable]
        public struct ActivatingGameObjects
        {
            public GameObject[] gameObjects;
        }
    }
}
