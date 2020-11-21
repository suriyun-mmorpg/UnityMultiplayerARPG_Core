using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SocialCharacterData : INetSerializable
    {
        public string id;
        public string userId;
        public string characterName;
        public int dataId;
        public short level;
        public int factionId;
        public int partyId;
        public int guildId;
        public byte guildRole;
        public int currentHp;
        public int maxHp;
        public int currentMp;
        public int maxMp;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            userId = reader.GetString();
            characterName = reader.GetString();
            dataId = reader.GetInt();
            level = reader.GetShort();
            factionId = reader.GetInt();
            partyId = reader.GetInt();
            guildId = reader.GetInt();
            guildRole = reader.GetByte();
            currentHp = reader.GetInt();
            maxHp = reader.GetInt();
            currentMp = reader.GetInt();
            maxMp = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(userId);
            writer.Put(characterName);
            writer.Put(dataId);
            writer.Put(level);
            writer.Put(factionId);
            writer.Put(partyId);
            writer.Put(guildId);
            writer.Put(guildRole);
            writer.Put(currentHp);
            writer.Put(maxHp);
            writer.Put(currentMp);
            writer.Put(maxMp);
        }

        public static SocialCharacterData Create(IPlayerCharacterData characterEntity)
        {
            return new SocialCharacterData()
            {
                id = characterEntity.Id,
                characterName = characterEntity.CharacterName,
                dataId = characterEntity.DataId,
                level = characterEntity.Level,
                factionId = characterEntity.FactionId,
                partyId = characterEntity.PartyId,
                guildId = characterEntity.GuildId,
                guildRole = characterEntity.GuildRole,
                currentHp = characterEntity.CurrentHp,
                currentMp = characterEntity.CurrentMp,
            };
        }
    }
}
