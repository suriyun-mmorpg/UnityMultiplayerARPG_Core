namespace MultiplayerARPG
{
    public class UIFindCharacters : UISocialGroup<UISocialCharacter>
    {
        public InputFieldWrapper inputCharacterName;

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateFoundCharacters += UpdateFoundCharactersUIs;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateFoundCharacters -= UpdateFoundCharactersUIs;
        }

        private void UpdateFoundCharactersUIs(SocialGroupData group)
        {
            if (group == null)
                return;

            memberAmount = group.CountMember();
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members = group.GetMembers();
            MemberList.Generate(members, (index, foundCharacter, ui) =>
            {
                UISocialCharacterData foundCharacterEntity = new UISocialCharacterData();
                foundCharacterEntity.socialCharacter = foundCharacter;

                UISocialCharacter uiFoundCharacter = ui.GetComponent<UISocialCharacter>();
                uiFoundCharacter.uiSocialGroup = this;
                uiFoundCharacter.Data = foundCharacterEntity;
                uiFoundCharacter.Show();
                MemberSelectionManager.Add(uiFoundCharacter);
                if (selectedIdx == index)
                    uiFoundCharacter.OnClickSelect();
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

        public void OnClickFindCharacters()
        {
            string characterName = string.Empty;
            if (inputCharacterName != null)
                characterName = inputCharacterName.text;
            BasePlayerCharacterController.OwningCharacter.RequestFindCharacters(characterName);
        }

        public void OnClickAddFriend()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(
                LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestAddFriend(friend.id);
            });
        }
    }
}
