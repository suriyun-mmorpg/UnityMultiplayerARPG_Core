using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCacheManager<T, TCache>
        where TCache : BaseCacheData<T>, new()
    {
        public float cacheLifeTime = 30f;

        protected Dictionary<string, TCache> _caches = new Dictionary<string, TCache>();

        public void OnUpdate()
        {
            if (_caches.Count <= 0)
                return;

            float time = Time.unscaledTime;
            List<string> keys = new List<string>(_caches.Keys);
            foreach (string key in keys)
            {
                if (time - _caches[key].TouchedTime < cacheLifeTime)
                    continue;
                _caches[key].Clear();
                _caches.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (TCache cache in _caches.Values)
            {
                cache?.Clear();
            }
            _caches.Clear();
        }

        public TCache GetOrMakeCache(string id, ref T data)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            if (_caches.TryGetValue(id, out TCache cacheData))
                return cacheData.Prepare(ref data) as TCache;
            cacheData = new TCache().Prepare(ref data) as TCache;
            _caches[id] = cacheData;
            return cacheData;
        }
    }
}