using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public struct EquipWeapons
{
    public CharacterItem rightHand;
    public CharacterItem leftHand;
}

public class NetFieldEquipWeapons : LiteNetLibNetField<EquipWeapons>
{
    public override void Deserialize(NetDataReader reader)
    {
        // Right hand
        var rightHand = new CharacterItem();
        rightHand.id = reader.GetString();
        rightHand.dataId = reader.GetInt();
        rightHand.level = reader.GetInt();
        rightHand.amount = reader.GetInt();
        // Left hand
        var leftHand = new CharacterItem();
        leftHand.id = reader.GetString();
        leftHand.dataId = reader.GetInt();
        leftHand.level = reader.GetInt();
        leftHand.amount = reader.GetInt();
        // Set result
        var newValue = new EquipWeapons();
        newValue.rightHand = rightHand;
        newValue.leftHand = leftHand;
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        // Right hand
        writer.Put(Value.rightHand.id);
        writer.Put(Value.rightHand.dataId);
        writer.Put(Value.rightHand.level);
        writer.Put(Value.rightHand.amount);
        // Left hand
        writer.Put(Value.leftHand.id);
        writer.Put(Value.leftHand.dataId);
        writer.Put(Value.leftHand.level);
        writer.Put(Value.leftHand.amount);
    }

    public override bool IsValueChanged(EquipWeapons newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncFieldEquipWeapons : LiteNetLibSyncField<NetFieldEquipWeapons, EquipWeapons> { }