using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ZoomButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private IWeaponAbilityController controller;
        private bool canZoom;

        private void LateUpdate()
        {
            if (BasePlayerCharacterController.Singleton != null && controller == null)
                controller = BasePlayerCharacterController.Singleton as IWeaponAbilityController;
            canZoom = controller != null && controller.WeaponAbility != null &&
                controller.WeaponAbility is ZoomWeaponAbility;

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canZoom);
            }
        }
    }
}
