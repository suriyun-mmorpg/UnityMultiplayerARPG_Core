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
        writer.PutValue(rightHand);
        // Left hand
        writer.PutValue(leftHand);
    }

    public void Deserialize(NetDataReader reader)
    {
        // Right hand
        rightHand = (CharacterItem)reader.GetValue(typeof(CharacterItem));
        // Left hand
        leftHand = (CharacterItem)reader.GetValue(typeof(CharacterItem));
    }
}

[System.Serializable]
public class SyncFieldEquipWeapons : LiteNetLibSyncField<EquipWeapons>
{
    protected override bool IsValueChanged(EquipWeapons newValue)
    {
        return true;
    }
}
