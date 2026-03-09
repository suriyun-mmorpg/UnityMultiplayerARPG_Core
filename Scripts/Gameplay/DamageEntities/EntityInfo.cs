namespace MultiplayerARPG
{
    public partial class EntityInfo
    {
        public static readonly EntityInfo Empty = new EntityInfo();

        /// <summary>
        /// If `Type` = `0`, determine that it is `NULL`, so if `Summoner Type` is `0`, determine that it has no summoner
        /// </summary>
        public byte Type { get; private set; }
        public uint ObjectId { get; private set; }
        public string Id { get; private set; }
        public string SubChannelId { get; private set; }
        public int DataId { get; private set; }
        public int FactionId { get; private set; }
        public int PartyId { get; private set; }
        public int GuildId { get; private set; }
        public bool IsInSafeArea { get; private set; }
        public EntityInfo Summoner { get; private set; }
        public bool HasSummoner => Summoner != null && Summoner.Type > 0;

        public EntityInfo()
        {
            Type = 0;
            ObjectId = 0;
            Id = string.Empty;
            SubChannelId = string.Empty;
            DataId = 0;
            FactionId = 0;
            PartyId = 0;
            GuildId = 0;
            IsInSafeArea = false;
            Summoner = null;
        }

        public EntityInfo SetEntityInfo(
            byte type,
            uint objectId,
            string id,
            string subChannelId,
            int dataId,
            int factionId,
            int partyId,
            int guildId,
            bool isInSafeArea,
            BaseCharacterEntity summonerEntity = null)
        {
            Type = type;
            ObjectId = objectId;
            Id = id;
            SubChannelId = subChannelId;
            DataId = dataId;
            FactionId = factionId;
            PartyId = partyId;
            GuildId = guildId;
            IsInSafeArea = isInSafeArea;
            Summoner = null;
            if (summonerEntity != null)
                Summoner = summonerEntity.GetInfo();
            return this;
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
