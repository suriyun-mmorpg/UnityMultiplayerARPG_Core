using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBasePlayerCharacterEntity : AssetReferenceLiteNetLibBehaviour<BasePlayerCharacterEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBasePlayerCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}