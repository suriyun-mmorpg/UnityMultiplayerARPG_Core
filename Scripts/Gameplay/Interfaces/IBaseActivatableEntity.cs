namespace MultiplayerARPG
{
    public interface IBaseActivatableEntity : ITargetableEntity
    {
        float GetActivatableDistance();
        /// <summary>
        /// If this returns `TRUE`, when character moved to it and activated, it will clear player's target from controller.
        /// </summary>
        /// <returns></returns>
        bool ShouldClearTargetAfterActivated();
    }
}
