using LiteNetLibManager;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildRoleSetting : UIBase
    {
        [Header("UI Elements")]
        public InputFieldWrapper inputFieldRoleName;
        public Toggle toggleCanInvite;
        public Toggle toggleCanKick;
        public InputFieldWrapper inputFieldShareExpPercentage;

        private byte guildRole;

        public void Show(byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            base.Show();

            this.guildRole = guildRole;
            if (inputFieldRoleName != null)
            {
                inputFieldRoleName.unityInputField.contentType = InputField.ContentType.Standard;
                inputFieldRoleName.text = roleName;
            }
            if (toggleCanInvite != null)
                toggleCanInvite.isOn = canInvite;
            if (toggleCanKick != null)
                toggleCanKick.isOn = canKick;
            if (inputFieldShareExpPercentage != null)
            {
                inputFieldShareExpPercentage.unityInputField.contentType = InputField.ContentType.IntegerNumber;
                inputFieldShareExpPercentage.text = shareExpPercentage.ToString("N0");
            }
        }

        public void OnClickSetting()
        {
            byte shareExpPercentage;
            if (inputFieldRoleName == null ||
                string.IsNullOrEmpty(inputFieldRoleName.text))
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_GUILD_ROLE_NAME_TOO_SHORT.ToString()));
                return;
            }
            if (inputFieldShareExpPercentage == null ||
                !byte.TryParse(inputFieldShareExpPercentage.text, out shareExpPercentage))
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_GUILD_ROLE_SHARE_EXP_NOT_NUMBER.ToString()));
                return;
            }

            GameInstance.ClientGuildHandlers.RequestChangeGuildRole(new RequestChangeGuildRoleMessage()
            {
                guildRole = guildRole,
                name = inputFieldRoleName.text,
                canInvite = toggleCanInvite != null && toggleCanInvite.isOn,
                canKick = toggleCanKick != null && toggleCanKick.isOn,
                shareExpPercentage = shareExpPercentage,
            }, ChangeGuildRoleCallback);
        }

        private void ChangeGuildRoleCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildRoleMessage response)
        {
            ClientGuildActions.ResponseChangeGuildRole(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            Hide();
        }
    }
}
