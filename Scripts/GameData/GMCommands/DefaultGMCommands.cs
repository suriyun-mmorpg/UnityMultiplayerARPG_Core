using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public override bool IsGMCommand(string chatMessage)
        {
            if (string.IsNullOrEmpty(chatMessage))
                return false;

            string[] splited = chatMessage.Split(' ');
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

        public override void HandleGMCommand(BaseGameNetworkManager manager, string sender, string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            string[] data = command.Split(' ');
            string commandKey = data[0];
            string receiver;
            BasePlayerCharacterEntity playerCharacter;
            if (IsDataLengthValid(commandKey, data.Length))
            {
                if (commandKey.Equals(Level))
                {
                    receiver = sender;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount > 0)
                        playerCharacter.Level = amount;
                }
                if (commandKey.Equals(StatPoint))
                {
                    receiver = sender;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.StatPoint = amount;
                }
                if (commandKey.Equals(SkillPoint))
                {
                    receiver = sender;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.SkillPoint = amount;
                }
                if (commandKey.Equals(Gold))
                {
                    receiver = sender;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[1], out amount) &&
                        amount >= 0)
                        playerCharacter.Gold = amount;
                }
                if (commandKey.Equals(AddItem))
                {
                    receiver = sender;
                    BaseItem item;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
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
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(data[2], out amount))
                        playerCharacter.Gold += amount;
                }
                if (commandKey.Equals(GiveItem))
                {
                    receiver = data[1];
                    BaseItem item;
                    short amount;
                    if (manager.TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
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
