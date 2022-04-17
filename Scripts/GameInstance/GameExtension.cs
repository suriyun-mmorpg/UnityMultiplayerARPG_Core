namespace MultiplayerARPG
{
    public static class GameExtension
    {
        static GameExtension()
        {
            DevExtUtils.InvokeStaticDevExtMethods(typeof(GameExtension), "Init");
        }
    }
}
