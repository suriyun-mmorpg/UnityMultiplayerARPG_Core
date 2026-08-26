using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterBuff : UIDataForCharacter<CharacterBuff>
    {
        public CharacterBuff CharacterBuff { get { return Data; } }
        public CalculatedBuff CalculatedBuff { get { return CharacterBuff.GetBuff(); } }
        public float Duration { get { return CalculatedBuff == null ? 0 : CalculatedBuff.GetDuration(); } }
        public bool NoDuration { get { return CalculatedBuff == null ? false : CalculatedBuff.NoDuration(); } }
        public Buff Buff { get { return CalculatedBuff == null ? null : CalculatedBuff.GetBuff(); } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Buff Duration}")]
        public UILocaleKeySetting formatKeyBuffDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_DURATION);
        [Tooltip("Format => {0} = {Buff Remains Duration}")]
        public UILocaleKeySetting formatKeyBuffRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public Image imageIcon;
        public TextWrapper uiTextDuration;
        public TextWrapper uiTextRemainsDuration;
        public Image imageDurationGage;
        public UIBuff uiBuff;

        protected float _buffRemainsDuration;
        private int _lastDisplayedDuration = -1;
        private int _lastDisplayedRemains = -1;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextTitle = null;
            imageIcon = null;
            uiTextDuration = null;
            uiTextRemainsDuration = null;
            imageDurationGage = null;
            uiBuff = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _buffRemainsDuration = 0f;
            _lastDisplayedDuration = -1;
            _lastDisplayedRemains = -1;
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (_buffRemainsDuration > 0f)
            {
                _buffRemainsDuration -= Time.deltaTime;
                if (_buffRemainsDuration <= 0f)
                    _buffRemainsDuration = 0f;
            }
            else
            {
                _buffRemainsDuration = 0f;
            }

            // Update UIs
            float buffDuration = NoDuration ? 0f : Duration;

            if (uiTextDuration != null)
            {
                int displayedDuration = Mathf.RoundToInt(buffDuration);
                if (displayedDuration != _lastDisplayedDuration)
                {
                    _lastDisplayedDuration = displayedDuration;
                    uiTextDuration.text = ZString.Format(
                        LanguageManager.GetText(formatKeyBuffDuration),
                        displayedDuration.ToString("N0"));
                }
            }

            if (uiTextRemainsDuration != null)
            {
                bool remainsActive = _buffRemainsDuration > 0f;
                uiTextRemainsDuration.SetGameObjectActive(remainsActive);
                int displayedRemains = Mathf.RoundToInt(_buffRemainsDuration);
                if (displayedRemains != _lastDisplayedRemains)
                {
                    _lastDisplayedRemains = displayedRemains;
                    uiTextRemainsDuration.text = ZString.Format(
                        LanguageManager.GetText(formatKeyBuffRemainsDuration),
                        displayedRemains.ToString("N0"));
                }
            }

            if (imageDurationGage != null)
            {
                imageDurationGage.fillAmount = buffDuration <= 0f ? 0f : _buffRemainsDuration / buffDuration;
                imageDurationGage.gameObject.SetActive(imageDurationGage.fillAmount > 0f);
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            // Update remains duration
            if (_buffRemainsDuration <= 0f)
                _buffRemainsDuration = NoDuration ? 0f : CharacterBuff.buffRemainsDuration;
        }

        protected override void UpdateData()
        {
            // Update remains duration
            if (Mathf.Abs(CharacterBuff.buffRemainsDuration - _buffRemainsDuration) > 1)
                _buffRemainsDuration = NoDuration ? 0f : CharacterBuff.buffRemainsDuration;

            BaseGameData tempGameData = null;
            switch (Data.type)
            {
                case BuffType.SkillBuff:
                case BuffType.SkillDebuff:
                    tempGameData = Data.GetSkill();
                    break;
                case BuffType.PotionBuff:
                    tempGameData = Data.GetItem();
                    break;
                case BuffType.GuildSkillBuff:
                    tempGameData = Data.GetGuildSkill();
                    break;
                case BuffType.StatusEffect:
                    tempGameData = Data.GetStatusEffect();
                    break;
            }

            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    tempGameData == null ? LanguageManager.GetUnknowTitle() : tempGameData.Title);
            }

            imageIcon.SetImageGameDataIcon(tempGameData);

            if (uiBuff != null)
            {
                if (tempGameData == null)
                {
                    uiBuff.Hide();
                }
                else
                {
                    Buff buff = Data.GetBuff().GetBuff();
                    uiBuff.Show();
                    uiBuff.Data = new UIBuffData(buff, Data.level);
                }
            }
        }
    }
}
