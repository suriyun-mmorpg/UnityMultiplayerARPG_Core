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

        private void Update()
        {
            if (textGuildName != null)
            {
                if (Guild != null)
                {
                    textGuildName.text = ZString.Format(LanguageManager.GetText(formatKeyGuildName), Guild.guildName);
                    textGuildName.gameObject.SetActive(true);
                }
                else
                {
                    textGuildName.gameObject.SetActive(false);
                }
            }
        }
    }
}
