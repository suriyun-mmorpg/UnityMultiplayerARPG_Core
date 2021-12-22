using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from Resources folder
    /// </summary>
    [CreateAssetMenu(fileName = "Resources Folder Game Database", menuName = "Create GameDatabase/Resources Folder Game Database", order = -5998)]
    public partial class ResourcesFolderGameDatabase : BaseGameDatabase
    {
        protected override async UniTask LoadDataImplement(GameInstance gameInstance)
        {
            Attribute[] attributes = Resources.LoadAll<Attribute>("");
            BaseItem[] items = Resources.LoadAll<BaseItem>("");
            ItemCraftFormula[] itemCraftFormulas = Resources.LoadAll<ItemCraftFormula>("");
            BaseSkill[] skills = Resources.LoadAll<BaseSkill>("");
            BaseNpcDialog[] npcDialogs = Resources.LoadAll<BaseNpcDialog>("");
            Quest[] quests = Resources.LoadAll<Quest>("");
            GuildSkill[] guildSkills = Resources.LoadAll<GuildSkill>("");
            GuildIcon[] guildIcons = Resources.LoadAll<GuildIcon>("");
            StatusEffect[] statusEffects = Resources.LoadAll<StatusEffect>("");
            PlayerCharacter[] playerCharacters = Resources.LoadAll<PlayerCharacter>("");
            MonsterCharacter[] monsterCharacters = Resources.LoadAll<MonsterCharacter>("");
            BaseMapInfo[] mapInfos = Resources.LoadAll<BaseMapInfo>("");
            Faction[] factions = Resources.LoadAll<Faction>("");
            Gacha[] gachas = Resources.LoadAll<Gacha>("");
            BaseCharacterEntity[] characterEntities = Resources.LoadAll<BaseCharacterEntity>("");
            VehicleEntity[] vehicleEntities = Resources.LoadAll<VehicleEntity>("");
            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(0, itemCraftFormulas);
            GameInstance.AddSkills(skills);
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddGuildIcons(guildIcons);
            GameInstance.AddStatusEffects(statusEffects);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddFactions(factions);
            GameInstance.AddGachas(gachas);
            GameInstance.AddCharacterEntities(characterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            this.InvokeInstanceDevExtMethods("LoadDataImplement", gameInstance);
            await UniTask.Yield();
        }
    }
}
