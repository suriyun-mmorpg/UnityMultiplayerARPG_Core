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
            canExitVehicle = GameInstance.PlayingCharacterEntity != null &&
                GameInstance.PlayingCharacterEntity.PassengingVehicleEntity != null;

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canExitVehicle);
            }
        }
    }
}
