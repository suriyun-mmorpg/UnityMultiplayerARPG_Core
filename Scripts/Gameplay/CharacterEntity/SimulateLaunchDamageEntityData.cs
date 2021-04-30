using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct SimulateLaunchDamageEntityData : INetSerializable
    {
        public SimulateLaunchDamageEntityState state;
        public int skillDataId;
        public short skillLevel;
        public byte randomSeed;
        public int hitIndex;
        public AimPosition aimPosition;
        public long time;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)state);
            writer.Put(randomSeed);
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                writer.PutPackedInt(skillDataId);
                writer.PutPackedShort(skillLevel);
                writer.PutPackedInt(hitIndex);
            }
            writer.PutValue(aimPosition);
            writer.PutPackedLong(time);
        }

        public void Deserialize(NetDataReader reader)
        {
            state = (SimulateLaunchDamageEntityState)reader.GetByte();
            randomSeed = reader.GetByte();
            if (state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                skillDataId = reader.GetPackedInt();
                skillLevel = reader.GetPackedShort();
                hitIndex = reader.GetPackedInt();
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
