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
        protected BasePlayerCharacterEntity _previousEntity;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (voiceSource != null)
            {
                if (voiceSource.clip != null)
                    voiceSource.Play();
            }
            GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
            if (GameInstance.PlayingCharacterEntity != null)
                GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = null;
            }
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
            RemoveEvents(_previousEntity);
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            RemoveEvents(_previousEntity);
            BasePlayerCharacterEntity playerCharacterEntity = playingCharacterData as BasePlayerCharacterEntity;
            _previousEntity = playerCharacterEntity;
            AddEvents(_previousEntity);
            ForceUpdate();
        }

        private void AddEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onLevelChange += PlayingCharacterEntity_onLevelChange;
            PlayingCharacterEntity.onDataIdChange += PlayingCharacterEntity_onDataIdChange;
            PlayingCharacterEntity.onFactionIdChange += PlayingCharacterEntity_onFactionIdChange;
            PlayingCharacterEntity.onNonEquipItemsOperation += PlayingCharacterEntity_onNonEquipItemsOperation;
            PlayingCharacterEntity.onQuestsOperation += PlayingCharacterEntity_onQuestsOperation;
        }

        private void RemoveEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onLevelChange -= PlayingCharacterEntity_onLevelChange;
            PlayingCharacterEntity.onDataIdChange -= PlayingCharacterEntity_onDataIdChange;
            PlayingCharacterEntity.onFactionIdChange -= PlayingCharacterEntity_onFactionIdChange;
            PlayingCharacterEntity.onNonEquipItemsOperation -= PlayingCharacterEntity_onNonEquipItemsOperation;
            PlayingCharacterEntity.onQuestsOperation -= PlayingCharacterEntity_onQuestsOperation;
        }

        private void PlayingCharacterEntity_onLevelChange(int level)
        {
            ForceUpdate();
        }

        private void PlayingCharacterEntity_onDataIdChange(int obj)
        {
            ForceUpdate();
        }

        private void PlayingCharacterEntity_onFactionIdChange(int obj)
        {
            ForceUpdate();
        }

        private void PlayingCharacterEntity_onNonEquipItemsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation op, int index)
        {
            ForceUpdate();
        }

        private void PlayingCharacterEntity_onQuestsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation op, int index)
        {
            ForceUpdate();
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
                Sprite iconSprite = Data == null ? null : Data.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (voiceSource != null)
            {
                voiceSource.Stop();
                AudioClip clip = Data == null ? null : Data.Voice;
                voiceSource.clip = clip;
                if (clip != null && enabled)
                    voiceSource.Play();
            }

            Data.RenderUI(this).Forget();
            _lastData = Data;
        }
    }
}
