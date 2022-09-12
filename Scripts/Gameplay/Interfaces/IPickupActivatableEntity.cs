namespace MultiplayerARPG
{
    public interface IPickupActivatableEntity : IBaseActivatableEntity
    {
        /// <summary>
        /// Can activate or not? return `TRUE` if it can.
        /// </summary>
        /// <returns></returns>
        bool CanPickupActivate();
        /// <summary>
        /// Put anything you want to do when interact the object.
        /// </summary>
        void OnPickupActivate();
    }
}
