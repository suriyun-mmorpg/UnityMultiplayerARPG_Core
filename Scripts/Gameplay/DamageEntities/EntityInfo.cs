namespace MultiplayerARPG
{
    public partial struct EntityInfo
    {
        public static readonly EntityInfo Empty = new EntityInfo(string.Empty, 0, string.Empty, 0, 0, 0, 0, false);

        public string Type { get; private set; }
        public uint ObjectId { get; private set; }
        public string Id { get; private set; }
        public int DataId { get; private set; }
        public int FactionId { get; private set; }
        public int PartyId { get; private set; }
        public int GuildId { get; private set; }
        public bool IsInSafeArea { get; private set; }
        public bool HasSummoner { get; private set; }
        public string SummonerType { get; private set; }
        public uint SummonerObjectId { get; private set; }
        public string SummonerId { get; private set; }
        public int SummonerDataId { get; private set; }
        public int SummonerFactionId { get; private set; }
        public int SummonerPartyId { get; private set; }
        public int SummonerGuildId { get; private set; }
        public bool SummonerIsInSafeArea { get; private set; }
        public EntityInfo Summoner
        {
            get
            {
                return new EntityInfo(
                  SummonerType,
                  SummonerObjectId,
                  SummonerId,
                  SummonerDataId,
                  SummonerFactionId,
                  SummonerPartyId,
                  SummonerGuildId,
                  SummonerIsInSafeArea);
            }
        }

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
            HasSummoner = false;
            SummonerType = string.Empty;
            SummonerObjectId = 0;
            SummonerId = string.Empty;
            SummonerDataId = 0;
            SummonerFactionId = 0;
            SummonerPartyId = 0;
            SummonerGuildId = 0;
            SummonerIsInSafeArea = false;
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
            {
                EntityInfo summonerInfo = summonerEntity.GetInfo();
                HasSummoner = true;
                SummonerType = summonerInfo.Type;
                SummonerObjectId = summonerInfo.ObjectId;
                SummonerId = summonerInfo.Id;
                SummonerDataId = summonerInfo.DataId;
                SummonerFactionId = summonerInfo.FactionId;
                SummonerPartyId = summonerInfo.PartyId;
                SummonerGuildId = summonerInfo.GuildId;
                SummonerIsInSafeArea = summonerInfo.IsInSafeArea;
            }
        }

        public bool TryGetEntity<T>(out T entity)
            where T : class, IGameEntity
        {
            if (BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(ObjectId, out entity) && entity.Entity is T)
                return true;
            entity = null;
            return false;
        }
    }
}
