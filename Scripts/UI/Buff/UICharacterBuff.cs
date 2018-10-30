using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterBuff : UIDataForCharacter<CharacterBuff>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";

        [Header("Generic Buff Format")]
        [Tooltip("Buff Duration Format => {0} = {Duration}")]
        public string buffDurationFormat = "Duration: {0}";
        [Tooltip("Buff Remains Duration Format => {0} = {Remains duration}")]
        public string buffRemainsDurationFormat = "{0}";

        [Header("UI Elements")]
        public Text textTitle;
        public TextWrapper uiTextTitle;
        public Image imageIcon;
        public Text textDuration;
        public TextWrapper uiTextDuration;
        public Text textRemainsDuration;
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
            MigrateUIComponents();
            var characterBuff = Data;

            if (buffRemainsDuration <= 0f)
            {
                buffRemainsDuration = characterBuff.buffRemainsDuration;
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
            var buffDuration = characterBuff.GetDuration();

            if (uiTextDuration != null)
                uiTextDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

            if (uiTextRemainsDuration != null)
            {
                if (buffRemainsDuration > 0f)
                    uiTextRemainsDuration.text = string.Format(buffRemainsDurationFormat, Mathf.CeilToInt(buffRemainsDuration).ToString("N0"));
                else
                    uiTextRemainsDuration.text = "";
            }

            if (imageDurationGage != null)
                imageDurationGage.fillAmount = buffDuration <= 0 ? 0 : buffRemainsDuration / buffDuration;
        }

        protected override void UpdateData()
        {
            MigrateUIComponents();

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
                uiTextTitle.text = string.Format(titleFormat, buffData == null ? "Unknow" : buffData.title);

            if (imageIcon != null)
            {
                var iconSprite = buffData == null ? null : buffData.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiBuff != null)
            {
                if (buffData == null)
                    uiBuff.Hide();
                else
                {
                    var buff = Data.GetBuff();
                    uiBuff.Show();
                    uiBuff.Data = new BuffTuple(buff, Data.level);
                }
            }
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDuration = MigrateUIHelpers.SetWrapperToText(textDuration, uiTextDuration);
            uiTextRemainsDuration = MigrateUIHelpers.SetWrapperToText(textRemainsDuration, uiTextRemainsDuration);
        }
    }
}
