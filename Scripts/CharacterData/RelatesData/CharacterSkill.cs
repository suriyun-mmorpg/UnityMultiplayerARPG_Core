using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterSkill
    {
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;

        [System.NonSerialized]
        private BaseSkill _cacheSkill;
        /*
        ~CharacterSkill()
        {
            ClearCachedData();
        }
        */
        private void ClearCachedData()
        {
            _cacheSkill = null;
        }

        private bool IsRecaching()
        {
            return _dirtyDataId != dataId || _dirtyLevel != level;
        }

        private void MakeAsCached()
        {
            _dirtyDataId = dataId;
            _dirtyLevel = level;
        }

        private void MakeCache()
        {
            if (!IsRecaching())
                return;
            MakeAsCached();
            ClearCachedData();
            if (!GameInstance.Skills.TryGetValue(dataId, out _cacheSkill))
                _cacheSkill = null;
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return _cacheSkill;
        }

        public static CharacterSkill Create(BaseSkill skill, int level = 1)
        {
            return Create(skill.DataId, level);
        }
    }

    [System.Serializable]
    public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
    {
    }
}
