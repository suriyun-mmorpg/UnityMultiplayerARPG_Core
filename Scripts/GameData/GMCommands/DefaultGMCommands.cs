using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultGMCommands : BaseGMCommands
    {
        /// <summary>
        /// Get list of all GM commands
        /// </summary>
        public const string Help = "/help";
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
        /// <summary>
        /// Set gold rate (0.1 = 1%, 1 = 100%)
        /// </summary>
        public const string GoldRate = "/gold_rate";
        /// <summary>
        /// Set exp rate (0.1 = 1%, 1 = 100%)
        /// </summary>
        public const string ExpRate = "/exp_rate";
        /// <summary>
        /// Warp to specific map
        /// </summary>
        public const string Warp = "/warp";
        /// <summary>
        /// Warp to specific character to specific map and position
        /// </summary>
        public const string WarpCharacter = "/warp_character";
        /// <summary>
        /// Warp to specific character
        /// </summary>
        public const string WarpToCharacter = "/warp_to_character";
        /// <summary>
        /// Summon specific character
        /// </summary>
        public const string Summon = "/summon";
        /// <summary>
        /// Summon monster
        /// </summary>
        public const string Monster = "/monster";
        /// <summary>
        /// Kill specific character
        /// </summary>
        public const string Kill = "/kill";
        /// <summary>
        /// Suicide
        /// </summary>
        public const string Suicide = "/suicide";
        /// <summary>
        /// Mute specific character
        /// </summary>
        public const string Mute = "/mute";
        /// <summary>
        /// Unmute specific character
        /// </summary>
        public const string Unmute = "/unmute";
        /// <summary>
        /// Ban specific character
        /// </summary>
        public const string Ban = "/ban";
        /// <summary>
        /// Unban specific character
        /// </summary>
        public const string Unban = "/unban";
        /// <summary>
        /// Kick specific character
        /// </summary>
        public const string Kick = "/kick";

        public const string HelpResponse = "/level {level} = Set character's level to {level} value.\n" +
            "/statpoint {amount} = Set character's stat point to {amount} value.\n" +
            "/skillpoint {amount} = Set character's skill point to {amount} value.\n" +
            "/gold {amount} = Set character's gold to {amount} value.\n" +
            "/add_item {item_id} {amount} = Add item which its ID is {item_id}x{amount}.\n" +
            "/give_gold {name} {amount} = Increase {amount} of gold to character which its name is {name}.\n" +
            "/give_item {name} {item_id} {amount} = Add item which its ID is {item_id}x{amount} to character which its name is {name}.\n" +
            "/gold_rate {rate} = Set server's gold drop rate to {rate}.\n" +
            "/exp_rate {rate} = Set server's exp rewarding rate to {rate}.\n" +
            "/warp {map_id} = Warp to specific map.\n" +
            "/warp_character {name} {map_id} {x} {y} {z} = Warp to specific character to specific map and position.\n" +
            "/warp_to_character {name} = Warp to character which its name is {name}.\n" +
            "/summon {name} = Summon character which its name is {name}.\n" +
            "/monster {monster_id} {level} {amount} = Summon monster which its ID is {monster_id}, lv. {level}, amount {amount}.\n" +
            "/kill {name} = Kill character which its name is {name}.\n" +
            "/suicide = Kill yourself.\n" +
            "/mute {name} {duration} = Mute character which its name is {name} for {duration} minutes.\n" +
            "/unmute {name} = Unmute character which its name is {name}.\n" +
            "/ban {name} {duration} = Ban character's account which its name is {name} for {duration} days.\n" +
            "/unban {name} = Unban character's account which its name is {name}.\n" +
            "/kick {name} = Kick character which its name is {name}.\n";

        public virtual bool IsDataLengthValid(string command, int dataLength)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            if (command.ToLower().Equals(Help.ToLower()))
                return true;
            if (command.ToLower().Equals(Level.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(StatPoint.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(SkillPoint.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Gold.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(AddItem.ToLower()) && dataLength == 3)
                return true;
            if (command.ToLower().Equals(GiveGold.ToLower()) && dataLength == 3)
                return true;
            if (command.ToLower().Equals(GiveItem.ToLower()) && dataLength == 4)
                return true;
            if (command.ToLower().Equals(GoldRate.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(ExpRate.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Warp.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(WarpCharacter.ToLower()) && dataLength == 6)
                return true;
            if (command.ToLower().Equals(WarpToCharacter.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Summon.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Monster.ToLower()) && dataLength == 4)
                return true;
            if (command.ToLower().Equals(Kill.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Suicide.ToLower()))
                return true;
            if (command.ToLower().Equals(Mute.ToLower()) && dataLength == 3)
                return true;
            if (command.ToLower().Equals(Unmute.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Ban.ToLower()) && dataLength == 3)
                return true;
            if (command.ToLower().Equals(Unban.ToLower()) && dataLength == 2)
                return true;
            if (command.ToLower().Equals(Kick.ToLower()) && dataLength == 2)
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
            if (command.ToLower().Equals(Help.ToLower()) ||
                command.ToLower().Equals(Level.ToLower()) ||
                command.ToLower().Equals(StatPoint.ToLower()) ||
                command.ToLower().Equals(SkillPoint.ToLower()) ||
                command.ToLower().Equals(Gold.ToLower()) ||
                command.ToLower().Equals(AddItem.ToLower()) ||
                command.ToLower().Equals(GiveGold.ToLower()) ||
                command.ToLower().Equals(GiveItem.ToLower()) ||
                command.ToLower().Equals(GoldRate.ToLower()) ||
                command.ToLower().Equals(ExpRate.ToLower()) ||
                command.ToLower().Equals(Warp.ToLower()) ||
                command.ToLower().Equals(WarpCharacter.ToLower()) ||
                command.ToLower().Equals(WarpToCharacter.ToLower()) ||
                command.ToLower().Equals(Summon.ToLower()) ||
                command.ToLower().Equals(Monster.ToLower()) ||
                command.ToLower().Equals(Kill.ToLower()) ||
                command.ToLower().Equals(Suicide.ToLower()) ||
                command.ToLower().Equals(Mute.ToLower()) ||
                command.ToLower().Equals(Unmute.ToLower()) ||
                command.ToLower().Equals(Ban.ToLower()) ||
                command.ToLower().Equals(Unban.ToLower()) ||
                command.ToLower().Equals(Kick.ToLower()))
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

        public override string HandleGMCommand(BasePlayerCharacterEntity characterEntity, string chatMessage)
        {
            if (string.IsNullOrEmpty(chatMessage))
                return string.Empty;

            string response = string.Empty;
            string[] data = chatMessage.Split(' ');
            string commandKey = data[0];
            string receiver;
            BasePlayerCharacterEntity targetCharacter;
            if (IsDataLengthValid(commandKey, data.Length))
            {
                if (commandKey.ToLower().Equals(Help.ToLower()))
                {
                    response = HelpResponse;
                }
                if (commandKey.ToLower().Equals(Level.ToLower()))
                {
                    short amount;
                    if (!short.TryParse(data[1], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        characterEntity.Level = amount;
                        response = $"Set character level to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(StatPoint.ToLower()))
                {
                    int amount;
                    if (!int.TryParse(data[1], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        characterEntity.StatPoint = amount;
                        response = $"Set character statpoint to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(SkillPoint.ToLower()))
                {
                    int amount;
                    if (!int.TryParse(data[1], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        characterEntity.SkillPoint = amount;
                        response = $"Set character skillpoint to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(Gold.ToLower()))
                {
                    short amount;
                    if (!short.TryParse(data[1], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        characterEntity.Gold = amount;
                        response = $"Set character gold to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(AddItem.ToLower()))
                {
                    BaseItem item;
                    short amount;
                    if (!short.TryParse(data[2], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (!GameInstance.Items.TryGetValue(data[1].GenerateHashId(), out item))
                    {
                        response = "Cannot find the item";
                    }
                    else if (characterEntity != null)
                    {
                        if (amount > item.MaxStack)
                            amount = item.MaxStack;
                        if (characterEntity.IncreasingItemsWillOverwhelming(item.DataId, amount))
                        {
                            response = $"Cannot add item {item.Title}x{amount}, cannot carry any more of those items";
                        }
                        else
                        {
                            characterEntity.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                            response = $"Add item {item.Title}x{amount} to character's inventory";
                        }
                    }
                }
                if (commandKey.ToLower().Equals(GiveGold.ToLower()))
                {
                    receiver = data[1];
                    short amount;
                    if (!short.TryParse(data[2], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out targetCharacter))
                    {
                        targetCharacter.Gold = targetCharacter.Gold.Increase(amount);
                        response = $"Add gold for character: {receiver}";
                    }
                }
                if (commandKey.ToLower().Equals(GiveItem.ToLower()))
                {
                    receiver = data[1];
                    BaseItem item;
                    short amount;
                    if (!short.TryParse(data[3], out amount) || amount <= 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (!GameInstance.Items.TryGetValue(data[2].GenerateHashId(), out item))
                    {
                        response = "Cannot find the item";
                    }
                    else if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(receiver, out targetCharacter))
                    {
                        if (amount > item.MaxStack)
                            amount = item.MaxStack;
                        if (targetCharacter.IncreasingItemsWillOverwhelming(item.DataId, amount))
                        {
                            response = $"Cannot add item {item.Title}x{amount} to {receiver}'s inventory, cannot carry any more of those items";
                        }
                        else
                        {
                            targetCharacter.AddOrSetNonEquipItems(CharacterItem.Create(item, 1, amount));
                            response = $"Add item {item.Title}x{amount} to {receiver}'s inventory";
                        }
                    }
                }
                if (commandKey.ToLower().Equals(GoldRate.ToLower()))
                {
                    float amount;
                    if (!float.TryParse(data[1], out amount) || amount < 0f)
                    {
                        response = "Wrong input data";
                    }
                    else
                    {
                        GameInstance.Singleton.GameplayRule.GoldRate = amount;
                        response = $"Set gold rate to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(ExpRate.ToLower()))
                {
                    float amount;
                    if (!float.TryParse(data[1], out amount) || amount < 0f)
                    {
                        response = "Wrong input data";
                    }
                    else
                    {
                        GameInstance.Singleton.GameplayRule.ExpRate = amount;
                        response = $"Set exp rate to {amount}";
                    }
                }
                if (commandKey.ToLower().Equals(Warp.ToLower()))
                {
                    if (!GameInstance.MapInfos.ContainsKey(data[1]))
                    {
                        response = "Cannot find the map";
                    }
                    else if (characterEntity != null)
                    {
                        BaseMapInfo mapInfo = GameInstance.MapInfos[data[1]];
                        BaseGameNetworkManager.Singleton.WarpCharacter(characterEntity, data[1], mapInfo.StartPosition, false, Vector3.zero);
                        response = $"Warping to: {data[1]} {mapInfo.StartPosition}";
                    }
                }
                if (commandKey.ToLower().Equals(WarpCharacter.ToLower()))
                {
                    float x, y, z;
                    if (!float.TryParse(data[3], out x) || 
                        !float.TryParse(data[4], out y) || 
                        !float.TryParse(data[5], out z))
                    {
                        response = "Wrong input data";
                    }
                    else if (!GameInstance.MapInfos.ContainsKey(data[2]))
                    {
                        response = "Cannot find the map";
                    }
                    else if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(data[1], out targetCharacter))
                    {
                        BaseGameNetworkManager.Singleton.WarpCharacter(targetCharacter, data[1], new Vector3(x,y,z), false, Vector3.zero);
                    }
                }
                if (commandKey.ToLower().Equals(WarpToCharacter.ToLower()))
                {
                    // TODO: Add function to find character from all maps
                    if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(data[1], out targetCharacter))
                    {
                        response = "Cannot find the character";
                    }
                    else if (characterEntity != null)
                    {
                        BaseGameNetworkManager.Singleton.WarpCharacter(characterEntity, targetCharacter.CurrentMapName, targetCharacter.CurrentPosition, false, Vector3.zero);
                        response = $"Warping to: {targetCharacter.CurrentMapName} {targetCharacter.CurrentPosition}";
                    }
                }
                if (commandKey.ToLower().Equals(Summon.ToLower()))
                {
                    // TODO: Add function to find character from all maps
                    if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(data[1], out targetCharacter))
                    {
                        response = "Cannot find the character";
                    }
                    else if (characterEntity != null)
                    {
                        BaseGameNetworkManager.Singleton.WarpCharacter(targetCharacter, characterEntity.CurrentMapName, characterEntity.CurrentPosition, false, Vector3.zero);
                        response = $"Summon character: {data[1]}";
                    }
                }
                if (commandKey.ToLower().Equals(Monster.ToLower()))
                {
                    BaseMonsterCharacterEntity targetMonster = null;
                    foreach (BaseMonsterCharacterEntity monster in GameInstance.MonsterCharacterEntities.Values)
                    {
                        if (monster.name.Equals(data[1]) ||
                            monster.Identity.AssetId.Equals(data[1]))
                        {
                            targetMonster = monster;
                            break;
                        }
                    }
                    short level;
                    int amount;
                    if (targetMonster == null)
                    {
                        response = "Cannot find the monster";
                    }
                    else if (!short.TryParse(data[2], out level) || !int.TryParse(data[3], out amount))
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        for (int i = 0; i < amount; ++i)
                        {
                            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(targetMonster.Identity.HashAssetId, characterEntity.MovementTransform.position, Quaternion.identity);
                            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
                            entity.Level = level;
                            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                        }
                    }
                }
                if (commandKey.ToLower().Equals(Kill.ToLower()))
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(data[1], out targetCharacter))
                    {
                        if (targetCharacter.IsDead())
                        {
                            response = "Character is already dead";
                        }
                        else
                        {
                            targetCharacter.CurrentHp = 0;
                            targetCharacter.Killed(EntityInfo.Empty);
                            response = $"Kill character: {data[1]}";
                        }
                    }
                }
                if (commandKey.ToLower().Equals(Suicide.ToLower()))
                {
                    if (characterEntity != null)
                    {
                        characterEntity.CurrentHp = 0;
                        characterEntity.Killed(EntityInfo.Empty);
                        response = "Suicided";
                    }
                }
                if (commandKey.ToLower().Equals(Mute.ToLower()))
                {
                    int amount;
                    if (!int.TryParse(data[2], out amount) || amount < 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        GameInstance.ServerUserHandlers.MuteCharacterByName(data[1], amount);
                        response = $"Mute character named: {data[1]}";
                    }
                }
                if (commandKey.ToLower().Equals(Unmute.ToLower()))
                {
                    if (characterEntity != null)
                    {
                        GameInstance.ServerUserHandlers.UnmuteCharacterByName(data[1]);
                        response = $"Unmute character named: {data[1]}";
                    }
                }
                if (commandKey.ToLower().Equals(Ban.ToLower()))
                {
                    int amount;
                    if (!int.TryParse(data[2], out amount) || amount < 0)
                    {
                        response = "Wrong input data";
                    }
                    else if (characterEntity != null)
                    {
                        GameInstance.ServerUserHandlers.BanUserByCharacterName(data[1], amount);
                        response = $"Ban user's who own character named: {data[1]}";
                    }
                }
                if (commandKey.ToLower().Equals(Unban.ToLower()))
                {
                    if (characterEntity != null)
                    {
                        GameInstance.ServerUserHandlers.UnbanUserByCharacterName(data[1]);
                        response = $"Unban user's who own character named: {data[1]}";
                    }
                }
                if (commandKey.ToLower().Equals(Kick.ToLower()))
                {
                    if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterByName(data[1], out targetCharacter))
                    {
                        response = "Cannot find the character";
                    }
                    else
                    {
                        BaseGameNetworkManager.Singleton.Transport.ServerDisconnect(targetCharacter.ConnectionId);
                        response = $"Kick character: {data[1]}";
                    }
                }
            }
            return response;
        }
    }
}
