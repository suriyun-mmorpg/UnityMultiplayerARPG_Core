using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class StateFlag
    {
        private readonly HashSet<object> _sources = new();

        public bool IsActive => _sources.Count > 0;

        public void Add(object source) => _sources.Add(source);
        public void Remove(object source) => _sources.Remove(source);
        public void Clear() => _sources.Clear();
    }
}