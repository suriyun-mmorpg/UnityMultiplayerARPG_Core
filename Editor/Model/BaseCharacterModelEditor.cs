using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterModelEditor : BaseCustomEditor
    {
        protected override void SetFieldCondition()
        {
            ShowOnBool("IsMainOrFpsModel", true, "hiddingObjects");
            ShowOnBool("IsMainOrFpsModel", true, "hiddingRenderers");
            ShowOnBool("IsMainOrFpsModel", true, "fpsHiddingObjects");
            ShowOnBool("IsMainOrFpsModel", true, "fpsHiddingRenderers");
            ShowOnBool("IsMainOrFpsModel", true, "effectContainers");
            ShowOnBool("IsMainOrFpsModel", true, "setEffectContainersBySetters");
            ShowOnBool("IsMainOrFpsModel", true, "equipmentContainers");
            ShowOnBool("IsMainOrFpsModel", true, "setEquipmentContainersBySetters");
            ShowOnBool("IsMainOrFpsModel", true, "deactivateInstantiatedObjects");
            ShowOnBool("IsMainOrFpsModel", true, "activateInstantiatedObject");
        }
    }
}
