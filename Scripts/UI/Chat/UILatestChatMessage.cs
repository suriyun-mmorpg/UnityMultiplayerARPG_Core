namespace MultiplayerARPG
{
    public partial class UILatestChatMessage : UIChatMessage
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            ClientGenericActions.onClientReceiveChatMessage += OnReceiveChat;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClientGenericActions.onClientReceiveChatMessage -= OnReceiveChat;
        }

        private void OnReceiveChat(ChatMessage chatMessage)
        {
            Data = chatMessage;
        }
    }
}
