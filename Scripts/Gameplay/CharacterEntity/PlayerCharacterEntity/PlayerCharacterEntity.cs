namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseCharacterMovement>();
            if (CharacterMovement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    CharacterMovement = gameObject.AddComponent<RigidBodyCharacterMovement>();
                else
                    CharacterMovement = gameObject.AddComponent<RigidBodyCharacterMovement2D>();
            }
        }
    }
}
