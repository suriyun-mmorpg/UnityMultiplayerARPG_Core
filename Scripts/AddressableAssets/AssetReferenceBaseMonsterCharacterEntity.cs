using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceBaseMonsterCharacterEntity : AssetReferenceLiteNetLibBehaviour<BaseMonsterCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBaseMonsterCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}