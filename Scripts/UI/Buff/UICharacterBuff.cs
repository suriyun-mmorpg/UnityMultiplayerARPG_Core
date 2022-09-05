using Cysharp.Text;
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

        protected override void OnDisable()
        {
            base.OnDisable();
            buffRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (buffRemainsDuration > 0f)
            {
                buffRemainsDuration -= Time.deltaTime;
                if (buffRemainsDuration <= 0f)
                    buffRemainsDuration = 0f;
            }
            else
            {
                buffRemainsDuration = 0f;
            }

            // Update UIs
            float buffDuration = CharacterBuff.GetBuff().GetDuration();

            if (uiTextDuration != null)
            {
                uiTextDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyBuffDuration),
                    buffDuration.ToString("N0"));
            }

            if (uiTextRemainsDuration != null)
            {
                uiTextRemainsDuration.SetGameObjectActive(buffRemainsDuration > 0);
                uiTextRemainsDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyBuffRemainsDuration),
                    buffRemainsDuration.ToString("N0"));
            }

            if (imageDurationGage != null)
            {
                imageDurationGage.fillAmount = buffDuration <= 0 ? 0 : buffRemainsDuration / buffDuration;
                imageDurationGage.gameObject.SetActive(imageDurationGage.fillAmount > 0f);
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            // Update remains duration
            if (buffRemainsDuration <= 0f && CharacterBuff != null)
                buffRemainsDuration = CharacterBuff.buffRemainsDuration;
        }

        protected override void UpdateData()
        {
            // Update remains duration
            if (CharacterBuff != null && Mathf.Abs(CharacterBuff.buffRemainsDuration - buffRemainsDuration) > 1)
                buffRemainsDuration = CharacterBuff.buffRemainsDuration;

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

            if (imageIcon != null)
            {
                Sprite iconSprite = tempGameData == null ? null : tempGameData.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

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
