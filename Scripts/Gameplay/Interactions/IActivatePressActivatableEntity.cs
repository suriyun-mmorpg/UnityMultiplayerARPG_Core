namespace MultiplayerARPG
{
    public interface IActivatePressActivatableEntity
    {
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
        bool CanKeyPressActivate();
        void OnKeyPressActivate();
    }
}
