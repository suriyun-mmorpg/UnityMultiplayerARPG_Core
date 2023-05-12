using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct SimulateActionTriggerData : INetSerializable
    {
        public SimulateActionTriggerState state;
        public int simulateSeed;
        public byte triggerIndex;
        public uint targetObjectId;
        public int skillDataId;
        public int skillLevel;
        public AimPosition aimPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)state);
            writer.PutPackedInt(simulateSeed);
            writer.Put(triggerIndex);
            if (state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                writer.PutPackedUInt(targetObjectId);
                writer.PutPackedInt(skillDataId);
                writer.PutPackedInt(skillLevel);
            }
            writer.Put(aimPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            state = (SimulateActionTriggerState)reader.GetByte();
            simulateSeed = reader.GetPackedInt();
            triggerIndex = reader.GetByte();
            if (state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                targetObjectId = reader.GetPackedUInt();
                skillDataId = reader.GetPackedInt();
                skillLevel = reader.GetPackedInt();
            }
            aimPosition = reader.Get<AimPosition>();
        }

        public BaseSkill GetSkill()
        {
            if (state.HasFlag(SimulateActionTriggerState.IsSkill) && GameInstance.Skills.TryGetValue(skillDataId, out BaseSkill skill))
                return skill;
            return null;
        }
    }
}
