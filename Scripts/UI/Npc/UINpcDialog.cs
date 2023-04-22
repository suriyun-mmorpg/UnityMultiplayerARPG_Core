using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UINpcDialog : UISelectionEntry<BaseNpcDialog>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public AudioSource voiceSource;

        protected BaseNpcDialog _lastData;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (voiceSource != null)
            {
                if (voiceSource.clip != null)
                    voiceSource.Play();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = null;
            }
        }

        protected override void UpdateData()
        {
            if (_lastData != null)
                _lastData.UnrenderUI(this);

            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (voiceSource != null)
            {
                voiceSource.Stop();
                AudioClip clip = Data == null ? null : Data.voice;
                voiceSource.clip = clip;
                if (clip != null && enabled)
                    voiceSource.Play();
            }

            Data.RenderUI(this).Forget();
            _lastData = Data;
        }
    }
}
