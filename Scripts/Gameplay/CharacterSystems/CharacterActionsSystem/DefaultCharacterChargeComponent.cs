using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class DefaultCharacterChargeComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterChargeComponent
    {
        public bool IsCharging { get; protected set; }
        public float MoveSpeedRateWhileCharging { get; protected set; }

        protected bool sendingClientStartCharge;
        protected bool sendingClientStopCharge;
        protected bool sendingServerStartCharge;
        protected bool sendingServerStopCharge;
        protected bool sendingIsLeftHand;

        public virtual void ClearChargeStates()
        {
            IsCharging = false;
        }

        protected void PlayChargeAnimation(bool isLeftHand)
        {
            // Get weapon type data
            IWeaponItem weaponItem = Entity.GetAvailableWeapon(ref isLeftHand).GetWeaponItem();
            int weaponTypeDataId = weaponItem.WeaponType.DataId;
            // Play animation
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
            {
                // TPS model
                Entity.CharacterModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                Entity.CharacterModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
            {
                // Vehicle model
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayEquippedWeaponCharge(isLeftHand);
            }
            if (IsClient)
            {
                if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                {
                    // FPS model
                    Entity.FpsModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                    Entity.FpsModel.PlayEquippedWeaponCharge(isLeftHand);
                }
            }
            // Set weapon charging state
            MoveSpeedRateWhileCharging = Entity.GetMoveSpeedRateWhileCharging(weaponItem);
            IsCharging = true;
        }

        protected void StopChargeAnimation()
        {
            // Play animation
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                Entity.CharacterModel.StopWeaponChargeAnimation();
            if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).StopWeaponChargeAnimation();
            if (IsClient)
            {
                if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    Entity.FpsModel.StopWeaponChargeAnimation();
            }
            // Set weapon charging state
            IsCharging = false;
        }

        public void StartCharge(bool isLeftHand)
        {
            // Simulate start charge at client immediately
            PlayChargeAnimation(isLeftHand);

            // Tell the server to attack
            if (!IsServer)
            {
                sendingClientStartCharge = true;
                sendingIsLeftHand = isLeftHand;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                sendingServerStartCharge = true;
                sendingIsLeftHand = isLeftHand;
            }
        }

        public void StopCharge()
        {
            // Simulate stop charge at client immediately
            StopChargeAnimation();

            // Tell the server to attack
            if (!IsServer)
            {
                sendingClientStopCharge = true;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                sendingServerStopCharge = true;
            }
        }

        public bool WriteClientStartChargeState(NetDataWriter writer)
        {
            if (sendingClientStartCharge)
            {
                writer.Put(sendingIsLeftHand);
                sendingClientStartCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteServerStartChargeState(NetDataWriter writer)
        {
            if (sendingServerStartCharge)
            {
                writer.Put(sendingIsLeftHand);
                sendingServerStartCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteClientStopChargeState(NetDataWriter writer)
        {
            if (sendingClientStopCharge)
            {
                sendingClientStopCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteServerStopChargeState(NetDataWriter writer)
        {
            if (sendingServerStopCharge)
            {
                sendingServerStopCharge = false;
                return true;
            }
            return false;
        }

        public void ReadClientStartChargeStateAtServer(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
#if !CLIENT_BUILD
            // Tell clients to start charge later
            sendingServerStartCharge = true;
            sendingIsLeftHand = isLeftHand;
            // Start charge at server immediately
            PlayChargeAnimation(isLeftHand);
#endif
        }

        public void ReadServerStartChargeStateAtClient(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            if (IsOwnerClient)
            {
                // Don't start charge again (it already played in `StartCharge` function)
                return;
            }
            PlayChargeAnimation(isLeftHand);
        }

        public void ReadClientStopChargeStateAtServer(NetDataReader reader)
        {
#if !CLIENT_BUILD
            // Tell clients to stop charge later
            sendingServerStopCharge = true;
            // Stop charge at server immediately
            StopChargeAnimation();
#endif
        }

        public void ReadServerStopChargeStateAtClient(NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't stop charge again (it already played in `StopCharge` function)
                return;
            }
            StopChargeAnimation();
        }
    }
}
