namespace MultiplayerARPG
{
    public static class GameNetworkManagerEntensions
    {
        public static IServerUserHandlers GetServerPlayerCharacterHandlers(this LiteNetLibManager.LiteNetLibManager manager)
        {
            return manager as IServerUserHandlers;
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
