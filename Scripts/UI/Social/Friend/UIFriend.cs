namespace MultiplayerARPG
{
    public class UIFriend : UISocialGroup<UISocialCharacter>
    {
        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateFriends += UpdateFriendsUIs;
            BasePlayerCharacterController.OwningCharacter.RequestGetFriends();
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateFriends -= UpdateFriendsUIs;
        }

        private void UpdateFriendsUIs(SocialGroupData group)
        {
            if (group == null)
                return;

            memberAmount = group.CountMember();
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members = group.GetMembers();
            MemberList.Generate(members, (index, friend, ui) =>
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
        }

        public override bool CanKick(string characterId)
        {
            return false;
        }

        public override int GetMaxMemberAmount()
        {
            // TODO: Implement this
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
                BasePlayerCharacterController.OwningCharacter.RequestRemoveFriend(friend.id);
            });
        }
    }
}
