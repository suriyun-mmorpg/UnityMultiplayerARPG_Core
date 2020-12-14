using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class StorageEntity : BuildingEntity
    {
        [Header("Storage Settings")]
        public Storage storage;
        public bool lockable;
        public UnityEvent onInitialOpen;
        public UnityEvent onInitialClose;
        public UnityEvent onOpen;
        public UnityEvent onClose;
        public bool canUseByEveryone;
        [SerializeField]
        protected SyncFieldBool isOpen = new SyncFieldBool();
        private bool dirtyIsOpen;
        public override bool Activatable { get { return true; } }
        public override bool Lockable { get { return lockable; } }

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
            bool updatingIsOpen = GameInstance.ServerStorageHandlers.IsStorageEntityOpen(this);
            if (updatingIsOpen != dirtyIsOpen)
            {
                dirtyIsOpen = updatingIsOpen;
                isOpen.Value = updatingIsOpen;
            }
        }
    }
}
