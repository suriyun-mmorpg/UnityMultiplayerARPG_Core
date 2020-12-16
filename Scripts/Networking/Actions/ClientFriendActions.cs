using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ClientFriendActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseFindCharactersMessage> onResponseFindCharacters;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseGetFriendsMessage> onResponseGetFriends;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseAddFriendMessage> onResponseAddFriend;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRemoveFriendMessage> onResponseRemoveFriend;
        public static System.Action<List<SocialCharacterData>> onNotifyFriendsUpdated;

        public static async UniTaskVoid ResponseFindCharacters(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseFindCharactersMessage response)
        {
            await UniTask.Yield();
            if (onResponseFindCharacters != null)
                onResponseFindCharacters.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseGetFriends(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGetFriendsMessage response)
        {
            await UniTask.Yield();
            if (onResponseGetFriends != null)
                onResponseGetFriends.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseAddFriend(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAddFriendMessage response)
        {
            await UniTask.Yield();
            if (onResponseAddFriend != null)
                onResponseAddFriend.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseRemoveFriend(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRemoveFriendMessage response)
        {
            await UniTask.Yield();
            if (onResponseRemoveFriend != null)
                onResponseRemoveFriend.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyFriendsUpdated(List<SocialCharacterData> friends)
        {
            if (onNotifyFriendsUpdated != null)
                onNotifyFriendsUpdated.Invoke(friends);
        }
    }
}
