using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceNpcEntity : AssetReferenceLiteNetLibBehaviour<NpcEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceNpcEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}