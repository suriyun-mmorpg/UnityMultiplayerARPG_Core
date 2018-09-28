namespace MultiplayerARPG
{
    public struct SocialCharacterData
    {
        public const byte FLAG_ONLINE = 1 << 0;

        public string id;
        public string characterName;
        public int dataId;
        public int level;
        /// <summary>
        /// Member flags require 1 to check online state, so party / guild / etc flags will have online flag with value = 1
        /// </summary>
        public byte memberFlags;
        public int currentHp;
        public int maxHp;
        public int currentMp;
        public int maxMp;
        
        public bool IsOnline()
        {
            return (memberFlags & FLAG_ONLINE) != 0;
        }
    }
}
