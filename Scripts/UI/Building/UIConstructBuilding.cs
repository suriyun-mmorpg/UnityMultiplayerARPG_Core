namespace MultiplayerARPG
{
    public partial class UIConstructBuilding : UIBase
    {
        public void OnClickConfirmBuild()
        {
            BasePlayerCharacterController controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.ConfirmBuild();
        }

        public void OnClickCancelBuild()
        {
            BasePlayerCharacterController controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.CancelBuild();
        }
    }
}
