using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct SocialCharacterData : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            DeserializeWithoutHpMp(reader);
            currentHp = reader.GetPackedInt();
            maxHp = reader.GetPackedInt();
            currentMp = reader.GetPackedInt();
            maxMp = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            SerializeWithoutHpMp(writer);
            writer.PutPackedInt(currentHp);
            writer.PutPackedInt(maxHp);
            writer.PutPackedInt(currentMp);
            writer.PutPackedInt(maxMp);
        }

        public void DeserializeWithoutHpMp(NetDataReader reader)
        {
            id = reader.GetString();
            userId = reader.GetString();
            characterName = reader.GetString();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedInt();
            factionId = reader.GetPackedInt();
            partyId = reader.GetPackedInt();
            guildId = reader.GetPackedInt();
            guildRole = reader.GetByte();
            iconDataId = reader.GetPackedInt();
            frameDataId = reader.GetPackedInt();
            titleDataId = reader.GetPackedInt();
        }

        public void SerializeWithoutHpMp(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(userId);
            writer.Put(characterName);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(level);
            writer.PutPackedInt(factionId);
            writer.PutPackedInt(partyId);
            writer.PutPackedInt(guildId);
            writer.Put(guildRole);
            writer.PutPackedInt(iconDataId);
            writer.PutPackedInt(frameDataId);
            writer.PutPackedInt(titleDataId);
        }

        public static SocialCharacterData Create(BasePlayerCharacterEntity character)
        {
            return new SocialCharacterData()
            {
                id = character.Id,
                characterName = character.CharacterName,
                dataId = character.DataId,
                level = character.Level,
                factionId = character.FactionId,
                partyId = character.PartyId,
                guildId = character.GuildId,
                guildRole = character.GuildRole,
                currentHp = character.CurrentHp,
                maxHp = character.MaxHp,
                currentMp = character.CurrentMp,
                maxMp = character.MaxMp,
                iconDataId = character.IconDataId,
                frameDataId = character.FrameDataId,
                titleDataId = character.TitleDataId,
            };
        }
    }
}
