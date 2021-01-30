using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultClientFriendHandlers : MonoBehaviour, IClientFriendHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestGetFriends(ResponseDelegate<ResponseGetFriendsMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.GetFriends, EmptyMessage.Value, responseDelegate: callback);
        }

        public bool RequestFindCharacters(RequestFindCharactersMessage data, ResponseDelegate<ResponseFindCharactersMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.FindCharacters, data, responseDelegate: callback);
        }
        public bool RequestAddFriend(RequestAddFriendMessage data, ResponseDelegate<ResponseAddFriendMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.AddFriend, data, responseDelegate: callback);
        }

        public bool RequestRemoveFriend(RequestRemoveFriendMessage data, ResponseDelegate<ResponseRemoveFriendMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.RemoveFriend, data, responseDelegate: callback);
        }
    }
}
