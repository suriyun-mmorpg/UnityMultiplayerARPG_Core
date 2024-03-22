using Cysharp.Text;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Newtonsoft.Json;

namespace MultiplayerARPG
{
    public partial struct CharacterBuff
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
        private CalculatedBuff _cacheBuff/* = new CalculatedBuff()*/;
        [System.NonSerialized]
        private bool _recachingBuff/* = false*/;

        [JsonIgnore]
        public EntityInfo BuffApplier { get; private set; }
        [JsonIgnore]
        public CharacterItem BuffApplierWeapon { get; private set; }
        /*
        ~CharacterBuff()
        {
            ClearCachedData();
            _cacheBuff = null;
            BuffApplierWeapon = null;
        }
        */
        private void ClearCachedData()
        {
            _cacheKey = null;
            _cacheSkill = null;
            _cacheItem = null;
            _cacheGuildSkill = null;
            _cacheStatusEffect = null;
        }

        private bool IsRecaching()
        {
            return _dirtyDataId != dataId || _dirtyType != type || _dirtyLevel != level;
        }

        private void MakeAsCached()
        {
            _dirtyType = type;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
        }

        private void MakeCache()
        {
            if (!IsRecaching())
                return;
            MakeAsCached();
            ClearCachedData();
            _recachingBuff = true;
            _cacheKey = ZString.Concat((byte)type, '_', dataId);
            switch (type)
            {
                case BuffType.SkillBuff:
                case BuffType.SkillDebuff:
                    if (!GameInstance.Skills.TryGetValue(dataId, out _cacheSkill))
                        _cacheSkill = null;
                    break;
                case BuffType.PotionBuff:
                    if (!GameInstance.Items.TryGetValue(dataId, out _cacheItem))
                        _cacheItem = null;
                    break;
                case BuffType.GuildSkillBuff:
                    if (!GameInstance.GuildSkills.TryGetValue(dataId, out _cacheGuildSkill))
                        _cacheGuildSkill = null;
                    break;
                case BuffType.StatusEffect:
                    if (!GameInstance.StatusEffects.TryGetValue(dataId, out _cacheStatusEffect))
                        _cacheStatusEffect = null;
                    break;
            }
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
            if (_recachingBuff)
            {
                _recachingBuff = false;
                Buff tempBuff = Buff.Empty;
                switch (type)
                {
                    case BuffType.SkillBuff:
                        if (_cacheSkill != null && _cacheSkill.TryGetBuff(out Buff buff))
                            tempBuff = buff;
                        break;
                    case BuffType.SkillDebuff:
                        if (_cacheSkill != null && _cacheSkill.TryGetDebuff(out Buff debuff))
                            tempBuff = debuff;
                        break;
                    case BuffType.PotionBuff:
                        if (_cacheItem != null && _cacheItem.IsPotion())
                            tempBuff = (_cacheItem as IPotionItem).BuffData.Value;
                        break;
                    case BuffType.GuildSkillBuff:
                        if (_cacheGuildSkill != null)
                            tempBuff = _cacheGuildSkill.Buff;
                        break;
                    case BuffType.StatusEffect:
                        if (_cacheStatusEffect != null)
                            tempBuff = _cacheStatusEffect.Buff;
                        break;
                }
                _cacheBuff.Build(tempBuff, level);
            }
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
