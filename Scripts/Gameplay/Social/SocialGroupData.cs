using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerARPG
{
    public class SocialGroupData : INetSerializable
    {
        protected Dictionary<string, SocialCharacterData> members;
        protected SocialCharacterData tempMemberData;

        public int id { get; protected set; }
        public string leaderId { get; protected set; }

        public static SocialSystemSetting SystemSetting { get { return GameInstance.Singleton.SocialSystemSetting; } }

        public SocialGroupData()
        {
            members = new Dictionary<string, SocialCharacterData>();
        }

        public SocialGroupData(int id) : this()
        {
            this.id = id;
        }

        public SocialGroupData(int id, string leaderId) : this(id)
        {
            this.leaderId = leaderId;
            AddMember(new SocialCharacterData() { id = leaderId });
        }

        public SocialCharacterData CreateMemberData(BasePlayerCharacterEntity playerCharacter)
        {
            return SocialCharacterData.Create(playerCharacter);
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacter)
        {
            AddMember(CreateMemberData(playerCharacter));
        }

        public virtual void AddMember(SocialCharacterData memberData)
        {
            if (!members.ContainsKey(memberData.id))
            {
                members.Add(memberData.id, memberData);
                return;
            }
            SocialCharacterData oldMemberData = members[memberData.id];
            oldMemberData.characterName = memberData.characterName;
            oldMemberData.dataId = memberData.dataId;
            oldMemberData.level = memberData.level;
            members[memberData.id] = oldMemberData;
        }

        public void UpdateMember(BasePlayerCharacterEntity playerCharacter)
        {
            UpdateMember(CreateMemberData(playerCharacter));
        }

        public virtual void UpdateMember(SocialCharacterData memberData)
        {
            if (!members.ContainsKey(memberData.id))
                return;
            members[memberData.id] = memberData;
        }

        public virtual bool RemoveMember(string characterId)
        {
            return members.Remove(characterId);
        }

        public virtual void ClearMembers()
        {
            members.Clear();
        }

        public bool IsMember(string characterId)
        {
            return members.ContainsKey(characterId);
        }

        public int CountMember()
        {
            return members.Count;
        }

        public bool ContainsMemberId(string characterId)
        {
            return members.ContainsKey(characterId);
        }

        public string[] GetMemberIds()
        {
            return members.Keys.ToArray();
        }

        public SocialCharacterData[] GetMembers()
        {
            return members.Values.ToArray();
        }

        public bool TryGetMember(string id, out SocialCharacterData result)
        {
            return members.TryGetValue(id, out result);
        }

        public SocialCharacterData GetMember(string id)
        {
            return members[id];
        }

        public bool IsLeader(string characterId)
        {
            return characterId.Equals(leaderId);
        }

        public virtual void SetLeader(string characterId)
        {
            if (members.ContainsKey(characterId))
                leaderId = characterId;
        }

        public SocialCharacterData GetLeader()
        {
            return members[leaderId];
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(leaderId);
            writer.PutDictionary(members);
        }

        public virtual void Deserialize(NetDataReader reader)
        {
            id = reader.GetInt();
            leaderId = reader.GetString();
            members = reader.GetDictionary<string, SocialCharacterData>();
        }

        public bool UpdateSocialGroupMember(UpdateSocialMemberMessage message)
        {
            if (id != message.socialId)
                return false;

            switch (message.type)
            {
                case UpdateSocialMemberMessage.UpdateType.Add:
                    AddMember(message.character);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Update:
                    UpdateMember(message.character);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Remove:
                    RemoveMember(message.character.id);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Clear:
                    ClearMembers();
                    break;
            }
            return true;
        }
    }
}
