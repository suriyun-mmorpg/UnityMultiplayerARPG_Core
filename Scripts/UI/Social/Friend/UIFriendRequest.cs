using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIFriendRequest : UISocialGroup<UISocialCharacter>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        public void Refresh()
        {
            GameInstance.ClientFriendHandlers.RequestGetFriendRequests(GetFriendRequestsCallback);
        }

        private void GetFriendRequestsCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseGetFriendRequestsMessage response)
        {
            ClientFriendActions.ResponseGetFriendRequests(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UpdateFriendRequestsUIs(response.friendRequests);
        }

        private void UpdateFriendRequestsUIs(List<SocialCharacterData> friends)
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
                UISocialCharacter uiRequester = ui.GetComponent<UISocialCharacter>();
                uiRequester.uiSocialGroup = this;
                uiRequester.Data = character;
                uiRequester.Show();
                uiRequester.onFriendRequestAccepted.RemoveListener(Refresh);
                uiRequester.onFriendRequestAccepted.AddListener(Refresh);
                uiRequester.onFriendRequestDeclined.RemoveListener(Refresh);
                uiRequester.onFriendRequestDeclined.AddListener(Refresh);
                MemberSelectionManager.Add(uiRequester);
                if (selectedIdx == index)
                    uiRequester.OnClickSelect();
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
