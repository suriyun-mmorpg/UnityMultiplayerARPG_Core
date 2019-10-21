using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        public void HandleGMCommand(string sender, string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            string[] splited = command.Split(' ');
            string commandKey = splited[0];
            string receiver;
            BasePlayerCharacterEntity playerCharacter;
            if (GMCommands.IsSplitedLengthValid(commandKey, splited.Length))
            {
                if (commandKey.Equals(GMCommands.Level))
                {
                    receiver = sender;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(splited[1], out amount) &&
                        amount > 0)
                        playerCharacter.Level = amount;
                }
                if (commandKey.Equals(GMCommands.StatPoint))
                {
                    receiver = sender;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(splited[1], out amount) &&
                        amount >= 0)
                        playerCharacter.StatPoint = amount;
                }
                if (commandKey.Equals(GMCommands.SkillPoint))
                {
                    receiver = sender;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(splited[1], out amount) &&
                        amount >= 0)
                        playerCharacter.SkillPoint = amount;
                }
                if (commandKey.Equals(GMCommands.Gold))
                {
                    receiver = sender;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(splited[1], out amount) &&
                        amount >= 0)
                        playerCharacter.Gold = amount;
                }
                if (commandKey.Equals(GMCommands.AddItem))
                {
                    receiver = sender;
                    Item item;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        GameInstance.Items.TryGetValue(splited[1].GenerateHashId(), out item) &&
                        short.TryParse(splited[2], out amount))
                    {
                        if (amount > item.maxStack)
                            amount = item.maxStack;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                    }
                }
                if (commandKey.Equals(GMCommands.GiveGold))
                {
                    receiver = splited[1];
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        short.TryParse(splited[2], out amount))
                        playerCharacter.Gold += amount;
                }
                if (commandKey.Equals(GMCommands.GiveItem))
                {
                    receiver = splited[1];
                    Item item;
                    short amount;
                    if (TryGetPlayerCharacterByName(receiver, out playerCharacter) &&
                        GameInstance.Items.TryGetValue(splited[2].GenerateHashId(), out item) &&
                        short.TryParse(splited[3], out amount))
                    {
                        if (amount > item.maxStack)
                            amount = item.maxStack;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                    }
                }
            }
        }
    }
}
