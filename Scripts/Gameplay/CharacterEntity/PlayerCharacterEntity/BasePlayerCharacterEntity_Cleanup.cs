namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public override void Clean()
        {
            base.Clean();
            characterDatabases.Nulling();
            characterDatabases = null;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
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
#if !DISABLE_CLASSIC_PK
            onIsPkOnChange = null;
            onPkPointChange = null;
            onConsecutivePkKillsChange = null;
#endif
            onIsWarpingChange = null;
            onHotkeysOperation = null;
            onQuestsOperation = null;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            onCurrenciesOperation = null;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            onServerBoolsOperation = null;
            onServerIntsOperation = null;
            onServerFloatsOperation = null;
            onPrivateBoolsOperation = null;
            onPrivateIntsOperation = null;
            onPrivateFloatsOperation = null;
            onPublicBoolsOperation = null;
            onPublicIntsOperation = null;
            onPublicFloatsOperation = null;
#endif
        }
    }
}
