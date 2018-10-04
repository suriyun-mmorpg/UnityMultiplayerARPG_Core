using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData : SocialGroupData
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

        public override byte GetMemberFlags(SocialCharacterData memberData)
        {
            return (byte)GetPartyMemberFlags(memberData.id);
        }

        public PartyMemberFlags GetPartyMemberFlags(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return GetPartyMemberFlags(playerCharacterEntity.Id);
        }

        public PartyMemberFlags GetPartyMemberFlags(string characterId)
        {
            if (!members.ContainsKey(characterId))
                return 0;

            if (IsLeader(characterId))
                return (PartyMemberFlags.IsLeader | PartyMemberFlags.CanInvite | PartyMemberFlags.CanKick);
            else
                return ((SystemSetting.PartyMemberCanInvite ? PartyMemberFlags.CanInvite : 0) | (SystemSetting.PartyMemberCanKick ? PartyMemberFlags.CanKick : 0));
        }

        public void GetSortedMembers(out SocialCharacterData[] sortedMembers)
        {
            var i = 0;
            sortedMembers = new SocialCharacterData[members.Count];
            sortedMembers[i++] = members[leaderId];
            foreach (var memberId in members.Keys)
            {
                if (memberId.Equals(leaderId))
                    continue;
                sortedMembers[i++] = members[memberId];
            }
        }

        public static bool IsLeader(PartyMemberFlags flags)
        {
            return (flags & PartyMemberFlags.IsLeader) != 0;
        }

        public static bool CanInvite(PartyMemberFlags flags)
        {
            return (flags & PartyMemberFlags.CanInvite) != 0;
        }

        public static bool CanKick(PartyMemberFlags flags)
        {
            return (flags & PartyMemberFlags.CanKick) != 0;
        }
    }
}
