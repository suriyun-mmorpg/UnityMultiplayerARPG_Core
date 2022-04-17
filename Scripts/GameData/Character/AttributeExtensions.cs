namespace MultiplayerARPG
{
    public static class AttributeExtensions
    {
        public static CharacterStats GetStats(this Attribute attribute, float level)
        {
            if (attribute == null)
                return new CharacterStats();
            return attribute.statsIncreaseEachLevel * level;
        }

        public static CharacterStats GetStats(this AttributeAmount attributeAmount)
        {
            if (attributeAmount.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeAmount.attribute;
            return attribute.GetStats(attributeAmount.amount);
        }

        public static CharacterStats GetStats(this AttributeIncremental attributeIncremental, short level)
        {
            if (attributeIncremental.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeIncremental.attribute;
            return attribute.GetStats(attributeIncremental.amount.GetAmount(level));
        }
    }
}
