using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterHotkey
{
    public const byte HOTKEY_TYPE_NONE = 0;
    public const byte HOTKEY_TYPE_SKILL = 1;
    public const byte HOTKEY_TYPE_ITEM = 2;

    public byte type;
    public int dataIndex;

    public CharacterSkill GetCharacterSkill(ICharacterData characterData)
    {
        if (type == HOTKEY_TYPE_SKILL && dataIndex >= 0 && dataIndex < characterData.Skills.Count)
            return characterData.Skills[dataIndex];
        return CharacterSkill.Empty;
    }

    public CharacterItem GetCharacterItem(ICharacterData characterData)
    {
        if (type == HOTKEY_TYPE_ITEM && dataIndex >= 0 && dataIndex < characterData.NonEquipItems.Count)
            return characterData.NonEquipItems[dataIndex];
        return CharacterItem.Empty;
    }
}

public class NetFieldCharacterHotkey : LiteNetLibNetField<CharacterHotkey>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterHotkey();
        newValue.type = reader.GetByte();
        newValue.dataIndex = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.type);
        writer.Put(Value.dataIndex);
    }

    public override bool IsValueChanged(CharacterHotkey newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<NetFieldCharacterHotkey, CharacterHotkey> { }

