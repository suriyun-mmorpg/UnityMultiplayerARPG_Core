using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using MultiplayerARPG;

public partial class PlayerCharacterSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        var data = (PlayerCharacterData)obj;
        info.AddValue("id", data.Id);
        info.AddValue("dataId", data.DataId);
        info.AddValue("characterName", data.CharacterName);
        info.AddValue("level", data.Level);
        info.AddValue("exp", data.Exp);
        info.AddValue("currentHp", data.CurrentHp);
        info.AddValue("currentMp", data.CurrentMp);
        info.AddValue("currentStamina", data.CurrentStamina);
        info.AddValue("currentFood", data.CurrentFood);
        info.AddValue("currentWater", data.CurrentWater);
        info.AddValue("equipWeapons", data.EquipWeapons);
        info.AddListValue("attributes", data.Attributes);
        info.AddListValue("skills", data.Skills);
        info.AddListValue("buffs", data.Buffs);
        info.AddListValue("equipItems", data.EquipItems);
        info.AddListValue("nonEquipItems", data.NonEquipItems);
        // Player Character
        info.AddValue("statPoint", data.StatPoint);
        info.AddValue("skillPoint", data.SkillPoint);
        info.AddValue("gold", data.Gold);
        info.AddValue("currentMapName", data.CurrentMapName);
        info.AddValue("currentPosition", data.CurrentPosition);
        info.AddValue("respawnMapName", data.RespawnMapName);
        info.AddValue("respawnPosition", data.RespawnPosition);
        info.AddValue("lastUpdate", data.LastUpdate);
        info.AddListValue("hotkeys", data.Hotkeys);
        info.AddListValue("quests", data.Quests);
        this.InvokeInstanceDevExtMethods("GetObjectData", obj, info, context);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        var data = (PlayerCharacterData)obj;
        data.Id = info.GetString("id");
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("databaseId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId))
            data.DataId = stringId.GenerateHashId();
        else
            data.DataId = info.GetInt32("dataId");
        data.CharacterName = info.GetString("characterName");
        data.Level = info.GetInt16("level");
        data.Exp = info.GetInt32("exp");
        data.CurrentHp = info.GetInt32("currentHp");
        data.CurrentMp = info.GetInt32("currentMp");
        data.CurrentStamina = info.GetInt32("currentStamina");
        data.CurrentFood = info.GetInt32("currentFood");
        data.CurrentWater = info.GetInt32("currentWater");
        data.EquipWeapons = (EquipWeapons)info.GetValue("equipWeapons", typeof(EquipWeapons));
        data.Attributes = info.GetListValue<CharacterAttribute>("attributes");
        data.Skills = info.GetListValue<CharacterSkill>("skills");
        data.Buffs = info.GetListValue<CharacterBuff>("buffs");
        data.EquipItems = info.GetListValue<CharacterItem>("equipItems");
        data.NonEquipItems = info.GetListValue<CharacterItem>("nonEquipItems");
        // Player Character
        data.StatPoint = info.GetInt16("statPoint");
        data.SkillPoint = info.GetInt16("skillPoint");
        data.Gold = info.GetInt32("gold");
        data.CurrentMapName = info.GetString("currentMapName");
        data.CurrentPosition = (Vector3)info.GetValue("currentPosition", typeof(Vector3));
        data.RespawnMapName = info.GetString("respawnMapName");
        data.RespawnPosition = (Vector3)info.GetValue("respawnPosition", typeof(Vector3));
        data.LastUpdate = info.GetInt32("lastUpdate");
        data.Hotkeys = info.GetListValue<CharacterHotkey>("hotkeys");
        data.Quests = info.GetListValue<CharacterQuest>("quests");
        this.InvokeInstanceDevExtMethods("SetObjectData", obj, info, context, selector);
        obj = data;
        return obj;
    }
}
