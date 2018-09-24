using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        public InputFieldWrapper inputFieldGuildName;

        public void OnClickCreate()
        {
            BasePlayerCharacterController.OwningCharacter.RequestCreateGuild(
                inputFieldGuildName != null ? inputFieldGuildName.text : string.Empty);
            Hide();
        }
    }
}
