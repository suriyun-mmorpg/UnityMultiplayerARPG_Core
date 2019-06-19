namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            if (Movement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    Movement = gameObject.AddComponent<RigidBodyEntityMovement>();
                else
                    Movement = gameObject.AddComponent<RigidBodyEntityMovement2D>();
            }
        }
    }
}
