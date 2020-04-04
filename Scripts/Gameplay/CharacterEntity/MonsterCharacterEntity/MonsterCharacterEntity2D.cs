using LiteNetLibManager;

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
            base.InitialRequiredComponents();
            if (Movement == null)
                Logging.LogError(ToString(), "Did not setup entity movement component to this entity.");
        }
    }
}
