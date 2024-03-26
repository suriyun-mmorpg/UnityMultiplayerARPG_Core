using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceBasePlayerCharacterEntity : AssetReferenceLiteNetLibBehaviour<BasePlayerCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBasePlayerCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}