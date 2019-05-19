using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UICharacterEntity : UIDamageableEntity<BaseCharacterEntity>
    {
        [Header("Character Entity - String Formats")]
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Current Mp}, {1} = {Max Mp}")]
        public UILocaleKeySetting formatKeyMp = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_CURRENT_MP);
        [Tooltip("Format => {0} = {Count Down Duration}")]
        public UILocaleKeySetting formatKeySkillCastDuration = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_SIMPLE);

        [Header("Character Entity - UI Elements")]
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextMp;
        public Image imageMpGage;
        public GameObject uiSkillCastContainer;
        public TextWrapper uiTextSkillCast;
        public Image imageSkillCastGage;
        public UICharacter uiCharacter;

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
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data == null ? "0" : Data.Level.ToString("N0"));
            }

            currentMp = 0;
            maxMp = 0;
            castingSkillCountDown = 0;
            castingSkillDuration = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.CacheMaxMp;
                castingSkillCountDown = Data.castingSkillCountDown;
                castingSkillDuration = Data.castingSkillDuration;
            }

            if (uiTextMp != null)
            {
                uiTextMp.text = string.Format(
                    LanguageManager.GetText(formatKeyMp),
                    currentMp.ToString("N0"),
                    maxMp.ToString("N0"));
            }

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            if (uiSkillCastContainer != null)
                uiSkillCastContainer.SetActive(castingSkillCountDown > 0 && castingSkillDuration > 0);

            if (uiTextSkillCast != null)
            {
                uiTextSkillCast.text = string.Format(
                    LanguageManager.GetText(formatKeySkillCastDuration), castingSkillCountDown.ToString("N2"));
            }

            if (imageSkillCastGage != null)
                imageSkillCastGage.fillAmount = castingSkillDuration <= 0 ? 0 : 1 - (castingSkillCountDown / castingSkillDuration);
        }

        protected override void UpdateUI()
        {
            if (!ValidateToUpdateUI())
            {
                CacheCanvas.enabled = false;
                return;
            }
            base.UpdateUI();

            // Update character UI every `updateUIRepeatRate` seconds
            if (uiCharacter != null)
                uiCharacter.Data = Data;
        }
    }
}
