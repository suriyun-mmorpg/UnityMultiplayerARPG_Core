using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientFriendActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseFindCharactersMessage> onResponseFindCharacters;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseGetFriendsMessage> onResponseGetFriends;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseAddFriendMessage> onResponseAddFriend;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRemoveFriendMessage> onResponseRemoveFriend;
        public static System.Action<SocialCharacterData[]> onNotifyFriendsUpdated;

        public static void ResponseFindCharacters(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseFindCharactersMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseFindCharacters != null)
                onResponseFindCharacters.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseGetFriends(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGetFriendsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseGetFriends != null)
                onResponseGetFriends.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseAddFriend(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAddFriendMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseAddFriend != null)
                onResponseAddFriend.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseRemoveFriend(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRemoveFriendMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRemoveFriend != null)
                onResponseRemoveFriend.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyFriendsUpdated(SocialCharacterData[] friends)
        {
            if (onNotifyFriendsUpdated != null)
                onNotifyFriendsUpdated.Invoke(friends);
        }
    }
}
