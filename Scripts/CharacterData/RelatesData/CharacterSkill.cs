using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterSkill
    {
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;

        [System.NonSerialized]
        private BaseSkill _cacheSkill;

        private void MakeCache()
        {
            if (_dirtyDataId == dataId && _dirtyLevel == level)
                return;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
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
