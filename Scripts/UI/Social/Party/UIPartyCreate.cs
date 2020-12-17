using Cysharp.Threading.Tasks;
using LiteNetLibManager;
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
            GameInstance.ClientPartyHandlers.RequestCreateParty(new RequestCreatePartyMessage()
            {
                characterId = GameInstance.ClientUserHandlers.CharacterId,
                shareExp = toggleShareExp != null && toggleShareExp.isOn,
                shareItem = toggleShareItem != null && toggleShareItem.isOn,
            }, ClientPartyActions.ResponseCreateParty);
            Hide();
        }
    }
}
