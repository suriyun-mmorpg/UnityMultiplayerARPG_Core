using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildMessage2Updater : MonoBehaviour
    {
        public InputFieldWrapper inputField;

        public void UpdateData()
        {
            if (inputField == null) return;
            GameInstance.ClientGuildHandlers.RequestChangeGuildMessage2(new RequestChangeGuildMessageMessage()
            {
                message = inputField.text,
            }, ClientGuildActions.ResponseChangeGuildMessage2);
        }
    }
}
