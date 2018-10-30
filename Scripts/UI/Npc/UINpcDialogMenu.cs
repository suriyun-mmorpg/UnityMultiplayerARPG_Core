using UnityEngine;

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
        public TextWrapper uiTextTitle;
        public UINpcDialog uiNpcDialog;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = Data.title;
        }

        public void OnClickMenu()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSelectNpcDialogMenu((byte)Data.menuIndex);
        }
    }
}
