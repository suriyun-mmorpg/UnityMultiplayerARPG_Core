namespace MultiplayerARPG
{
    [System.Obsolete("This is deprecated, use `PlayerCharacterEntity` instead")]
    /// <summary>
    /// This is deprecated, but still keep it for backward compatibilities.
    /// Use `PlayerCharacterEntity` instead
    /// </summary>
    public partial class PlayerCharacterEntity2D : BasePlayerCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            CharacterMovement = GetComponent<BaseCharacterMovement>();
            if (CharacterMovement == null)
                CharacterMovement = gameObject.AddComponent<RigidBodyCharacterMovement2D>();
        }
    }
}
