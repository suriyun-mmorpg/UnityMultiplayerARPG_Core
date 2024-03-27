using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseRewardDropEntity : AssetReferenceLiteNetLibBehaviour<BaseRewardDropEntity>
    {
        public AssetReferenceBaseRewardDropEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBaseRewardDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}