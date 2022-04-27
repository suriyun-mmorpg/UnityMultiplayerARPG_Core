using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct SimulateActionTriggerData : INetSerializable
    {
        public SimulateActionTriggerState state;
        public int randomSeed;
        public uint targetObjectId;
        public int skillDataId;
        public short skillLevel;
        public AimPosition aimPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)state);
            writer.PutPackedInt(randomSeed);
            if (state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                writer.PutPackedUInt(targetObjectId);
                writer.PutPackedInt(skillDataId);
                writer.PutPackedShort(skillLevel);
            }
            writer.Put(aimPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            state = (SimulateActionTriggerState)reader.GetByte();
            randomSeed = reader.GetPackedInt();
            if (state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                targetObjectId = reader.GetPackedUInt();
                skillDataId = reader.GetPackedInt();
                skillLevel = reader.GetPackedShort();
            }
            aimPosition = reader.Get<AimPosition>();
        }

        public BaseSkill GetSkill()
        {
            if (state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                BaseSkill skill;
                if (GameInstance.Skills.TryGetValue(skillDataId, out skill))
                    return skill;
            }
            return null;
        }
    }
}
