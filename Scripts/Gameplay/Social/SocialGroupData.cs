using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class SocialGroupData : INetSerializable
    {
        public static SocialSystemSetting SystemSetting { get { return GameInstance.Singleton.SocialSystemSetting; } }

        public SocialCharacterData CreateMemberData(BasePlayerCharacterEntity playerCharacter)
        {
            return SocialCharacterData.Create(playerCharacter);
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacter)
        {
            AddMember(CreateMemberData(playerCharacter));
        }

        public void UpdateMember(BasePlayerCharacterEntity playerCharacter)
        {
            UpdateMember(CreateMemberData(playerCharacter));
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
