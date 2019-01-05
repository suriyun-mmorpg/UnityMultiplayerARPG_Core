namespace MultiplayerARPG
{
    public partial class UICurrentBuilding : UIBase
    {
        public void OnClickDestroy()
        {
            BasePlayerCharacterController controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.DestroyBuilding();
        }

        public void OnClickDeselect()
        {
            BasePlayerCharacterController controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.DeselectBuilding();
        }
    }
}
