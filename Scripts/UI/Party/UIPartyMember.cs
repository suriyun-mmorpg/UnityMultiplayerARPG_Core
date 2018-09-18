using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIPartyMember : UISelectionEntry<PartyMemberEntityTuple>
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
        public GameObject[] visibleObjects;
        [Tooltip("These objects will be activated when partyMemberData -> isVisible is false")]
        public GameObject[] invisibleObjects;
        [Tooltip("These objects will be activated when this party member is leader")]
        public GameObject[] leaderMemberObjects;
        [Tooltip("These objects will be activated when this party member is not leader")]
        public GameObject[] nonLeaderMemberObjects;
        [Tooltip("These objects will be activated when owning character is leader")]
        public GameObject[] leaderObjects;
        [Tooltip("These objects will be activated when owning character is not leader")]
        public GameObject[] nonLeaderObjects;
        [Header("Class information")]
        public TextWrapper uiTextClassTitle;
        public TextWrapper uiTextClassDescription;
        public Image imageClassIcon;
        
        protected override void UpdateData()
        {
            if (uiTextName != null)
                uiTextName.text = string.Format(nameFormat, string.IsNullOrEmpty(Data.partyMember.characterName) ? "Unknow" : Data.partyMember.characterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Data.partyMember.level.ToString("N0"));

            // Hp
            var currentHp = Data.partyMember.currentHp;
            var maxHp = Data.partyMember.maxHp;

            if (uiTextHp != null)
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 1 : (float)currentHp / (float)maxHp;

            // Mp
            var currentMp = Data.partyMember.currentMp;
            var maxMp = Data.partyMember.maxMp;

            if (uiTextMp != null)
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 1 : (float)currentMp / (float)maxMp;

            // Buffs
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data.characterEntity);

            // Member status
            foreach (var visibleObject in visibleObjects)
            {
                if (visibleObject != null)
                    visibleObject.SetActive(Data.partyMember.isVisible);
            }

            foreach (var invisibleObject in invisibleObjects)
            {
                if (invisibleObject != null)
                    invisibleObject.SetActive(!Data.partyMember.isVisible);
            }

            foreach (var leaderObject in leaderMemberObjects)
            {
                if (leaderObject != null)
                    leaderObject.SetActive(!string.IsNullOrEmpty(Data.partyMember.id) && Data.partyMember.id.Equals(uiParty.leaderId));
            }

            foreach (var nonLeaderObject in nonLeaderMemberObjects)
            {
                if (nonLeaderObject != null)
                    nonLeaderObject.SetActive(string.IsNullOrEmpty(Data.partyMember.id) || !Data.partyMember.id.Equals(uiParty.leaderId));
            }

            // Character class data
            PlayerCharacter character = null;
            GameInstance.PlayerCharacters.TryGetValue(Data.partyMember.dataId, out character);

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

        public void OnClickKickMember()
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Kick Member", string.Format("You sure you want to kick {0} from party?", Data.partyMember.characterName), false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromParty(Data.partyMember.id);
            });
        }
    }
}
