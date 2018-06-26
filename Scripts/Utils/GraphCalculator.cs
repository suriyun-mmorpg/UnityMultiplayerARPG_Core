using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class GraphCalculator<T>
    {
        public T minValue;
        public T maxValue;
        public float growth;
        public abstract T Calculate(short currentLevel, short maxLevel);
    }

    [System.Serializable]
    public class Int32GraphCalculator : GraphCalculator<int>
    {
        public Int32GraphCalculator Clone()
        {
            var result = new Int32GraphCalculator();
            result.minValue = minValue;
            result.maxValue = maxValue;
            result.growth = growth;
            return result;
        }

        public override int Calculate(short currentLevel, short maxLevel)
        {
            if (currentLevel <= 0)
                currentLevel = 1;
            if (maxLevel <= 0)
                maxLevel = 1;
            if (currentLevel == 1)
                return minValue;
            if (currentLevel == maxLevel)
                return maxValue;
            return minValue + Mathf.RoundToInt((maxValue - minValue) * Mathf.Pow((float)(currentLevel - 1) / (float)(maxLevel - 1), growth));
        }

        public static Int32GraphCalculator operator *(Int32GraphCalculator a, float multiplier)
        {
            var result = a.Clone();
            result.minValue = Mathf.RoundToInt(a.minValue * multiplier);
            result.maxValue = Mathf.RoundToInt(a.maxValue * multiplier);
            return result;
        }
    }

    [System.Serializable]
    public class SingleGraphCalculator : GraphCalculator<float>
    {
        public SingleGraphCalculator Clone()
        {
            var result = new SingleGraphCalculator();
            result.minValue = minValue;
            result.maxValue = maxValue;
            result.growth = growth;
            return result;
        }

        public override float Calculate(short currentLevel, short maxLevel)
        {
            if (currentLevel <= 0)
                currentLevel = 1;
            if (maxLevel <= 0)
                maxLevel = 1;
            if (currentLevel == 1)
                return minValue;
            if (currentLevel == maxLevel)
                return maxValue;
            return minValue + ((maxValue - minValue) * Mathf.Pow((float)(currentLevel - 1) / (float)(maxLevel - 1), growth));
        }

        public static SingleGraphCalculator operator *(SingleGraphCalculator a, float multiplier)
        {
            var result = a.Clone();
            result.minValue = a.minValue * multiplier;
            result.maxValue = a.maxValue * multiplier;
            return result;
        }
    }
}
