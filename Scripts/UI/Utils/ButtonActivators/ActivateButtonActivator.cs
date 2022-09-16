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
                if (controller.ActivatableEntityDetector.activatableEntities.Count > 0)
                {
                    IActivatableEntity activatable;
                    for (int i = 0; i < controller.ActivatableEntityDetector.activatableEntities.Count; ++i)
                    {
                        activatable = controller.ActivatableEntityDetector.activatableEntities[i];
                        if (activatable.CanActivate())
                        {
                            canActivate = true;
                            break;
                        }
                    }
                }
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
