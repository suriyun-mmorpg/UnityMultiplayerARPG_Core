using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICurrentBuilding : UIBase
    {
        public BasePlayerCharacterController Controller { get { return BasePlayerCharacterController.Singleton; } }

        public Button buttonDestroy;
        public Button buttonSetPassword;
        public Button buttonLock;
        public Button buttonUnlock;

        public override void Show()
        {
            base.Show();
            if (buttonDestroy != null)
            {
                buttonDestroy.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayerCharacterEntity);
            }
            if (buttonSetPassword != null)
            {
                buttonSetPassword.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayerCharacterEntity);
            }
            if (buttonLock != null)
            {
                buttonLock.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayerCharacterEntity);
            }
            if (buttonUnlock != null)
            {
                buttonUnlock.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayerCharacterEntity);
            }
        }

        public void OnClickDeselect()
        {
            Controller.DeselectBuilding();
            Hide();
        }

        public void OnClickDestroy()
        {
            Controller.DestroyBuilding();
            Hide();
        }

        public void OnClickSetPassword()
        {
            Controller.SetBuildingPassword();
            Hide();
        }

        public void OnClickLock()
        {
            Controller.LockBuilding();
            Hide();
        }

        public void OnClickUnlock()
        {
            Controller.UnlockBuilding();
            Hide();
        }
    }
}
