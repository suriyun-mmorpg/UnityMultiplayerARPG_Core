using Insthync.AddressableAssetTools;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceAreaBuffEntity : AssetReferenceComponent<AreaBuffEntity>
    {
        public AssetReferenceAreaBuffEntity(string guid) : base(guid)
        {
        }
    }
}
