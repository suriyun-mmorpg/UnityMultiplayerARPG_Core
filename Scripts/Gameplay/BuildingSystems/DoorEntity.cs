using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class DoorEntity : BuildingEntity
    {
        [Header("Door data")]
        public UnityEvent onOpen;
        public UnityEvent onClose;
        [SerializeField]
        private SyncFieldBool isOpen = new SyncFieldBool();

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

        private void OnIsOpenChange(bool isOpen)
        {
            if (isOpen)
                onOpen.Invoke();
            else
                onClose.Invoke();
        }
    }
}
