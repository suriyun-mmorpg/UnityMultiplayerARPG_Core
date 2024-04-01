namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public override void Clean()
        {
            base.Clean();
            characterDatabases.Nulling();
#if UNITY_EDITOR || !LNLM_NO_PREFABS
            controllerPrefab = null;
#endif
            addressableControllerPrefab = null;
            Building = null;
            Crafting = null;
            Dealing = null;
            Dueling = null;
            Vending = null;
            NpcAction = null;
            Pk = null;
            // Events
            onDataIdChange = null;
            onFactionIdChange = null;
            onStatPointChange = null;
            onSkillPointChange = null;
            onGoldChange = null;
            onUserGoldChange = null;
            onUserCashChange = null;
            onPartyIdChange = null;
            onGuildIdChange = null;
            onIconDataIdChange = null;
            onFrameDataIdChange = null;
            onTitleDataIdChange = null;
            onIsPkOnChange = null;
            onPkPointChange = null;
            onConsecutivePkKillsChange = null;
            onIsWarpingChange = null;
            onHotkeysOperation = null;
            onQuestsOperation = null;
            onCurrenciesOperation = null;
            onPrivateBoolsOperation = null;
            onPrivateIntsOperation = null;
            onPrivateFloatsOperation = null;
            onPublicBoolsOperation = null;
            onPublicIntsOperation = null;
            onPublicFloatsOperation = null;
        }
    }
}
