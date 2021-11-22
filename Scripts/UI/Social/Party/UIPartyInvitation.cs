using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class UIPartyInvitation : UISelectionEntry<PartyInvitationData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;

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
            }, AcceptPartyInvitationCallback);
        }

        private void AcceptPartyInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptPartyInvitationMessage response)
        {
            ClientPartyActions.ResponseAcceptPartyInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.ClientPartyHandlers.RequestDeclinePartyInvitation(new RequestDeclinePartyInvitationMessage()
            {
                partyId = Data.PartyId,
            }, DeclinePartyInvitationCallback);
            Hide();
        }

        private void DeclinePartyInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclinePartyInvitationMessage response)
        {
            ClientPartyActions.ResponseDeclinePartyInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }
    }
}
