using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseCharacterEntity : AssetReferenceBaseGameEntity<BaseCharacterEntity>
    {
        public AssetReferenceBaseCharacterEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBaseCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}