using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>Adapters for dictionary-facing UI and extension APIs.</summary>
    public static class CurrencyDictionaryView
    {
        public static void CopyToDictionary(CurrencyAmount[] source, Dictionary<Currency, int> result, float rate)
        {
            CurrencyAmounts amounts = default;
            GameDataHelpers.CombineCurrencies(source, ref amounts, rate);
            amounts.CopyTo(result);
        }

        public static void CopyToDictionary(List<CurrencyAmount> source, Dictionary<Currency, int> result, float rate)
        {
            CurrencyAmounts amounts = default;
            GameDataHelpers.CombineCurrencies(source, ref amounts, rate);
            amounts.CopyTo(result);
        }

        public static void CopyTo(this CurrencyAmounts source, Dictionary<Currency, int> result)
        {
            if (result == null)
                return;
            result.Clear();
            uint mask = source.OccupiedMask;
            for (int slot = 0; slot < RuntimeGameDataSlots.CurrencyCount; ++slot)
            {
                if ((mask & (1u << slot)) != 0)
                    result[RuntimeGameDataSlots.GetCurrency(slot)] = source[slot];
            }
        }

        public static CurrencyAmounts ToCurrencyAmounts(this Dictionary<Currency, int> source)
        {
            CurrencyAmounts result = default;
            if (source == null)
                return result;
            foreach (KeyValuePair<Currency, int> entry in source)
            {
                if (entry.Key != null)
                    result.Add(RuntimeGameDataSlots.GetSlot(entry.Key), entry.Value);
            }
            return result;
        }
    }
}
