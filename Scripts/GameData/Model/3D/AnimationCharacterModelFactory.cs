using UnityEngine;

namespace MultiplayerARPG
{
    public class AnimationCharacterModelFactory : ICharacterModelFactory
    {
        public string Name => "Animation Character Model";
        public DimensionType DimensionType => DimensionType.Dimension3D;

        public AnimationCharacterModelFactory()
        {

        }

        public bool ValidateSourceObject(GameObject obj)
        {
            Animation comp = obj.GetComponentInChildren<Animation>();
            if (comp == null)
            {
                Debug.LogError("Cannot create new entity with `AnimationCharacterModel`, can't find `Animation` component");
                Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        public BaseCharacterModel Setup(GameObject obj)
        {
            AnimationCharacterModel characterModel = obj.AddComponent<AnimationCharacterModel>();
            characterModel.legacyAnimation = obj.GetComponentInChildren<Animation>();
            return characterModel;
        }
    }
}
