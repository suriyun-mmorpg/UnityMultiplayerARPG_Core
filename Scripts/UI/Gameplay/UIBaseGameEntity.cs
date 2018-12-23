using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseGameEntity<T> : UISelectionEntry<T>
        where T : BaseGameEntity
    {
        [Header("Base Game Entity - Display Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";

        [Header("Base Game Entity - UI Elements")]
        public TextWrapper uiTextTitle;

        protected override void Update()
        {
            base.Update();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Data == null ? "Unknow" : Data.Title);
        }
    }

    public class UIBaseGameEntity : UIBaseGameEntity<BaseGameEntity>
    {
        protected override void UpdateData()
        {
        }
    }
}
