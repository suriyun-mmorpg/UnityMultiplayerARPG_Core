using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseRewardDropEntity : AssetReferenceLiteNetLibBehaviour<BaseRewardDropEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBaseRewardDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}