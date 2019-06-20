using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class CharacterModelManager : MonoBehaviour
    {
        [SerializeField]
        private BaseCharacterModel mainModel;
        public BaseCharacterModel MainModel
        {
            get
            {
                if (mainModel == null)
                    mainModel = GetComponent<BaseCharacterModel>();
                return mainModel;
            }
        }

        [SerializeField]
        private VehicleCharacterModel[] vehicleModels;

        private BaseCharacterModel activeModel;
        public BaseCharacterModel ActiveModel
        {
            get
            {
                if (activeModel == null)
                    activeModel = MainModel;
                return activeModel;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mainModel == null)
                mainModel = GetComponent<BaseCharacterModel>();

            if (mainModel != null)
                mainModel.modelManager = this;
        }
#endif
    }

    [System.Serializable]
    public struct VehicleCharacterModel
    {
        public VehicleType vehicleType;
        public BaseCharacterModel[] modelsForEachSeats;
    }
}
