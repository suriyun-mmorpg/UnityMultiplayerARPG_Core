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
            MigrateUIComponents();

            if (uiTextTitle != null)
                uiTextTitle.text = Data.title;
        }

        public void OnClickMenu()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSelectNpcDialogMenu((byte)Data.menuIndex);
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(title, uiTextTitle);
        }
    }
}
