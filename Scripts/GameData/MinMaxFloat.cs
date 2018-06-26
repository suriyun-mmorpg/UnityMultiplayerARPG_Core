namespace MultiplayerARPG
{
    [System.Serializable]
    public struct MinMaxFloat
    {
        public float min;
        public float max;

        public static MinMaxFloat operator +(MinMaxFloat a, MinMaxFloat b)
        {
            var result = new MinMaxFloat();
            result.min = a.min + b.min;
            result.max = a.max + b.max;
            return result;
        }

        public static MinMaxFloat operator -(MinMaxFloat a, MinMaxFloat b)
        {
            var result = new MinMaxFloat();
            result.min = a.min - b.min;
            result.max = a.max - b.max;
            return result;
        }

        public static MinMaxFloat operator +(MinMaxFloat a, float amount)
        {
            var result = new MinMaxFloat();
            result.min = a.min + amount;
            result.max = a.max + amount;
            return result;
        }

        public static MinMaxFloat operator -(MinMaxFloat a, float amount)
        {
            var result = new MinMaxFloat();
            result.min = a.min - amount;
            result.max = a.max - amount;
            return result;
        }

        public static MinMaxFloat operator *(MinMaxFloat a, float multiplier)
        {
            var result = new MinMaxFloat();
            result.min = a.min * multiplier;
            result.max = a.max * multiplier;
            return result;
        }

        public static MinMaxFloat operator /(MinMaxFloat a, float divider)
        {
            var result = new MinMaxFloat();
            result.min = a.min / divider;
            result.max = a.max / divider;
            return result;
        }

        public float Random()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
