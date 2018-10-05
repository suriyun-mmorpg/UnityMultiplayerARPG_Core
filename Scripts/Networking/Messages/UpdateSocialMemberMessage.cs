using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class UpdateSocialMemberMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Update,
            Remove,
        }
        public UpdateType type;
        public int id;
        public string characterId;
        public SocialCharacterData member;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            characterId = reader.GetString();
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    member = new SocialCharacterData();
                    member.id = characterId;
                    member.characterName = reader.GetString();
                    member.dataId = reader.GetInt();
                    member.level = reader.GetInt();
                    member.isOnline = reader.GetBool();
                    // Read extra data
                    if (member.isOnline)
                    {
                        member.currentHp = reader.GetInt();
                        member.maxHp = reader.GetInt();
                        member.currentMp = reader.GetInt();
                        member.maxMp = reader.GetInt();
                    }
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            writer.Put(characterId);
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    writer.Put(member.characterName);
                    writer.Put(member.dataId);
                    writer.Put(member.level);
                    writer.Put(member.isOnline);
                    // Put extra data
                    if (member.isOnline)
                    {
                        writer.Put(member.currentHp);
                        writer.Put(member.maxHp);
                        writer.Put(member.currentMp);
                        writer.Put(member.maxMp);
                    }
                    break;
            }
        }
    }
}
