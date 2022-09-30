using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIPlayerFrame : UISelectionEntry<PlayerFrame>
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
            PlayerFrame frame = Data;
            if (frame == null)
                frame = GameInstance.PlayerFrames.Values.FirstOrDefault();
            if (imageIcon != null)
            {
                Sprite iconSprite = frame == null ? null : frame.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }
        }

        public void SetDataByDataId(int dataId)
        {
            PlayerFrame frame;
            if (GameInstance.PlayerFrames.TryGetValue(dataId, out frame))
                Data = frame;
            else
                Data = GameInstance.PlayerFrames.Values.FirstOrDefault();
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
