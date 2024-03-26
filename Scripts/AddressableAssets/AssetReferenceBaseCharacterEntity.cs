using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseCharacterEntity : AssetReferenceLiteNetLibBehaviour<BaseCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBaseCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}