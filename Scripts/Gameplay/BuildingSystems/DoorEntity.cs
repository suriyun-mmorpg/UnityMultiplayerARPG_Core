using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class DoorEntity : BuildingEntity
    {
        [Category(6, "Door Settings")]
        [SerializeField]
        protected bool lockable = false;
        public override bool Lockable { get { return lockable; } }

        [Category("Events")]
        [SerializeField]
        protected UnityEvent onInitialOpen = new UnityEvent();
        [SerializeField]
        protected UnityEvent onInitialClose = new UnityEvent();
        [SerializeField]
        protected UnityEvent onOpen = new UnityEvent();
        [SerializeField]
        protected UnityEvent onClose = new UnityEvent();

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldBool isOpen = new SyncFieldBool();

        public bool IsOpen
        {
            get { return isOpen.Value; }
            set { isOpen.Value = value; }
        }

        public override void OnSetup()
        {
            base.OnSetup();
            isOpen.onChange += OnIsOpenChange;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            isOpen.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isOpen.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            isOpen.onChange -= OnIsOpenChange;
        }

        private void OnIsOpenChange(bool isInitial, bool isOpen)
        {
            if (isInitial)
            {
                if (isOpen)
                    onInitialOpen.Invoke();
                else
                    onInitialClose.Invoke();
            }
            else
            {
                if (isOpen)
                    onOpen.Invoke();
                else
                    onClose.Invoke();
            }
        }

        public override bool CanActivate()
        {
            return !this.IsDead();
        }

        public override void OnActivate()
        {
            if (IsOpen)
            {
                GameInstance.PlayingCharacterEntity.Building.CallServerCloseDoor(ObjectId);
                return;
            }
            if (!Lockable || !IsLocked)
            {
                GameInstance.PlayingCharacterEntity.Building.CallServerOpenDoor(ObjectId, string.Empty);
            }
            else
            {
                UISceneGlobal.Singleton.ShowPasswordDialog(
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                    (password) =>
                    {
                        GameInstance.PlayingCharacterEntity.Building.CallServerOpenDoor(ObjectId, password);
                    }, string.Empty, PasswordContentType, PasswordLength, 
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()));
            }
        }
    }
}
