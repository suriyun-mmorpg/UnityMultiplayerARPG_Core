using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterEntity : UIDamageableEntity<BaseCharacterEntity>
    {
        [Header("Character Entity - String Formats")]
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Count Down Duration}")]
        public UILocaleKeySetting formatKeySkillCastDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("Character Entity - UI Elements")]
        public TextWrapper uiTextLevel;
        // Mp
        public UIGageValue uiGageMp;
        // Skill cast
        public GameObject uiSkillCastContainer;
        public TextWrapper uiTextSkillCast;
        public Image imageSkillCastGage;
        public UICharacterBuffs uiCharacterBuffs;

        protected int currentMp;
        protected int maxMp;
        protected float castingSkillCountDown;
        protected float castingSkillDuration;

        protected override void Update()
        {
            base.Update();

            if (!CacheCanvas.enabled)
                return;

            if (uiTextLevel != null)
            {
                uiTextLevel.text = ZString.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data == null ? "1" : Data.Level.ToString("N0"));
            }

            currentMp = 0;
            maxMp = 0;
            castingSkillCountDown = 0;
            castingSkillDuration = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.GetCaches().MaxMp;
                castingSkillCountDown = Data.CastingSkillCountDown;
                castingSkillDuration = Data.CastingSkillDuration;
            }
            if (uiGageMp != null)
                uiGageMp.Update(currentMp, maxMp);

            if (uiSkillCastContainer != null)
                uiSkillCastContainer.SetActive(castingSkillCountDown > 0 && castingSkillDuration > 0);

            if (uiTextSkillCast != null)
            {
                uiTextSkillCast.text = ZString.Format(
                    LanguageManager.GetText(formatKeySkillCastDuration), castingSkillCountDown.ToString("N2"));
            }

            if (imageSkillCastGage != null)
                imageSkillCastGage.fillAmount = castingSkillDuration <= 0 ? 0 : 1 - (castingSkillCountDown / castingSkillDuration);
        }

        protected override bool ValidateToUpdateUI()
        {
            return base.ValidateToUpdateUI() && (Data.IsOwnerClient || !Data.IsHide());
        }

        protected override void UpdateUI()
        {
            if (!ValidateToUpdateUI())
            {
                CacheCanvas.enabled = false;
                return;
            }
            base.UpdateUI();

            // Update character buffs every `updateUIRepeatRate` seconds
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data);
        }
    }
}
