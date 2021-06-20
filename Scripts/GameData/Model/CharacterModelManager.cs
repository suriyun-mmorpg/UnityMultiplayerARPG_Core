using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterModelManager : BaseGameEntityComponent<BaseGameEntity>
    {
        public const byte HIDE_SETTER_ENTITY = 0;
        public const byte HIDE_SETTER_CONTROLLER = 1;

        [SerializeField]
        private BaseCharacterModel mainModel;
        public BaseCharacterModel MainModel { get { return mainModel; } set { mainModel = value; } }

        [SerializeField]
        private BaseCharacterModel fpsModelPrefab;
        [SerializeField]
        [FormerlySerializedAs("fpsModelOffsets")]
        [Tooltip("Position offsets from fps model container (Camera's transform)")]
        private Vector3 fpsModelPositionOffsets;
        [SerializeField]
        [Tooltip("Rotation offsets from fps model container (Camera's transform)")]
        private Vector3 fpsModelRotationOffsets;
        public BaseCharacterModel FpsModel { get; private set; }

        [SerializeField]
        private VehicleCharacterModel[] vehicleModels;

        public Dictionary<int, VehicleCharacterModel> CacheVehicleModels { get; private set; }

        private bool isSetupModels;
        private bool isSetupActiveModel;
        private BaseCharacterModel activeModel;
        public BaseCharacterModel ActiveModel
        {
            get
            {
                if (!isSetupActiveModel)
                {
                    // Setup models (assign ID)
                    SetupModels();
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
            SetupModels();
        }

        private void SetupModels()
        {
            if (isSetupModels)
                return;
            SetupMainModel();
            SetupVehicleModels();
            isSetupModels = true;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (SetupMainModel())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool SetupMainModel()
        {
            if (!mainModel)
            {
                mainModel = GetComponent<BaseCharacterModel>();
                if (mainModel)
                {
                    mainModel.Id = new StringBuilder(Entity.Identity.AssetId).ToString();
                    return true;
                }
                else
                {
                    Logging.LogError(ToString(), "Can't find main model");
                    return false;
                }
            }
            else
            {
                mainModel.Id = new StringBuilder(Entity.Identity.AssetId).ToString();
            }
            return false;
        }

        private bool SetupVehicleModels()
        {
            if (CacheVehicleModels == null)
            {
                CacheVehicleModels = new Dictionary<int, VehicleCharacterModel>();
                if (vehicleModels != null)
                {
                    foreach (VehicleCharacterModel vehicleModel in vehicleModels)
                    {
                        if (!vehicleModel.vehicleType) continue;
                        for (int i = 0; i < vehicleModel.modelsForEachSeats.Length; ++i)
                        {
                            vehicleModel.modelsForEachSeats[i].Id = new StringBuilder(Entity.Identity.AssetId).Append(vehicleModel.vehicleType.Id).Append(i).ToString();
                        }
                        CacheVehicleModels[vehicleModel.vehicleType.DataId] = vehicleModel;
                    }
                }
                return true;
            }
            return false;
        }

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
                FpsModel.gameObject.SetActive(IsFps);
        }

        public BaseCharacterModel InstantiateFpsModel(Transform container)
        {
            if (fpsModelPrefab == null)
                return null;
            FpsModel = Instantiate(fpsModelPrefab, container);
            FpsModel.transform.localPosition = fpsModelPositionOffsets;
            FpsModel.transform.localRotation = Quaternion.Euler(fpsModelRotationOffsets);
            FpsModel.SetEquipItems(MainModel.equipItems);
            FpsModel.SetEquipWeapons(MainModel.equipWeapons);
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
