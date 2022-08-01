using UnityEngine;

namespace MultiplayerARPG
{
    public class ActivateButtonActivator : MonoBehaviour
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
                canActivate = controller.ActivatableEntityDetector.players.Count > 0 ||
                    controller.ActivatableEntityDetector.npcs.Count > 0 ||
                    controller.ActivatableEntityDetector.buildings.Count > 0;
            }


            if (shooterController != null && shooterController.SelectedGameEntity != null)
            {
                if ((shooterController.SelectedGameEntity is BasePlayerCharacterEntity || shooterController.SelectedGameEntity is NpcEntity) &&
                    Vector3.Distance(shooterController.SelectedGameEntity.EntityTransform.position, shooterController.PlayingCharacterEntity.EntityTransform.position) <= GameInstance.Singleton.conversationDistance)
                {
                    canActivate = true;
                }

                if (!canActivate)
                {
                    BuildingEntity buildingEntity = shooterController.SelectedGameEntity as BuildingEntity;
                    if (buildingEntity != null && !buildingEntity.IsBuildMode && buildingEntity.Activatable)
                    {
                        canActivate = true;
                    }
                }
            }

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canActivate);
            }
        }
    }
}
