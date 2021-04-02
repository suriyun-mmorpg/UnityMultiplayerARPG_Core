using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class UIFriend : UISocialGroup<UISocialCharacter>
    {
        public GameObject listEmptyObject;
        public UnityEvent onFriendRemoved;

        protected override void OnEnable()
        {
            base.OnEnable();
            ClientFriendActions.onNotifyFriendsUpdated += UpdateFriendsUIs;
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClientFriendActions.onNotifyFriendsUpdated -= UpdateFriendsUIs;
        }

        public void Refresh()
        {
            GameInstance.ClientFriendHandlers.RequestGetFriends(GetFriendsCallback);
        }

        private void GetFriendsCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseGetFriendsMessage response)
        {
            ClientFriendActions.ResponseGetFriends(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UpdateFriendsUIs(response.friends);
        }

        private void UpdateFriendsUIs(SocialCharacterData[] friends)
        {
            if (friends == null)
                return;

            memberAmount = friends.Length;
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            MemberList.Generate(friends, (index, friend, ui) =>
            {
                UISocialCharacterData friendEntity = new UISocialCharacterData();
                friendEntity.socialCharacter = friend;

                UISocialCharacter uiFriend = ui.GetComponent<UISocialCharacter>();
                uiFriend.uiSocialGroup = this;
                uiFriend.Data = friendEntity;
                uiFriend.Show();
                MemberSelectionManager.Add(uiFriend);
                if (selectedIdx == index)
                    uiFriend.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(friends.Length == 0);
        }

        public override bool CanKick(string characterId)
        {
            return false;
        }

        public override int GetMaxMemberAmount()
        {
            return 0;
        }

        public override int GetSocialId()
        {
            return 1;
        }

        public override bool IsLeader(string characterId)
        {
            return false;
        }

        public override bool OwningCharacterCanKick()
        {
            return false;
        }

        public override bool OwningCharacterIsLeader()
        {
            return false;
        }

        public void OnClickRemoveFriend()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestRemoveFriend(new RequestRemoveFriendMessage()
                {
                    friendId = friend.id,
                }, RemoveFriendCallback);
            });
        }

        private void RemoveFriendCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseRemoveFriendMessage response)
        {
            ClientFriendActions.ResponseRemoveFriend(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRemoved.Invoke();
        }
    }
}
