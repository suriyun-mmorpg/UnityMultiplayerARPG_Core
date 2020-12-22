using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientFriendHandlers : MonoBehaviour, IClientFriendHandlers
    {
        public bool RequestGetFriends(RequestGetFriendsMessage data, ResponseDelegate<ResponseGetFriendsMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.GetFriends, data, responseDelegate: callback);
        }

        public bool RequestFindCharacters(RequestFindCharactersMessage data, ResponseDelegate<ResponseFindCharactersMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.FindCharacters, data, responseDelegate: callback);
        }
        public bool RequestAddFriend(RequestAddFriendMessage data, ResponseDelegate<ResponseAddFriendMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.AddFriend, data, responseDelegate: callback);
        }

        public bool RequestRemoveFriend(RequestRemoveFriendMessage data, ResponseDelegate<ResponseRemoveFriendMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.RemoveFriend, data, responseDelegate: callback);
        }
    }
}
