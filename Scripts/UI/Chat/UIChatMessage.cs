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
        public TextWrapper uiTextSenderOnly;
        public TextWrapper uiTextMessageOnly;
        public TextWrapper uiTextTimestamp;
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
            if (uiTextSenderOnly != null)
                uiTextSenderOnly.text = Data.sender;
            if (uiTextMessageOnly != null)
                uiTextMessageOnly.text = Data.message;
            InvokeRepeating(nameof(UpdateTimestamp), 0f, 5f);
        }

        private void UpdateTimestamp()
        {
            if (uiTextTimestamp != null)
            {
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(Data.timestamp).ToLocalTime();
                uiTextTimestamp.text = new System.DateTime(dateTime.Ticks).GetPrettyDate();
            }
        }

        public void OnClickEntry()
        {
            if (uiChatHandler != null)
                uiChatHandler.OnClickEntry(this);
        }
    }
}
