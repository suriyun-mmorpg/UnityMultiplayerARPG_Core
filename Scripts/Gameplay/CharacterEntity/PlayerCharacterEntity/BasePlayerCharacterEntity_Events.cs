using LiteNetLibManager;
using NotifiableCollection;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        // Note: You may use `Awake` dev extension to setup an events and `OnDestroy` to desetup an events
        // Sync variables
        public event System.Action<int> onDataIdChange;
        public event System.Action<int> onFactionIdChange;
        public event System.Action<float> onStatPointChange;
        public event System.Action<float> onSkillPointChange;
        public event System.Action<int> onGoldChange;
        public event System.Action<int> onUserGoldChange;
        public event System.Action<int> onUserCashChange;
        public event System.Action<int> onPartyIdChange;
        public event System.Action<int> onGuildIdChange;
        public event System.Action<int> onIconDataIdChange;
        public event System.Action<int> onFrameDataIdChange;
        public event System.Action<int> onTitleDataIdChange;
#if !DISABLE_CLASSIC_PK
        public event System.Action<bool> onIsPkOnChange;
        public event System.Action<int> onPkPointChange;
        public event System.Action<int> onConsecutivePkKillsChange;
#endif
        public event System.Action<int> onReputationChange;
        public event System.Action<bool> onIsWarpingChange;
        // Sync lists
        public event System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
        public event System.Action<LiteNetLibSyncList.Operation, int> onCurrenciesOperation;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
        public event System.Action<NotifiableListAction, int> onServerBoolsOperation;
        public event System.Action<NotifiableListAction, int> onServerIntsOperation;
        public event System.Action<NotifiableListAction, int> onServerFloatsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPrivateBoolsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPrivateIntsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPrivateFloatsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPublicBoolsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPublicIntsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onPublicFloatsOperation;
#endif
        public event System.Action<LiteNetLibSyncList.Operation, int> onGuildSkillsOperation;

        public override void OnRewardItem(RewardGivenType givenType, BaseItem item, int amount)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, givenType, item.DataId, amount);
            GameInstance.ServerLogHandlers.LogRewardItem(this, givenType, item, amount);
        }

        public override void OnRewardItem(RewardGivenType givenType, CharacterItem item)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, givenType, item.dataId, item.amount);
            GameInstance.ServerLogHandlers.LogRewardItem(this, givenType, item);
        }

        public override void OnRewardGold(RewardGivenType givenType, int gold)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(ConnectionId, givenType, gold);
            GameInstance.ServerLogHandlers.LogRewardGold(this, givenType, gold);
        }

        public override void OnRewardExp(RewardGivenType givenType, int exp, bool isLevelUp)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, givenType, exp);
            GameInstance.ServerLogHandlers.LogRewardExp(this, givenType, exp, isLevelUp);
        }

        public override void OnRewardCurrency(RewardGivenType givenType, Currency currency, int amount)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, givenType, currency.DataId, amount);
            GameInstance.ServerLogHandlers.LogRewardCurrency(this, givenType, currency, amount);
        }

        public override void OnRewardCurrency(RewardGivenType givenType, CharacterCurrency currency)
        {
            GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, givenType, currency.dataId, currency.amount);
            GameInstance.ServerLogHandlers.LogRewardCurrency(this, givenType, currency);
        }
    }
}
