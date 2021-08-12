using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildMessageUpdater : MonoBehaviour
    {
        public InputFieldWrapper inputField;

        public void UpdateData()
        {
            if (inputField == null) return;
            GameInstance.ClientGuildHandlers.RequestChangeGuildMessage(new RequestChangeGuildMessageMessage()
            {
                message = inputField.text,
            }, ClientGuildActions.ResponseChangeGuildMessage);
        }
    }
}
