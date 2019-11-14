using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PickUpButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private bool canActivate;
        private PlayerCharacterController controller;
        private ShooterPlayerCharacterController shooterController;

        private void LateUpdate()
        {
            canActivate = false;

            controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            shooterController = BasePlayerCharacterController.Singleton as ShooterPlayerCharacterController;

            if (controller != null)
            {
                canActivate = controller.ItemDropEntityDetector.itemDrops.Count > 0;
            }

            if (shooterController != null && shooterController.SelectedEntity != null)
            {
                canActivate = shooterController.SelectedEntity is ItemDropEntity;
            }

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canActivate);
            }
        }
    }
}
