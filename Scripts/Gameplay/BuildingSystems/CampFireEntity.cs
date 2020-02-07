using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class CampFireEntity : StorageEntity
    {
        [Header("Campfire data")]
        public UnityEvent onInitialTurnOn;
        public UnityEvent onInitialTurnOff;
        public UnityEvent onTurnOn;
        public UnityEvent onTurnOff;
        [SerializeField]
        protected SyncFieldBool isTurnOn = new SyncFieldBool();

        public override void OnSetup()
        {
            base.OnSetup();
            isTurnOn.onChange += OnIsTurnOnChange;
            RegisterNetFunction(NetFuncTurnOn);
            RegisterNetFunction(NetFuncTurnOff);
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            isTurnOn.deliveryMethod = DeliveryMethod.ReliableOrdered;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            isTurnOn.onChange -= OnIsTurnOnChange;
        }

        private void OnIsTurnOnChange(bool isInitial, bool isTurnOn)
        {
            if (isInitial)
            {
                if (isTurnOn)
                    onInitialTurnOn.Invoke();
                else
                    onInitialTurnOff.Invoke();
            }
            else
            {
                if (isTurnOn)
                    onTurnOn.Invoke();
                else
                    onTurnOff.Invoke();
            }
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (!IsServer)
                return;
            // Consume fuel
        }

        protected void NetFuncTurnOn()
        {
            isTurnOn.Value = true;
        }

        protected void NetFuncTurnOff()
        {
            isTurnOn.Value = false;
        }

        public void RequestTurnOn()
        {
            CallNetFunction(NetFuncTurnOn, FunctionReceivers.Server);
        }

        public void RequestTurnOff()
        {
            CallNetFunction(NetFuncTurnOff, FunctionReceivers.Server);
        }
    }
}
