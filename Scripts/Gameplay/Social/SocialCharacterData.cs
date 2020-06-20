using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SocialCharacterData : INetSerializable
    {
        public string id;
        public string characterName;
        public int dataId;
        public short level;
        public int currentHp;
        public int maxHp;
        public int currentMp;
        public int maxMp;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            characterName = reader.GetString();
            dataId = reader.GetInt();
            level = reader.GetShort();
            currentHp = reader.GetInt();
            maxHp = reader.GetInt();
            currentMp = reader.GetInt();
            maxMp = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(characterName);
            writer.Put(dataId);
            writer.Put(level);
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
                currentHp = characterEntity.CurrentHp,
                currentMp = characterEntity.CurrentMp,
            };
        }
    }
}
