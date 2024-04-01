namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseRewardDropEntity : AssetReferenceBaseGameEntity<BaseRewardDropEntity>
    {
        public AssetReferenceBaseRewardDropEntity(string guid) : base(guid)
        {
        }
    }
}