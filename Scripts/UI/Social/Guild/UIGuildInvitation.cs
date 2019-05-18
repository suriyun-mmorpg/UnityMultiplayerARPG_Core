using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIGuildInvitation : UISelectionEntry<BasePlayerCharacterEntity>
    {
        /// <summary>
        /// Format => {0} = {Guild Name}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Guild Name}")]
        public string formatGuildName = "{0}";

        public UICharacter uiAnotherCharacter;
        public TextWrapper uiTextGuildName;

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity anotherCharacter = Data;

            if (uiAnotherCharacter != null)
                uiAnotherCharacter.Data = anotherCharacter;

            if (uiTextGuildName != null)
                uiTextGuildName.text = string.Format(formatGuildName, Data.TitleB);
        }

        public void OnClickAccept()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestAcceptGuildInvitation();
            Hide();
        }

        public void OnClickDecline()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestDeclineGuildInvitation();
            Hide();
        }
    }
}
