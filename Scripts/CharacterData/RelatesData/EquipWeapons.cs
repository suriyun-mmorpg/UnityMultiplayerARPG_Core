using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public class EquipWeapons : INetSerializable
{
    public CharacterItem rightHand;
    public CharacterItem leftHand;

    public EquipWeapons()
    {
        rightHand = new CharacterItem();
        leftHand = new CharacterItem();
    }

    public void Serialize(NetDataWriter writer)
    {
        // Right hand
        writer.Put(rightHand.dataId);
        writer.Put(rightHand.level);
        writer.Put(rightHand.amount);
        writer.Put(rightHand.durability);
        // Left hand
        writer.Put(leftHand.dataId);
        writer.Put(leftHand.level);
        writer.Put(leftHand.amount);
        writer.Put(leftHand.durability);
    }

    public void Deserialize(NetDataReader reader)
    {
        // Right hand
        rightHand = new CharacterItem();
        rightHand.dataId = reader.GetInt();
        rightHand.level = reader.GetShort();
        rightHand.amount = reader.GetShort();
        rightHand.durability = reader.GetFloat();
        // Left hand
        leftHand = new CharacterItem();
        leftHand.dataId = reader.GetInt();
        leftHand.level = reader.GetShort();
        leftHand.amount = reader.GetShort();
        leftHand.durability = reader.GetFloat();
    }
}

[System.Serializable]
public class SyncFieldEquipWeapons : LiteNetLibSyncField<EquipWeapons>
{
}
