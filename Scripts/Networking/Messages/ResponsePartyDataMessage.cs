using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponsePartyDataMessage : BaseAckMessage
    {
        public bool shareExp;
        public bool shareItem;
        public string leaderId;
        public SocialCharacterData[] members;

        public override void DeserializeData(NetDataReader reader)
        {
            shareExp = reader.GetBool();
            shareItem = reader.GetBool();
            leaderId = reader.GetString();
            var length = reader.GetInt();
            var members = new SocialCharacterData[length];
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    var entry = new SocialCharacterData();
                    entry.id = reader.GetString();
                    entry.characterName = reader.GetString();
                    entry.dataId = reader.GetInt();
                    entry.level = reader.GetInt();
                    entry.isOnline = reader.GetBool();
                    // Read extra data
                    if (entry.isOnline)
                    {
                        entry.currentHp = reader.GetInt();
                        entry.maxHp = reader.GetInt();
                        entry.currentMp = reader.GetInt();
                        entry.maxMp = reader.GetInt();
                    }
                    members[i] = entry;
                }
            }
            this.members = members;
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(shareExp);
            writer.Put(shareItem);
            writer.Put(leaderId);
            var length = members == null ? 0 : members.Length;
            writer.Put(length);
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    var entry = members[i];
                    writer.Put(entry.id);
                    writer.Put(entry.characterName);
                    writer.Put(entry.dataId);
                    writer.Put(entry.level);
                    writer.Put(entry.isOnline);
                    // Put extra data
                    if (entry.isOnline)
                    {
                        writer.Put(entry.currentHp);
                        writer.Put(entry.maxHp);
                        writer.Put(entry.currentMp);
                        writer.Put(entry.maxMp);
                    }
                }
            }
        }
    }
}
