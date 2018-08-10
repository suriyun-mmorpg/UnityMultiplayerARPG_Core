using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashPackageBuyValidationMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
            CharacterNotFound,
            PackageNotFound,
            Invalid,
        }
        public Error error;
        public int dataId;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            dataId = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(dataId);
        }
    }
}
