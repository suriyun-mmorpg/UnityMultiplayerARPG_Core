using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceBaseCharacterEntity : AssetReferenceLiteNetLibBehaviour<BaseCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBaseCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}