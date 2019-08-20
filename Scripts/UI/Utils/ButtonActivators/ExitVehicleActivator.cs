using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ExitVehicleActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private bool canExitVehicle;

        private void LateUpdate()
        {
            canExitVehicle = BasePlayerCharacterController.OwningCharacter != null &&
                BasePlayerCharacterController.OwningCharacter.PassengingVehicleEntity != null;

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canExitVehicle);
            }
        }
    }
}
