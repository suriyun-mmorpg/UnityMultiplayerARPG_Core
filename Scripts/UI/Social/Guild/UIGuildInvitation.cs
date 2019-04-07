using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIGuildInvitation : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [Header("Display Format")]
        [Tooltip("Guild Name Format => {0} = {Guild name}")]
        public string guildNameFormat = "{0}";

        public UICharacter uiAnotherCharacter;
        public TextWrapper uiTextGuildName;

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity anotherCharacter = Data;

            if (uiAnotherCharacter != null)
                uiAnotherCharacter.Data = anotherCharacter;

            if (uiTextGuildName != null)
                uiTextGuildName.text = string.Format(guildNameFormat, Data.TitleB);
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
