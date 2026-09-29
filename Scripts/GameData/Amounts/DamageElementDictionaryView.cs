using System.Collections.Generic;

namespace MultiplayerARPG
{
    // Compatibility boundary for UI and public APIs that still expose dictionaries.
    public static class DamageElementDictionaryView
    {
        public static void CopyToDictionary(this DamageElementMinMaxFloatAmounts source, Dictionary<DamageElement, MinMaxFloat> result)
        {
            result.Clear();
            for (int slot = 0; slot < RuntimeGameDataSlots.DamageElementCount; ++slot)
            {
                if (source.Contains(slot))
                    result[RuntimeGameDataSlots.GetDamageElement(slot)] = source[slot];
            }
        }
    }
}
