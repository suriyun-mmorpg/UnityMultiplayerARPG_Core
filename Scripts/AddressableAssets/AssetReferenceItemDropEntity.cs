namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceItemDropEntity : AssetReferenceBaseGameEntity<ItemDropEntity>
    {
        public AssetReferenceItemDropEntity(string guid) : base(guid)
        {
        }
    }
}