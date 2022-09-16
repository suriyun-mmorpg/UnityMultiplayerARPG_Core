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
                if (controller.ItemDropEntityDetector.pickupActivatableEntities.Count > 0)
                {
                    IPickupActivatableEntity pickupActivatable;
                    for (int i = 0; i < controller.ItemDropEntityDetector.pickupActivatableEntities.Count; ++i)
                    {
                        pickupActivatable = controller.ItemDropEntityDetector.pickupActivatableEntities[i];
                        if (pickupActivatable.CanPickupActivate())
                        {
                            canActivate = true;
                            break;
                        }
                    }
                }
            }

            if (shooterController != null && shooterController.SelectedGameEntity != null)
            {
                canActivate = shooterController.SelectedGameEntity is ItemDropEntity;
            }

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canActivate);
            }
        }
    }
}
