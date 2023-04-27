using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterSkillUsage
    {
        [System.NonSerialized]
        private int _dirtyDataId;

        [System.NonSerialized]
        private BaseSkill _cacheSkill;
        [System.NonSerialized]
        private GuildSkill _cacheGuildSkill;
        [System.NonSerialized]
        private IUsableItem _cacheUsableItem;

        private void MakeCache()
        {
            if (_dirtyDataId == dataId)
                return;
            _dirtyDataId = dataId;
            _cacheSkill = null;
            _cacheGuildSkill = null;
            _cacheUsableItem = null;
            switch (type)
            {
                case SkillUsageType.Skill:
                    GameInstance.Skills.TryGetValue(dataId, out _cacheSkill);
                    break;
                case SkillUsageType.GuildSkill:
                    GameInstance.GuildSkills.TryGetValue(dataId, out _cacheGuildSkill);
                    break;
                case SkillUsageType.UsableItem:
                    if (GameInstance.Items.TryGetValue(dataId, out BaseItem item))
                        _cacheUsableItem = item as IUsableItem;
                    break;
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return _cacheSkill;
        }

        public GuildSkill GetGuildSkill()
        {
            MakeCache();
            return _cacheGuildSkill;
        }

        public IUsableItem GetUsableItem()
        {
            MakeCache();
            return _cacheUsableItem;
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
