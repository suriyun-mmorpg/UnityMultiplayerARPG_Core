using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGameMessageHandler : MonoBehaviour
    {
        public TextWrapper messagePrefab;
        public Transform messageContainer;
        public float visibleDuration;

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientReceiveGameMessage += OnReceiveGameMessage;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientReceiveGameMessage -= OnReceiveGameMessage;
        }

        private void OnReceiveGameMessage(GameMessage gameMessage)
        {
            if (messagePrefab == null)
                return;

            TextWrapper newMessage = Instantiate(messagePrefab);
            newMessage.text = LanguageManager.GetText(gameMessage.type.ToString());
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }
    }
}
