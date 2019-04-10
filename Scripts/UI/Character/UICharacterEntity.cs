using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UICharacterEntity : UIDamageableEntity<BaseCharacterEntity>
    {
        [Header("Character Entity - Display Format")]
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}")]
        public string mpFormat = "Mp: {0}/{1}";
        [Tooltip("Skill Cast Format => {0} = {Count down duration}")]
        public string skillCastFormat = "{0}";

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
                uiTextLevel.text = string.Format(levelFormat, Data == null ? "0" : Data.Level.ToString("N0"));

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
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            if (uiSkillCastContainer != null)
                uiSkillCastContainer.SetActive(castingSkillCountDown > 0 && castingSkillDuration > 0);

            if (uiTextSkillCast != null)
                uiTextSkillCast.text = string.Format(skillCastFormat, castingSkillCountDown.ToString("N0"));

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
