namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseCharacterMovement>();
            if (CharacterMovement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    CharacterMovement = gameObject.AddComponent<NavMeshCharacterMovement>();
                else
                    CharacterMovement = gameObject.AddComponent<RigidBodyCharacterMovement2D>();
            }
        }
    }
}
