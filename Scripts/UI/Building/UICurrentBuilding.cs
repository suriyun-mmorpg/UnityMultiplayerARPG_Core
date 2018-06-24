using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurrentBuilding : UIBase
{
    public void OnClickDestroy()
    {
        var controller = BasePlayerCharacterController.Singleton;
        if (controller != null)
            controller.DestroyBuilding();
    }

    public void OnClickDeselect()
    {
        var controller = BasePlayerCharacterController.Singleton;
        if (controller != null)
            controller.DeselectBuilding();
    }
}
