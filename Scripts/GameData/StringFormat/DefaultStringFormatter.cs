namespace MultiplayerARPG
{
    public class DefaultStringFormatter : BaseStringFormatter
    {
        public override string PreprocessFormat(string format)
        {
            format = format.Replace("@characterName", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.CharacterName : "?");
            format = format.Replace("@level", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.Level.ToString("N0") : "?");
            format = format.Replace("@exp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.Exp.ToString("N0") : "?");
            format = format.Replace("@nextExp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.GetNextLevelExp().ToString("N0") : "?");
            format = format.Replace("@currentHp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.CurrentHp.ToString("N0") : "?");
            format = format.Replace("@maxHp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.GetCaches().MaxHp.ToString("N0") : "?");
            format = format.Replace("@currentMp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.CurrentMp.ToString("N0") : "?");
            format = format.Replace("@maxMp", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.GetCaches().MaxMp.ToString("N0") : "?");
            format = format.Replace("@currentMapName", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.CurrentMapName : "?");
            format = format.Replace("@currentPosition", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.CurrentPosition.ToString() : "?");
            format = format.Replace("@respawnMapName", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.RespawnMapName : "?");
            format = format.Replace("@respawnPosition", GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.RespawnPosition.ToString() : "?");
            return format;
        }
    }
}