using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIFriend : UISocialGroup<UISocialCharacter>
    {
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

        private void UpdateFriendsUIs(List<SocialCharacterData> friends)
        {
            if (friends == null)
                return;

            memberAmount = friends.Count;
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            MemberList.Generate(friends, (index, character, ui) =>
            {
                UISocialCharacter uiFriend = ui.GetComponent<UISocialCharacter>();
                uiFriend.uiSocialGroup = this;
                uiFriend.Data = character;
                uiFriend.Show();
                MemberSelectionManager.Add(uiFriend);
                if (selectedIdx == index)
                    uiFriend.OnClickSelect();
            });
            if (memberListEmptyObject != null)
                memberListEmptyObject.SetActive(friends.Count == 0);
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
    }
}
