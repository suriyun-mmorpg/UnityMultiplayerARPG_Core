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
                tempUI.onGuildRequestAccepted.RemoveListener(Refresh);
                tempUI.onGuildRequestAccepted.AddListener(Refresh);
                tempUI.onGuildRequestDeclined.RemoveListener(Refresh);
                tempUI.onGuildRequestDeclined.AddListener(Refresh);
                MemberSelectionManager.Add(tempUI);
                if (index == 0 || selectedId.Equals(data.id))
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
