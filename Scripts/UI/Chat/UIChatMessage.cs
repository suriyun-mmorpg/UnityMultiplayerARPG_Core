using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatMessage : UISelectionEntry<ChatMessage>
{
    [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
    public string globalFormat = "<color=white>(GLOBAL) {0}: {1}</color>";
    [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
    public string whisperFormat = "<color=green>(WHISPER) {0}: {1}</color>";
    [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
    public string partyFormat = "<color=cyan>(PARTY) {0}: {1}</color>";
    [Tooltip("Chat message format {0} = Character Name, {1} = Message")]
    public string guildFormat = "<color=blue>(GUILD) {0}: {1}</color>";
    public Text textMessage;
    public UIChatHandler uiChatHandler;
    protected override void UpdateData()
    {
        var format = string.Empty;
        switch (Data.channel)
        {
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

        if (textMessage != null)
            textMessage.text = string.Format(format, Data.sender, Data.message);
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
