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
            uint mask = source._occupiedMask;
            int count = RuntimeGameDataSlots.DamageElementCount;
            // Amounts can outlive a slot registration; only combine currently registered slots.
            if (count < Capacity)
                mask &= (1u << count) - 1u;
            if (mask == 0)
                return;
            if ((mask & (1u << 0)) != 0)
            {
                _value1.min += source._value1.min;
                _value1.max += source._value1.max;
            }
            if ((mask & (1u << 1)) != 0)
            {
                _value2.min += source._value2.min;
                _value2.max += source._value2.max;
            }
            if ((mask & (1u << 2)) != 0)
            {
                _value3.min += source._value3.min;
                _value3.max += source._value3.max;
            }
            if ((mask & (1u << 3)) != 0)
            {
                _value4.min += source._value4.min;
                _value4.max += source._value4.max;
            }
            if ((mask & (1u << 4)) != 0)
            {
                _value5.min += source._value5.min;
                _value5.max += source._value5.max;
            }
            if ((mask & (1u << 5)) != 0)
            {
                _value6.min += source._value6.min;
                _value6.max += source._value6.max;
            }
            if ((mask & (1u << 6)) != 0)
            {
                _value7.min += source._value7.min;
                _value7.max += source._value7.max;
            }
            if ((mask & (1u << 7)) != 0)
            {
                _value8.min += source._value8.min;
                _value8.max += source._value8.max;
            }
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 8)) != 0)
            {
                _value9.min += source._value9.min;
                _value9.max += source._value9.max;
            }
            if ((mask & (1u << 9)) != 0)
            {
                _value10.min += source._value10.min;
                _value10.max += source._value10.max;
            }
            if ((mask & (1u << 10)) != 0)
            {
                _value11.min += source._value11.min;
                _value11.max += source._value11.max;
            }
            if ((mask & (1u << 11)) != 0)
            {
                _value12.min += source._value12.min;
                _value12.max += source._value12.max;
            }
            if ((mask & (1u << 12)) != 0)
            {
                _value13.min += source._value13.min;
                _value13.max += source._value13.max;
            }
            if ((mask & (1u << 13)) != 0)
            {
                _value14.min += source._value14.min;
                _value14.max += source._value14.max;
            }
            if ((mask & (1u << 14)) != 0)
            {
                _value15.min += source._value15.min;
                _value15.max += source._value15.max;
            }
            if ((mask & (1u << 15)) != 0)
            {
                _value16.min += source._value16.min;
                _value16.max += source._value16.max;
            }
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 16)) != 0)
            {
                _value17.min += source._value17.min;
                _value17.max += source._value17.max;
            }
            if ((mask & (1u << 17)) != 0)
            {
                _value18.min += source._value18.min;
                _value18.max += source._value18.max;
            }
            if ((mask & (1u << 18)) != 0)
            {
                _value19.min += source._value19.min;
                _value19.max += source._value19.max;
            }
            if ((mask & (1u << 19)) != 0)
            {
                _value20.min += source._value20.min;
                _value20.max += source._value20.max;
            }
            if ((mask & (1u << 20)) != 0)
            {
                _value21.min += source._value21.min;
                _value21.max += source._value21.max;
            }
            if ((mask & (1u << 21)) != 0)
            {
                _value22.min += source._value22.min;
                _value22.max += source._value22.max;
            }
            if ((mask & (1u << 22)) != 0)
            {
                _value23.min += source._value23.min;
                _value23.max += source._value23.max;
            }
            if ((mask & (1u << 23)) != 0)
            {
                _value24.min += source._value24.min;
                _value24.max += source._value24.max;
            }
            if ((mask & (1u << 24)) != 0)
            {
                _value25.min += source._value25.min;
                _value25.max += source._value25.max;
            }
            if ((mask & (1u << 25)) != 0)
            {
                _value26.min += source._value26.min;
                _value26.max += source._value26.max;
            }
            if ((mask & (1u << 26)) != 0)
            {
                _value27.min += source._value27.min;
                _value27.max += source._value27.max;
            }
            if ((mask & (1u << 27)) != 0)
            {
                _value28.min += source._value28.min;
                _value28.max += source._value28.max;
            }
            if ((mask & (1u << 28)) != 0)
            {
                _value29.min += source._value29.min;
                _value29.max += source._value29.max;
            }
            if ((mask & (1u << 29)) != 0)
            {
                _value30.min += source._value30.min;
                _value30.max += source._value30.max;
            }
            if ((mask & (1u << 30)) != 0)
            {
                _value31.min += source._value31.min;
                _value31.max += source._value31.max;
            }
            if ((mask & (1u << 31)) != 0)
            {
                _value32.min += source._value32.min;
                _value32.max += source._value32.max;
            }
#endif
            _occupiedMask |= mask;
        }

        public static DamageElementMinMaxFloatAmounts operator +(DamageElementMinMaxFloatAmounts left, DamageElementMinMaxFloatAmounts right)
        {
            left.Combine(right);
            return left;
        }

        public void MultiplyRates(DamageElementMinMaxFloatAmounts rates)
        {
            uint mask = _occupiedMask;
            int count = RuntimeGameDataSlots.DamageElementCount;
            if (count < Capacity)
                mask &= (1u << count) - 1u;
            if (mask == 0)
                return;
            uint shared = mask & rates._occupiedMask;
            if ((mask & (1u << 0)) != 0) _value1 = (shared & (1u << 0)) != 0 ? _value1 * rates._value1 : default;
            if ((mask & (1u << 1)) != 0) _value2 = (shared & (1u << 1)) != 0 ? _value2 * rates._value2 : default;
            if ((mask & (1u << 2)) != 0) _value3 = (shared & (1u << 2)) != 0 ? _value3 * rates._value3 : default;
            if ((mask & (1u << 3)) != 0) _value4 = (shared & (1u << 3)) != 0 ? _value4 * rates._value4 : default;
            if ((mask & (1u << 4)) != 0) _value5 = (shared & (1u << 4)) != 0 ? _value5 * rates._value5 : default;
            if ((mask & (1u << 5)) != 0) _value6 = (shared & (1u << 5)) != 0 ? _value6 * rates._value6 : default;
            if ((mask & (1u << 6)) != 0) _value7 = (shared & (1u << 6)) != 0 ? _value7 * rates._value7 : default;
            if ((mask & (1u << 7)) != 0) _value8 = (shared & (1u << 7)) != 0 ? _value8 * rates._value8 : default;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 8)) != 0) _value9 = (shared & (1u << 8)) != 0 ? _value9 * rates._value9 : default;
            if ((mask & (1u << 9)) != 0) _value10 = (shared & (1u << 9)) != 0 ? _value10 * rates._value10 : default;
            if ((mask & (1u << 10)) != 0) _value11 = (shared & (1u << 10)) != 0 ? _value11 * rates._value11 : default;
            if ((mask & (1u << 11)) != 0) _value12 = (shared & (1u << 11)) != 0 ? _value12 * rates._value12 : default;
            if ((mask & (1u << 12)) != 0) _value13 = (shared & (1u << 12)) != 0 ? _value13 * rates._value13 : default;
            if ((mask & (1u << 13)) != 0) _value14 = (shared & (1u << 13)) != 0 ? _value14 * rates._value14 : default;
            if ((mask & (1u << 14)) != 0) _value15 = (shared & (1u << 14)) != 0 ? _value15 * rates._value15 : default;
            if ((mask & (1u << 15)) != 0) _value16 = (shared & (1u << 15)) != 0 ? _value16 * rates._value16 : default;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 16)) != 0) _value17 = (shared & (1u << 16)) != 0 ? _value17 * rates._value17 : default;
            if ((mask & (1u << 17)) != 0) _value18 = (shared & (1u << 17)) != 0 ? _value18 * rates._value18 : default;
            if ((mask & (1u << 18)) != 0) _value19 = (shared & (1u << 18)) != 0 ? _value19 * rates._value19 : default;
            if ((mask & (1u << 19)) != 0) _value20 = (shared & (1u << 19)) != 0 ? _value20 * rates._value20 : default;
            if ((mask & (1u << 20)) != 0) _value21 = (shared & (1u << 20)) != 0 ? _value21 * rates._value21 : default;
            if ((mask & (1u << 21)) != 0) _value22 = (shared & (1u << 21)) != 0 ? _value22 * rates._value22 : default;
            if ((mask & (1u << 22)) != 0) _value23 = (shared & (1u << 22)) != 0 ? _value23 * rates._value23 : default;
            if ((mask & (1u << 23)) != 0) _value24 = (shared & (1u << 23)) != 0 ? _value24 * rates._value24 : default;
            if ((mask & (1u << 24)) != 0) _value25 = (shared & (1u << 24)) != 0 ? _value25 * rates._value25 : default;
            if ((mask & (1u << 25)) != 0) _value26 = (shared & (1u << 25)) != 0 ? _value26 * rates._value26 : default;
            if ((mask & (1u << 26)) != 0) _value27 = (shared & (1u << 26)) != 0 ? _value27 * rates._value27 : default;
            if ((mask & (1u << 27)) != 0) _value28 = (shared & (1u << 27)) != 0 ? _value28 * rates._value28 : default;
            if ((mask & (1u << 28)) != 0) _value29 = (shared & (1u << 28)) != 0 ? _value29 * rates._value29 : default;
            if ((mask & (1u << 29)) != 0) _value30 = (shared & (1u << 29)) != 0 ? _value30 * rates._value30 : default;
            if ((mask & (1u << 30)) != 0) _value31 = (shared & (1u << 30)) != 0 ? _value31 * rates._value31 : default;
            if ((mask & (1u << 31)) != 0) _value32 = (shared & (1u << 31)) != 0 ? _value32 * rates._value32 : default;
#endif
            _occupiedMask = (_occupiedMask & ~mask) | shared;
        }

        public void Scale(float multiplier)
        {
            uint mask = _occupiedMask;
            int count = RuntimeGameDataSlots.DamageElementCount;
            if (count < Capacity)
                mask &= (1u << count) - 1u;
            if (mask == 0)
                return;
            if ((mask & (1u << 0)) != 0) _value1 *= multiplier;
            if ((mask & (1u << 1)) != 0) _value2 *= multiplier;
            if ((mask & (1u << 2)) != 0) _value3 *= multiplier;
            if ((mask & (1u << 3)) != 0) _value4 *= multiplier;
            if ((mask & (1u << 4)) != 0) _value5 *= multiplier;
            if ((mask & (1u << 5)) != 0) _value6 *= multiplier;
            if ((mask & (1u << 6)) != 0) _value7 *= multiplier;
            if ((mask & (1u << 7)) != 0) _value8 *= multiplier;
#if DAMAGE_ELEMENT_AMOUNTS_16 || DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 8)) != 0) _value9 *= multiplier;
            if ((mask & (1u << 9)) != 0) _value10 *= multiplier;
            if ((mask & (1u << 10)) != 0) _value11 *= multiplier;
            if ((mask & (1u << 11)) != 0) _value12 *= multiplier;
            if ((mask & (1u << 12)) != 0) _value13 *= multiplier;
            if ((mask & (1u << 13)) != 0) _value14 *= multiplier;
            if ((mask & (1u << 14)) != 0) _value15 *= multiplier;
            if ((mask & (1u << 15)) != 0) _value16 *= multiplier;
#endif
#if DAMAGE_ELEMENT_AMOUNTS_32
            if ((mask & (1u << 16)) != 0) _value17 *= multiplier;
            if ((mask & (1u << 17)) != 0) _value18 *= multiplier;
            if ((mask & (1u << 18)) != 0) _value19 *= multiplier;
            if ((mask & (1u << 19)) != 0) _value20 *= multiplier;
            if ((mask & (1u << 20)) != 0) _value21 *= multiplier;
            if ((mask & (1u << 21)) != 0) _value22 *= multiplier;
            if ((mask & (1u << 22)) != 0) _value23 *= multiplier;
            if ((mask & (1u << 23)) != 0) _value24 *= multiplier;
            if ((mask & (1u << 24)) != 0) _value25 *= multiplier;
            if ((mask & (1u << 25)) != 0) _value26 *= multiplier;
            if ((mask & (1u << 26)) != 0) _value27 *= multiplier;
            if ((mask & (1u << 27)) != 0) _value28 *= multiplier;
            if ((mask & (1u << 28)) != 0) _value29 *= multiplier;
            if ((mask & (1u << 29)) != 0) _value30 *= multiplier;
            if ((mask & (1u << 30)) != 0) _value31 *= multiplier;
            if ((mask & (1u << 31)) != 0) _value32 *= multiplier;
#endif
        }

        public static DamageElementMinMaxFloatAmounts operator *(DamageElementMinMaxFloatAmounts left, DamageElementMinMaxFloatAmounts right)
        {
            left.MultiplyRates(right);
            return left;
        }

        public static DamageElementMinMaxFloatAmounts operator *(DamageElementMinMaxFloatAmounts amounts, float multiplier)
        {
            amounts.Scale(multiplier);
            return amounts;
        }

        public MinMaxFloat Sum()
        {
            MinMaxFloat result = default;
            for (int i = 0; i < RuntimeGameDataSlots.DamageElementCount; ++i)
            {
                if ((_occupiedMask & (1u << i)) != 0)
                    result += this[i];
            }
            return result;
        }

        private static void ValidateIndex(int index)
        {
            if (index < 0 || index >= Capacity)
                throw new IndexOutOfRangeException($"Invalid amount slot: {index}");
        }
    }
}
