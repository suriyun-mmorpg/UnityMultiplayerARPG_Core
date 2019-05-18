using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIChatMessage : UISelectionEntry<ChatMessage>
    {
        public TextWrapper uiTextMessage;
        public UIChatHandler uiChatHandler;
        protected override void UpdateData()
        {
            string format = string.Empty;
            switch (Data.channel)
            {
                case ChatChannel.Local:
                    format = LanguageManager.GetText(UILocaleKeys.UI_CHAT_FORMAT_LOCAL.ToString());
                    break;
                case ChatChannel.Global:
                    format = LanguageManager.GetText(UILocaleKeys.UI_CHAT_FORMAT_GLOBAL.ToString());
                    break;
                case ChatChannel.Whisper:
                    format = LanguageManager.GetText(UILocaleKeys.UI_CHAT_FORMAT_WHISPER.ToString());
                    break;
                case ChatChannel.Party:
                    format = LanguageManager.GetText(UILocaleKeys.UI_CHAT_FORMAT_PARTY.ToString());
                    break;
                case ChatChannel.Guild:
                    format = LanguageManager.GetText(UILocaleKeys.UI_CHAT_FORMAT_GUILD.ToString());
                    break;
            }

            if (uiTextMessage != null)
                uiTextMessage.text = string.Format(format, Data.sender, Data.message);
        }

        public void OnClickEntry()
        {
            if (uiChatHandler != null)
            {
                uiChatHandler.ShowEnterChatField();
                uiChatHandler.EnterChatMessage = uiChatHandler.whisperCommand + " " + Data.sender;
            }
        }
    }
}
