using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIRefineItem : UIDataForCharacter<CharacterItem>
    {
        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";
        [Tooltip("Success Rate Format => {0} = {Rate}")]
        public string successRateFormat = "Success Rate: {0}%";

        [Header("UI Elements")]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public Text textRequireGold;
        public Text textSuccessRate;

        protected override void UpdateData()
        {
            var characterItem = Data;
            var level = characterItem.level;
            var equipmentItem = characterItem.GetEquipmentItem();
            var canRefine = characterItem != null && equipmentItem != null && level < equipmentItem.MaxLevel;
            ItemRefineLevel refineLevel = !canRefine ? null : equipmentItem.itemRefineInfo.levels[level - 1];
            if (uiRefiningItem != null)
            {
                if (characterItem == null)
                    uiRefiningItem.Hide();
                else
                {
                    uiRefiningItem.Setup(new CharacterItemLevelTuple(characterItem, level), character, indexOfData, string.Empty);
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
        }
    }
}
