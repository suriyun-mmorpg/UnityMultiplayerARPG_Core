using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIRefineItem : UISelectionEntry<int>
    {
        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";
        [Tooltip("Success Rate Format => {0} = {Rate}")]
        public string successRateFormat = "Success Rate: {0}%";
        [Tooltip("Refining Level Format => {0} = {Refining Level}")]
        public string refiningLevelFormat = "Refining To: +{0}";

        [Header("UI Elements")]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSuccessRate;
        public TextWrapper uiTextRefiningLevel;

        private bool hasSetData;

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UIRefineItem - Update UI");
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
                    uiRefiningItem.Setup(new CharacterItemTuple(characterItem, level, string.Empty), owningCharacter, Data);
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
                    uiRequireItemAmounts.Data = refineLevel.RequireItems;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!canRefine)
                    uiTextRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"));
                else
                    uiTextRequireGold.text = string.Format(requireGoldFormat, refineLevel.RequireGold.ToString("N0"));
            }

            if (uiTextSuccessRate != null)
            {
                if (!canRefine)
                    uiTextSuccessRate.text = string.Format(successRateFormat, 0.ToString("N2"));
                else
                    uiTextSuccessRate.text = string.Format(successRateFormat, refineLevel.SuccessRate.ToString("N2"));
            }

            if (uiTextRefiningLevel != null)
            {
                if (!canRefine)
                    uiTextRefiningLevel.text = string.Format(refiningLevelFormat, (level - 1).ToString("N0"));
                else
                    uiTextRefiningLevel.text = string.Format(refiningLevelFormat, level.ToString("N0"));
            }
            Profiler.EndSample();
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
            BasePlayerCharacterController.OwningCharacter.RequestRefineItem((ushort)Data);
        }
    }
}
