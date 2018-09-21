using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UISocialCharacter : UISelectionEntry<SocialCharacterEntityTuple>
    {
        [Header("Display Format")]
        [Tooltip("Name Format => {0} = {Character name}")]
        public string nameFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";

        [Header("Stats")]
        [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}")]
        public string hpFormat = "Hp: {0}/{1}";
        [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}")]
        public string mpFormat = "Mp: {0}/{1}";

        [Header("Class")]
        [Tooltip("Class Title Format => {0} = {Class title}")]
        public string classTitleFormat = "Class: {0}";
        [Tooltip("Class Description Format => {0} = {Class description}")]
        public string classDescriptionFormat = "{0}";

        [Header("UI Elements")]
        public UIParty uiParty;
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextHp;
        public Image imageHpGage;
        public TextWrapper uiTextMp;
        public Image imageMpGage;
        public UICharacterBuffs uiCharacterBuffs;
        [Header("Member states objects")]
        [Tooltip("These objects will be activated when partyMemberData -> isVisible is true")]
        public GameObject[] memberIsVisibleObjects;
        [Tooltip("These objects will be activated when partyMemberData -> isVisible is false")]
        public GameObject[] memberIsNotInvisibleObjects;
        [Tooltip("These objects will be activated when this party member is leader")]
        public GameObject[] memberIsLeaderObjects;
        [Tooltip("These objects will be activated when this party member is not leader")]
        public GameObject[] memberIsNotLeaderObjects;
        [Tooltip("These objects will be activated when owning character is leader")]
        public GameObject[] owningCharacterIsLeaderObjects;
        [Tooltip("These objects will be activated when owning character is not leader")]
        public GameObject[] owningCharacterIsNotLeaderObjects;
        [Header("Class information")]
        public TextWrapper uiTextClassTitle;
        public TextWrapper uiTextClassDescription;
        public Image imageClassIcon;

        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = string.Format(nameFormat, string.IsNullOrEmpty(Data.socialCharacter.characterName) ? "Unknow" : Data.socialCharacter.characterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Data.socialCharacter.level.ToString("N0"));

            // Hp
            var currentHp = Data.socialCharacter.currentHp;
            var maxHp = Data.socialCharacter.maxHp;

            if (uiTextHp != null)
            {
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));
                uiTextHp.gameObject.SetActive(maxHp > 0);
            }

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;

            // Mp
            var currentMp = Data.socialCharacter.currentMp;
            var maxMp = Data.socialCharacter.maxMp;

            if (uiTextMp != null)
            {
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));
                uiTextMp.gameObject.SetActive(maxMp > 0);
            }

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            // Buffs
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data.characterEntity);

            // Member status
            foreach (var obj in memberIsVisibleObjects)
            {
                if (obj != null)
                    obj.SetActive(Data.socialCharacter.isOnline);
            }

            foreach (var obj in memberIsNotInvisibleObjects)
            {
                if (obj != null)
                    obj.SetActive(!Data.socialCharacter.isOnline);
            }

            foreach (var obj in memberIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!string.IsNullOrEmpty(Data.socialCharacter.id) && Data.socialCharacter.id.Equals(uiParty.leaderId));
            }

            foreach (var obj in memberIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(string.IsNullOrEmpty(Data.socialCharacter.id) || !Data.socialCharacter.id.Equals(uiParty.leaderId));
            }

            foreach (var obj in owningCharacterIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(BasePlayerCharacterController.OwningCharacter.Id.Equals(uiParty.leaderId));
            }

            foreach (var obj in owningCharacterIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!BasePlayerCharacterController.OwningCharacter.Id.Equals(uiParty.leaderId));
            }

            // Character class data
            PlayerCharacter character = null;
            GameInstance.PlayerCharacters.TryGetValue(Data.socialCharacter.dataId, out character);

            if (uiTextClassTitle != null)
                uiTextClassTitle.text = string.Format(classTitleFormat, character == null ? "N/A" : character.title);

            if (uiTextClassDescription != null)
                uiTextClassDescription.text = string.Format(classDescriptionFormat, character == null ? "N/A" : character.description);

            if (imageClassIcon != null)
            {
                var iconSprite = character == null ? null : character.icon;
                imageClassIcon.gameObject.SetActive(iconSprite != null);
                imageClassIcon.sprite = iconSprite;
            }
        }
    }
}
