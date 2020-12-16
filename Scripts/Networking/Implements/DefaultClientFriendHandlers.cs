using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientFriendHandlers : MonoBehaviour, IClientFriendHandlers
    {
        public bool RequestGetFriends(RequestGetFriendsMessage data, ResponseDelegate<ResponseGetFriendsMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.GetFriends, data, responseDelegate: callback);
        }

        public bool RequestFindCharacters(RequestFindCharactersMessage data, ResponseDelegate<ResponseFindCharactersMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.FindCharacters, data, responseDelegate: callback);
        }
        public bool RequestAddFriend(RequestAddFriendMessage data, ResponseDelegate<ResponseAddFriendMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.AddFriend, data, responseDelegate: callback);
        }

        public bool RequestRemoveFriend(RequestRemoveFriendMessage data, ResponseDelegate<ResponseRemoveFriendMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.RemoveFriend, data, responseDelegate: callback);
        }
    }
}
