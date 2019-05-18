using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UISocialCharacter : UISelectionEntry<SocialCharacterEntityTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public string formatKeyName = UILocaleKeys.UI_FORMAT_SIMPLE.ToString();
        [Tooltip("Format => {0} = {Level}")]
        public string formatKeyLevel = UILocaleKeys.UI_FORMAT_LEVEL.ToString();
        [Tooltip("Format => {0} = {Current Hp}, {1} = {Max Hp}")]
        public string formatKeyHp = UILocaleKeys.UI_FORMAT_CURRENT_HP.ToString();
        [Tooltip("Format => {0} = {Current Mp}, {1} = {Max Mp}")]
        public string formatKeyMp = UILocaleKeys.UI_FORMAT_CURRENT_MP.ToString();

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
            {
                uiTextName.text = string.Format(
                    LanguageManager.GetText(formatKeyName),
                    string.IsNullOrEmpty(Data.socialCharacter.characterName) ? LanguageManager.GetUnknowTitle() : Data.socialCharacter.characterName);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data.socialCharacter.level.ToString("N0"));
            }

            // Hp
            int currentHp = Data.socialCharacter.currentHp;
            int maxHp = Data.socialCharacter.maxHp;

            if (uiTextHp != null)
            {
                uiTextHp.text = string.Format(
                    LanguageManager.GetText(formatKeyHp),
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
                    LanguageManager.GetText(formatKeyMp),
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
