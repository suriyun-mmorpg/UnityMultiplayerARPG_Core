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

        private BuildingEntity buildingEntity;

        public void Show(BuildingEntity buildingEntity)
        {
            if (buildingEntity == null)
            {
                // Don't show
                return;
            }
            this.buildingEntity = buildingEntity;
            base.Show();
        }

        protected virtual void OnEnable()
        {
            if (textTitle != null)
                textTitle.text = buildingEntity.Title;
            if (lockableObjects != null && lockableObjects.Length > 0)
            {
                foreach (GameObject lockableObject in lockableObjects)
                {
                    lockableObject.SetActive(buildingEntity.Lockable);
                }
            }
            if (lockedObjects != null && lockedObjects.Length > 0)
            {
                foreach (GameObject lockedObject in lockedObjects)
                {
                    lockedObject.SetActive(buildingEntity.IsLocked);
                }
            }
            if (unlockedObjects != null && unlockedObjects.Length > 0)
            {
                foreach (GameObject unlockedObject in unlockedObjects)
                {
                    unlockedObject.SetActive(!buildingEntity.IsLocked);
                }
            }
            if (buttonDestroy != null)
                buttonDestroy.interactable = buildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            if (buttonSetPassword != null)
                buttonSetPassword.interactable = buildingEntity.Lockable && buildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            if (buttonLock != null)
                buttonLock.interactable = buildingEntity.Lockable && buildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            if (buttonUnlock != null)
                buttonUnlock.interactable = buildingEntity.Lockable && buildingEntity.IsCreator(Controller.PlayingCharacterEntity);
            if (buttonActivate != null)
                buttonActivate.interactable = buildingEntity.CanActivate();
        }

        private void Update()
        {
            if (IsVisible() && (buildingEntity == null || buildingEntity.IsDead()))
                Hide();
        }

        public void OnClickDeselect()
        {
            Controller.DeselectBuilding();
            Hide();
        }

        public void OnClickDestroy()
        {
            Controller.DestroyBuilding(buildingEntity);
            Hide();
        }

        public void OnClickSetPassword()
        {
            Controller.SetBuildingPassword(buildingEntity);
            Hide();
        }

        public void OnClickLock()
        {
            Controller.LockBuilding(buildingEntity);
            Hide();
        }

        public void OnClickUnlock()
        {
            Controller.UnlockBuilding(buildingEntity);
            Hide();
        }

        public void OnClickActivate()
        {
            Controller.ActivateBuilding(buildingEntity);
            Hide();
        }
    }
}
