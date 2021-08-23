using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildAutoAcceptRequestUpdater : MonoBehaviour
    {
        public Toggle toggle;

        public void UpdateData()
        {
            if (toggle == null) return;
            GameInstance.ClientGuildHandlers.RequestChangeGuildAutoAcceptRequests(new RequestChangeGuildAutoAcceptRequestsMessage()
            {
                autoAcceptRequests = toggle.isOn,
            }, ClientGuildActions.ResponseChangeGuildAutoAcceptRequests);
        }
    }
}
