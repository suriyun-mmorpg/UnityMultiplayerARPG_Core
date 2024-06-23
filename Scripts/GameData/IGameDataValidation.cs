namespace MultiplayerARPG
{
    public interface IGameDataValidation
    {
        /// <summary>
        /// Return TRUE if it has changes
        /// </summary>
        /// <returns></returns>
        bool OnValidateGameData();
    }
}
