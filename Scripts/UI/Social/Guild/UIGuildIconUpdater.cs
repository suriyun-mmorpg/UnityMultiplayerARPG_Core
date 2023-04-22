using Newtonsoft.Json;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildIconUpdater : MonoBehaviour
    {
        public UIGuildIcon uiGuildIcon;

        public void UpdateData()
        {
            if (uiGuildIcon == null) return;
            UpdateData(uiGuildIcon.Data);
        }

        public void UpdateData(GuildIcon data)
        {
            if (GameInstance.JoinedGuild == null || data == null)
            {
                // No joined guild data, so it can't update guild data
                return;
            }
            // Get current guild options before modify and save
            GuildOptions options = new GuildOptions();
            if (!string.IsNullOrEmpty(GameInstance.JoinedGuild.options))
                options = JsonConvert.DeserializeObject<GuildOptions>(GameInstance.JoinedGuild.options);
            options.iconDataId = data.DataId;
            GameInstance.ClientGuildHandlers.RequestChangeGuildOptions(new RequestChangeGuildOptionsMessage()
            {
                options = JsonConvert.SerializeObject(options),
            }, ClientGuildActions.ResponseChangeGuildOptions);
        }
    }
}
