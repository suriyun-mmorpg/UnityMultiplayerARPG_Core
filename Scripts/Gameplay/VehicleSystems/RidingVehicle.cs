using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct RidingVehicle : INetSerializable
    {
        public uint objectId;
        public byte seatIndex;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(objectId);
            writer.Put(seatIndex);
        }

        public void Deserialize(NetDataReader reader)
        {
            objectId = reader.GetPackedUInt();
            seatIndex = reader.GetByte();
        }
    }

    [System.Serializable]
    public class SyncFieldRidingVehicle : LiteNetLibSyncField<RidingVehicle>
    {
        protected override bool IsValueChanged(RidingVehicle newValue)
        {
            return true;
        }
    }
}
