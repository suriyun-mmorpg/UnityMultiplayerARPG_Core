namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseEntityMovement>();
            if (CharacterMovement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    CharacterMovement = gameObject.AddComponent<RigidBodyEntityMovement>();
                else
                    CharacterMovement = gameObject.AddComponent<RigidBodyEntityMovement2D>();
            }
        }
    }
}
