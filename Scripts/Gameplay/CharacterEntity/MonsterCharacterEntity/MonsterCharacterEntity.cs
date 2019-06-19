namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            if (Movement == null)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    Movement = gameObject.AddComponent<NavMeshEntityMovement>();
                else
                    Movement = gameObject.AddComponent<RigidBodyEntityMovement2D>();
            }
        }
    }
}
