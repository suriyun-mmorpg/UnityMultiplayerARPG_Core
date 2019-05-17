using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UISocialCharacter : UISelectionEntry<SocialCharacterEntityTuple>
    {
        [Header("Display Format")]
        [Tooltip("Name Format => {0} = {Character name}")]
        public string nameFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}, {1} = {Level Label}")]
        public string levelFormat = "{1}: {0}";

        [Header("Stats")]
        [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}, {2} = {Hp Label}")]
        public string hpFormat = "{2}: {0}/{1}";
        [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}, {2} = {Mp Label}")]
        public string mpFormat = "{2}: {0}/{1}";

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
                uiTextName.text = string.Format(nameFormat, string.IsNullOrEmpty(Data.socialCharacter.characterName) ? LanguageManager.GetUnknowTitle() : Data.socialCharacter.characterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Data.socialCharacter.level.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));

            // Hp
            int currentHp = Data.socialCharacter.currentHp;
            int maxHp = Data.socialCharacter.maxHp;

            if (uiTextHp != null)
            {
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_HP.ToString()));
                uiTextHp.gameObject.SetActive(maxHp > 0);
            }

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;

            // Mp
            int currentMp = Data.socialCharacter.currentMp;
            int maxMp = Data.socialCharacter.maxMp;

            if (uiTextMp != null)
            {
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_MP.ToString()));
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
