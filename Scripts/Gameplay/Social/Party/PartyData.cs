using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class PartyData : SocialGroupData
    {
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }

        public PartyData(int id, bool shareExp, bool shareItem, string leaderId)
            : base(id, leaderId)
        {
            this.shareExp = shareExp;
            this.shareItem = shareItem;
        }

        public PartyData(int id, bool shareExp, bool shareItem, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, shareExp, shareItem, leaderCharacterEntity.Id)
        {
            AddMember(leaderCharacterEntity);
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
                if (!BaseGameNetworkManager.IsCharacterOnline(memberId))
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
    }
}
