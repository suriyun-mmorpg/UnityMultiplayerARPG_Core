using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIGuildInvitation : UISelectionEntry<GuildInvitationData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Guild Level}")]
        public UILocaleKeySetting formatKeyGuildLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        // TODO: `uiAnotherCharacter` will be deprecated, still keep it for migration
        [HideInInspector]
        public UICharacter uiAnotherCharacter;

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextGuildName;
        public TextWrapper uiTextGuildLevel;

        protected override void Awake()
        {
            base.Awake();
            OnValidate();
        }

        private void OnValidate()
        {
            if (uiAnotherCharacter != null)
            {
                uiTextName = uiAnotherCharacter.uiTextName;
                uiTextLevel = uiAnotherCharacter.uiTextLevel;
                uiAnotherCharacter = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = string.Format(LanguageManager.GetText(formatKeyName), Data.InviterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(LanguageManager.GetText(formatKeyLevel), Data.InviterLevel.ToString("N0"));

            if (uiTextGuildName != null)
                uiTextGuildName.text = string.Format(LanguageManager.GetText(formatKeyGuildName), Data.GuildName);

            if (uiTextGuildLevel != null)
                uiTextGuildLevel.text = string.Format(LanguageManager.GetText(formatKeyGuildLevel), Data.GuildLevel.ToString("N0"));
        }

        public void OnClickAccept()
        {
            GameInstance.ClientGuildHandlers.RequestAcceptGuildInvitation(new RequestAcceptGuildInvitationMessage()
            {
                guildId = Data.GuildId,
            }, ClientGuildActions.ResponseAcceptGuildInvitation);
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.ClientGuildHandlers.RequestDeclineGuildInvitation(new RequestDeclineGuildInvitationMessage()
            {
                guildId = Data.GuildId,
            }, ClientGuildActions.ResponseDeclineGuildInvitation);
            Hide();
        }
    }
}
