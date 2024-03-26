using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseMonsterCharacterEntity : AssetReferenceLiteNetLibBehaviour<BaseMonsterCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBaseMonsterCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}