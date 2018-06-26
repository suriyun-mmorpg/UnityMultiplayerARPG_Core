using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum HotkeyType : byte
    {
        None,
        Skill,
        Item,
    }

    [System.Serializable]
    public class CharacterHotkey
    {
        public static readonly CharacterHotkey Empty = new CharacterHotkey();
        public string hotkeyId;
        public HotkeyType type;
        public int dataId;
        [System.NonSerialized]
        private HotkeyType dirtyType;
        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private Skill cacheSkill;
        [System.NonSerialized]
        private Item cacheItem;

        private void MakeCache()
        {
            if (type == HotkeyType.None || (!GameInstance.Skills.ContainsKey(dataId) && !GameInstance.Items.ContainsKey(dataId)))
            {
                cacheSkill = null;
                cacheItem = null;
                return;
            }
            if (dirtyDataId != dataId || type != dirtyType)
            {
                dirtyDataId = dataId;
                dirtyType = type;
                cacheSkill = null;
                cacheItem = null;
                if (type == HotkeyType.Skill)
                    cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
                if (type == HotkeyType.Item)
                    cacheItem = GameInstance.Items.TryGetValue(dataId, out cacheItem) ? cacheItem : null;
            }
        }

        public Skill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public Item GetItem()
        {
            MakeCache();
            return cacheItem;
        }
    }

    public class NetFieldCharacterHotkey : LiteNetLibNetField<CharacterHotkey>
    {
        public override void Deserialize(NetDataReader reader)
        {
            var newValue = new CharacterHotkey();
            newValue.hotkeyId = reader.GetString();
            newValue.type = (HotkeyType)reader.GetByte();
            newValue.dataId = reader.GetInt();
            Value = newValue;
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(Value.hotkeyId);
            writer.Put((byte)Value.type);
            writer.Put(Value.dataId);
        }

        public override bool IsValueChanged(CharacterHotkey newValue)
        {
            return true;
        }
    }

    [System.Serializable]
    public class SyncListCharacterHotkey : LiteNetLibSyncList<NetFieldCharacterHotkey, CharacterHotkey>
    {
    }
}
