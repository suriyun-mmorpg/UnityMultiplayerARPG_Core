using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponsePartyInfoMessage : BaseAckMessage
    {
        public PartyMemberData[] members;

        public override void DeserializeData(NetDataReader reader)
        {
            var length = reader.GetInt();
            var members = new PartyMemberData[length];
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    var entry = new PartyMemberData();
                    entry.id = reader.GetString();
                    entry.characterName = reader.GetString();
                    entry.dataId = reader.GetInt();
                    entry.level = reader.GetInt();
                    entry.currentHp = reader.GetInt();
                    entry.maxHp = reader.GetInt();
                    entry.currentMp = reader.GetInt();
                    entry.maxMp = reader.GetInt();
                    entry.isLeader = reader.GetBool();
                    members[i] = entry;
                }
            }
            this.members = members;
        }

        public override void SerializeData(NetDataWriter writer)
        {
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
                    writer.Put(entry.currentHp);
                    writer.Put(entry.maxHp);
                    writer.Put(entry.currentMp);
                    writer.Put(entry.maxMp);
                    writer.Put(entry.isLeader);
                }
            }
        }
    }
}
