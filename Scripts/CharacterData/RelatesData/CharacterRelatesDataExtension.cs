using MultiplayerARPG;

public static class CharacterRelatesDataExtension
{
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

    public static bool IsEmpty(this CharacterQuest data)
    {
        return data == null || data.Equals(CharacterQuest.Empty);
    }

    public static bool IsEmpty(this CharacterSkill data)
    {
        return data == null || data.Equals(CharacterSkill.Empty);
    }
}
