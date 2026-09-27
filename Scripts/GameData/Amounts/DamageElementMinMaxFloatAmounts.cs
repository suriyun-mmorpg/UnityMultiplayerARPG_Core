using System;

namespace MultiplayerARPG
{
    /// <summary>Runtime amounts indexed by slots assigned to registered game data.</summary>
    public struct DamageElementMinMaxFloatAmounts
    {
#if DAMAGE_ELEMENT_AMOUNTS_32
        public const int Capacity = 32;
#elif DAMAGE_ELEMENT_AMOUNTS_16
        public const int Capacity = 16;
#else
        public const int Capacity = 8;
#endif

        private uint _occupiedMask;
        public uint OccupiedMask => _occupiedMask;
        private MinMaxFloat _value1;
        private MinMaxFloat _value2;
        private MinMaxFloat _value3;
        private MinMaxFloat _value4;
        private MinMaxFloat _value5;
        private MinMaxFloat _value6;
        private MinMaxFloat _value7;
        private MinMaxFloat _value8;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
        private MinMaxFloat _value9;
        private MinMaxFloat _value10;
        private MinMaxFloat _value11;
        private MinMaxFloat _value12;
        private MinMaxFloat _value13;
        private MinMaxFloat _value14;
        private MinMaxFloat _value15;
        private MinMaxFloat _value16;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
        private MinMaxFloat _value17;
        private MinMaxFloat _value18;
        private MinMaxFloat _value19;
        private MinMaxFloat _value20;
        private MinMaxFloat _value21;
        private MinMaxFloat _value22;
        private MinMaxFloat _value23;
        private MinMaxFloat _value24;
        private MinMaxFloat _value25;
        private MinMaxFloat _value26;
        private MinMaxFloat _value27;
        private MinMaxFloat _value28;
        private MinMaxFloat _value29;
        private MinMaxFloat _value30;
        private MinMaxFloat _value31;
        private MinMaxFloat _value32;
#endif

        public bool Contains(int index)
        {
            ValidateIndex(index);
            return (_occupiedMask & (1u << index)) != 0;
        }

        public bool TryGetValue(int index, out MinMaxFloat value)
        {
            if (Contains(index))
            {
                value = this[index];
                return true;
            }
            value = default;
            return false;
        }

        [Newtonsoft.Json.JsonIgnore]
        public MinMaxFloat this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _value1;
                    case 1: return _value2;
                    case 2: return _value3;
                    case 3: return _value4;
                    case 4: return _value5;
                    case 5: return _value6;
                    case 6: return _value7;
                    case 7: return _value8;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
                    case 8: return _value9;
                    case 9: return _value10;
                    case 10: return _value11;
                    case 11: return _value12;
                    case 12: return _value13;
                    case 13: return _value14;
                    case 14: return _value15;
                    case 15: return _value16;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
                    case 16: return _value17;
                    case 17: return _value18;
                    case 18: return _value19;
                    case 19: return _value20;
                    case 20: return _value21;
                    case 21: return _value22;
                    case 22: return _value23;
                    case 23: return _value24;
                    case 24: return _value25;
                    case 25: return _value26;
                    case 26: return _value27;
                    case 27: return _value28;
                    case 28: return _value29;
                    case 29: return _value30;
                    case 30: return _value31;
                    case 31: return _value32;
#endif
                    default: throw new IndexOutOfRangeException($"Invalid amount slot: {index}");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: _value1 = value; break;
                    case 1: _value2 = value; break;
                    case 2: _value3 = value; break;
                    case 3: _value4 = value; break;
                    case 4: _value5 = value; break;
                    case 5: _value6 = value; break;
                    case 6: _value7 = value; break;
                    case 7: _value8 = value; break;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
                    case 8: _value9 = value; break;
                    case 9: _value10 = value; break;
                    case 10: _value11 = value; break;
                    case 11: _value12 = value; break;
                    case 12: _value13 = value; break;
                    case 13: _value14 = value; break;
                    case 14: _value15 = value; break;
                    case 15: _value16 = value; break;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
                    case 16: _value17 = value; break;
                    case 17: _value18 = value; break;
                    case 18: _value19 = value; break;
                    case 19: _value20 = value; break;
                    case 20: _value21 = value; break;
                    case 21: _value22 = value; break;
                    case 22: _value23 = value; break;
                    case 23: _value24 = value; break;
                    case 24: _value25 = value; break;
                    case 25: _value26 = value; break;
                    case 26: _value27 = value; break;
                    case 27: _value28 = value; break;
                    case 28: _value29 = value; break;
                    case 29: _value30 = value; break;
                    case 30: _value31 = value; break;
                    case 31: _value32 = value; break;
#endif
                    default: throw new IndexOutOfRangeException($"Invalid amount slot: {index}");
                }
                _occupiedMask |= 1u << index;
            }
        }

        public void Add(int index, MinMaxFloat amount)
        {
            switch (index)
            {
                case 0: _value1 += amount; break;
                case 1: _value2 += amount; break;
                case 2: _value3 += amount; break;
                case 3: _value4 += amount; break;
                case 4: _value5 += amount; break;
                case 5: _value6 += amount; break;
                case 6: _value7 += amount; break;
                case 7: _value8 += amount; break;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
                case 8: _value9 += amount; break;
                case 9: _value10 += amount; break;
                case 10: _value11 += amount; break;
                case 11: _value12 += amount; break;
                case 12: _value13 += amount; break;
                case 13: _value14 += amount; break;
                case 14: _value15 += amount; break;
                case 15: _value16 += amount; break;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
                case 16: _value17 += amount; break;
                case 17: _value18 += amount; break;
                case 18: _value19 += amount; break;
                case 19: _value20 += amount; break;
                case 20: _value21 += amount; break;
                case 21: _value22 += amount; break;
                case 22: _value23 += amount; break;
                case 23: _value24 += amount; break;
                case 24: _value25 += amount; break;
                case 25: _value26 += amount; break;
                case 26: _value27 += amount; break;
                case 27: _value28 += amount; break;
                case 28: _value29 += amount; break;
                case 29: _value30 += amount; break;
                case 30: _value31 += amount; break;
                case 31: _value32 += amount; break;
#endif
                default: throw new IndexOutOfRangeException($"Invalid amount slot: {index}");
            }
            _occupiedMask |= 1u << index;
        }

        public void Remove(int index)
        {
            ValidateIndex(index);
            this[index] = default;
            _occupiedMask &= ~(1u << index);
        }

        public void Clear()
        {
            this = default;
        }

        public void Combine(System.Collections.Generic.KeyValuePair<DamageElement, MinMaxFloat> entry)
        {
            DamageElement element = entry.Key == null ? GameInstance.Singleton.DefaultDamageElement : entry.Key;
            Add(RuntimeGameDataSlots.GetSlot(element), entry.Value);
        }

        public void Combine(System.Collections.Generic.Dictionary<DamageElement, MinMaxFloat> source, float rate = 1f)
        {
            if (source == null)
                return;
            foreach (var entry in source)
            {
                DamageElement element = entry.Key == null ? GameInstance.Singleton.DefaultDamageElement : entry.Key;
                Add(RuntimeGameDataSlots.GetSlot(element), entry.Value * rate);
            }
        }

        public void Combine(DamageElementMinMaxFloatAmounts source)
        {
            uint mask = source.OccupiedMask;
            for (int i = 0; i < RuntimeGameDataSlots.DamageElementCount; ++i)
            {
                if ((mask & (1u << i)) != 0)
                    Add(i, source[i]);
            }
        }

        public void MultiplyRates(DamageElementMinMaxFloatAmounts rates)
        {
            uint common = _occupiedMask & rates.OccupiedMask;
            for (int i = 0; i < RuntimeGameDataSlots.DamageElementCount; ++i)
            {
                if ((_occupiedMask & (1u << i)) == 0)
                    continue;
                if ((common & (1u << i)) == 0)
                {
                    Remove(i);
                    continue;
                }
                this[i] *= rates[i];
            }
        }

        public void CopyTo(System.Collections.Generic.Dictionary<DamageElement, MinMaxFloat> result)
        {
            result.Clear();
            for (int i = 0; i < RuntimeGameDataSlots.DamageElementCount; ++i)
            {
                if ((_occupiedMask & (1u << i)) != 0)
                    result[RuntimeGameDataSlots.GetDamageElement(i)] = this[i];
            }
        }

        private static void ValidateIndex(int index)
        {
            if (index < 0 || index >= Capacity)
                throw new IndexOutOfRangeException($"Invalid amount slot: {index}");
        }
    }
}
