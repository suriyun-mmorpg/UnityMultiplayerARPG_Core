using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class CharacterSummonCacheData : BaseCacheData<CharacterSummon>
    {
        private SummonType _type;
        private int _dataId;
        private int _level;

        private CalculatedBuff _cacheBuff = null;
        private bool _recachingBuff = false;

        public BaseMonsterCharacterEntity CacheEntity { get; set; }

        public override BaseCacheData<CharacterSummon> Prepare(in CharacterSummon source)
        {
            base.Prepare(in source);
            if (source.type == _type && source.dataId == _dataId && source.level == _level)
                return this;
            _type = source.type;
            _dataId = source.dataId;
            _level = source.level;
            _recachingBuff = true;
            return this;
        }

        public override void Clear()
        {
            _cacheBuff = null;
        }

        public BaseSkill GetSkill()
        {
            if (_type != SummonType.Skill)
                return null;
            if (GameInstance.Skills.TryGetValue(_dataId, out BaseSkill skill) && skill.TryGetSummon(out _))
                return skill;
            return null;
        }

        public IPetItem GetPetItem()
        {
            if (_type != SummonType.PetItem)
                return null;
            if (GameInstance.Items.TryGetValue(_dataId, out BaseItem item) && item.IsPet())
                return item as IPetItem;
            return null;
        }

        /// <summary>
        /// Return `TRUE` if it is addressable
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="addressablePrefab"></param>
        /// <returns></returns>
        public bool GetPrefab(out BaseMonsterCharacterEntity prefab, out AssetReferenceBaseMonsterCharacterEntity addressablePrefab)
        {
            prefab = null;
            addressablePrefab = null;
            switch (_type)
            {
                case SummonType.Skill:
                    if (GameInstance.Skills.TryGetValue(_dataId, out BaseSkill skill) && skill.TryGetSummon(out SkillSummon skillSummon))
                    {
                        if (skillSummon.MonsterCharacterEntity != null)
                        {
                            prefab = skillSummon.MonsterCharacterEntity;
                            return false;
                        }
                        else if (skillSummon.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = skillSummon.AddressableMonsterCharacterEntity;
                            return true;
                        }
                    }
                    break;
                case SummonType.PetItem:
                    if (GameInstance.Items.TryGetValue(_dataId, out BaseItem item) && item.IsPet())
                    {
                        IPetItem petItem = item as IPetItem;
                        if (petItem.MonsterCharacterEntity != null)
                        {
                            prefab = petItem.MonsterCharacterEntity;
                            return false;
                        }
                        else if (petItem.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = petItem.AddressableMonsterCharacterEntity;
                            return true;
                        }
                    }
                    break;
                case SummonType.Custom:
                    return GameInstance.CustomSummonManager.GetPrefab(out prefab, out addressablePrefab);
            }
            return false;
        }

        public CalculatedBuff GetBuff()
        {
            if (_cacheBuff == null)
                _cacheBuff = new CalculatedBuff();
            if (!_recachingBuff)
                return _cacheBuff;
            _recachingBuff = false;
            Buff tempBuff = Buff.Empty;
            BaseMonsterCharacterEntity tempPrefab;
            if (GetPrefab(out BaseMonsterCharacterEntity prefab, out AssetReferenceBaseMonsterCharacterEntity addressablePrefab))
            {
                tempPrefab = addressablePrefab.GetOrLoadAsset<AssetReferenceBaseMonsterCharacterEntity, BaseCharacterEntity>() as BaseMonsterCharacterEntity;
                if (tempPrefab != null && tempPrefab.CharacterDatabase != null)
                    tempBuff = tempPrefab.CharacterDatabase.SummonerBuff;
            }
            else
            {
                tempPrefab = prefab;
                if (tempPrefab != null && tempPrefab.CharacterDatabase != null)
                    tempBuff = tempPrefab.CharacterDatabase.SummonerBuff;
            }
            _cacheBuff.Build(tempBuff, _level);
            return _cacheBuff;
        }
    }
}