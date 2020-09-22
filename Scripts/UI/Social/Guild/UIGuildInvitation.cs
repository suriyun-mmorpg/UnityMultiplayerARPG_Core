using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIGuildInvitation : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        public UICharacter uiAnotherCharacter;
        public TextWrapper uiTextGuildName;

        protected override void UpdateData()
        {
            if (uiAnotherCharacter != null)
            {
                uiAnotherCharacter.NotForOwningCharacter = true;
                uiAnotherCharacter.Data = Data;
            }

            if (uiTextGuildName != null)
                uiTextGuildName.text = string.Format(LanguageManager.GetText(formatKeyGuildName), Data.TitleB);
        }

        public void OnClickAccept()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerAcceptGuildInvitation();
            Hide();
        }

        public void OnClickDecline()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerDeclineGuildInvitation();
            Hide();
        }
    }
}
