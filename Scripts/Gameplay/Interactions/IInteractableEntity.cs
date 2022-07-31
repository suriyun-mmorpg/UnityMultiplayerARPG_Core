namespace MultiplayerARPG
{
    public interface IInteractableEntity
    {
        float GetInteractableDistance();
        bool ShouldBeAttackTarget();
        bool CanInteract();
        void OnInteract();
        bool CanHoldInteract();
        void OnHoldInteract();
    }
}
