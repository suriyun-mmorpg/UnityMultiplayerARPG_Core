using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIItemCraftFormula : UISelectionEntry<ItemCraftFormula>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Craft Duration}")]
        public UILocaleKeySetting formatKeyCraftDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRAFT_DURATION);

        [Header("UI Elements")]
        public TextWrapper uiTextDuration;
        public UIItemCraft uiItemCraft;
        public InputFieldWrapper inputAmount;

        public UIItemCraftFormulas CraftFormulaManager { get; set; }

        protected override void UpdateData()
        {
            if (uiTextDuration != null)
            {
                uiTextDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyCraftDuration),
                    Data.CraftDuration.ToString("N0"));
            }

            if (uiItemCraft != null)
            {
                if (Data == null)
                {
                    uiItemCraft.Hide();
                }
                else
                {
                    uiItemCraft.Show();
                    uiItemCraft.Data = Data.ItemCraft;
                }
            }
        }

        public void OnClickAppend()
        {
            int amount;
            if (inputAmount == null || !int.TryParse(inputAmount.text, out amount))
                amount = 1;
            GameInstance.PlayingCharacterEntity.CallServerAppendCraftingQueueItem(CraftFormulaManager.CraftingQueueManager.Source.ObjectId, Data.DataId, amount);
        }
    }
}
