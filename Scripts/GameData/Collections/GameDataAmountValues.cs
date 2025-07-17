using System;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GameDataAmountValues<TType> : IReadOnlyList<float>
        where TType : BaseGameData
    {
        private static readonly Stack<GameDataAmountValues<TType>> s_Pool = new Stack<GameDataAmountValues<TType>>();
        private static readonly object s_PoolLock = new object();

        private GameDataDictionary<TType> _source;
        private uint _currentVersion;
        private List<float> _amountValues;

        private bool _isInPool;

        private GameDataAmountValues() // Private constructor to enforce pooling
        {
            _amountValues = new List<float>();
        }

        ~GameDataAmountValues()
        {
            // Auto-return to pool as a safety fallback
            Release();
        }

        public static GameDataAmountValues<TType> Get(GameDataDictionary<TType> source)
        {
            GameDataAmountValues<TType> instance;
            lock (s_PoolLock)
            {
                instance = s_Pool.Count > 0 ? s_Pool.Pop() : new GameDataAmountValues<TType>();
            }
            instance._isInPool = false;
            instance._source = source;
            instance._currentVersion = source.Version;
            instance.PrepareValues(true);
            return instance;
        }

        public void Release()
        {
            if (_isInPool) return; // Prevent double return
            _isInPool = true;

            _source = null;
            _currentVersion = 0;
            _amountValues.Clear();

            lock (s_PoolLock)
            {
                s_Pool.Push(this);
            }
        }

        public float this[int index] => _amountValues[index];
        public int Count => _amountValues.Count;

        public IEnumerator<float> GetEnumerator()
        {
            foreach (float amount in _amountValues)
                yield return amount;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void PrepareValues(bool force = false)
        {
            if (!force && !IsOutdate())
                return;

            _amountValues.Clear();
            for (int i = 0; i < _source.Count; ++i)
            {
                _amountValues.Add(0f);
            }
            _currentVersion = _source.Version;
        }

        public void ResetValues()
        {
            PrepareValues();
            for (int i = 0; i < _amountValues.Count; ++i)
            {
                _amountValues[i] = 0f;
            }
        }

        public void SetValues(IEnumerable<IGameDataKeyFloatAmountValue<TType>> valuesSource)
        {
            ResetValues();
            foreach (var valueSource in valuesSource)
            {
                int index = _source.IndexOf(valueSource.Key);
                if (index < 0) continue;
                _amountValues[index] = valueSource.Value;
            }
        }

        public void SetValues(IEnumerable<IGameDataKeyIncrementalFloatAmountValue<TType>> valuesSource, int level)
        {
            ResetValues();
            foreach (var valueSource in valuesSource)
            {
                int index = _source.IndexOf(valueSource.Key);
                if (index < 0) continue;
                _amountValues[index] = valueSource.Value.GetAmount(level);
            }
        }

        public void MakeChanges(IList<float> destination)
        {
            int i;
            for (i = 0; i < destination.Count; ++i)
            {
                destination[i] += _amountValues[i];
            }
            for (; i < _amountValues.Count; ++i)
            {
                destination.Add(_amountValues[i]);
            }
        }

        public bool IsOutdate() => _currentVersion != _source.Version;
    }
}
