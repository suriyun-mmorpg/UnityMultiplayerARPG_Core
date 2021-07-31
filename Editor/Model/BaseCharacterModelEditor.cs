using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(BaseCharacterModel), true)]
    [CanEditMultipleObjects]
    public class BaseCharacterModelEditor : BaseCustomEditor
    {
        protected override void SetFieldCondition()
        {
            ShowOnBool("IsMainModel", true, "hiddingObjects");
            ShowOnBool("IsMainModel", true, "hiddingRenderers");
            ShowOnBool("IsMainModel", true, "fpsHiddingObjects");
            ShowOnBool("IsMainModel", true, "fpsHiddingRenderers");
            ShowOnBool("IsMainModel", true, "effectContainers");
            ShowOnBool("IsMainModel", true, "setEffectContainersBySetters");
            ShowOnBool("IsMainModel", true, "equipmentContainers");
            ShowOnBool("IsMainModel", true, "setEquipmentContainersBySetters");
            ShowOnBool("IsMainModel", true, "deactivateInstantiatedObjects");
            ShowOnBool("IsMainModel", true, "activateInstantiatedObject");
        }
    }
}
