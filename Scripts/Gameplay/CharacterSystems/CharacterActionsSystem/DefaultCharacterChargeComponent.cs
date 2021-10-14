using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class DefaultCharacterChargeComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterChargeComponent
    {
        public bool IsCharging { get; protected set; }
        public float MoveSpeedRateWhileCharging { get; protected set; }

        public virtual void ClearChargeStates()
        {
            IsCharging = false;
        }
        public bool CallAllPlayChargeAnimation(bool isLeftHand)
        {
            if (Entity.IsDead())
                return false;
            RPC(AllPlayChargeAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
            return true;
        }

        [AllRpc]
        protected void AllPlayChargeAnimation(bool isLeftHand)
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

        public bool CallAllStopChargeAnimation()
        {
            if (Entity.IsDead())
                return false;
            RPC(AllStopChargeAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        [AllRpc]
        protected void AllStopChargeAnimation()
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


        public bool CallServerStartWeaponCharge(bool isLeftHand)
        {
            RPC(ServerStartWeaponCharge, isLeftHand);
            return true;
        }

        /// <summary>
        /// Is function will be called at server to order character to start weapon charging
        /// </summary>
        [ServerRpc]
        protected virtual void ServerStartWeaponCharge(bool isLeftHand)
        {
            // TODO: May have charge power which increase attack damage
            CallAllPlayChargeAnimation(isLeftHand);
        }

        public bool CallServerStopWeaponCharge()
        {
            RPC(ServerStopWeaponCharge);
            return true;
        }

        /// <summary>
        /// Is function will be called at server to order character to stop weapon charging
        /// </summary>
        [ServerRpc]
        protected virtual void ServerStopWeaponCharge()
        {
            // TODO: If there is charge power, stop it. But there is no charge power yet so just stop playing animation
            CallAllStopChargeAnimation();
        }

        public void StartCharge(bool isLeftHand)
        {
            CallServerStartWeaponCharge(isLeftHand);
        }

        public void StopCharge()
        {
            CallServerStopWeaponCharge();
        }
    }
}
