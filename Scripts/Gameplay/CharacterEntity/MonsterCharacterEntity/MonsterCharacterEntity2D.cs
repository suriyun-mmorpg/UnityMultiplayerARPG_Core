namespace MultiplayerARPG
{
    [System.Obsolete("This is deprecated, but still keep it for backward compatibilities. Use `MonsterCharacterEntity` instead")]
    /// <summary>
    /// This is deprecated, but still keep it for backward compatibilities.
    /// Use `MonsterCharacterEntity` instead
    /// </summary>
    public partial class MonsterCharacterEntity2D : BaseMonsterCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseEntityMovement>();
            if (CharacterMovement == null)
                CharacterMovement = gameObject.AddComponent<RigidBodyEntityMovement2D>();
        }
    }
}
