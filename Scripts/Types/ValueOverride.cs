using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class ValueOverride<T>
    {
        private struct Entry
        {
            public T Value;
            public int Priority;
        }

        private readonly Dictionary<object, Entry> _entries = new();

        public void Set(object source, T value, int priority)
        {
            _entries[source] = new Entry { Value = value, Priority = priority };
        }

        public void Remove(object source)
        {
            _entries.Remove(source);
        }

        public T GetValue(T defaultValue)
        {
            if (_entries.Count == 0)
                return defaultValue;

            Entry best = default;
            bool has = false;

            foreach (var e in _entries)
            {
                if (!has || e.Value.Priority > best.Priority)
                {
                    best = e.Value;
                    has = true;
                }
            }

            return best.Value;
        }

        public bool TryGetValue(out T value)
        {
            if (_entries.Count == 0)
            {
                value = default;
                return false;
            }

            Entry best = default;
            bool has = false;

            foreach (var e in _entries)
            {
                if (!has || e.Value.Priority > best.Priority)
                {
                    best = e.Value;
                    has = true;
                }
            }

            value = best.Value;
            return true;
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
