using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildName : UIBase
    {
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        public TextWrapper textGuildName;

        public GuildData Guild { get { return GameInstance.JoinedGuild; } }

        private string _lastGuildName;
        private bool _appliedHasGuild;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            textGuildName = null;
        }

        private void Update()
        {
            if (textGuildName == null)
                return;
            GuildData guild = Guild;
            bool hasGuild = guild != null;
            string guildName = hasGuild ? guild.guildName : null;
            if (hasGuild == _appliedHasGuild && guildName == _lastGuildName)
                return;
            _appliedHasGuild = hasGuild;
            _lastGuildName = guildName;
            if (hasGuild)
            {
                textGuildName.text = ZString.Format(LanguageManager.GetText(formatKeyGuildName), guildName);
                textGuildName.gameObject.SetActive(true);
            }
            else
            {
                textGuildName.gameObject.SetActive(false);
            }
        }
    }
}
