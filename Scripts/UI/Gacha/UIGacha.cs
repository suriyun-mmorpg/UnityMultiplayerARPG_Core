using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGacha : UISelectionEntry<Gacha>
    {
        [Header("UI Elements")]
        public UIGachas uiGachas;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;

        protected override void UpdateData()
        {
        }
    }
}
