using System.Collections.Generic;
using UnityEngine;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class PlayerCharacterData : CharacterData, IPlayerCharacterData, INetSerializable
    {
        private List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
        private List<CharacterQuest> quests = new List<CharacterQuest>();
        private List<CharacterCurrency> currencies = new List<CharacterCurrency>();

        public string UserId { get; set; }
        public int FactionId { get; set; }
        public float StatPoint { get; set; }
        public float SkillPoint { get; set; }
        public int Gold { get; set; }
        public int UserGold { get; set; }
        public int UserCash { get; set; }
        public int PartyId { get; set; }
        public int GuildId { get; set; }
        public byte GuildRole { get; set; }
        public int SharedGuildExp { get; set; }
        public string CurrentMapName { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public Vector3 CurrentRotation { get; set; }
        public string RespawnMapName { get; set; }
        public Vector3 RespawnPosition { get; set; }
        public int MountDataId { get; set; }
        public long LastDeadTime { get; set; }
        public long LastUpdate { get; set; }

        public IList<CharacterHotkey> Hotkeys
        {
            get { return hotkeys; }
            set
            {
                hotkeys = new List<CharacterHotkey>();
                hotkeys.AddRange(value);
            }
        }

        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set
            {
                quests = new List<CharacterQuest>();
                quests.AddRange(value);
            }
        }

        public IList<CharacterCurrency> Currencies
        {
            get { return currencies; }
            set
            {
                currencies = new List<CharacterCurrency>();
                currencies.AddRange(value);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            this.DeserializeCharacterData(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            this.SerializeCharacterData(writer);
        }
    }

    public class PlayerCharacterDataLastUpdateComparer : IComparer<PlayerCharacterData>
    {
        private int sortMultiplier = 1;
        public PlayerCharacterDataLastUpdateComparer Asc()
        {
            sortMultiplier = 1;
            return this;
        }

        public PlayerCharacterDataLastUpdateComparer Desc()
        {
            sortMultiplier = -1;
            return this;
        }

        public int Compare(PlayerCharacterData x, PlayerCharacterData y)
        {
            return x.LastUpdate.CompareTo(y.LastUpdate) * sortMultiplier;
        }
    }
}
