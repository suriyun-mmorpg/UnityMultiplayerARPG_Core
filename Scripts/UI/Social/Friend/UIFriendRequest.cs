using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIFriendRequest : UISocialGroup<UISocialCharacter>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            onFriendRequestAccepted.RemoveListener(Refresh);
            onFriendRequestAccepted.AddListener(Refresh);
            onFriendRequestDeclined.RemoveListener(Refresh);
            onFriendRequestDeclined.AddListener(Refresh);
            Refresh();
        }

        public void Refresh()
        {
            GameInstance.ClientFriendHandlers.RequestGetFriendRequests(GetFriendRequestsCallback);
            // Update notification count
            UIFriendRequestNotification[] notifications = FindObjectsOfType<UIFriendRequestNotification>();
            for (int i = 0; i < notifications.Length; ++i)
            {
                notifications[i].Refresh();
            }
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

            string selectedId = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.SelectedUI.Data.id : string.Empty;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            UISocialCharacter tempUI;
            MemberList.Generate(friends, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UISocialCharacter>();
                tempUI.uiSocialGroup = this;
                tempUI.Data = data;
                tempUI.Show();
                tempUI.onFriendRequestAccepted.RemoveListener(Refresh);
                tempUI.onFriendRequestAccepted.AddListener(Refresh);
                tempUI.onFriendRequestDeclined.RemoveListener(Refresh);
                tempUI.onFriendRequestDeclined.AddListener(Refresh);
                MemberSelectionManager.Add(tempUI);
                if (selectedId.Equals(data.id))
                    tempUI.OnClickSelect();
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
