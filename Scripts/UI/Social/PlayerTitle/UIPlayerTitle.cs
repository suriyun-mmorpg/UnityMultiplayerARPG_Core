using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIPlayerTitle : UISelectionEntry<PlayerTitle>
    {
        public Image imageIcon;
        public GameObject[] lockedObjects = new GameObject[0];
        public GameObject[] unlockedObjects = new GameObject[0];
        public bool IsLocked { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateData();
        }

        protected override void UpdateData()
        {
            PlayerTitle icon = Data;
            if (icon == null)
                icon = GameInstance.PlayerTitles.Values.FirstOrDefault();
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
            PlayerTitle guildIcon;
            if (GameInstance.PlayerTitles.TryGetValue(dataId, out guildIcon))
                Data = guildIcon;
            else
                Data = null;
        }

        public void SetIsLocked(bool isLocked)
        {
            foreach (GameObject lockedObject in lockedObjects)
            {
                lockedObject.SetActive(isLocked);
            }
            foreach (GameObject unlockedObject in unlockedObjects)
            {
                unlockedObject.SetActive(!isLocked);
            }
        }
    }
}
