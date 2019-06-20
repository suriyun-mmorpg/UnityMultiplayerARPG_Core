using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterModelEditor : BaseCustomEditor
    {
        protected override void SetFieldCondition()
        {
            ShowOnBool("isMainModel", true, "hiddingObjects");
            ShowOnBool("isMainModel", true, "effectContainers");
            ShowOnBool("isMainModel", true, "setEffectContainersBySetters");
            ShowOnBool("isMainModel", true, "equipmentContainers");
            ShowOnBool("isMainModel", true, "setEquipmentContainersBySetters");
        }
    }
}
