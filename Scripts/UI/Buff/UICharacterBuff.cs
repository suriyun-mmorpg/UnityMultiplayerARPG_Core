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

        protected float collectedDeltaTime;

        protected override void Update()
        {
            base.Update();
            MigrateUIComponents();
            var characterBuff = Data;
            collectedDeltaTime += Time.deltaTime;

            var buffRemainsDuration = characterBuff.buffRemainsDuration - collectedDeltaTime;
            if (buffRemainsDuration < 0)
                buffRemainsDuration = 0;
            var buffDuration = characterBuff.GetDuration();

            if (uiTextDuration != null)
                uiTextDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

            if (uiTextRemainsDuration != null)
                uiTextRemainsDuration.text = string.Format(buffRemainsDurationFormat, Mathf.CeilToInt(buffRemainsDuration).ToString("N0"));

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

            collectedDeltaTime = 0f;

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
                    uiBuff.Data = new BuffLevelTuple(buff, Data.level);
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
