namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceItemsContainerEntity : AssetReferenceBaseGameEntity<ItemsContainerEntity>
    {
        public AssetReferenceItemsContainerEntity(string guid) : base(guid)
        {
        }
    }
}