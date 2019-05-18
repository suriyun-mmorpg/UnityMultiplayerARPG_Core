using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UISocialCharacter : UISelectionEntry<SocialCharacterEntityTuple>
    {
        /// <summary>
        /// Format => {0} = {Character Name}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public string formatName = "{0}";
        /// <summary>
        /// Format => {0} = {Level Label}, {1} = {Level}
        /// </summary>
        [Tooltip("Format => {0} = {Level Label}, {1} = {Level}")]
        public string formatLevel = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Hp Label}, {1} = {Current Hp}, {2} = {Max Hp}
        /// </summary>
        [Tooltip("Format => {0} = {Hp Label}, {1} = {Current Hp}, {2} = {Max Hp}")]
        public string formatHp = "{0}: {1}/{2}";
        /// <summary>
        /// Format => {0} = {Mp Label}, {1} = {Current Mp}, {2} = {Max Mp}
        /// </summary>
        [Tooltip("Format => {0} = {Mp Label}, {1} = {Current Mp}, {2} = {Max Mp}")]
        public string formatMp = "{0}: {1}/{2}";

        [Header("UI Elements")]
        public UISocialGroup uiSocialGroup;
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextHp;
        public Image imageHpGage;
        public TextWrapper uiTextMp;
        public Image imageMpGage;
        public UICharacterBuffs uiCharacterBuffs;
        [Header("Member states objects")]
        [Tooltip("These objects will be activated when social member -> isOnline is true")]
        public GameObject[] memberIsOnlineObjects;
        [Tooltip("These objects will be activated when social member -> isOnline is false")]
        public GameObject[] memberIsNotOnlineObjects;
        [Tooltip("These objects will be activated when this social member is leader")]
        public GameObject[] memberIsLeaderObjects;
        [Tooltip("These objects will be activated when this social member is not leader")]
        public GameObject[] memberIsNotLeaderObjects;
        public UICharacterClass uiCharacterClass;

        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = string.Format(formatName, string.IsNullOrEmpty(Data.socialCharacter.characterName) ? LanguageManager.GetUnknowTitle() : Data.socialCharacter.characterName);

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    formatLevel,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_LEVEL.ToString()),
                    Data.socialCharacter.level.ToString("N0"));
            }

            // Hp
            int currentHp = Data.socialCharacter.currentHp;
            int maxHp = Data.socialCharacter.maxHp;

            if (uiTextHp != null)
            {
                uiTextHp.text = string.Format(
                    formatHp,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_HP.ToString()),
                    currentHp.ToString("N0"),
                    maxHp.ToString("N0"));
                uiTextHp.gameObject.SetActive(maxHp > 0);
            }

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;

            // Mp
            int currentMp = Data.socialCharacter.currentMp;
            int maxMp = Data.socialCharacter.maxMp;

            if (uiTextMp != null)
            {
                uiTextMp.text = string.Format(
                    formatMp,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_MP.ToString()),
                    currentMp.ToString("N0"),
                    maxMp.ToString("N0"));
                uiTextMp.gameObject.SetActive(maxMp > 0);
            }

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            // Buffs
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data.characterEntity);

            // Member status
            foreach (GameObject obj in memberIsOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(uiSocialGroup.IsOnline(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsNotOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(!uiSocialGroup.IsOnline(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!string.IsNullOrEmpty(Data.socialCharacter.id) && uiSocialGroup.IsLeader(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(string.IsNullOrEmpty(Data.socialCharacter.id) || !uiSocialGroup.IsLeader(Data.socialCharacter.id));
            }

            // Character class data
            PlayerCharacter character = null;
            GameInstance.PlayerCharacters.TryGetValue(Data.socialCharacter.dataId, out character);
            if (uiCharacterClass != null)
                uiCharacterClass.Data = character;
        }
    }
}
