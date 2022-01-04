using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum SkillUsageType : byte
    {
        Skill,
        GuildSkill,
    }

    [System.Serializable]
    public class CharacterSkillUsage : INetSerializable
    {
        public static readonly CharacterSkillUsage Empty = new CharacterSkillUsage();
        public SkillUsageType type;
        public int dataId;
        public float coolDownRemainsDuration;

        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private GuildSkill cacheGuildSkill;

        private void MakeCache()
        {
            if (dirtyDataId != dataId)
            {
                dirtyDataId = dataId;
                cacheSkill = null;
                cacheGuildSkill = null;
                switch (type)
                {
                    case SkillUsageType.Skill:
                        GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
                        break;
                    case SkillUsageType.GuildSkill:
                        GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill);
                        break;
                }
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public GuildSkill GetGuildSkill()
        {
            MakeCache();
            return cacheGuildSkill;
        }

        public void Use(ICharacterData character, short level)
        {
            coolDownRemainsDuration = 0f;
            switch (type)
            {
                case SkillUsageType.Skill:
                    if (GetSkill() != null)
                    {
                        coolDownRemainsDuration = GetSkill().GetCoolDownDuration(level);
                        int tempAmount;
                        // Consume HP
                        tempAmount = GetSkill().GetTotalConsumeHp(level, character);
                        if (tempAmount < 0)
                            tempAmount = 0;
                        character.CurrentHp -= tempAmount;
                        // Consume MP
                        tempAmount = GetSkill().GetTotalConsumeMp(level, character);
                        if (tempAmount < 0)
                            tempAmount = 0;
                        character.CurrentMp -= tempAmount;
                        // Consume Stamina
                        tempAmount = GetSkill().GetTotalConsumeStamina(level, character);
                        if (tempAmount < 0)
                            tempAmount = 0;
                        character.CurrentStamina -= tempAmount;
                    }
                    break;
                case SkillUsageType.GuildSkill:
                    if (GetGuildSkill() != null)
                    {
                        coolDownRemainsDuration = GetGuildSkill().GetCoolDownDuration(level);
                    }
                    break;
            }
        }

        public bool ShouldRemove()
        {
            return coolDownRemainsDuration <= 0f;
        }

        public void Update(float deltaTime)
        {
            coolDownRemainsDuration -= deltaTime;
        }

        public CharacterSkillUsage Clone()
        {
            return new CharacterSkillUsage()
            {
                type = type,
                dataId = dataId,
                coolDownRemainsDuration = coolDownRemainsDuration,
            };
        }

        public static CharacterSkillUsage Create(SkillUsageType type, int dataId)
        {
            return new CharacterSkillUsage()
            {
                type = type,
                dataId = dataId,
                coolDownRemainsDuration = 0f,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.PutPackedInt(dataId);
            writer.Put(coolDownRemainsDuration);
        }

        public void Deserialize(NetDataReader reader)
        {
            type = (SkillUsageType)reader.GetByte();
            dataId = reader.GetPackedInt();
            coolDownRemainsDuration = reader.GetFloat();
        }
    }

    [System.Serializable]
    public sealed class SyncListCharacterSkillUsage : LiteNetLibSyncList<CharacterSkillUsage>
    {
        protected override CharacterSkillUsage DeserializeValueForSetOrDirty(int index, NetDataReader reader)
        {
            CharacterSkillUsage result = this[index];
            result.coolDownRemainsDuration = reader.GetFloat();
            return result;
        }

        protected override void SerializeValueForSetOrDirty(int index, NetDataWriter writer, CharacterSkillUsage value)
        {
            writer.Put(value.coolDownRemainsDuration);
        }
    }
}
