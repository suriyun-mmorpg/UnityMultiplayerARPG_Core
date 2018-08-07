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

            var skill = Data.GetSkill();
            var item = Data.GetItem();

            collectedDeltaTime = 0f;

            if (Data.type == BuffType.SkillBuff || Data.type == BuffType.SkillDebuff)
            {
                if (uiTextTitle != null)
                    uiTextTitle.text = string.Format(titleFormat, skill == null ? "Unknow" : skill.title);

                if (imageIcon != null)
                {
                    var iconSprite = skill == null ? null : skill.icon;
                    imageIcon.gameObject.SetActive(iconSprite != null);
                    imageIcon.sprite = iconSprite;
                }
            }

            if (Data.type == BuffType.PotionBuff)
            {
                if (uiTextTitle != null)
                    uiTextTitle.text = string.Format(titleFormat, item == null ? "Unknow" : item.title);

                if (imageIcon != null)
                {
                    var iconSprite = item == null ? null : item.icon;
                    imageIcon.gameObject.SetActive(iconSprite != null);
                    imageIcon.sprite = iconSprite;
                }
            }

            if (uiBuff != null)
            {
                if (skill == null && item == null)
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
            uiTextTitle = UIWrapperHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDuration = UIWrapperHelpers.SetWrapperToText(textDuration, uiTextDuration);
            uiTextRemainsDuration = UIWrapperHelpers.SetWrapperToText(textRemainsDuration, uiTextRemainsDuration);
        }
    }
}
