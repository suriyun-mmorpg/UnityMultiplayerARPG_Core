using LiteNetLibManager;

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
        public event System.Action<bool> onIsWarpingChange;
        // Sync lists
        public event System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onCurrenciesOperation;
    }
}
