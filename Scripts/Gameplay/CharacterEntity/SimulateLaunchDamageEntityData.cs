using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct SimulateLaunchDamageEntityData : INetSerializable
    {
        public SimulateLaunchDamageEntityState state;
        public int skillDataId;
        public short skillLevel;
        public byte randomSeed;
        public int hitIndex;
        public Vector3 aimPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)state);
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                writer.PutPackedInt(skillDataId);
                writer.PutPackedShort(skillLevel);
                writer.PutPackedInt(hitIndex);
            }
            else
            {
                writer.Put(randomSeed);
            }
            writer.PutVector3(aimPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            state = (SimulateLaunchDamageEntityState)reader.GetByte();
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                skillDataId = reader.GetPackedInt();
                skillLevel = reader.GetPackedShort();
                hitIndex = reader.GetPackedInt();
            }
            else
            {
                randomSeed = reader.GetByte();
            }
            aimPosition = reader.GetVector3();
        }

        public BaseSkill GetSkill()
        {
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                BaseSkill skill;
                if (GameInstance.Skills.TryGetValue(skillDataId, out skill))
                    return skill;
            }
            return null;
        }
    }
}
