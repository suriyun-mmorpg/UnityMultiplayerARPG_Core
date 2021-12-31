using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIGuildRequest : UISocialGroup<UISocialCharacter>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        public void Refresh()
        {
            GameInstance.ClientGuildHandlers.RequestGetGuildRequests(GetGuildRequestsCallback);
        }

        private void GetGuildRequestsCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseGetGuildRequestsMessage response)
        {
            ClientGuildActions.ResponseGetGuildRequests(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UpdateGuildRequestsUIs(response.guildRequests);
        }

        private void UpdateGuildRequestsUIs(List<SocialCharacterData> friends)
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
                uiRequester.onGuildRequestAccepted.RemoveListener(Refresh);
                uiRequester.onGuildRequestAccepted.AddListener(Refresh);
                uiRequester.onGuildRequestDeclined.RemoveListener(Refresh);
                uiRequester.onGuildRequestDeclined.AddListener(Refresh);
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
