using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseGameEntity<T> : UISelectionEntry<T>
        where T : BaseGameEntity
    {
        [Header("Base Game Entity - Display Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("TitleB Format => {0} = {TitleB}")]
        public string titleBFormat = "{0}";

        [Header("Base Game Entity - UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextTitleB;

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

            if (uiTextTitleB != null)
            {
                tempTitle = Data == null ? string.Empty : Data.TitleB;
                uiTextTitleB.text = string.Format(titleBFormat, tempTitle);
                uiTextTitleB.gameObject.SetActive(!string.IsNullOrEmpty(tempTitle));
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
