using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class StorageEntity : BuildingEntity
    {
        [Header("Storage data")]
        public Storage storage;
        public UnityEvent onInitialOpen;
        public UnityEvent onInitialClose;
        public UnityEvent onOpen;
        public UnityEvent onClose;
        [SerializeField]
        private SyncFieldBool isOpen = new SyncFieldBool();
        private bool dirtyIsOpen;

        public override void OnSetup()
        {
            base.OnSetup();
            isOpen.onChange += OnIsOpenChange;
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
            bool updatingIsOpen = gameManager.IsStorageEntityOpen(this);
            if (updatingIsOpen != dirtyIsOpen)
            {
                dirtyIsOpen = updatingIsOpen;
                isOpen.Value = updatingIsOpen;
            }
        }
    }
}
