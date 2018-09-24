using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData : SocialGroupData
    {
        public int id { get; private set; }
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        public string leaderId { get; private set; }
        
        public PartyData(int id, bool shareExp, bool shareItem, string leaderId)
            : base()
        {
            this.id = id;
            this.shareExp = shareExp;
            this.shareItem = shareItem;
            this.leaderId = leaderId;
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

        public bool IsLeader(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return IsLeader(playerCharacterEntity.Id);
        }

        public bool IsLeader(string characterId)
        {
            return characterId.Equals(leaderId);
        }

    }
}
