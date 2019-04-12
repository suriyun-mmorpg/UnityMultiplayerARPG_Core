using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIEquipmentSockets : UIBaseEquipmentBonus<EnhancedSocketsWithMaxSocketTuple>
    {
        [Header("Equipment Sockets Format")]
        [Tooltip("Filled socket format => {0} = {Item Index}, {1} = {Item Title}, {2} = {Effects}")]
        [Multiline]
        public string filledSocketFormat = "<color=#800080ff>({0}) - {1}\n{2}</color>";
        [Tooltip("Empty socket format => {0} = {Item Index}")]
        public string emptySocketFormat = "<color=#800080ff>({0}) - Empty</color>";

        protected override void UpdateData()
        {
            string allBonusText = string.Empty;
            Item tempItem;
            string tempText;
            for (int i = 0; i < Data.maxSocket; ++i)
            {
                if (i < Data.sockets.Count && GameInstance.Items.TryGetValue(Data.sockets[i], out tempItem))
                {
                    tempText = GetEquipmentBonusText(tempItem.socketEnhanceEffect);
                    if (!string.IsNullOrEmpty(tempText))
                    {
                        if (!string.IsNullOrEmpty(allBonusText))
                            allBonusText += "\n";
                        allBonusText += string.Format(filledSocketFormat, i + 1, tempItem.Title, tempText);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(allBonusText))
                        allBonusText += "\n";
                    allBonusText += string.Format(emptySocketFormat, i + 1);
                }
            }
            if (uiTextAllBonus != null)
            {
                uiTextAllBonus.gameObject.SetActive(!string.IsNullOrEmpty(allBonusText));
                uiTextAllBonus.text = allBonusText;
            }
        }
    }
}
