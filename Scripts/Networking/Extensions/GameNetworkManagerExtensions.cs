namespace MultiplayerARPG
{
    public static class GameNetworkManagerEntensions
    {
        public static IServerPlayerCharacterHandlers GetServerPlayerCharacterHandlers(this LiteNetLibManager.LiteNetLibManager manager)
        {
            return manager as IServerPlayerCharacterHandlers;
        }

        public static IServerPartyHandlers GetServerPartyHandlers(this LiteNetLibManager.LiteNetLibManager manager)
        {
            return manager as IServerPartyHandlers;
        }

        public static IServerGuildHandlers GetServerGuildHandlers(this LiteNetLibManager.LiteNetLibManager manager)
        {
            return manager as IServerGuildHandlers;
        }

        public static IServerStorageHandlers GetServerStorageHandlers(this LiteNetLibManager.LiteNetLibManager manager)
        {
            return manager as IServerStorageHandlers;
        }
    }
}
