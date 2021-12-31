using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterSkill : INetSerializable
    {
        public static readonly CharacterSkill Empty = new CharacterSkill();
        public int dataId;
        public short level;

        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private short dirtyLevel;

        [System.NonSerialized]
        private BaseSkill cacheSkill;

        private void MakeCache()
        {
            if (dirtyDataId != dataId || dirtyLevel != level)
            {
                dirtyDataId = dataId;
                dirtyLevel = level;
                cacheSkill = null;
                GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public CharacterSkill Clone()
        {
            CharacterSkill clone = new CharacterSkill();
            clone.dataId = dataId;
            clone.level = level;
            return clone;
        }

        public static CharacterSkill Create(BaseSkill skill, short level = 1)
        {
            return Create(skill.DataId, level);
        }

        public static CharacterSkill Create(int dataId, short level = 1)
        {
            return new CharacterSkill()
            {
                dataId = dataId,
                level = level,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.PutPackedShort(level);
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            level = reader.GetPackedShort();
        }
    }

    [System.Serializable]
    public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
    {
    }
}
