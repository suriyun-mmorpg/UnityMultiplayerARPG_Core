using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class UIGuildListEntry : UISelectionEntry<GuildListEntry>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public TextWrapper textGuildName;
        public TextWrapper textLevel;

        [Header("Events")]
        public UnityEvent onGuildRequested;

        protected override void UpdateData()
        {
            if (textGuildName != null)
            {
                textGuildName.text = string.Format(
                    LanguageManager.GetText(formatKeyGuildName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.GuildName);
            }

            if (textLevel != null)
            {
                textLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data == null ? "0" : Data.Level.ToString("N0"));
            }
        }

        public void OnClickSendGuildRequest()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_REQUEST.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_GUILD_REQUEST_DESCRIPTION.ToString()), Data.GuildName), false, true, true, false, null, () =>
            {
                GameInstance.ClientGuildHandlers.RequestSendGuildRequest(new RequestSendGuildRequestMessage()
                {
                    guildId = Data.Id,
                }, SendGuildRequestCallback);
            });
        }

        private void SendGuildRequestCallback(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSendGuildRequestMessage response)
        {
            ClientGuildActions.ResponseSendGuildRequest(responseHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            onGuildRequested.Invoke();
        }
    }
}
