using NotifiableCollection;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GameDataDictionary<TType> : NotifiableDictionary<int, TType>
        where TType : BaseGameData
    {
        private uint? _indexVersion;
        /// <summary>
        /// `Key` is key from `NotifiableDictionary<int, TType>`, `Value` is indexes which will be prepared in `PrepareIndexes()`
        /// </summary>
        private readonly Dictionary<int, int> _indexes = new Dictionary<int, int>();

        public bool IsIndexesOutdate()
        {
            return !_indexVersion.HasValue || _indexVersion.Value != Version;
        }

        public void PrepareIndexes()
        {
            if (!IsIndexesOutdate())
                return;
            _indexVersion = Version;
            _indexes.Clear();
            int index = 0;
            foreach (int key in Keys)
            {
                _indexes[index++] = key;
            }
        }

        public int IndexOf(TType key)
        {
            if (key == null)
                return -1;
            return IndexOf(key.DataId);
        }

        public int IndexOf(int key)
        {
            PrepareIndexes();
            if (_indexes.TryGetValue(key, out int index))
                return index;
            return -1;
        }

        public GameDataAmountValues<TType> CreateAmountValue()
        {
            return GameDataAmountValues<TType>.Get(this);
        }
    }
}
