using UnityEngine;

namespace MultiplayerARPG
{
    public class AnimatorCharacterModelFactory : ICharacterModelFactory
    {
        public string Name => "Animator Character Model";
        public DimensionType DimensionType => DimensionType.Dimension3D;

        public AnimatorCharacterModelFactory()
        {

        }

        public bool ValidateSourceObject(GameObject obj)
        {
            Animator comp = obj.GetComponentInChildren<Animator>();
            if (comp == null)
            {
                Debug.LogError("Cannot create new entity with `AnimatorCharacterModel`, can't find `Animator` component");
                Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        public BaseCharacterModel Setup(GameObject obj)
        {
            AnimatorCharacterModel characterModel = obj.AddComponent<AnimatorCharacterModel>();
            characterModel.animator = obj.GetComponentInChildren<Animator>();
            return characterModel;
        }
    }
}
