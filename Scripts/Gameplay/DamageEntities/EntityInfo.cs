namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct EntityInfo
    {
        public string Type { get; set; }
        public uint ObjectId { get; set; }
        public string Id { get; set; }
        public int DataId { get; set; }
        public int FactionId { get; set; }
        public int PartyId { get; set; }
        public int GuildId { get; set; }
        public bool IsInSafeArea { get; set; }
        public EntityInfo? Summoner { get; set; }

        public EntityInfo(
            string type,
            uint objectId,
            string id,
            int dataId,
            int factionId,
            int partyId,
            int guildId,
            bool isInSafeArea)
        {
            Type = type;
            ObjectId = objectId;
            Id = id;
            DataId = dataId;
            FactionId = factionId;
            PartyId = partyId;
            GuildId = guildId;
            IsInSafeArea = isInSafeArea;
            Summoner = null;
        }

        public EntityInfo(
            string type,
            uint objectId,
            string id,
            int dataId,
            int factionId,
            int partyId,
            int guildId,
            bool isInSafeArea,
            BaseCharacterEntity summonerEntity)
            : this(
                  type,
                  objectId,
                  id,
                  dataId,
                  factionId,
                  partyId,
                  guildId,
                  isInSafeArea)
        {
            if (summonerEntity != null)
                Summoner = summonerEntity.GetInfo();
        }

        public bool TryGetEntity<T>(out T entity)
            where T : class, IGameEntity
        {
            if (BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(ObjectId, out entity))
                return true;
            entity = null;
            return false;
        }
    }
}
