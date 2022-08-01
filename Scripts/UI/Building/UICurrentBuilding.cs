using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICurrentBuilding : UIBase
    {
        public BasePlayerCharacterController Controller { get { return BasePlayerCharacterController.Singleton; } }
        public TextWrapper textTitle;
        [Tooltip("These game objects will be activate if target building entity's `locakable` = `TRUE`")]
        public GameObject[] lockableObjects;
        [Tooltip("These game objects will be activate if target building entity's `isLocked` = `TRUE`")]
        public GameObject[] lockedObjects;
        [Tooltip("These game objects will be activate if target building entity's `isLocked` = `FALSE`")]
        public GameObject[] unlockedObjects;
        public Button buttonDestroy;
        public Button buttonSetPassword;
        public Button buttonLock;
        public Button buttonUnlock;
        public Button buttonActivate;

        public override void Show()
        {
            if (Controller.TargetBuildingEntity == null)
            {
                // Don't show
                return;
            }
            base.Show();
        }

        protected virtual void OnEnable()
        {
            if (textTitle != null)
                textTitle.text = Controller.TargetBuildingEntity.Title;
            if (lockableObjects != null && lockableObjects.Length > 0)
            {
                foreach (GameObject lockableObject in lockableObjects)
                {
                    lockableObject.SetActive(Controller.TargetBuildingEntity.Lockable);
                }
            }
            if (lockedObjects != null && lockedObjects.Length > 0)
            {
                foreach (GameObject lockedObject in lockedObjects)
                {
                    lockedObject.SetActive(Controller.TargetBuildingEntity.IsLocked);
                }
            }
            if (unlockedObjects != null && unlockedObjects.Length > 0)
            {
                foreach (GameObject unlockedObject in unlockedObjects)
                {
                    unlockedObject.SetActive(!Controller.TargetBuildingEntity.IsLocked);
                }
            }
            if (buttonDestroy != null)
            {
                buttonDestroy.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            }
            if (buttonSetPassword != null)
            {
                buttonSetPassword.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            }
            if (buttonLock != null)
            {
                buttonLock.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            }
            if (buttonUnlock != null)
            {
                buttonUnlock.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Lockable &&
                    Controller.TargetBuildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            }
            if (buttonActivate != null)
            {
                buttonActivate.interactable = Controller.TargetBuildingEntity != null &&
                    Controller.TargetBuildingEntity.Activatable;
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

        public void OnClickActivate()
        {
            Controller.ActivateBuilding();
            Hide();
        }
    }
}
