using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIFindCharacters : UISocialGroup<UISocialCharacter>
    {
        public InputFieldWrapper inputCharacterName;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (inputCharacterName)
                inputCharacterName.text = string.Empty;
            OnClickFindCharacters();
        }

        private void UpdateFoundCharactersUIs(List<SocialCharacterData> foundCharacters)
        {
            if (foundCharacters == null)
                return;

            memberAmount = foundCharacters.Count;
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            MemberList.Generate(foundCharacters, (index, character, ui) =>
            {
                UISocialCharacter uiFoundCharacter = ui.GetComponent<UISocialCharacter>();
                uiFoundCharacter.uiSocialGroup = this;
                uiFoundCharacter.Data = character;
                uiFoundCharacter.Show();
                MemberSelectionManager.Add(uiFoundCharacter);
                if (selectedIdx == index)
                    uiFoundCharacter.OnClickSelect();
            });
            if (memberListEmptyObject != null)
                memberListEmptyObject.SetActive(foundCharacters.Count == 0);
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

        public void OnClickFindCharacters()
        {
            string characterName = string.Empty;
            if (inputCharacterName != null)
                characterName = inputCharacterName.text;
            GameInstance.ClientFriendHandlers.RequestFindCharacters(new RequestFindCharactersMessage()
            {
                characterName = characterName,
            }, FindCharactersCallback);
        }

        private void FindCharactersCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSocialCharacterListMessage response)
        {
            ClientFriendActions.ResponseFindCharacters(responseHandler, responseCode, response);
            if (responseCode == AckResponseCode.Success)
                UpdateFoundCharactersUIs(response.characters);
        }
    }
}
