namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceHarvestableEntity : AssetReferenceBaseGameEntity<HarvestableEntity>
    {
        public AssetReferenceHarvestableEntity(string guid) : base(guid)
        {
        }
    }
}