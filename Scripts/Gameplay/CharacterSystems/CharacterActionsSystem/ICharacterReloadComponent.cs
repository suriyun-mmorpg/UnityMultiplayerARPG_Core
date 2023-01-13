using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public interface ICharacterReloadComponent
    {
        int ReloadingAmmoAmount { get; }
        bool IsReloading { get; }
        float LastReloadEndTime { get; }
        float MoveSpeedRateWhileReloading { get; }
        MovementRestriction MovementRestrictionWhileReloading { get; }
        float ReloadTotalDuration { get; set; }
        float[] ReloadTriggerDurations { get; set; }

        void CancelReload();
        void ClearReloadStates();
        void Reload(bool isLeftHand);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientReloadState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerReloadState(NetDataWriter writer);
        void ReadClientReloadStateAtServer(NetDataReader reader);
        void ReadServerReloadStateAtClient(NetDataReader reader);
    }
}
