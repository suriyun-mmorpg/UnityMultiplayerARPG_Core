using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IClientUserContentHandlers
    {
        bool RequestAvailableContents(RequestAvailableContentsMessage data, ResponseDelegate<ResponseAvailableContentsMessage> callback);
        bool RequestUnlockContent(RequestUnlockContentMessage data, ResponseDelegate<ResponseUnlockContentMessage> callback);
    }
}