namespace MultiplayerARPG
{
    public static partial class GameExtensionInstance
    {
        public static CharacterStatsDelegate onAddCharacterStats;
        public static CharacterStatsAndNumberDelegate onMultiplyCharacterStatsWithNumber;
        public static CharacterStatsDelegate onMultiplyCharacterStats;
        public static RandomCharacterStatsDelegate onRandomCharacterStats;
        public static CalculatedBuffDelegate onBuildCalculatedBuff;
        public static CalculatedItemBuffDelegate onBuildCalculatedItemBuff;

        static GameExtensionInstance()
        {
            DevExtUtils.InvokeStaticDevExtMethods(typeof(GameExtensionInstance), "Init");
        }
    }
}
