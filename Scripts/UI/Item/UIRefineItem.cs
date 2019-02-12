using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class UIRefineItem : UISelectionEntry<InventoryType>
    {
        public CharacterItem CharacterItem
        {
            get
            {
                switch (Data)
                {
                    case InventoryType.NonEquipItems:
                        if (IndexOfData >= 0 && IndexOfData < OwningCharacter.NonEquipItems.Count)
                            return OwningCharacter.NonEquipItems[IndexOfData];
                        break;
                    case InventoryType.EquipItems:
                        if (IndexOfData >= 0 && IndexOfData < OwningCharacter.EquipItems.Count)
                            return OwningCharacter.EquipItems[IndexOfData];
                        break;
                    case InventoryType.EquipWeaponRight:
                        return OwningCharacter.EquipWeapons.rightHand;
                    case InventoryType.EquipWeaponLeft:
                        return OwningCharacter.EquipWeapons.leftHand;
                }
                return null;
            }
        }
        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }
        public int IndexOfData { get; protected set; }
        public short Level { get { return (short)(CharacterItem != null ? CharacterItem.level : 1); } }
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get { return EquipmentItem != null && Level < EquipmentItem.MaxLevel; } }
        public ItemRefineLevel RefineLevel { get { return EquipmentItem.itemRefineInfo.levels[Level - 1]; } }

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

        public void Setup(InventoryType inventoryType, int indexOfData)
        {
            IndexOfData = indexOfData;
            Data = inventoryType;
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UIRefineItem - Update UI");
            if (uiRefiningItem != null)
            {
                if (CharacterItem == null)
                    uiRefiningItem.Hide();
                else
                {
                    uiRefiningItem.Setup(new CharacterItemTuple(CharacterItem, Level, Data), OwningCharacter, IndexOfData);
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
            IndexOfData = -1;
            base.Hide();
        }

        protected override void UpdateData()
        {
            // Do nothing
        }

        public void OnClickRefine()
        {
            if (IndexOfData < 0)
                return;
            OwningCharacter.RequestRefineItem((byte)Data, (short)IndexOfData);
        }
    }
}
