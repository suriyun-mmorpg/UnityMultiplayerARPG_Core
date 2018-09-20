using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UIPartyMemberSelectionManager))]
    public class UIParty : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Member Amount Format => {0} = {current amount}, {1} = {max amount}")]
        public string memberAmountFormat = "Member Amount: {0}/{1}";

        [Header("UI Elements")]
        public UIPartyMember uiPartyMemberDialog;
        public UIPartyMember uiPartyMemberPrefab;
        public Transform uiPartyMemberContainer;
        public Toggle toggleShareExp;
        public Toggle toggleShareItem;
        public TextWrapper textMemberAmount;
        public UIPartyCreate uiPartyCreate;
        public UIPartySetting uiPartySetting;
        [Tooltip("These objects will be activated when owning character is in party")]
        public GameObject[] owningCharacterIsInPartyObjects;
        [Tooltip("These objects will be activated when owning character is not in party")]
        public GameObject[] owningCharacterIsNotInPartyObjects;
        [Tooltip("These objects will be activated when owning character is leader")]
        public GameObject[] owningCharacterIsLeaderObjects;
        [Tooltip("These objects will be activated when owning character is not leader")]
        public GameObject[] owningCharacterIsNotLeaderObjects;
        public float refreshDuration = 1f;
        private float lastRefreshTime;
        private string currentCharacterId = string.Empty;
        private int currentPartyId = 0;

        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        public int memberAmount { get; private set; }
        public string leaderId { get; private set; }

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPartyMemberPrefab.gameObject;
                    cacheList.uiContainer = uiPartyMemberContainer;
                }
                return cacheList;
            }
        }

        private UIPartyMemberSelectionManager selectionManager;
        public UIPartyMemberSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UIPartyMemberSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                return cacheGameNetworkManager;
            }
        }

        private void Update()
        {
            if (!currentCharacterId.Equals(BasePlayerCharacterController.OwningCharacter.Id) ||
                currentPartyId != BasePlayerCharacterController.OwningCharacter.PartyId)
            {
                currentCharacterId = BasePlayerCharacterController.OwningCharacter.Id;
                currentPartyId = BasePlayerCharacterController.OwningCharacter.PartyId;
                UpdateUIs();

                // Refresh party info
                if (currentPartyId <= 0)
                    CacheList.HideAll();
            }

            // Refresh party info
            if (currentPartyId > 0)
            {
                if (Time.unscaledTime - lastRefreshTime >= refreshDuration)
                {
                    lastRefreshTime = Time.unscaledTime;
                    RefreshPartyInfo();
                }
            }
        }

        private void UpdateUIs()
        {
            if (toggleShareExp != null)
            {
                toggleShareExp.interactable = false;
                toggleShareExp.isOn = shareExp;
            }

            if (toggleShareItem != null)
            {
                toggleShareItem.interactable = false;
                toggleShareItem.isOn = shareItem;
            }

            if (textMemberAmount != null)
                textMemberAmount.text = string.Format(memberAmountFormat, memberAmount.ToString("N0"), GameInstance.Singleton.maxPartyMember.ToString("N0"));

            foreach (var obj in owningCharacterIsInPartyObjects)
            {
                if (obj != null)
                    obj.SetActive(currentPartyId > 0);
            }

            foreach (var obj in owningCharacterIsNotInPartyObjects)
            {
                if (obj != null)
                    obj.SetActive(currentPartyId <= 0);
            }

            foreach (var obj in owningCharacterIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(currentCharacterId.Equals(leaderId));
            }

            foreach (var obj in owningCharacterIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!currentCharacterId.Equals(leaderId));
            }
        }

        public void RefreshPartyInfo()
        {
            // Load cash shop item list
            CacheGameNetworkManager.RequestPartyData(ResponsePartyInfo);
        }

        public override void Show()
        {
            base.Show();
            SelectionManager.eventOnSelect.RemoveListener(OnSelectPartyMember);
            SelectionManager.eventOnSelect.AddListener(OnSelectPartyMember);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectPartyMember);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectPartyMember);
            RefreshPartyInfo();
            UpdateUIs();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            if (uiPartyCreate != null)
                uiPartyCreate.Hide();
            if (uiPartySetting != null)
                uiPartySetting.Hide();
            base.Hide();
        }

        protected void OnSelectPartyMember(UIPartyMember ui)
        {
            if (uiPartyMemberDialog != null)
            {
                uiPartyMemberDialog.selectionManager = SelectionManager;
                uiPartyMemberDialog.Data = ui.Data;
                uiPartyMemberDialog.Show();
            }
        }

        protected void OnDeselectPartyMember(UIPartyMember ui)
        {
            if (uiPartyMemberDialog != null)
                uiPartyMemberDialog.Hide();
        }

        private void ResponsePartyInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponsePartyDataMessage)message;
            if (responseCode == AckResponseCode.Success)
            {
                shareExp = castedMessage.shareExp;
                shareItem = castedMessage.shareItem;
                leaderId = castedMessage.leaderId;
                memberAmount = castedMessage.members.Length;
                UpdateUIs();

                var selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
                SelectionManager.DeselectSelectedUI();
                SelectionManager.Clear();

                CacheList.Generate(castedMessage.members, (index, partyMember, ui) =>
                {
                    var partyMemberEntity = new PartyMemberEntityTuple();
                    partyMemberEntity.partyMember = partyMember;
                    CacheGameNetworkManager.TryGetPlayerCharacterById(partyMember.id, out partyMemberEntity.characterEntity);

                    var uiPartyMember = ui.GetComponent<UIPartyMember>();
                    uiPartyMember.uiParty = this;
                    uiPartyMember.Data = partyMemberEntity;
                    uiPartyMember.Show();
                    SelectionManager.Add(uiPartyMember);
                    if (selectedIdx == index)
                        uiPartyMember.OnClickSelect();
                });
            }
        }

        private bool IsLeader()
        {
            return currentPartyId > 0 && currentCharacterId.Equals(leaderId);
        }

        public void OnClickCreateParty()
        {
            // If already in the party, return
            if (currentPartyId > 0)
                return;
            // Show create party dialog
            if (uiPartyCreate != null)
                uiPartyCreate.Show(false, false);
        }

        public void OnClickSettingParty()
        {
            // If not in the party or not leader, return
            if (!IsLeader())
                return;
            // Show setup party dialog
            if (uiPartySetting != null)
                uiPartySetting.Show(shareExp, shareItem);
        }

        public void OnClickKickFromParty()
        {
            // If not in the party or not leader, return
            if (!IsLeader() || SelectionManager.SelectedUI == null)
                return;

            var partyMember = SelectionManager.SelectedUI.Data.partyMember;
            UISceneGlobal.Singleton.ShowMessageDialog("Kick Member", string.Format("You sure you want to kick {0} from party?", partyMember.characterName), false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromParty(partyMember.id);
            });
        }

        public void OnClickLeaveParty()
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Leave Party", "You sure you want to leave party?", false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestLeaveParty();
            });
        }
    }
}
