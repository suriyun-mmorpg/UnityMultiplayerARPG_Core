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
        rightHand.dataId = reader.GetInt();
        rightHand.level = reader.GetShort();
        rightHand.amount = reader.GetShort();
        // Left hand
        var leftHand = new CharacterItem();
        leftHand.dataId = reader.GetInt();
        leftHand.level = reader.GetShort();
        leftHand.amount = reader.GetShort();
        // Set result
        var newValue = new EquipWeapons();
        newValue.rightHand = rightHand;
        newValue.leftHand = leftHand;
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        // Right hand
        writer.Put(Value.rightHand.dataId);
        writer.Put(Value.rightHand.level);
        writer.Put(Value.rightHand.amount);
        // Left hand
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