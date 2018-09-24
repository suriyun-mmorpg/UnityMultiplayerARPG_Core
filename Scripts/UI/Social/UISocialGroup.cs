using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UISocialCharacterSelectionManager))]
    public abstract class UISocialGroup : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Member Amount Format => {0} = {current amount}, {1} = {max amount}")]
        public string memberAmountFormat = "Member Amount: {0}/{1}";

        [Header("UI Elements")]
        public UISocialCharacter uiMemberDialog;
        public UISocialCharacter uiMemberPrefab;
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

        protected string currentCharacterId = string.Empty;
        protected int currentSocialId = 0;
        public int memberAmount { get; protected set; }
        public string leaderId { get; protected set; }

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiMemberPrefab.gameObject;
                    cacheList.uiContainer = uiMemberContainer;
                }
                return cacheList;
            }
        }

        private UISocialCharacterSelectionManager selectionManager;
        public UISocialCharacterSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UISocialCharacterSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        protected virtual void Update()
        {
            if (!currentCharacterId.Equals(BasePlayerCharacterController.OwningCharacter.Id) ||
                currentSocialId != GetSocialId())
            {
                currentCharacterId = BasePlayerCharacterController.OwningCharacter.Id;
                currentSocialId = GetSocialId();
                UpdateUIs();

                // Refresh guild info
                if (currentSocialId <= 0)
                    CacheList.HideAll();
            }
        }

        protected virtual void UpdateUIs()
        {
            if (textMemberAmount != null)
                textMemberAmount.text = string.Format(memberAmountFormat, memberAmount.ToString("N0"), GameInstance.Singleton.maxGuildMember.ToString("N0"));


            foreach (var obj in owningCharacterIsInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId > 0);
            }

            foreach (var obj in owningCharacterIsNotInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId <= 0);
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

        public override void Show()
        {
            base.Show();
            SelectionManager.eventOnSelect.RemoveListener(OnSelectMember);
            SelectionManager.eventOnSelect.AddListener(OnSelectMember);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectMember);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectMember);
            UpdateUIs();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
            {
                uiMemberDialog.selectionManager = SelectionManager;
                uiMemberDialog.Data = ui.Data;
                uiMemberDialog.Show();
            }
        }

        protected void OnDeselectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
                uiMemberDialog.Hide();
        }

        protected bool IsLeader()
        {
            return currentSocialId > 0 && currentCharacterId.Equals(leaderId);
        }

        public abstract int GetSocialId();
    }
}
