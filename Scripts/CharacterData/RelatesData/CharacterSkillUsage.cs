using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class CharacterSkillUsage : INetSerializable
    {
        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private GuildSkill cacheGuildSkill;
        [System.NonSerialized]
        private IUsableItem cacheUsableItem;

        private void MakeCache()
        {
            if (dirtyDataId != dataId)
            {
                dirtyDataId = dataId;
                cacheSkill = null;
                cacheGuildSkill = null;
                cacheUsableItem = null;
                switch (type)
                {
                    case SkillUsageType.Skill:
                        GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
                        break;
                    case SkillUsageType.GuildSkill:
                        GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill);
                        break;
                    case SkillUsageType.UsableItem:
                        if (GameInstance.Items.TryGetValue(dataId, out BaseItem item))
                            cacheUsableItem = item as IUsableItem;
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

        public IUsableItem GetUsableItem()
        {
            MakeCache();
            return cacheUsableItem;
        }

        public void Use(ICharacterData character, int level)
        {
            coolDownRemainsDuration = 0f;
            switch (type)
            {
                case SkillUsageType.UsableItem:
                    if (GetUsableItem() != null)
                    {
                        coolDownRemainsDuration = GetUsableItem().UseItemCooldown;
                    }
                    break;
                case SkillUsageType.GuildSkill:
                    if (GetGuildSkill() != null)
                    {
                        coolDownRemainsDuration = GetGuildSkill().GetCoolDownDuration(level);
                    }
                    break;
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
