using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class UISocialCharacter : UISelectionEntry<UISocialCharacterData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public UISocialGroup uiSocialGroup;
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public UIGageValue uiGageHp;
        public UIGageValue uiGageMp;

        public UICharacterBuffs uiCharacterBuffs;
        [Header("Member states objects")]
        [Tooltip("These objects will be activated when social member -> isOnline is true")]
        public GameObject[] memberIsOnlineObjects;
        [Tooltip("These objects will be activated when social member -> isOnline is false")]
        public GameObject[] memberIsNotOnlineObjects;
        [Tooltip("These objects will be activated when this social member is leader")]
        public GameObject[] memberIsLeaderObjects;
        [Tooltip("These objects will be activated when this social member is not leader")]
        public GameObject[] memberIsNotLeaderObjects;
        public UICharacterClass uiCharacterClass;

        [Header("Events")]
        public UnityEvent onFriendAdded = new UnityEvent();
        public UnityEvent onFriendRemoved = new UnityEvent();
        public UnityEvent onFriendRequested = new UnityEvent();
        public UnityEvent onFriendRequestAccepted = new UnityEvent();
        public UnityEvent onFriendRequestDeclined = new UnityEvent();
        public UnityEvent onGuildRequestAccepted = new UnityEvent();
        public UnityEvent onGuildRequestDeclined = new UnityEvent();

        protected override void Update()
        {
            base.Update();

            // Member status
            foreach (GameObject obj in memberIsOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(GameInstance.ClientOnlineCharacterHandlers.IsCharacterOnline(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsNotOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(!GameInstance.ClientOnlineCharacterHandlers.IsCharacterOnline(Data.socialCharacter.id));
            }

            GameInstance.ClientOnlineCharacterHandlers.RequestOnlineCharacter(Data.socialCharacter.id);
        }

        protected override void UpdateData()
        {
            if (uiTextName != null)
            {
                uiTextName.text = string.Format(
                    LanguageManager.GetText(formatKeyName),
                    string.IsNullOrEmpty(Data.socialCharacter.characterName) ? LanguageManager.GetUnknowTitle() : Data.socialCharacter.characterName);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data.socialCharacter.level.ToString("N0"));
            }

            // Hp
            int currentHp = Data.socialCharacter.currentHp;
            int maxHp = Data.socialCharacter.maxHp;
            if (uiGageHp != null)
            {
                uiGageHp.Update(currentHp, maxHp);
                if (uiGageHp.textValue != null)
                    uiGageHp.textValue.SetGameObjectActive(maxHp > 0);
            }

            // Mp
            int currentMp = Data.socialCharacter.currentMp;
            int maxMp = Data.socialCharacter.maxMp;
            if (uiGageMp != null)
            {
                uiGageMp.Update(currentMp, maxMp);
                if (uiGageMp.textValue != null)
                    uiGageMp.textValue.SetGameObjectActive(maxMp > 0);
            }

            // Buffs
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data.characterEntity);

            foreach (GameObject obj in memberIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!string.IsNullOrEmpty(Data.socialCharacter.id) && uiSocialGroup.IsLeader(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(string.IsNullOrEmpty(Data.socialCharacter.id) || !uiSocialGroup.IsLeader(Data.socialCharacter.id));
            }

            // Character class data
            PlayerCharacter character;
            GameInstance.PlayerCharacters.TryGetValue(Data.socialCharacter.dataId, out character);
            if (uiCharacterClass != null)
                uiCharacterClass.Data = character;
        }

        public void OnClickAddFriend()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_ADD_DESCRIPTION.ToString()), Data.socialCharacter.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestAddFriend(new RequestAddFriendMessage()
                {
                    friendId = Data.socialCharacter.id,
                }, AddFriendCallback);
            });
        }

        public void AddFriendCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseAddFriendMessage response)
        {
            ClientFriendActions.ResponseAddFriend(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendAdded.Invoke();
        }

        public void OnClickRemoveFriend()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString()), Data.socialCharacter.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestRemoveFriend(new RequestRemoveFriendMessage()
                {
                    friendId = Data.socialCharacter.id,
                }, RemoveFriendCallback);
            });
        }

        private void RemoveFriendCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseRemoveFriendMessage response)
        {
            ClientFriendActions.ResponseRemoveFriend(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRemoved.Invoke();
        }

        public void OnClickSendFriendRequest()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString()), Data.socialCharacter.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestSendFriendRequest(new RequestSendFriendRequestMessage()
                {
                    requesteeId = Data.socialCharacter.id,
                }, SendFriendRequestCallback);
            });
        }

        private void SendFriendRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSendFriendRequestMessage response)
        {
            ClientFriendActions.ResponseSendFriendRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRequested.Invoke();
        }

        public void OnClickAcceptFriendRequest()
        {
            GameInstance.ClientFriendHandlers.RequestAcceptFriendRequest(new RequestAcceptFriendRequestMessage()
            {
                requesterId = Data.socialCharacter.id,
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
            GameInstance.ClientFriendHandlers.RequestDeclineFriendRequest(new RequestDeclineFriendRequestMessage()
            {
                requesterId = Data.socialCharacter.id,
            }, DeclineFriendRequestCallback);
        }

        private void DeclineFriendRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDeclineFriendRequestMessage response)
        {
            ClientFriendActions.ResponseDeclineFriendRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onFriendRequestDeclined.Invoke();
        }

        public void OnClickAcceptGuildRequest()
        {
            GameInstance.ClientGuildHandlers.RequestAcceptGuildRequest(new RequestAcceptGuildRequestMessage()
            {
                requesterId = Data.socialCharacter.id,
            }, AcceptGuildRequestCallback);
        }

        private void AcceptGuildRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseAcceptGuildRequestMessage response)
        {
            ClientGuildActions.ResponseAcceptGuildRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onGuildRequestAccepted.Invoke();
        }

        public void OnClickDeclineGuildRequest()
        {
            GameInstance.ClientGuildHandlers.RequestDeclineGuildRequest(new RequestDeclineGuildRequestMessage()
            {
                requesterId = Data.socialCharacter.id,
            }, DeclineGuildRequestCallback);
        }

        private void DeclineGuildRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDeclineGuildRequestMessage response)
        {
            ClientGuildActions.ResponseDeclineGuildRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onGuildRequestDeclined.Invoke();
        }
    }
}
