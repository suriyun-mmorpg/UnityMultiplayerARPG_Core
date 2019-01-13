using MultiplayerARPG;

public static class CharacterRelatesDataExtension
{
    public static bool IsEmpty(this CharacterStats data)
    {
        return data.Equals(CharacterStats.Empty);
    }

    public static bool IsEmpty(this CharacterAttribute data)
    {
        return data == null || data.Equals(CharacterAttribute.Empty);
    }

    public static bool IsEmpty(this CharacterBuff data)
    {
        return data == null || data.Equals(CharacterBuff.Empty);
    }

    public static bool IsEmpty(this CharacterHotkey data)
    {
        return data == null || data.Equals(CharacterHotkey.Empty);
    }

    public static bool IsEmpty(this CharacterItem data)
    {
        return data == null || data.Equals(CharacterItem.Empty);
    }

    public static bool IsValid(this CharacterItem data)
    {
        return !data.IsEmpty() && data.GetItem() != null && data.amount > 0;
    }

    public static bool IsEmpty(this CharacterQuest data)
    {
        return data == null || data.Equals(CharacterQuest.Empty);
    }

    public static bool IsEmpty(this CharacterSkill data)
    {
        return data == null || data.Equals(CharacterSkill.Empty);
    }

    public static bool IsEmpty(this CharacterSkillUsage data)
    {
        return data == null || data.Equals(CharacterSkillUsage.Empty);
    }

    public static bool IsEmpty(this CharacterSummon data)
    {
        return data == null || data.Equals(CharacterSummon.Empty);
    }
}
