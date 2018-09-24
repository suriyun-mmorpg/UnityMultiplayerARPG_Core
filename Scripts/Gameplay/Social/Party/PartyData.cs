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
    }
}
