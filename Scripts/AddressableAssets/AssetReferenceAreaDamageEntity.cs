using Insthync.AddressableAssetTools;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceAreaDamageEntity : AssetReferenceComponent<AreaDamageEntity>
    {
        public AssetReferenceAreaDamageEntity(string guid) : base(guid)
        {
        }
    }
}
