using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public abstract partial class UISocialGroup : UIBase
    {
        public abstract int GetSocialId();
        public abstract int GetMaxMemberAmount();
        public abstract bool IsLeader(string characterId);
        public abstract bool CanKick(string characterId);
        public abstract bool OwningCharacterIsLeader();
        public abstract bool OwningCharacterCanKick();
    }
    
    public abstract class UISocialGroup<T> : UISocialGroup
        where T : UISocialCharacter
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Amount}, {1} = {Max Amount}")]
        public UILocaleKeySetting formatKeyMemberAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SOCIAL_MEMBER_AMOUNT);
        [Tooltip("Format => {0} = {Current Amount}")]
        public UILocaleKeySetting formatKeyMemberAmountNoLimit = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SOCIAL_MEMBER_AMOUNT_NO_LIMIT);

        [Header("UI Elements")]
        [FormerlySerializedAs("listEmptyObject")]
        public GameObject memberListEmptyObject;
        public T uiMemberDialog;
        public T uiMemberPrefab;
        public Transform uiMemberContainer;
        public TextWrapper textMemberAmount;
        [Tooltip("These objects will be activated when owning character is in social group")]
        public GameObject[] owningCharacterIsInGroupObjects;
        [Tooltip("These objects will be activated when owning character is not in social group")]
        public GameObject[] owningCharacterIsNotInGroupObjects;
        [Tooltip("These objects will be activated when owning character is leader")]
        public GameObject[] owningCharacterIsLeaderObjects;
        [Tooltip("These objects will be activated when owning character is not leader")]
        public GameObject[] owningCharacterIsNotLeaderObjects;
        [Tooltip("These objects will be activated when owning character can kick")]
        public GameObject[] owningCharacterCanKickObjects;
        [Tooltip("These objects will be activated when owning character cannot kick")]
        public GameObject[] owningCharacterCannotKickObjects;

        [Header("Events")]
        public UnityEvent onFriendAdded = new UnityEvent();
        public UnityEvent onFriendRemoved = new UnityEvent();
        public UnityEvent onFriendRequested = new UnityEvent();
        public UnityEvent onFriendRequestAccepted = new UnityEvent();
        public UnityEvent onFriendRequestDeclined = new UnityEvent();
        public UnityEvent onPartyInvited = new UnityEvent();
        public UnityEvent onGuildInvited = new UnityEvent();


        protected int currentSocialId = 0;
        public int memberAmount { get; protected set; }

        private UIList memberList;
        public UIList MemberList
        {
            get
            {
                if (memberList == null)
                {
                    memberList = gameObject.AddComponent<UIList>();
                    memberList.uiPrefab = uiMemberPrefab.gameObject;
                    memberList.uiContainer = uiMemberContainer;
                }
                return memberList;
            }
        }

        private UISocialCharacterSelectionManager memberSelectionManager;
        public UISocialCharacterSelectionManager MemberSelectionManager
        {
            get
            {
                if (memberSelectionManager == null)
                    memberSelectionManager = gameObject.GetOrAddComponent<UISocialCharacterSelectionManager>();
                memberSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return memberSelectionManager;
            }
        }

        protected virtual void Update()
        {
            if (currentSocialId != GetSocialId())
            {
                currentSocialId = GetSocialId();
                UpdateUIs();

                // Refresh guild info
                if (currentSocialId <= 0)
                    MemberList.HideAll();
            }
        }

        protected virtual void UpdateUIs()
        {
            if (textMemberAmount != null)
            {
                if (GetMaxMemberAmount() > 0)
                {
                    textMemberAmount.text = string.Format(
                        LanguageManager.GetText(formatKeyMemberAmount),
                        memberAmount.ToString("N0"),
                        GetMaxMemberAmount().ToString("N0"));
                }
                else
                {
                    textMemberAmount.text = string.Format(
                        LanguageManager.GetText(formatKeyMemberAmountNoLimit),
                        memberAmount.ToString("N0"));
                }
            }
            
            foreach (GameObject obj in owningCharacterIsInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId > 0);
            }

            foreach (GameObject obj in owningCharacterIsNotInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId <= 0);
            }

            foreach (GameObject obj in owningCharacterIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(OwningCharacterIsLeader());
            }

            foreach (GameObject obj in owningCharacterIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!OwningCharacterIsLeader());
            }

            foreach (GameObject obj in owningCharacterCanKickObjects)
            {
                if (obj != null)
                    obj.SetActive(OwningCharacterCanKick());
            }

            foreach (GameObject obj in owningCharacterCannotKickObjects)
            {
                if (obj != null)
                    obj.SetActive(!OwningCharacterCanKick());
            }
        }

        protected virtual void OnEnable()
        {
            MemberSelectionManager.eventOnSelect.RemoveListener(OnSelectMember);
            MemberSelectionManager.eventOnSelect.AddListener(OnSelectMember);
            MemberSelectionManager.eventOnDeselect.RemoveListener(OnDeselectMember);
            MemberSelectionManager.eventOnDeselect.AddListener(OnDeselectMember);
            if (uiMemberDialog != null)
                uiMemberDialog.onHide.AddListener(OnMemberDialogHide);
            UpdateUIs();
        }

        protected virtual void OnDisable()
        {
            if (uiMemberDialog != null)
                uiMemberDialog.onHide.RemoveListener(OnMemberDialogHide);
            MemberSelectionManager.DeselectSelectedUI();
        }

        protected void OnMemberDialogHide()
        {
            MemberSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
            {
                uiMemberDialog.selectionManager = MemberSelectionManager;
                uiMemberDialog.Data = ui.Data;
                uiMemberDialog.Show();
            }
        }

        protected void OnDeselectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
            {
                uiMemberDialog.onHide.RemoveListener(OnMemberDialogHide);
                uiMemberDialog.Hide();
                uiMemberDialog.onHide.AddListener(OnMemberDialogHide);
            }
        }

        public void OnClickAddFriend()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data;
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

        public void OnClickRemoveFriend()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientFriendHandlers.RequestRemoveFriend(new RequestRemoveFriendMessage()
                {
                    friendId = friend.id,
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
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_FRIEND_REQUEST.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_FRIEND_REQUEST_DESCRIPTION.ToString()), friend.characterName), false, true, true, false, null, () =>
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

        public void OnClickAcceptFriendRequest()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data;
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

            SocialCharacterData friend = MemberSelectionManager.SelectedUI.Data;
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

        public void OnClickSendPartyInvitation()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData character = MemberSelectionManager.SelectedUI.Data;
            GameInstance.ClientPartyHandlers.RequestSendPartyInvitation(new RequestSendPartyInvitationMessage()
            {
                inviteeId = character.id,
            }, SendPartyInvitationCallback);
        }

        private void SendPartyInvitationCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSendPartyInvitationMessage response)
        {
            ClientPartyActions.ResponseSendPartyInvitation(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onPartyInvited.Invoke();
        }

        public void OnClickSendGuildInvitation()
        {
            if (MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData character = MemberSelectionManager.SelectedUI.Data;
            GameInstance.ClientGuildHandlers.RequestSendGuildInvitation(new RequestSendGuildInvitationMessage()
            {
                inviteeId = character.id,
            }, SendGuildInvitationCallback);
        }

        private void SendGuildInvitationCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSendGuildInvitationMessage response)
        {
            ClientGuildActions.ResponseSendGuildInvitation(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onGuildInvited.Invoke();
        }
    }
}
