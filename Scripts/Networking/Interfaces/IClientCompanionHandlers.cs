using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IClientCompanionHandlers
    {
        bool RequestGetCompanions(ResponseDelegate<ResponseGetCompanionsMessage> callback);
        bool RequestSelectCompanion(RequestSelectCompanionMessage data, ResponseDelegate<ResponseSelectCompanionMessage> callback);
    }
}
