using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ZoomButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private ShooterPlayerCharacterController controller;
        private bool canZoom;

        private void LateUpdate()
        {
            if (BasePlayerCharacterController.Singleton != null && controller == null)
                controller = BasePlayerCharacterController.Singleton as ShooterPlayerCharacterController;
            canZoom = controller != null && controller.weaponAbility != null &&
                controller.weaponAbility is ZoomWeaponAbility;

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canZoom);
            }
        }
    }
}
