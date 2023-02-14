using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum BuffType : byte
    {
        SkillBuff,
        SkillDebuff,
        PotionBuff,
        GuildSkillBuff,
        StatusEffect,
    }

    [System.Serializable]
    public partial class CharacterBuff : INetSerializable
    {
        public static readonly CharacterBuff Empty = new CharacterBuff();
        public string id;
        public BuffType type;
        public int dataId;
        public int level;
        public float buffRemainsDuration;

        [System.NonSerialized]
        private BuffType dirtyType;
        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private int dirtyLevel;

        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private BaseItem cacheItem;
        [System.NonSerialized]
        private GuildSkill cacheGuildSkill;
        [System.NonSerialized]
        private StatusEffect cacheStatusEffect;
        [System.NonSerialized]
        private CalculatedBuff cacheBuff;
        [System.NonSerialized]
        private string cacheKey;

        public EntityInfo BuffApplier { get; private set; }
        public CharacterItem BuffApplierWeapon { get; private set; }

        private void MakeCache()
        {
            if (dirtyDataId != dataId || dirtyType != type || dirtyLevel != level)
            {
                cacheKey = type + "_" + dataId;
                dirtyDataId = dataId;
                dirtyType = type;
                dirtyLevel = level;
                cacheSkill = null;
                cacheItem = null;
                cacheGuildSkill = null;
                Buff tempBuff = Buff.Empty;
                switch (type)
                {
                    case BuffType.SkillBuff:
                    case BuffType.SkillDebuff:
                        if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill) && cacheSkill != null)
                            tempBuff = type == BuffType.SkillBuff ? cacheSkill.Buff : cacheSkill.Debuff;
                        break;
                    case BuffType.PotionBuff:
                        if (GameInstance.Items.TryGetValue(dataId, out cacheItem) && cacheItem != null && cacheItem.IsPotion())
                            tempBuff = (cacheItem as IPotionItem).Buff;
                        break;
                    case BuffType.GuildSkillBuff:
                        if (GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill) && cacheGuildSkill != null)
                            tempBuff = cacheGuildSkill.Buff;
                        break;
                    case BuffType.StatusEffect:
                        if (GameInstance.StatusEffects.TryGetValue(dataId, out cacheStatusEffect) && cacheStatusEffect != null)
                            tempBuff = cacheStatusEffect.Buff;
                        break;
                }
                cacheBuff = new CalculatedBuff(tempBuff, level);
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public BaseItem GetItem()
        {
            MakeCache();
            return cacheItem;
        }

        public GuildSkill GetGuildSkill()
        {
            MakeCache();
            return cacheGuildSkill;
        }

        public StatusEffect GetStatusEffect()
        {
            MakeCache();
            return cacheStatusEffect;
        }

        public CalculatedBuff GetBuff()
        {
            MakeCache();
            return cacheBuff;
        }

        public string GetKey()
        {
            MakeCache();
            return cacheKey;
        }

        public bool ShouldRemove()
        {
            return buffRemainsDuration <= 0f;
        }

        public void Apply(EntityInfo buffApplier, CharacterItem buffApplierWeapon)
        {
            SetApplier(buffApplier, buffApplierWeapon);
            buffRemainsDuration = GetBuff().GetDuration();
        }

        public void SetApplier(EntityInfo buffApplier, CharacterItem buffApplierWeapon)
        {
            BuffApplier = buffApplier;
            BuffApplierWeapon = buffApplierWeapon;
        }

        public void Update(float deltaTime)
        {
            buffRemainsDuration -= deltaTime;
        }

        public CharacterBuff Clone(bool generateNewId = false)
        {
            return new CharacterBuff()
            {
                id = generateNewId ? GenericUtils.GetUniqueId() : id,
                type = type,
                dataId = dataId,
                level = level,
                buffRemainsDuration = buffRemainsDuration,
            };
        }

        public static CharacterBuff Create(BuffType type, int dataId, int level = 1)
        {
            return new CharacterBuff()
            {
                id = GenericUtils.GetUniqueId(),
                type = type,
                dataId = dataId,
                level = level,
                buffRemainsDuration = 0f,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put((byte)type);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(level);
            writer.Put(buffRemainsDuration);
        }

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            type = (BuffType)reader.GetByte();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedInt();
            buffRemainsDuration = reader.GetFloat();
        }
    }

    [System.Serializable]
    public class SyncListCharacterBuff : LiteNetLibSyncList<CharacterBuff>
    {
        protected override CharacterBuff DeserializeValueForSetOrDirty(int index, NetDataReader reader)
        {
            CharacterBuff result = this[index];
            result.buffRemainsDuration = reader.GetFloat();
            return result;
        }

        protected override void SerializeValueForSetOrDirty(int index, NetDataWriter writer, CharacterBuff value)
        {
            writer.Put(value.buffRemainsDuration);
        }
    }
}
