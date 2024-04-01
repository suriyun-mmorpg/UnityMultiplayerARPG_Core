namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBuildingEntity : AssetReferenceBaseGameEntity<BuildingEntity>
    {
        public AssetReferenceBuildingEntity(string guid) : base(guid)
        {
        }
    }
}