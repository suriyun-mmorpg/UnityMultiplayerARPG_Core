using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuilding : UIBase
{
    public void OnClickConfirmBuild()
    {
        var controller = BasePlayerCharacterController.Singleton;
        if (controller != null)
            controller.ConfirmBuild();
    }

    public void OnClickCancelBuild()
    {
        var controller = BasePlayerCharacterController.Singleton;
        if (controller != null)
            controller.CancelBuild();
    }
}
