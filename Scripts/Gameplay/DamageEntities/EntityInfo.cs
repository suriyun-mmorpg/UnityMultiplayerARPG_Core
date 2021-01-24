namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class EntityInfo
    {
        public string type;
        public uint objectId;
        public string id;
        public int dataId;
        public int factionId;
        public int partyId;
        public int guildId;
        public bool isInSafeArea;
        public EntityInfo summonerInfo;

        public bool TryGetEntity<T>(out T entity)
            where T : class, IGameEntity
        {
            if (BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(objectId, out entity))
                return true;
            entity = null;
            return false;
        }
    }
}
