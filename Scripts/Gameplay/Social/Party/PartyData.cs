using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public sealed class PartyData : SocialGroupData, INetSerializable
    {
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }

        public PartyData()
            : base()
        {

        }

        public PartyData(int id)
            : base(id)
        {

        }

        public PartyData(int id, string leaderId)
            : base(id, leaderId)
        {

        }

        public PartyData(int id, bool shareExp, bool shareItem, string leaderId)
            : this(id, leaderId)
        {
            this.shareExp = shareExp;
            this.shareItem = shareItem;
        }

        public PartyData(int id, bool shareExp, bool shareItem, SocialCharacterData leaderCharacter)
            : this(id, shareExp, shareItem, leaderCharacter.id)
        {
            AddMember(leaderCharacter);
        }

        public void Setting(bool shareExp, bool shareItem)
        {
            this.shareExp = shareExp;
            this.shareItem = shareItem;
        }

        public void GetSortedMembers(out SocialCharacterData[] sortedMembers)
        {
            int i = 0;
            List<SocialCharacterData> offlineMembers = new List<SocialCharacterData>();
            sortedMembers = new SocialCharacterData[members.Count];
            sortedMembers[i++] = members[leaderId];
            SocialCharacterData tempMember;
            foreach (string memberId in members.Keys)
            {
                if (memberId.Equals(leaderId))
                    continue;
                tempMember = members[memberId];
                if (!GameInstance.ClientOnlineCharacterHandlers.IsCharacterOnline(memberId))
                {
                    offlineMembers.Add(tempMember);
                    continue;
                }
                sortedMembers[i++] = tempMember;
            }
            foreach (SocialCharacterData offlineMember in offlineMembers)
            {
                sortedMembers[i++] = offlineMember;
            }
        }

        public int MaxMember()
        {
            return SystemSetting.MaxPartyMember;
        }

        public bool CanInvite(string characterId)
        {
            if (IsLeader(characterId))
                return true;
            else
                return SystemSetting.PartyMemberCanInvite;
        }

        public bool CanKick(string characterId)
        {
            if (IsLeader(characterId))
                return true;
            else
                return SystemSetting.PartyMemberCanKick;
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(shareExp);
            writer.Put(shareItem);
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            shareExp = reader.GetBool();
            shareItem = reader.GetBool();
        }
    }
}
