using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct AimPosition : INetSerializable
    {
        public bool hasValue;
        public Vector3 value;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(hasValue);
            if (hasValue)
                writer.PutVector3(value);
        }

        public void Deserialize(NetDataReader reader)
        {
            hasValue = reader.GetBool();
            if (hasValue)
                value = reader.GetVector3();
        }

        public static AimPosition CreateForAttack(BasePlayerCharacterEntity entity, ref bool isLeftHand)
        {
            return new AimPosition()
            {
                hasValue = true,
                value = entity.GetDefaultAttackAimPosition(ref isLeftHand),
            };
        }

        public static AimPosition CreateForSkill(BasePlayerCharacterEntity entity, Vector3? vector3, bool isAttack, ref bool isLeftHand)
        {
            AimPosition aimPosition = new AimPosition();
            aimPosition.hasValue = vector3.HasValue;
            if (aimPosition.hasValue)
            {
                aimPosition.value = vector3.Value;
            }
            else if (isAttack)
            {
                aimPosition.value = entity.GetDefaultAttackAimPosition(ref isLeftHand);
            }
            return aimPosition;
        }
    }
}
