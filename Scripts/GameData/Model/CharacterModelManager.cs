using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterModelManager : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public const byte HIDE_SETTER_ENTITY = 0;
        public const byte HIDE_SETTER_CONTROLLER = 1;

        [SerializeField]
        private BaseCharacterModel mainModel;
        public BaseCharacterModel MainModel { get { return mainModel; } }

        [SerializeField]
        private BaseCharacterModel fpsModelPrefab;
        public BaseCharacterModel FpsModel { get; private set; }

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

        private bool isSetupActiveModel;
        private BaseCharacterModel activeModel;
        public BaseCharacterModel ActiveModel
        {
            get
            {
                if (!isSetupActiveModel)
                {
                    // Check for main model
                    if (mainModel == null)
                        mainModel = GetComponent<BaseCharacterModel>();
                    // Clear active model to make sure it will initialize correctly
                    activeModel = null;
                    SwitchModel(MainModel);
                    isSetupActiveModel = true;
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
        public bool IsFps { get; private set; }

        private readonly Dictionary<byte, bool> hideStates = new Dictionary<byte, bool>();
        private int dirtyVehicleDataId;
        private byte dirtySeatIndex;

        public override void EntityAwake()
        {
            SetupModelManager();
        }

        private bool SetupModelManager()
        {
            bool hasChanges = false;

            if (mainModel != null && mainModel.ModelManager != this)
            {
                mainModel.ModelManager = this;
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
                        if (modelsForEachSeat.ModelManager != this)
                        {
                            modelsForEachSeat.ModelManager = this;
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
            if (activeModel != null && nextModel == activeModel) return;
            BaseCharacterModel previousModel = activeModel;
            activeModel = nextModel;
            activeModel.SwitchModel(previousModel);
        }

        public void SetIsHide(byte setter, bool isHide)
        {
            hideStates[setter] = isHide;
            UpdateVisibleState();
        }

        public void SetIsFps(bool isFps)
        {
            if (IsFps == isFps)
                return;
            IsFps = isFps;
            UpdateVisibleState();
        }

        public void UpdateVisibleState()
        {
            GameEntityModel.EVisibleState mainModelVisibleState = GameEntityModel.EVisibleState.Visible;
            if (IsFps)
                mainModelVisibleState = GameEntityModel.EVisibleState.Fps;
            if (IsHide)
                mainModelVisibleState = GameEntityModel.EVisibleState.Invisible;
            // Set visible state to main model
            MainModel.SetVisibleState(mainModelVisibleState);
            // FPS model will hide when it's not fps mode
            if (FpsModel != null)
                FpsModel.SetVisibleState(IsFps ? GameEntityModel.EVisibleState.Visible : GameEntityModel.EVisibleState.Invisible);
        }

        public BaseCharacterModel InstantiateFpsModel(Transform container)
        {
            if (fpsModelPrefab == null)
                return null;
            FpsModel = Instantiate(fpsModelPrefab, container);
            FpsModel.transform.localPosition = Vector3.zero;
            FpsModel.transform.localRotation = Quaternion.identity;
            return FpsModel;
        }
    }

    [System.Serializable]
    public struct VehicleCharacterModel
    {
        public VehicleType vehicleType;
        public BaseCharacterModel[] modelsForEachSeats;
    }
}
