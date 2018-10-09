using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIPartyCreate : UIBase
    {
        public Toggle toggleShareExp;
        public Toggle toggleShareItem;

        public void Show(bool shareExp, bool shareItem)
        {
            base.Show();
            if (toggleShareExp != null)
                toggleShareExp.isOn = shareExp;
            if (toggleShareItem != null)
                toggleShareItem.isOn = shareItem;
        }

        public void OnClickCreate()
        {
            BasePlayerCharacterController.OwningCharacter.RequestCreateParty(
                toggleShareExp != null && toggleShareExp.isOn,
                toggleShareItem != null && toggleShareItem.isOn);
            Hide();
        }
    }
}
