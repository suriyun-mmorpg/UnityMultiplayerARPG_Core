using LiteNetLib.Utils;
using LiteNetLibManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class NetworkReponseUtils
    {
        public static bool ShowUnhandledResponseMessageDialog(this AckResponseCode responseCode, Action errorHandler)
        {
            switch (responseCode)
            {
                case AckResponseCode.Unimplemented:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString()));
                    return true;
                case AckResponseCode.Error:
                    if (errorHandler != null)
                        errorHandler.Invoke();
                    return true;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                    return true;
            }
            return false;
        }
    }
}
