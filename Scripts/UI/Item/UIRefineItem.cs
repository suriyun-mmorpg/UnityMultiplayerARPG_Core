using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIRefineItem : BaseUICharacterItemByIndex
    {
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get { return EquipmentItem != null && Level < EquipmentItem.MaxLevel; } }
        public ItemRefineLevel RefineLevel { get { return EquipmentItem.itemRefineInfo.levels[Level - 1]; } }

        [Header("Format for UI Refine Item")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";
        [Tooltip("Success Rate Format => {0} = {Rate}")]
        public string successRateFormat = "Success Rate: {0}%";
        [Tooltip("Refining Level Format => {0} = {Refining Level}")]
        public string refiningLevelFormat = "Refining To: +{0}";

        [Header("UI Elements for UI Refine Item")]
        [HideInInspector]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSuccessRate;
        public TextWrapper uiTextRefiningLevel;

        protected override void Awake()
        {
            base.Awake();
            if (uiCharacterItem == null && uiRefiningItem != null)
                uiCharacterItem = uiRefiningItem;
        }

        public void OnUpdateCharacterItems()
        {
            if (uiCharacterItem != null)
            {
                if (CharacterItem == null)
                    uiCharacterItem.Hide();
                else
                {
                    uiCharacterItem.Setup(new CharacterItemTuple(CharacterItem, Level, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
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
        }

        public override void Hide()
        {
            Data = new CharacterItemByIndexTuple(InventoryType.NonEquipItems, -1);
            base.Hide();
        }

        public void OnClickRefine()
        {
            if (IndexOfData < 0)
                return;
            OwningCharacter.RequestRefineItem((byte)InventoryType, (short)IndexOfData);
        }
    }
}
