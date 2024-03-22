using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterBuff
    {
        public EntityInfo BuffApplier => CharacterRelatesDataCacheManager.CharacterBuffs.GetBuffApplier(this);
        public CharacterItem BuffApplierWeapon => CharacterRelatesDataCacheManager.CharacterBuffs.GetBuffApplierWeapon(this);

        public BaseSkill GetSkill()
        {
            return CharacterRelatesDataCacheManager.CharacterBuffs.GetSkill(this);
        }

        public BaseItem GetItem()
        {
            return CharacterRelatesDataCacheManager.CharacterBuffs.GetItem(this);
        }

        public GuildSkill GetGuildSkill()
        {
            return  CharacterRelatesDataCacheManager.CharacterBuffs.GetGuildSkill(this);
        }

        public StatusEffect GetStatusEffect()
        {
            return  CharacterRelatesDataCacheManager.CharacterBuffs.GetStatusEffect(this);
        }

        public CalculatedBuff GetBuff()
        {
            return CharacterRelatesDataCacheManager.CharacterBuffs.GetBuff(this);
        }

        public string GetKey()
        {
            return CharacterRelatesDataCacheManager.CharacterBuffs.GetKey(this);
        }

        public void SetApplier(EntityInfo buffApplier, CharacterItem buffApplierWeapon)
        {
            CharacterRelatesDataCacheManager.CharacterBuffs.SetApplier(this, buffApplier, buffApplierWeapon);
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
