using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIPartySetting : UIBase
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

        public void OnClickSetting()
        {
            GameInstance.ClientPartyHandlers.RequestChangePartySetting(new RequestChangePartySettingMessage()
            {
                shareExp = toggleShareExp != null && toggleShareExp.isOn,
                shareItem = toggleShareItem != null && toggleShareItem.isOn,
            }, ChangePartySettingCallback);
        }

        private void ChangePartySettingCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangePartySettingMessage response)
        {
            ClientPartyActions.ResponseChangePartySetting(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }
    }
}
