using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIEquipWeaponsManager : MonoBehaviour
    {
        [Tooltip("Index of this array is equip weapons set")]
        public ActivatingGameObjects[] activatingGameObjects;

        private byte dirtyEquipWeaponSet;
        private bool updatingActivatingGameObjects;

        private void Awake()
        {
            for (int i = 0; i < activatingGameObjects.Length; ++i)
            {
                if (activatingGameObjects[i].toggle == null)
                    continue;
                int j = i;
                activatingGameObjects[i].toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (updatingActivatingGameObjects)
                        return;

                    if (isOn)
                    {
                        OnClickSwitchEquipWeaponSet(activatingGameObjects[j].equipWeaponSet);
                    }
                });
            }
        }

        private void Start()
        {
            updatingActivatingGameObjects = false;
            dirtyEquipWeaponSet = BasePlayerCharacterController.OwningCharacter.EquipWeaponSet;
            UpdateActivatingGameObjects();
        }

        private void LateUpdate()
        {
            if (BasePlayerCharacterController.OwningCharacter == null)
                return;

            if (dirtyEquipWeaponSet != BasePlayerCharacterController.OwningCharacter.EquipWeaponSet)
            {
                dirtyEquipWeaponSet = BasePlayerCharacterController.OwningCharacter.EquipWeaponSet;
                UpdateActivatingGameObjects();
            }
        }

        private void UpdateActivatingGameObjects()
        {
            updatingActivatingGameObjects = true;
            for (int i = 0; i < activatingGameObjects.Length; ++i)
            {
                if (activatingGameObjects[i].toggle != null)
                    activatingGameObjects[i].toggle.isOn = activatingGameObjects[i].equipWeaponSet == BasePlayerCharacterController.OwningCharacter.EquipWeaponSet;

                for (int j = 0; j < activatingGameObjects[i].gameObjects.Length; ++j)
                {
                    activatingGameObjects[i].gameObjects[j].SetActive(activatingGameObjects[i].equipWeaponSet == BasePlayerCharacterController.OwningCharacter.EquipWeaponSet);
                }
            }
            updatingActivatingGameObjects = false;
        }

        public void OnClickSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            BasePlayerCharacterController.OwningCharacter.RequestSwitchEquipWeaponSet(equipWeaponSet);
        }

        [System.Serializable]
        public struct ActivatingGameObjects
        {
            [Range(0, 15)]
            public byte equipWeaponSet;
            public Toggle toggle;
            public GameObject[] gameObjects;
        }
    }
}
