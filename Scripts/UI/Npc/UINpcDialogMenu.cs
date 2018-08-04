using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class UINpcDialogMenuAction
    {
        public string title;
        public int menuIndex;
    }

    public partial class UINpcDialogMenu : UISelectionEntry<UINpcDialogMenuAction>
    {
        [Header("UI Elements")]
        public Text title;
        public TextWrapper uiTextTitle;
        public UINpcDialog uiNpcDialog;

        protected override void UpdateData()
        {
            UpdateUIComponents();

            if (uiTextTitle != null)
                uiTextTitle.text = Data.title;
        }

        public void OnClickMenu()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSelectNpcDialogMenu(Data.menuIndex);
        }

        [ContextMenu("Update UI Components")]
        public void UpdateUIComponents()
        {
            uiTextTitle = UIWrapperHelpers.SetWrapperToText(title, uiTextTitle);
        }
    }
}
