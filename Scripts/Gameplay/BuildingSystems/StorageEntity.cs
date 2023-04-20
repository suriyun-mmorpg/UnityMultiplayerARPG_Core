using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class StorageEntity : BuildingEntity
    {
        [Category(6, "Storage Settings")]
        [SerializeField]
        protected Storage storage = new Storage();
        public Storage Storage { get { return storage; } }

        [SerializeField]
        protected bool lockable = false;
        public override bool Lockable { get { return lockable; } }

        [SerializeField]
        protected bool canUseByEveryone = false;
        public bool CanUseByEveryone { get { return canUseByEveryone; } }

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

        private bool _dirtyIsOpen;

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

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (IsServer)
            {
                bool updatingIsOpen = GameInstance.ServerStorageHandlers.IsStorageEntityOpen(this);
                if (updatingIsOpen != _dirtyIsOpen)
                {
                    _dirtyIsOpen = updatingIsOpen;
                    isOpen.Value = updatingIsOpen;
                }
            }
        }

        public override bool CanActivate()
        {
            return !this.IsDead();
        }

        public override void OnActivate()
        {
            if (!Lockable || !IsLocked)
            {
                GameInstance.PlayingCharacterEntity.Building.CallServerOpenStorage(ObjectId, string.Empty);
            }
            else
            {
                UISceneGlobal.Singleton.ShowPasswordDialog(
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                    (password) =>
                    {
                        GameInstance.PlayingCharacterEntity.Building.CallServerOpenStorage(ObjectId, password);
                    }, string.Empty, PasswordContentType, PasswordLength,
                    LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()));
            }
        }
    }
}
