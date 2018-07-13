using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIRefineItem : UISelectionEntry<int>
    {
        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";
        [Tooltip("Success Rate Format => {0} = {Rate}")]
        public string successRateFormat = "Success Rate: {0}%";
        [Tooltip("Refining Level Format => {0} = {Refining Level}")]
        public string refiningLevelFormat = "{0}";

        [Header("UI Elements")]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public Text textRequireGold;
        public Text textSuccessRate;
        public Text textRefiningLevel;

        private bool hasSetData;

        protected override void UpdateUI()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            CharacterItem characterItem = null;
            Item equipmentItem = null;
            short level = 1;
            if (Data >= 0 && Data < owningCharacter.NonEquipItems.Count)
            {
                characterItem = owningCharacter.NonEquipItems[Data];
                equipmentItem = characterItem.GetEquipmentItem();
                level = characterItem.level;
            }
            var canRefine = characterItem != null && equipmentItem != null && level < equipmentItem.MaxLevel;
            ItemRefineLevel refineLevel = !canRefine ? null : equipmentItem.itemRefineInfo.levels[level - 1];
            if (uiRefiningItem != null)
            {
                if (characterItem == null)
                    uiRefiningItem.Hide();
                else
                {
                    uiRefiningItem.Setup(new CharacterItemLevelTuple(characterItem, level), owningCharacter, Data, string.Empty);
                    uiRefiningItem.Show();
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (!canRefine)
                    uiRequireItemAmounts.Hide();
                else
                {
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = refineLevel.CacheRequireItems;
                }
            }

            if (textRequireGold != null)
            {
                if (!canRefine)
                    textRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"));
                else
                    textRequireGold.text = string.Format(requireGoldFormat, refineLevel.requireGold.ToString("N0"));
            }

            if (textSuccessRate != null)
            {
                if (!canRefine)
                    textSuccessRate.text = string.Format(successRateFormat, 0.ToString("N2"));
                else
                    textSuccessRate.text = string.Format(successRateFormat, refineLevel.successRate.ToString("N2"));
            }

            if (textRefiningLevel != null)
            {
                if (!canRefine)
                    textRefiningLevel.text = string.Format(refiningLevelFormat, "+" + (level - 1).ToString("N0"));
                else
                    textRefiningLevel.text = string.Format(refiningLevelFormat, "+" + level.ToString("N0"));
            }
        }

        public override void Hide()
        {
            hasSetData = false;
            base.Hide();
        }

        protected override void UpdateData()
        {
            hasSetData = true;
        }

        public void OnClickRefine()
        {
            if (!hasSetData)
                return;
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestRefineItem(Data);
        }
    }
}
