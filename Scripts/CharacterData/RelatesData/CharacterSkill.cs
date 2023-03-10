using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterSkill : INetSerializable
    {
        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private int dirtyLevel;

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
            return new CharacterSkill()
            {
                dataId = dataId,
                level = level,
            };
        }

        public static CharacterSkill Create(BaseSkill skill, int level = 1)
        {
            return Create(skill.DataId, level);
        }

        public static CharacterSkill Create(int dataId, int level = 1)
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
            writer.PutPackedInt(level);
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            level = reader.GetPackedInt();
        }
    }

    [System.Serializable]
    public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
    {
    }
}
