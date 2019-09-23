using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterBuff : UIDataForCharacter<CharacterBuff>
    {
        public CharacterBuff CharacterBuff { get { return Data; } }

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

        protected float buffRemainsDuration;

        private void OnDisable()
        {
            buffRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (buffRemainsDuration <= 0f)
            {
                buffRemainsDuration = CharacterBuff.buffRemainsDuration;
                if (buffRemainsDuration <= 1f)
                    buffRemainsDuration = 0f;
            }

            if (buffRemainsDuration > 0f)
            {
                buffRemainsDuration -= Time.deltaTime;
                if (buffRemainsDuration <= 0f)
                    buffRemainsDuration = 0f;
            }
            else
                buffRemainsDuration = 0f;

            // Update UIs
            float buffDuration = CharacterBuff.GetDuration();

            if (uiTextDuration != null)
            {
                uiTextDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyBuffDuration),
                    buffDuration.ToString("N0"));
            }

            if (uiTextRemainsDuration != null)
            {
                uiTextRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyBuffRemainsDuration),
                    buffRemainsDuration.ToString("N0"));
                uiTextRemainsDuration.gameObject.SetActive(buffRemainsDuration > 0);
            }

            if (imageDurationGage != null)
                imageDurationGage.fillAmount = buffDuration <= 0 ? 0 : buffRemainsDuration / buffDuration;
        }

        protected override void UpdateData()
        {
            BaseGameData buffData = null;
            switch (Data.type)
            {
                case BuffType.SkillBuff:
                case BuffType.SkillDebuff:
                    buffData = Data.GetSkill();
                    break;
                case BuffType.PotionBuff:
                    buffData = Data.GetItem();
                    break;
                case BuffType.GuildSkillBuff:
                    buffData = Data.GetGuildSkill();
                    break;
            }

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    buffData == null ? LanguageManager.GetUnknowTitle() : buffData.Title);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = buffData == null ? null : buffData.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiBuff != null)
            {
                if (buffData == null)
                    uiBuff.Hide();
                else
                {
                    Buff buff = Data.GetBuff();
                    uiBuff.Show();
                    uiBuff.Data = new UIBuffData(buff, Data.level);
                }
            }
        }
    }
}
