using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIChatMessage : UISelectionEntry<ChatMessage>
    {
        [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
        public string localFormat = "<color=white>(LOCAL) {0}: {1}</color>";
        [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
        public string globalFormat = "<color=white>(GLOBAL) {0}: {1}</color>";
        [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
        public string whisperFormat = "<color=green>(WHISPER) {0}: {1}</color>";
        [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
        public string partyFormat = "<color=cyan>(PARTY) {0}: {1}</color>";
        [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
        public string guildFormat = "<color=blue>(GUILD) {0}: {1}</color>";
        public TextWrapper uiTextMessage;
        public UIChatHandler uiChatHandler;
        protected override void UpdateData()
        {
            string format = string.Empty;
            switch (Data.channel)
            {
                case ChatChannel.Local:
                    format = localFormat;
                    break;
                case ChatChannel.Global:
                    format = globalFormat;
                    break;
                case ChatChannel.Whisper:
                    format = whisperFormat;
                    break;
                case ChatChannel.Party:
                    format = partyFormat;
                    break;
                case ChatChannel.Guild:
                    format = guildFormat;
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
