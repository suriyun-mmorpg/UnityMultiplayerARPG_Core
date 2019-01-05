namespace MultiplayerARPG
{
    public class GMCommands
    {
        /// <summary>
        /// Set character level: /level {level}
        /// </summary>
        public const string Level = "/level";
        /// <summary>
        /// Set character stat point: /statpoint {Stat Point}
        /// </summary>
        public const string StatPoint = "/statpoint";
        /// <summary>
        /// Set character skill point: /skillpoint {Skill Point}
        /// </summary>
        public const string SkillPoint = "/skillpoint";
        /// <summary>
        /// Set character gold: /gold {Gold}
        /// </summary>
        public const string Gold = "/gold";
        /// <summary>
        /// Give character item: /item {Item Id} {amount}
        /// </summary>
        public const string AddItem = "/add_item";
        /// <summary>
        /// Give gold to another character: /give_gold {Character Name} {Gold}
        /// </summary>
        public const string GiveGold = "/give_gold";
        /// <summary>
        /// Give item to another character: /give_item {Character Name} {Item Id} {amount}
        /// </summary>
        public const string GiveItem = "/give_item";

        public static bool IsSplitedLengthValid(string commandKey, int splitedLength)
        {
            if (string.IsNullOrEmpty(commandKey))
                return false;

            if (commandKey.Equals(Level) && splitedLength == 2)
                return true;
            if (commandKey.Equals(StatPoint) && splitedLength == 2)
                return true;
            if (commandKey.Equals(SkillPoint) && splitedLength == 2)
                return true;
            if (commandKey.Equals(Gold) && splitedLength == 2)
                return true;
            if (commandKey.Equals(AddItem) && splitedLength == 3)
                return true;
            if (commandKey.Equals(GiveGold) && splitedLength == 3)
                return true;
            if (commandKey.Equals(GiveItem) && splitedLength == 4)
                return true;

            return false;
        }

        public static bool IsGMCommand(string enterMessage)
        {
            if (string.IsNullOrEmpty(enterMessage))
                return false;

            string[] splited = enterMessage.Split(' ');
            string commandKey = splited[0];
            if (commandKey.Equals(Level))
                return true;
            if (commandKey.Equals(StatPoint))
                return true;
            if (commandKey.Equals(SkillPoint))
                return true;
            if (commandKey.Equals(Gold))
                return true;
            if (commandKey.Equals(AddItem))
                return true;
            if (commandKey.Equals(GiveGold))
                return true;
            if (commandKey.Equals(GiveItem))
                return true;

            return false;
        }
    }
}
