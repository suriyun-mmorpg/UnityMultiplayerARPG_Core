using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class UIFriendRequest : UISocialGroup<UISocialCharacter>
    {
        public GameObject listEmptyObject;
        public UnityEvent onFriendRequestAccepted;
        public UnityEvent onFriendRequestDeclined;

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

        private void UpdateFriendRequestsUIs(SocialCharacterData[] friends)
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

        public void OnClickAcceptFriendRequest()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            GameInstance.ClientFriendHandlers.RequestAcceptFriendRequest(new RequestAcceptFriendRequestMessage()
            {
                requesterId = friend.id,
            }, AcceptFriendRequestCallback);
        }

        private void AcceptFriendRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseAcceptFriendRequestMessage response)
        {
            ClientFriendActions.ResponseAcceptFriendRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRequestAccepted.Invoke();
        }

        public void OnClickDeclineFriendRequest()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            GameInstance.ClientFriendHandlers.RequestDeclineFriendRequest(new RequestDeclineFriendRequestMessage()
            {
                requesterId = friend.id,
            }, DeclineFriendRequestCallback);
        }

        private void DeclineFriendRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDeclineFriendRequestMessage response)
        {
            ClientFriendActions.ResponseDeclineFriendRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRequestDeclined.Invoke();
        }
    }
}
