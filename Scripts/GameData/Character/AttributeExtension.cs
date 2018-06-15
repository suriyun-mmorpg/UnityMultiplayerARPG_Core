using System.Collections;
using System.Collections.Generic;

public static class AttributeExtension
{
    public static CharacterStats GetStats(this Attribute attribute, short level)
    {
        if (attribute == null)
            return new CharacterStats();
        return attribute.statsIncreaseEachLevel * level;
    }

    public static CharacterStats GetStats(this AttributeAmount attributeAmount)
    {
        if (attributeAmount.attribute == null)
            return new CharacterStats();
        var attribute = attributeAmount.attribute;
        return attribute.GetStats(attributeAmount.amount);
    }

    public static CharacterStats GetStats(this AttributeIncremental attributeIncremental, short level)
    {
        if (attributeIncremental.attribute == null)
            return new CharacterStats();
        var attribute = attributeIncremental.attribute;
        return attribute.GetStats(attributeIncremental.amount.GetAmount(level));
    }
}
