using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseGameEntity<T> : UISelectionEntry<T>
        where T : BaseGameEntity
    {
        [Header("Base Game Entity - Display Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Title2 Format => {0} = {Title2}")]
        public string title2Format = "{0}";

        [Header("Base Game Entity - UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextTitle2;

        protected override void Update()
        {
            base.Update();

            string tempTitle;
            if (uiTextTitle != null)
            {
                tempTitle = Data == null ? string.Empty : Data.Title;
                uiTextTitle.text = string.Format(titleFormat, tempTitle);
                uiTextTitle.gameObject.SetActive(!string.IsNullOrEmpty(tempTitle));
            }

            if (uiTextTitle2 != null)
            {
                tempTitle = Data == null ? string.Empty : Data.Title2;
                uiTextTitle2.text = string.Format(title2Format, tempTitle);
                uiTextTitle2.gameObject.SetActive(!string.IsNullOrEmpty(tempTitle));
            }
        }
    }

    public class UIBaseGameEntity : UIBaseGameEntity<BaseGameEntity>
    {
        protected override void UpdateData()
        {
        }
    }
}
