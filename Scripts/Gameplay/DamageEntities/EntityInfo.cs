namespace MultiplayerARPG
{
    public partial struct EntityInfo
    {
        public static readonly EntityInfo Empty = new EntityInfo(string.Empty, 0, string.Empty, 0, 0, 0, 0, false);

        public string Type { get; set; }
        public uint ObjectId { get; set; }
        public string Id { get; set; }
        public int DataId { get; set; }
        public int FactionId { get; set; }
        public int PartyId { get; set; }
        public int GuildId { get; set; }
        public bool IsInSafeArea { get; set; }
        public bool HasSummoner { get; set; }
        public string SummonerType { get; set; }
        public uint SummonerObjectId { get; set; }
        public string SummonerId { get; set; }
        public int SummonerDataId { get; set; }
        public int SummonerFactionId { get; set; }
        public int SummonerPartyId { get; set; }
        public int SummonerGuildId { get; set; }
        public bool SummonerIsInSafeArea { get; set; }
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
            if (BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(ObjectId, out entity))
                return true;
            entity = null;
            return false;
        }
    }
}
