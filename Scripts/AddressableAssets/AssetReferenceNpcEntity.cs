using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceNpcEntity : AssetReferenceLiteNetLibBehaviour<NpcEntity>
    {
        public AssetReferenceNpcEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceNpcEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}