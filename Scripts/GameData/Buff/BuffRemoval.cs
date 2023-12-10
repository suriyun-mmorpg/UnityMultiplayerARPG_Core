using Cysharp.Text;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class BuffRemoval
    {
        [Tooltip("Source of buff (item level, skill level, status effect level and so on)")]
        public BuffSourceData source;
        [Tooltip("Chance to remove buff will be calculated by buff's source level (item level, skill level, status effect level and so on)")]
        public IncrementalFloat removalChance;
        [Min(0f)]
        [Tooltip("If removal chance is `1.5`, it will `100%` resist remove level `1` and `50%` resist remove level `2`.")]
        public float maxChance = 1f;
        [Range(0f, 1f)]
        [Tooltip("If value is `[0.8, 0.5, 0.25]`, and your removal chance is `2.15`, it will have chance `80%` to remove buff level `1`, `50%` to remove level `2`, and `15%` to remove level `3`.")]
        public float[] maxChanceEachLevels = new float[0];

        public string Title => source.Title;
        public string Description => source.Description;
        public Sprite Icon => source.Icon;

        public bool IsValid()
        {
            return source.IsValid();
        }

        public string GetId()
        {
            return source.GetId();
        }

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        public override string ToString()
        {
            return source.ToString();
        }

        public float GetChanceByLevel(float totalChance, int level)
        {
            if (totalChance > maxChance)
                totalChance = maxChance;
            float resistance = totalChance / level;
            if (maxChanceEachLevels == null || maxChanceEachLevels.Length == 0)
                return resistance;
            int resistIndex = Mathf.FloorToInt(totalChance);
            if (resistIndex >= 0)
            {
                if (resistIndex < maxChanceEachLevels.Length)
                    resistance = Mathf.Min(resistance, maxChanceEachLevels[resistIndex]);
                else
                    resistance = Mathf.Min(resistance, maxChanceEachLevels[maxChanceEachLevels.Length - 1]);
            }
            return resistance;
        }

        public string GetChanceEntriesText(float totalChance, string format, string separator = ",")
        {
            if (totalChance > maxChance)
                totalChance = maxChance;
            List<string> entry = new List<string>();
            for (int i = 0; i < totalChance; ++i)
            {
                int level = i + 1;
                float chance = GetChanceByLevel(totalChance, level);
                entry.Add(ZString.Format(
                        LanguageManager.GetText(format),
                        level.ToString("N0"),
                        (chance * 100f).ToString("N2")));
            }
            return string.Join(separator, entry);
        }
    }
}