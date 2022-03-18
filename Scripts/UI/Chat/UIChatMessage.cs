using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class UIChatMessage : UISelectionEntry<ChatMessage>
    {
        [Header("String Formats")]
        [Tooltip("Format {0} = Character Name, {1} = Message")]
        public string formatLocal = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_LOCAL);
        [Tooltip("Format {0} = Character Name, {1} = Message")]
        public string formatGlobal = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_GLOBAL);
        [Tooltip("Format {0} = Character Name, {1} = Message")]
        public string formatWhisper = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_WHISPER);
        [Tooltip("Format {0} = Character Name, {1} = Message")]
        public string formatParty = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_PARTY);
        [Tooltip("Format {0} = Character Name, {1} = Message")]
        public string formatGuild = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_GUILD);
        [Tooltip("Format {0} = Message")]
        public string formatSystem = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CHAT_SYSTEM);

        public TextWrapper uiTextMessage;
        public UIChatHandler uiChatHandler;
        public UnityEvent onIsTypeWriter = new UnityEvent();
        public UnityEvent onNotTypeWriter = new UnityEvent();
        protected override void UpdateData()
        {
            if (uiTextMessage != null)
            {
                string format = string.Empty;
                switch (Data.channel)
                {
                    case ChatChannel.Local:
                        format = LanguageManager.GetText(formatLocal);
                        break;
                    case ChatChannel.Global:
                        format = LanguageManager.GetText(formatGlobal);
                        break;
                    case ChatChannel.Whisper:
                        format = LanguageManager.GetText(formatWhisper);
                        break;
                    case ChatChannel.Party:
                        format = LanguageManager.GetText(formatParty);
                        break;
                    case ChatChannel.Guild:
                        format = LanguageManager.GetText(formatGuild);
                        break;
                }
                if (Data.channel == ChatChannel.System)
                {
                    uiTextMessage.text = string.Format(LanguageManager.GetText(formatSystem), Data.message);
                    onNotTypeWriter.Invoke();
                }
                else
                {
                    uiTextMessage.text = string.Format(format, Data.sender, Data.message);
                    if (GameInstance.PlayingCharacter != null && GameInstance.PlayingCharacter.CharacterName.Equals(Data.sender))
                        onIsTypeWriter.Invoke();
                    else
                        onNotTypeWriter.Invoke();
                }
            }
        }

        public void OnClickEntry()
        {
            if (uiChatHandler != null)
                uiChatHandler.OnClickEntry(this);
        }
    }
}
