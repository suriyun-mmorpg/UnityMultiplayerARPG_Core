using Cysharp.Text;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterBuff
    {
        [System.NonSerialized]
        private BuffType _dirtyType;
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;

        [System.NonSerialized]
        private string _cacheKey;
        [System.NonSerialized]
        private BaseSkill _cacheSkill;
        [System.NonSerialized]
        private BaseItem _cacheItem;
        [System.NonSerialized]
        private GuildSkill _cacheGuildSkill;
        [System.NonSerialized]
        private StatusEffect _cacheStatusEffect;
        [System.NonSerialized]
        private CalculatedBuff _cacheBuff = new CalculatedBuff();

        public EntityInfo BuffApplier { get; private set; }
        public CharacterItem BuffApplierWeapon { get; private set; }

        private void MakeCache()
        {
            if (_dirtyDataId == dataId && _dirtyType == type && _dirtyLevel == level)
                return;
            _dirtyType = type;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
            _cacheKey = ZString.Concat(type, "_", dataId);
            _cacheSkill = null;
            _cacheItem = null;
            _cacheGuildSkill = null;
            _cacheStatusEffect = null;
            Buff tempBuff = Buff.Empty;
            switch (type)
            {
                case BuffType.SkillBuff:
                case BuffType.SkillDebuff:
                    if (GameInstance.Skills.TryGetValue(dataId, out _cacheSkill) && _cacheSkill != null)
                        tempBuff = type == BuffType.SkillBuff ? _cacheSkill.Buff : _cacheSkill.Debuff;
                    break;
                case BuffType.PotionBuff:
                    if (GameInstance.Items.TryGetValue(dataId, out _cacheItem) && _cacheItem != null && _cacheItem.IsPotion())
                        tempBuff = (_cacheItem as IPotionItem).Buff;
                    break;
                case BuffType.GuildSkillBuff:
                    if (GameInstance.GuildSkills.TryGetValue(dataId, out _cacheGuildSkill) && _cacheGuildSkill != null)
                        tempBuff = _cacheGuildSkill.Buff;
                    break;
                case BuffType.StatusEffect:
                    if (GameInstance.StatusEffects.TryGetValue(dataId, out _cacheStatusEffect) && _cacheStatusEffect != null)
                        tempBuff = _cacheStatusEffect.Buff;
                    break;
            }
            _cacheBuff.Build(tempBuff, level);
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return _cacheSkill;
        }

        public BaseItem GetItem()
        {
            MakeCache();
            return _cacheItem;
        }

        public GuildSkill GetGuildSkill()
        {
            MakeCache();
            return _cacheGuildSkill;
        }

        public StatusEffect GetStatusEffect()
        {
            MakeCache();
            return _cacheStatusEffect;
        }

        public CalculatedBuff GetBuff()
        {
            MakeCache();
            return _cacheBuff;
        }

        public string GetKey()
        {
            MakeCache();
            return _cacheKey;
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
