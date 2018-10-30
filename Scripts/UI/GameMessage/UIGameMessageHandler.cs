using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGameMessageHandler : MonoBehaviour
    {
        public TextWrapper messagePrefab;
        public Transform messageContainer;
        public float visibleDuration;

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

        private void OnEnable()
        {
            if (CacheGameNetworkManager != null)
                CacheGameNetworkManager.onClientReceiveGameMessage += OnReceiveGameMessage;
        }

        private void OnDisable()
        {
            if (CacheGameNetworkManager != null)
                CacheGameNetworkManager.onClientReceiveGameMessage -= OnReceiveGameMessage;
        }

        private void OnReceiveGameMessage(GameMessage gameMessage)
        {
            if (messagePrefab == null)
                return;

            var newMessage = Instantiate(messagePrefab);
            newMessage.text = LanguageManager.GetText(gameMessage.type.ToString());
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }
    }
}
