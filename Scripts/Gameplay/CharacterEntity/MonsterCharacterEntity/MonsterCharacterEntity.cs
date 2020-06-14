using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override void Validate()
        {
            base.Validate();
            if (Movement == null)
                Logging.LogError(ToString(), "Did not setup entity movement component to this entity.");
        }
    }
}
