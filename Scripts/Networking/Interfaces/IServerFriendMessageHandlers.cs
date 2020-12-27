using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerFriendMessageHandlers
    {
        UniTaskVoid HandleRequestFindCharacters(
            RequestHandlerData requestHandler, RequestFindCharactersMessage request,
            RequestProceedResultDelegate<ResponseFindCharactersMessage> result);

        UniTaskVoid HandleRequestGetFriends(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseGetFriendsMessage> result);

        UniTaskVoid HandleRequestAddFriend(
            RequestHandlerData requestHandler, RequestAddFriendMessage request,
            RequestProceedResultDelegate<ResponseAddFriendMessage> result);

        UniTaskVoid HandleRequestRemoveFriend(
            RequestHandlerData requestHandler, RequestRemoveFriendMessage request,
            RequestProceedResultDelegate<ResponseRemoveFriendMessage> result);
    }
}
