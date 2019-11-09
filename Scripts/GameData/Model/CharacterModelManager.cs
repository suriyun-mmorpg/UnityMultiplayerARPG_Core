using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterModelManager : MonoBehaviour
    {
        public const byte HIDE_SETTER_ENTITY = 0;
        public const byte HIDE_SETTER_CONTROLLER = 1;

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

        private Dictionary<int, VehicleCharacterModel> cacheVehicleModels;
        public Dictionary<int, VehicleCharacterModel> CacheVehicleModels
        {
            get
            {
                if (cacheVehicleModels == null)
                {
                    cacheVehicleModels = new Dictionary<int, VehicleCharacterModel>();
                    foreach (VehicleCharacterModel vehicleModel in vehicleModels)
                    {
                        if (vehicleModel.vehicleType == null) continue;
                        cacheVehicleModels[vehicleModel.vehicleType.DataId] = vehicleModel;
                    }
                }
                return cacheVehicleModels;
            }
        }

        private BaseCharacterModel activeModel;
        public BaseCharacterModel ActiveModel
        {
            get
            {
                if (activeModel == null)
                {
                    activeModel = MainModel;
                    activeModel.SwitchModel(null);
                }
                return activeModel;
            }
        }

        public bool IsHide
        {
            get
            {
                foreach (bool hideState in hideStates.Values)
                {
                    if (hideState)
                        return true;
                }
                return false;
            }
        }

        private Dictionary<byte, bool> hideStates = new Dictionary<byte, bool>();
        private int dirtyVehicleDataId;
        private byte dirtySeatIndex;

        private void Awake()
        {
            SetupModelManager();
            SwitchModel(MainModel);
        }

        private bool SetupModelManager()
        {
            bool hasChanges = false;
            if (mainModel != null && mainModel.modelManager != this)
            {
                mainModel.modelManager = this;
                hasChanges = true;
            }

            if (vehicleModels != null && vehicleModels.Length > 0)
            {
                foreach (VehicleCharacterModel vehicleModel in vehicleModels)
                {
                    if (vehicleModel.modelsForEachSeats == null || vehicleModel.modelsForEachSeats.Length == 0) continue;
                    foreach (BaseCharacterModel modelsForEachSeat in vehicleModel.modelsForEachSeats)
                    {
                        if (modelsForEachSeat == null) continue;
                        if (modelsForEachSeat.modelManager != this)
                        {
                            modelsForEachSeat.modelManager = this;
                            hasChanges = true;
                        }
                    }
                }
            }
            return hasChanges;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool hasChanges = false;
            if (mainModel == null)
            {
                mainModel = GetComponent<BaseCharacterModel>();
                hasChanges = true;
            }

            if (SetupModelManager())
                hasChanges = true;

            if (hasChanges)
                EditorUtility.SetDirty(this);
        }
#endif

        public void UpdatePassengingVehicle(VehicleType vehicleType, byte seatIndex)
        {
            if (vehicleType != null)
            {
                if (dirtyVehicleDataId != vehicleType.DataId ||
                    dirtySeatIndex != seatIndex)
                {
                    dirtyVehicleDataId = vehicleType.DataId;
                    dirtySeatIndex = seatIndex;
                    VehicleCharacterModel tempData;
                    if (CacheVehicleModels.TryGetValue(dirtyVehicleDataId, out tempData) &&
                        seatIndex < tempData.modelsForEachSeats.Length)
                    {
                        SwitchModel(tempData.modelsForEachSeats[seatIndex]);
                    }
                    else
                    {
                        SwitchModel(MainModel);
                    }
                }
                return;
            }

            if (dirtyVehicleDataId != 0)
            {
                dirtyVehicleDataId = 0;
                dirtySeatIndex = 0;
                SwitchModel(MainModel);
            }
        }

        private void SwitchModel(BaseCharacterModel nextModel)
        {
            if (nextModel == activeModel) return;
            BaseCharacterModel previousModel = activeModel;
            activeModel = nextModel;
            activeModel.SwitchModel(previousModel);
        }

        public void SetHide(byte setter, bool hideState)
        {
            hideStates[setter] = hideState;
            MainModel.SetHide(IsHide);
        }
    }

    [System.Serializable]
    public struct VehicleCharacterModel
    {
        public VehicleType vehicleType;
        public BaseCharacterModel[] modelsForEachSeats;
    }
}
