using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct SimulateLaunchDamageEntityData : INetSerializable
    {
        public SimulateLaunchDamageEntityState state;
        public int skillDataId;
        public short skillLevel;
        public int randomSeed;
        public uint targetObjectId;
        public AimPosition aimPosition;
        public long time;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)state);
            writer.PutPackedInt(randomSeed);
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                writer.PutPackedInt(skillDataId);
                writer.PutPackedShort(skillLevel);
                writer.PutPackedUInt(targetObjectId);
            }
            writer.PutValue(aimPosition);
            writer.PutPackedLong(time);
        }

        public void Deserialize(NetDataReader reader)
        {
            state = (SimulateLaunchDamageEntityState)reader.GetByte();
            randomSeed = reader.GetPackedInt();
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                skillDataId = reader.GetPackedInt();
                skillLevel = reader.GetPackedShort();
                targetObjectId = reader.GetPackedUInt();
            }
            aimPosition = reader.GetValue<AimPosition>();
            time = reader.GetPackedLong();
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
