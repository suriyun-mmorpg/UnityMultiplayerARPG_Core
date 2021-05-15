namespace MultiplayerARPG
{
    public class DefaultGMCommands : BaseGMCommands
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

        public virtual bool IsDataLengthValid(string command, int dataLength)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            if (command.Equals(Level) && dataLength == 2)
                return true;
            if (command.Equals(StatPoint) && dataLength == 2)
                return true;
            if (command.Equals(SkillPoint) && dataLength == 2)
                return true;
            if (command.Equals(Gold) && dataLength == 2)
                return true;
            if (command.Equals(AddItem) && dataLength == 3)
                return true;
            if (command.Equals(GiveGold) && dataLength == 3)
                return true;
            if (command.Equals(GiveItem) && dataLength == 4)
                return true;

            return false;
        }

        public override bool IsGMCommand(string chatMessage, out string command)
        {
            command = string.Empty;
            if (string.IsNullOrEmpty(chatMessage))
                return false;

            string[] splited = chatMessage.Split(' ');
            command = splited[0];
            if (command.Equals(Level) ||
                command.Equals(StatPoint) ||
                command.Equals(SkillPoint) ||
                command.Equals(Gold) ||
                command.Equals(AddItem) ||
                command.Equals(GiveGold) ||
                command.Equals(GiveItem))
            {
                return true;
            }
            command = string.Empty;
            return false;
        }

        public override bool CanUseGMCommand(BasePlayerCharacterEntity characterEntity, string command)
        {
            // TODO: May allow user to use some GM commands by their user level.
            return characterEntity != null && characterEntity.UserLevel > 0;
        }

        public override void HandleGMCommand(string sender, string chatMessage)
        {
            if (string.IsNullOrEmpty(chatMessage))
                return;

            string[] data = chatMessage.Split(' ');
            string commandKey = data[0];
            string receiver;
            BasePlayerCharacterEntity playerCharacter;
            if (IsDataLengthValid(commandKey, data.Length))
            {
                if (commandKey.Equals(Level))
                {
                    receiver = sender;
                    short amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount > 0)
                        playerCharacter.Level = amount;
                }
                if (commandKey.Equals(StatPoint))
                {
                    receiver = sender;
                    int amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        int.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.StatPoint = amount;
                }
                if (commandKey.Equals(SkillPoint))
                {
                    receiver = sender;
                    int amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        int.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.SkillPoint = amount;
                }
                if (commandKey.Equals(Gold))
                {
                    receiver = sender;
                    short amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.Gold = amount;
                }
                if (commandKey.Equals(AddItem))
                {
                    receiver = sender;
                    BaseItem item;
                    short amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        GameInstance.Items.TryGetValue(data[1].GenerateHashId(), out item) &&
                        short.TryParse(data[2], out amount))
                    {
                        if (amount > item.MaxStack)
                            amount = item.MaxStack;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                    }
                }
                if (commandKey.Equals(GiveGold))
                {
                    receiver = data[1];
                    short amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[2], out amount))
                        playerCharacter.Gold = playerCharacter.Gold.Increase(amount);
                }
                if (commandKey.Equals(GiveItem))
                {
                    receiver = data[1];
                    BaseItem item;
                    short amount;
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        GameInstance.Items.TryGetValue(data[2].GenerateHashId(), out item) &&
                        short.TryParse(data[3], out amount))
                    {
                        if (amount > item.MaxStack)
                            amount = item.MaxStack;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                    }
                }
            }
        }
    }
}
