using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIEquipmentSockets : UIBaseEquipmentBonus<EnhancedSocketsWithMaxSocketTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Socket Index}, {1} = {Item Title}, {2} = {List Of Bonus}")]
        [Multiline]
        public string formatKeySocketFilled = UILocaleKeys.UI_FORMAT_EQUIPMENT_SOCKET_FILLED.ToString();
        [Tooltip("Format => {0} = {Socket Index}")]
        public string formatKeySocketEmpty = UILocaleKeys.UI_FORMAT_EQUIPMENT_SOCKET_EMPTY.ToString();

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
                        allBonusText += string.Format(
                            LanguageManager.GetText(formatKeySocketFilled),
                            i + 1,
                            tempItem.Title,
                            tempText);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(allBonusText))
                        allBonusText += "\n";
                    allBonusText += string.Format(
                        LanguageManager.GetText(formatKeySocketEmpty),
                        i + 1);
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
