using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientGachaHandlers
    {
        bool RequestOpenGacha(RequestOpenGachaMessage data, ResponseDelegate<ResponseOpenGachaMessage> callback);
    }
}
