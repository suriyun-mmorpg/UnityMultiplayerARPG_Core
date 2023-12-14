namespace MultiplayerARPG
{
    public interface ICharacterReloadComponent
    {
        int ReloadingAmmoAmount { get; }
        bool IsReloading { get; }
        float LastReloadEndTime { get; }
        bool IsSkipMovementValidationWhileReloading { get; }
        bool IsUseRootMotionWhileReloading { get; }
        float MoveSpeedRateWhileReloading { get; }
        MovementRestriction MovementRestrictionWhileReloading { get; }
        float ReloadTotalDuration { get; set; }
        float[] ReloadTriggerDurations { get; set; }

        void CancelReload();
        void ClearReloadStates();
        void Reload(bool isLeftHand);
    }
}
