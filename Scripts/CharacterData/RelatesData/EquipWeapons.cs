using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public class EquipWeapons : INetSerializableWithElement
{
    public CharacterItem rightHand;
    public CharacterItem leftHand;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

    public EquipWeapons()
    {
        rightHand = new CharacterItem();
        leftHand = new CharacterItem();
    }

    private void Validate()
    {
        if (rightHand == null)
            rightHand = new CharacterItem();

        if (leftHand == null)
            leftHand = new CharacterItem();

        rightHand.Element = Element;
        leftHand.Element = Element;
    }

    public void Serialize(NetDataWriter writer)
    {
        Validate();
        // Right hand
        rightHand.Serialize(writer);
        // Left hand
        leftHand.Serialize(writer);
    }

    public void Serialize(NetDataWriter writer, bool isOwnerClient)
    {
        Validate();
        // Right hand
        rightHand.Serialize(writer, isOwnerClient);
        // Left hand
        leftHand.Serialize(writer, isOwnerClient);
    }

    public void Deserialize(NetDataReader reader)
    {
        Validate();
        // Right hand
        rightHand.Deserialize(reader);
        // Left hand
        leftHand.Deserialize(reader);
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


[System.Serializable]
public class SyncListEquipWeapons : LiteNetLibSyncList<EquipWeapons>
{
}
