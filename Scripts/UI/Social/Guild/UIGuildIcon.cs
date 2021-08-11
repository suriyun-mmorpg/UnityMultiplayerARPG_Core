using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIGuildIcon : UISelectionEntry<GuildIcon>
    {
        public Image imageIcon;

        protected override void UpdateData()
        {
            if (Data == null)
                Data = GameInstance.GuildIcons.Values.FirstOrDefault();

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }
        }

        public void SetDataByDataId(int dataId)
        {
            GuildIcon guildIcon;
            if (GameInstance.GuildIcons.TryGetValue(dataId, out guildIcon))
                Data = guildIcon;
            else
                Data = null;
        }
    }
}
