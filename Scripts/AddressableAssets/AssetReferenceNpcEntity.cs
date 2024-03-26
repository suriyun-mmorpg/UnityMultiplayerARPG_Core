using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceNpcEntity : AssetReferenceLiteNetLibBehaviour<NpcEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceNpcEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}