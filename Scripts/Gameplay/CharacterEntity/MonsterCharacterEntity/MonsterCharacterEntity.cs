namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseEntityMovement>();
            if (CharacterMovement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    CharacterMovement = gameObject.AddComponent<NavMeshEntityMovement>();
                else
                    CharacterMovement = gameObject.AddComponent<RigidBodyEntityMovement2D>();
            }
        }
    }
}
