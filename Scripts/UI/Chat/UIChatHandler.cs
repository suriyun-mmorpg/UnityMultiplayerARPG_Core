using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatHandler : UIBase
{
    public string whisperCommand = "/w";
    /* TODO: Implement this later
    public string partyCommand = "/p";
    public string guildCommand = "/g";
    */
    public KeyCode enterChatKey = KeyCode.Return;
    public int chatEntrySize = 30;
    public GameObject[] enterChatActiveObjects;
    public InputField enterChatField;
    public UIChatMessage uiChatMessagePrefab;
    public Transform uiChatMessageContainer;
    public ScrollRect scrollRect;

    private readonly List<ChatMessage> chatMessages = new List<ChatMessage>();
    private bool enterChatFieldVisible;

    public string EnterChatMessage
    {
        get { return enterChatField == null ? string.Empty : enterChatField.text; }
        set { if (enterChatField != null) enterChatField.text = value; }
    }

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiChatMessagePrefab.gameObject;
                cacheList.uiContainer = uiChatMessageContainer;
            }
            return cacheList;
        }
    }

    private BaseGameNetworkManager cacheGameNetworkManager;
    public BaseGameNetworkManager CacheGameNetworkManager
    {
        get
        {
            if (cacheGameNetworkManager == null)
                cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
            return cacheGameNetworkManager;
        }
    }

    private void Start()
    {
        HideEnterChatField();
        if (CacheGameNetworkManager != null)
            CacheGameNetworkManager.onReceiveChat += OnReceiveChat;
        if (enterChatField != null)
        {
            enterChatField.onValueChanged.RemoveListener(OnInputFieldValueChange);
            enterChatField.onValueChanged.AddListener(OnInputFieldValueChange);
        }
    }

    private void OnDestroy()
    {
        if (CacheGameNetworkManager != null)
            CacheGameNetworkManager.onReceiveChat -= OnReceiveChat;
    }

    private void Update()
    {
        if (Input.GetKeyDown(enterChatKey))
        {
            if (!enterChatFieldVisible)
                ShowEnterChatField();
            else
                SendChatMessage();
        }
    }

    public void ToggleEnterChatField()
    {
        if (enterChatFieldVisible)
            HideEnterChatField();
        else
            ShowEnterChatField();
    }

    public void ShowEnterChatField()
    {
        foreach (var enterChatActiveObject in enterChatActiveObjects)
        {
            if (enterChatActiveObject != null)
                enterChatActiveObject.SetActive(true);
        }
        if (enterChatField != null)
        {
            enterChatField.Select();
            enterChatField.ActivateInputField();
        }
        enterChatFieldVisible = true;
    }

    public void HideEnterChatField()
    {
        foreach (var enterChatActiveObject in enterChatActiveObjects)
        {
            if (enterChatActiveObject != null)
                enterChatActiveObject.SetActive(false);
        }
        if (enterChatField != null)
            enterChatField.DeactivateInputField();
        enterChatFieldVisible = false;
    }

    public void SendChatMessage()
    {
        if (BasePlayerCharacterController.OwningCharacter == null)
            return;

        var trimText = EnterChatMessage.Trim();
        if (trimText.Length == 0)
            return;

        EnterChatMessage = string.Empty;
        var channel = ChatChannel.Global;
        var message = trimText;
        var sender = BasePlayerCharacterController.OwningCharacter.CharacterName;
        var receiver = string.Empty;
        var splitedText = trimText.Split(' ');
        if (splitedText.Length > 0)
        {
            var cmd = splitedText[0];
            if (cmd == whisperCommand && splitedText.Length > 2)
            {
                channel = ChatChannel.Whisper;
                receiver = splitedText[1];
                message = trimText.Substring(cmd.Length + receiver.Length + 1); // +1 for space
                EnterChatMessage = trimText.Substring(0, cmd.Length + receiver.Length + 1); // +1 for space
            }
        }
        CacheGameNetworkManager.EnterChat(channel, message, sender, receiver);
        HideEnterChatField();
    }

    private void OnReceiveChat(ChatMessage chatMessage)
    {
        chatMessages.Add(chatMessage);
        if (chatMessages.Count > chatEntrySize)
            chatMessages.RemoveAt(0);
        CacheList.Generate(chatMessages, (index, message, ui) =>
        {
            var uiChatMessage = ui.GetComponent<UIChatMessage>();
            uiChatMessage.uiChatHandler = this;
            uiChatMessage.Data = message;
            uiChatMessage.Show();
        });
        StartCoroutine(VerticalScroll(0f));
    }

    private void OnInputFieldValueChange(string text)
    {
        if (text.Length > 0 && !enterChatFieldVisible)
            ShowEnterChatField();
    }

    IEnumerator VerticalScroll(float normalize)
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            scrollRect.verticalScrollbar.value = normalize;
            Canvas.ForceUpdateCanvases();
        }
    }
}
