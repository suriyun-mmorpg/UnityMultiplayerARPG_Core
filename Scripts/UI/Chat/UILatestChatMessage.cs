namespace MultiplayerARPG
{
    public partial class UILatestChatMessage : UIChatMessage
    {
        protected override void Awake()
        {
            base.Awake();
            SetOnClientReceiveChatMessage();
        }

        private void OnDestroy()
        {
            RemoveOnClientReceiveChatMessage();
        }

        public void SetOnClientReceiveChatMessage()
        {
            RemoveOnClientReceiveChatMessage();
            ClientGenericActions.onClientReceiveChatMessage += OnReceiveChat;
        }

        public void RemoveOnClientReceiveChatMessage()
        {
            ClientGenericActions.onClientReceiveChatMessage -= OnReceiveChat;
        }

        private void OnReceiveChat(ChatMessage chatMessage)
        {
            Data = chatMessage;
        }
    }
}
