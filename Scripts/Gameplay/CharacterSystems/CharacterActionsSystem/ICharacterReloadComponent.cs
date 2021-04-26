namespace MultiplayerARPG
{
    public interface ICharacterReloadComponent
    {
        short ReloadingAmmoAmount { get; }
        bool IsReloading { get; }
        float MoveSpeedRateWhileReloading { get; }

        void CancelReload();
        void ClearReloadStates();
        void Reload(bool isLeftHand);
    }
}
