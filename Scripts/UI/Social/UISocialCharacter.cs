using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class UISocialCharacter : UISelectionEntry<UISocialCharacterData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public UISocialGroup uiSocialGroup;
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        // HP
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public TextWrapper uiTextHp;
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public Image imageHpGage;
        public UIGageValue uiGageHp;
        // MP
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public TextWrapper uiTextMp;
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public Image imageMpGage;
        public UIGageValue uiGageMp;

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

        protected override void Awake()
        {
            base.Awake();
            MigrateUIGageValue();
        }

        protected void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateUIGageValue())
                EditorUtility.SetDirty(this);
#endif
        }

        protected override void Update()
        {
            base.Update();

            // Member status
            foreach (GameObject obj in memberIsOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(BaseGameNetworkManager.IsCharacterOnline(Data.socialCharacter.id));
            }

            foreach (GameObject obj in memberIsNotOnlineObjects)
            {
                if (obj != null)
                    obj.SetActive(!BaseGameNetworkManager.IsCharacterOnline(Data.socialCharacter.id));
            }

            BaseGameNetworkManager.RequestOnlineCharacter(Data.socialCharacter.id);
        }

        private bool MigrateUIGageValue()
        {
            return UIGageValue.Migrate(ref uiGageHp, ref uiTextHp, ref imageHpGage) ||
                UIGageValue.Migrate(ref uiGageMp, ref uiTextMp, ref imageMpGage);
        }

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
            if (uiGageHp != null)
            {
                uiGageHp.Update(currentHp, maxHp);
                if (uiGageHp.textValue != null)
                    uiGageHp.textValue.gameObject.SetActive(maxHp > 0);
            }

            // Mp
            int currentMp = Data.socialCharacter.currentMp;
            int maxMp = Data.socialCharacter.maxMp;
            if (uiGageMp != null)
            {
                uiGageMp.Update(currentMp, maxMp);
                if (uiGageMp.textValue != null)
                    uiGageMp.textValue.gameObject.SetActive(maxMp > 0);
            }

            // Buffs
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data.characterEntity);

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
