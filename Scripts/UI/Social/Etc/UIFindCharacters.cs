using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class UIFindCharacters : UISocialGroup<UISocialCharacter>
    {
        public GameObject listEmptyObject;
        public InputFieldWrapper inputCharacterName;
        public UnityEvent onFriendAdded;
        public UnityEvent onFriendRequested;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (inputCharacterName)
                inputCharacterName.text = string.Empty;
            OnClickFindCharacters();
        }

        private void UpdateFoundCharactersUIs(SocialCharacterData[] foundCharacters)
        {
            if (foundCharacters == null)
                return;

            memberAmount = foundCharacters.Length;
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            MemberList.Generate(foundCharacters, (index, foundCharacter, ui) =>
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
            if (listEmptyObject != null)
                listEmptyObject.SetActive(foundCharacters.Length == 0);
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

        public void OnClickAddFriend()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestAddFriend(new RequestAddFriendMessage()
                {
                    friendId = friend.id,
                }, AddFriendCallback);
            });
        }

        public void AddFriendCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseAddFriendMessage response)
        {
            ClientFriendActions.ResponseAddFriend(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendAdded.Invoke();
        }

        public void OnClickSendFriendRequest()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestSendFriendRequest(new RequestSendFriendRequestMessage()
                {
                    requesteeId = friend.id,
                }, SendFriendRequestCallback);
            });
        }

        private void SendFriendRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSendFriendRequestMessage response)
        {
            ClientFriendActions.ResponseSendFriendRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRequested.Invoke();
        }
    }
}
