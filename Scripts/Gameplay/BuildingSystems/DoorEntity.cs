using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class DoorEntity : BuildingEntity
    {
        [Header("Door data")]
        public UnityEvent onInitialOpen;
        public UnityEvent onInitialClose;
        public UnityEvent onOpen;
        public UnityEvent onClose;
        [SerializeField]
        protected SyncFieldBool isOpen = new SyncFieldBool();
        public override bool Activatable { get { return true; } }

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
    }
}
