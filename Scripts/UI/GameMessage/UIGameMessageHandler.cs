using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGameMessageHandler : MonoBehaviour
    {
        public TextWrapper messagePrefab;
        public TextWrapper rewardExpPrefab;
        public TextWrapper rewardGoldPrefab;
        public TextWrapper rewardItemPrefab;
        [Tooltip("Format => {0} = {Exp Amount}")]
        public UILocaleKeySetting formatKeyRewardExp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_EXP);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyRewardGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_GOLD);
        [Tooltip("Format => {0} = {Item Title}, {1} => {Amount}")]
        public UILocaleKeySetting formatKeyRewardItem = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_ITEM);
        public Color errorMessageColor = Color.red;
        public Transform messageContainer;
        public float visibleDuration;

        private void Awake()
        {
            if (!rewardExpPrefab)
                rewardExpPrefab = messagePrefab;
            if (!rewardGoldPrefab)
                rewardGoldPrefab = messagePrefab;
            if (!rewardItemPrefab)
                rewardItemPrefab = messagePrefab;
        }

        private void OnEnable()
        {
            ClientGenericActions.onClientReceiveGameMessage += OnReceiveGameMessage;
            ClientGenericActions.onNotifyRewardExp += OnNotifyRewardExp;
            ClientGenericActions.onNotifyRewardGold += OnNotifyRewardGold;
            ClientGenericActions.onNotifyRewardItem += OnNotifyRewardItem;
        }

        private void OnDisable()
        {
            ClientGenericActions.onClientReceiveGameMessage -= OnReceiveGameMessage;
            ClientGenericActions.onNotifyRewardExp -= OnNotifyRewardExp;
            ClientGenericActions.onNotifyRewardGold -= OnNotifyRewardGold;
            ClientGenericActions.onNotifyRewardItem -= OnNotifyRewardItem;
        }

        private void OnReceiveGameMessage(UITextKeys message)
        {
            if (messagePrefab == null)
                return;

            TextWrapper newMessage = Instantiate(messagePrefab);
            newMessage.text = LanguageManager.GetText(message.ToString());
            if (message.ToString().ToUpper().StartsWith("UI_ERROR"))
                newMessage.color = errorMessageColor;
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }

        private void OnNotifyRewardExp(int exp)
        {
            if (rewardExpPrefab == null)
                return;

            TextWrapper newMessage = Instantiate(rewardExpPrefab);
            newMessage.text = string.Format(LanguageManager.GetText(formatKeyRewardExp.ToString()), exp);
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }

        private void OnNotifyRewardGold(int gold)
        {
            if (rewardGoldPrefab == null)
                return;

            TextWrapper newMessage = Instantiate(rewardGoldPrefab);
            newMessage.text = string.Format(LanguageManager.GetText(formatKeyRewardGold.ToString()), gold);
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }

        private void OnNotifyRewardItem(int dataId, short amount)
        {
            BaseItem item;
            if (rewardItemPrefab == null || !GameInstance.Items.TryGetValue(dataId, out item))
                return;

            TextWrapper newMessage = Instantiate(rewardItemPrefab);
            newMessage.text = string.Format(LanguageManager.GetText(formatKeyRewardItem.ToString()), item.Title, amount);
            newMessage.transform.SetParent(messageContainer);
            newMessage.transform.localScale = Vector3.one;
            newMessage.transform.localRotation = Quaternion.identity;
            Destroy(newMessage.gameObject, visibleDuration);
        }
    }
}
