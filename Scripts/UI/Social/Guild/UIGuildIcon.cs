using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIGuildIcon : UISelectionEntry<GuildIcon>
    {
        public Image imageIcon;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateData();
        }

        protected override void UpdateData()
        {
            GuildIcon icon = Data;
            if (icon == null)
                icon = GameInstance.GuildIcons.Values.FirstOrDefault();
            if (imageIcon != null)
            {
                Sprite iconSprite = icon == null ? null : icon.Icon;
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
                Data = GameInstance.GuildIcons.Values.FirstOrDefault();
        }
    }
}
