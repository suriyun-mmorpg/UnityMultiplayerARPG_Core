using LiteNetLibManager;

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

        private void UpdateGuildRequestsUIs(SocialCharacterData[] friends)
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

                UISocialCharacter uiGuild = ui.GetComponent<UISocialCharacter>();
                uiGuild.uiSocialGroup = this;
                uiGuild.Data = friendEntity;
                uiGuild.Show();
                MemberSelectionManager.Add(uiGuild);
                if (selectedIdx == index)
                    uiGuild.OnClickSelect();
            });
            if (memberListEmptyObject != null)
                memberListEmptyObject.SetActive(friends.Length == 0);
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
