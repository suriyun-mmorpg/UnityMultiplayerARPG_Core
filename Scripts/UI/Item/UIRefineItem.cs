using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class UIRefineItem : UISelectionEntry<int>
    {
        public CharacterItem CharacterItem
        {
            get
            {
                if (Data >= 0 && Data < BasePlayerCharacterController.OwningCharacter.NonEquipItems.Count)
                    return BasePlayerCharacterController.OwningCharacter.NonEquipItems[Data];
                return null;
            }
        }
        public short Level { get { return (short)(CharacterItem != null ? CharacterItem.level : 1); } }
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get { return EquipmentItem != null && Level < EquipmentItem.MaxLevel; } }
        public ItemRefineLevel RefineLevel { get { return !CanRefine ? null : EquipmentItem.itemRefineInfo.levels[Level - 1]; } }

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

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UIRefineItem - Update UI");
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;

            if (uiRefiningItem != null)
            {
                if (CharacterItem == null)
                    uiRefiningItem.Hide();
                else
                {
                    uiRefiningItem.Setup(new CharacterItemTuple(CharacterItem, Level, string.Empty), owningCharacter, Data);
                    uiRefiningItem.Show();
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (!CanRefine)
                    uiRequireItemAmounts.Hide();
                else
                {
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = RefineLevel.RequireItems;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!CanRefine)
                    uiTextRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"));
                else
                    uiTextRequireGold.text = string.Format(requireGoldFormat, RefineLevel.RequireGold.ToString("N0"));
            }

            if (uiTextSuccessRate != null)
            {
                if (!CanRefine)
                    uiTextSuccessRate.text = string.Format(successRateFormat, 0.ToString("N2"));
                else
                    uiTextSuccessRate.text = string.Format(successRateFormat, (RefineLevel.SuccessRate * 100f).ToString("N2"));
            }

            if (uiTextRefiningLevel != null)
            {
                if (!CanRefine)
                    uiTextRefiningLevel.text = string.Format(refiningLevelFormat, (Level - 1).ToString("N0"));
                else
                    uiTextRefiningLevel.text = string.Format(refiningLevelFormat, Level.ToString("N0"));
            }
            Profiler.EndSample();
        }

        public override void Hide()
        {
            Data = -1;
            base.Hide();
        }

        protected override void UpdateData()
        {
            // Do nothing
        }

        public void OnClickRefine()
        {
            if (Data < 0)
                return;
            BasePlayerCharacterController.OwningCharacter.RequestRefineItem((ushort)Data);
        }
    }
}
