using Cysharp.Text;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIGuildInvitation : UISelectionEntry<GuildInvitationData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Guild Level}")]
        public UILocaleKeySetting formatKeyGuildLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextGuildName;
        public TextWrapper uiTextGuildLevel;

        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = ZString.Format(LanguageManager.GetText(formatKeyName), Data.InviterName);

            if (uiTextLevel != null)
                uiTextLevel.text = ZString.Format(LanguageManager.GetText(formatKeyLevel), Data.InviterLevel.ToString("N0"));

            if (uiTextGuildName != null)
                uiTextGuildName.text = ZString.Format(LanguageManager.GetText(formatKeyGuildName), Data.GuildName);

            if (uiTextGuildLevel != null)
                uiTextGuildLevel.text = ZString.Format(LanguageManager.GetText(formatKeyGuildLevel), Data.GuildLevel.ToString("N0"));
        }

        public void OnClickAccept()
        {
            GameInstance.ClientGuildHandlers.RequestAcceptGuildInvitation(new RequestAcceptGuildInvitationMessage()
            {
                guildId = Data.GuildId,
                inviterId = Data.InviterId,
            }, AcceptGuildInvitationCallback);
        }

        private void AcceptGuildInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptGuildInvitationMessage response)
        {
            ClientGuildActions.ResponseAcceptGuildInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.ClientGuildHandlers.RequestDeclineGuildInvitation(new RequestDeclineGuildInvitationMessage()
            {
                guildId = Data.GuildId,
                inviterId = Data.InviterId,
            }, DeclineGuildInvitationCallback);
            Hide();
        }

        private void DeclineGuildInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclineGuildInvitationMessage response)
        {
            ClientGuildActions.ResponseDeclineGuildInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }
    }
}
