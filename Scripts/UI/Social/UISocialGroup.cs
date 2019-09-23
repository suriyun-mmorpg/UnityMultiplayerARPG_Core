using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UISocialGroup : UIBase
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
                    memberSelectionManager = GetComponent<UISocialCharacterSelectionManager>();
                if (memberSelectionManager == null)
                    memberSelectionManager = gameObject.AddComponent<UISocialCharacterSelectionManager>();
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

        public override void Show()
        {
            base.Show();
            MemberSelectionManager.eventOnSelect.RemoveListener(OnSelectMember);
            MemberSelectionManager.eventOnSelect.AddListener(OnSelectMember);
            MemberSelectionManager.eventOnDeselect.RemoveListener(OnDeselectMember);
            MemberSelectionManager.eventOnDeselect.AddListener(OnDeselectMember);
            if (uiMemberDialog != null)
                uiMemberDialog.onHide.AddListener(OnMemberDialogHide);
            UpdateUIs();
        }

        public override void Hide()
        {
            if (uiMemberDialog != null)
                uiMemberDialog.onHide.RemoveListener(OnMemberDialogHide);
            MemberSelectionManager.DeselectSelectedUI();
            base.Hide();
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
    }
}
