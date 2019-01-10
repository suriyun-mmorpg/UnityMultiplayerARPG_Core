using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterBuff : UIDataForCharacter<CharacterBuff>
    {
        public CharacterBuff CharacterBuff { get { return Data; } }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";

        [Header("Generic Buff Format")]
        [Tooltip("Buff Duration Format => {0} = {Duration}")]
        public string buffDurationFormat = "Duration: {0}";
        [Tooltip("Buff Remains Duration Format => {0} = {Remains duration}")]
        public string buffRemainsDurationFormat = "{0}";

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
                uiTextDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

            if (uiTextRemainsDuration != null)
            {
                uiTextRemainsDuration.text = string.Format(buffRemainsDurationFormat, Mathf.CeilToInt(buffRemainsDuration).ToString("N0"));
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
                uiTextTitle.text = string.Format(titleFormat, buffData == null ? "Unknow" : buffData.Title);

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
                    uiBuff.Data = new BuffTuple(buff, Data.level);
                }
            }
        }
    }
}
