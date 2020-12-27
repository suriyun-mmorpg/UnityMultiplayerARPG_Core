using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIPartyInvitation : UISelectionEntry<PartyInvitationData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        // TODO: `uiAnotherCharacter` will be deprecated, still keep it for migration
        [HideInInspector]
        public UICharacter uiAnotherCharacter;

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;

        protected override void Awake()
        {
            base.Awake();
            OnValidate();
        }

        private void OnValidate()
        {
            if (uiAnotherCharacter != null)
            {
                uiTextName = uiAnotherCharacter.uiTextName;
                uiTextLevel = uiAnotherCharacter.uiTextLevel;
                uiAnotherCharacter = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = string.Format(LanguageManager.GetText(formatKeyName), Data.InviterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(LanguageManager.GetText(formatKeyLevel), Data.InviterLevel.ToString("N0"));
        }

        public void OnClickAccept()
        {
            GameInstance.ClientPartyHandlers.RequestAcceptPartyInvitation(new RequestAcceptPartyInvitationMessage()
            {
                partyId = Data.PartyId,
            }, ClientPartyActions.ResponseAcceptPartyInvitation);
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.ClientPartyHandlers.RequestDeclinePartyInvitation(new RequestDeclinePartyInvitationMessage()
            {
                partyId = Data.PartyId,
            }, ClientPartyActions.ResponseDeclinePartyInvitation);
            Hide();
        }
    }
}
