using Cysharp.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public abstract class UIBaseGameEntity<T> : UISelectionEntry<T>
        where T : BaseGameEntity
    {
        public enum Visibility
        {
            VisibleWhenSelected,
            VisibleWhenNearby,
            AlwaysVisible,
        }

        [Header("Base Game Entity - String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("Base Game Entity - UI Elements")]
        public TextWrapper uiTextTitle;

        [Header("Visible Options")]
        public Visibility visibility;
        public float visibleDistance = 30f;

        private Canvas _cacheCanvas;
        public Canvas CacheCanvas
        {
            get
            {
                if (_cacheCanvas == null)
                    _cacheCanvas = gameObject.GetOrAddComponent<Canvas>();
                return _cacheCanvas;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CacheCanvas.enabled = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!CacheCanvas.enabled)
                return;

            string tempTitle;
            if (uiTextTitle != null)
            {
                tempTitle = Data == null ? string.Empty : Data.Title;
                uiTextTitle.SetGameObjectActive(!string.IsNullOrEmpty(tempTitle));
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    tempTitle);
            }
        }

        protected virtual bool ValidateToUpdateUI()
        {
            return Data != null && GameInstance.PlayingCharacterEntity != null;
        }

        protected override void UpdateUI()
        {
            if (!ValidateToUpdateUI())
            {
                CacheCanvas.enabled = false;
                return;
            }

            Profiler.BeginSample("UIBaseGameEntity - Update UI");
            BasePlayerCharacterEntity tempPlayingCharacter = GameInstance.PlayingCharacterEntity;
            if (tempPlayingCharacter == Data)
            {
                // Always show the UI when character is owning character
                CacheCanvas.enabled = true;
            }
            else
            {
                switch (visibility)
                {
                    case Visibility.VisibleWhenSelected:
                        BaseGameEntity tempTargetEntity = BasePlayerCharacterController.Singleton.SelectedGameEntity;
                        CacheCanvas.enabled = tempTargetEntity != null &&
                            tempTargetEntity.ObjectId == Data.ObjectId &&
                            Vector3.Distance(tempPlayingCharacter.EntityTransform.position, Data.EntityTransform.position) <= visibleDistance;
                        break;
                    case Visibility.VisibleWhenNearby:
                        CacheCanvas.enabled = Vector3.Distance(tempPlayingCharacter.EntityTransform.position, Data.EntityTransform.position) <= visibleDistance;
                        break;
                    case Visibility.AlwaysVisible:
                        CacheCanvas.enabled = true;
                        break;
                }
            }
            Profiler.EndSample();
        }

        protected override void UpdateData() { }
    }

    public class UIBaseGameEntity : UIBaseGameEntity<BaseGameEntity> { }
}
