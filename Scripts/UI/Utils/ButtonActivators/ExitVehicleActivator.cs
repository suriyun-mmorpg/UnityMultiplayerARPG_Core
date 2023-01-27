using UnityEngine;

namespace MultiplayerARPG
{
    public class ExitVehicleActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            bool canExitVehicle = GameInstance.PlayingCharacterEntity != null && GameInstance.PlayingCharacterEntity.PassengingVehicleEntity != null;
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canExitVehicle);
            }
        }
    }
}
