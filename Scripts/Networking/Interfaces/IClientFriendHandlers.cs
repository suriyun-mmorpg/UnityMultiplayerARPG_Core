using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientFriendHandlers
    {
        bool RequestFindCharacters(RequestFindCharactersMessage data, ResponseDelegate<ResponseFindCharactersMessage> callback);
        bool RequestGetFriends(RequestGetFriendsMessage data, ResponseDelegate<ResponseGetFriendsMessage> callback);
        bool RequestAddFriend(RequestAddFriendMessage data, ResponseDelegate<ResponseAddFriendMessage> callback);
        bool RequestRemoveFriend(RequestRemoveFriendMessage data, ResponseDelegate<ResponseRemoveFriendMessage> callback);
    }
}
